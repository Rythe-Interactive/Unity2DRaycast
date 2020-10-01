using UnityEditor;
namespace UnityEngine.Rendering
{
    public class CameraRenderer
    {
        //command buffer and Camera
        private CommandBuffer m_buffer;
        private Camera m_cam;

        //rendering variables
        private ScriptableRenderContext m_context;
        private ScriptableCullingParameters m_cullingParams;
        private CullingResults m_CullingResults;

        private CamerInfoComponent m_camInfo;
        //Ray tracing stuff
        private Material m_AAMaterial;



        //shaders to use
        private static Material errorMaterial;
        private static ShaderTagId standarSpriteShader = new ShaderTagId("Sprites/Default");
        private static ShaderTagId diffuseSprite = new ShaderTagId("Sprites/Diffuse");

        private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

        //custom shaders

        static ShaderTagId[] CustomShaderTagIds =
        {
        new ShaderTagId("Custom RP/unlit")
        };

        static ShaderTagId[] legacyShaderTagIds =
        {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
        };
        public void Render(ScriptableRenderContext context, Camera camera, bool useDynmaicBatching, bool useGPUInstancing, CamerInfoComponent info)
        {

            //setup camera && context
            m_camInfo = info;
            m_context = context;
            m_cam = camera;
            m_cam.cullingMask = -1;
            PrepareForSceneWindow();
            if (!Cull()) return;
            m_CullingResults = context.Cull(ref m_cullingParams);


            //create command buffer && set name
            m_buffer = new CommandBuffer();
            m_buffer.name = m_cam.name + " Buffer";
            //Setup buffer

            ClearBuffer();

            BeginBuffer();
            //Do "normal" rendering if cam should not use ray tracing, cam is scene view cam or game view is not ingame
            if (!m_camInfo.UseRayTracing || m_cam.cameraType == CameraType.SceneView || !Application.isPlaying)
            {
             
                //Draw stuff
                DrawVisibleGeometry(useDynmaicBatching, useGPUInstancing);
                //only exectues if in editor
                DrawUnsupportedShaders();
                //Draw Gizmos, only executes if in editor
                DrawGizmos();

            }
            //else run comput shader
            else
            {
                ExecuteComputeShader(info.RayCaster);
            }
            //Submit buffer
            Submit();
        }
        private void InitPostProcessing()
        {
            if (!m_camInfo.UseAA) return;
            //Create new material if null
            if (m_AAMaterial == null)
                m_AAMaterial = new Material(Shader.Find("Hidden/AntiAliasing"));
            m_AAMaterial.SetFloat("_Sample", m_camInfo.SampleCount);

        }
        private void ExecuteComputeShader(RayCastMaster master)
        {
            InitPostProcessing();

            int width = m_cam.scaledPixelWidth;
            int height = m_cam.scaledPixelHeight;
            m_buffer.Blit(master.Render(), BuiltinRenderTextureType.RenderTexture);

            if (m_camInfo.UseAA)
            {
                m_buffer.Blit(master.Render(), BuiltinRenderTextureType.RenderTexture, m_AAMaterial);
                m_camInfo.SampleCount++;
            }
            //else
            //{
            //    m_buffer.Blit(master.Render(), BuiltinRenderTextureType.RenderTexture);
            //}
        }
        private void ClearBuffer()
        {
            m_context.SetupCameraProperties(m_cam);

            CameraClearFlags flags = m_cam.clearFlags;
            //Clear buffer based on camera flag
            m_buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? m_cam.backgroundColor.linear : Color.clear);
        }
        private void BeginBuffer()
        {
            //begin && execute buffer
            m_buffer.BeginSample(m_buffer.name);
            ExecuteBuffer();
        }
        private void ExecuteBuffer()
        {
            m_context.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
        }
        private void Submit()
        {
            m_buffer.EndSample(m_buffer.name);
            ExecuteBuffer();
            m_context.Submit();
        }
        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            //get sorting settings from camera
            SortingSettings sortingSettings = new SortingSettings(m_cam)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            //configer draw settings with sorting settings and allowed shader
            DrawingSettings drawingsettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            //      DrawingSettings drawingsettings = new DrawingSettings(diffuseSprite, sortingSettings);
            //specify that all render queues are allowed
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);


            m_context.DrawSkybox(m_cam);

            m_context.DrawRenderers(m_CullingResults, ref drawingsettings, ref filteringSettings);

            ////Draw custom shaders
            //for (int i = 1; i < CustomShaderTagIds.Length + 1; i++)
            //{
            ////    Debug.Log("custom shader pass" + i);
            //    drawingsettings.SetShaderPassName(i, CustomShaderTagIds[i - 1]);
            //}
            //   m_context.DrawRenderers(m_CullingResults, ref drawingsettings, ref filteringSettings);

            ////m_context.DrawRenderers(m_CullingResults, ref drawingsettings, ref filteringSettings);


            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingsettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            m_context.DrawRenderers(m_CullingResults, ref drawingsettings, ref filteringSettings);



        }
        private void DrawUnsupportedShaders()
        {
#if UNITY_EDITOR

            if (errorMaterial == null) errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            DrawingSettings drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(m_cam)) { overrideMaterial = errorMaterial };
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            for (int i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }
            m_context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
#endif
        }
        //culls based on layer index
        private bool Cull(int LayerCount)
        {
            m_cam.cullingMask = (1 << LayerMask.NameToLayer("2D" + LayerCount.ToString()));
            if (m_cam.TryGetCullingParameters(out m_cullingParams)) return true;
            return false;

        }
        private bool Cull()
        {
            if (m_cam.TryGetCullingParameters(out m_cullingParams)) return true;

            return false;
        }
        private void DrawGizmos()
        {
#if UNITY_EDITOR
            if (Handles.ShouldRenderGizmos())
            {
                m_context.DrawGizmos(m_cam, GizmoSubset.PreImageEffects);
                m_context.DrawGizmos(m_cam, GizmoSubset.PostImageEffects);
            }
#endif
        }

        private void PrepareForSceneWindow()
        {
#if UNITY_EDITOR
            if (m_cam.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(m_cam);
            }
#endif
        }
    }
}