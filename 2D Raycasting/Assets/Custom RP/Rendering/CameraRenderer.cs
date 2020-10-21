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
        private Material m_Depthmat;


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
         //   DepthPass();


            //create command buffer && set name
            m_buffer = new CommandBuffer();
            m_buffer.name = m_cam.name + " Buffer";

            //Setup buffer
            ClearBuffer(m_buffer);

            BeginBuffer(m_buffer);

            DrawVisibleGeometry(useDynmaicBatching, useGPUInstancing);
            DrawUnsupportedShaders();

            //Do "normal" rendering if cam should not use ray tracing, cam is scene view cam or game view is not ingame
            if (!m_camInfo.UseRayTracing || m_cam.cameraType == CameraType.SceneView || !Application.isPlaying)
            {

                //Draw Gizmos, only executes if in editor
                DrawGizmos();
            }
            //else run comput shader
            else
            {
                ExecuteComputeShader(info.RayCaster);
            }
            //Submit buffer
            submitBuffer(m_buffer);

        }

        public void DepthPass()
        {
            Debug.Log("depth pass!");
            int kdepthBufferBits = 32;
            string profilerTag = "DepthPass";
            ShaderTagId shaderID = new ShaderTagId("DepthOnly");
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            CommandBuffer DepthBuffer = new CommandBuffer();
            DepthBuffer.name = m_cam.name + "Depth Buffer";
            //execute
            ClearBuffer(DepthBuffer);
            BeginBuffer(DepthBuffer);
            //m_context.ExecuteCommandBuffer(DepthBuffer);
            //DepthBuffer.Clear();

            SortingSettings sortingSettings = GetSortingSettings();
            DrawingSettings drawingSettings = GetDrawingSettings(sortingSettings, "CustomRP/CustumDepth");
            //   drawingSettings.perObjectData = PerObjectData.None;
            //    m_context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
            m_context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            m_context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);


            submitBuffer(DepthBuffer);

            //m_context.ExecuteCommandBuffer(DepthBuffer);
            //CommandBufferPool.Release(DepthBuffer);

        }
        private void InitPostProcessing()
        {
            if (!m_camInfo.UseAA) return;
            //Create new material if null
            if (m_AAMaterial == null)
                m_AAMaterial = new Material(Shader.Find("Hidden/AntiAliasing"));

            m_AAMaterial.SetFloat("TextureHeight", Camera.main.scaledPixelHeight);
            m_AAMaterial.SetFloat("_Sample", m_camInfo.SampleCount);

            if (m_Depthmat == null)
                m_Depthmat = new Material(Shader.Find("ImageEffects/DepthTest"));
        }
        private void ExecuteComputeShader(RayCastMaster master)
        {
            if (master == null)
            {
                Debug.LogError("Ray casting master is null!");
                return;
            }
            InitPostProcessing();

            int width = m_cam.scaledPixelWidth;
            int height = m_cam.scaledPixelHeight;
            //   RenderTexture depthBuffer = BuiltinRenderTextureType.Depth;
            //m_buffer.Blit(master.Render(), BuiltinRenderTextureType.RenderTexture);

            // Texture2D tex2d = m_cam.

            if (m_camInfo.UseAA)
            {
                // Debug.Log("executing");
                //    m_buffer.Blit(master.ConvergedRT, BuiltinRenderTextureType.RenderTexture, m_AAMaterial);
                //    m_AAMaterial.SetTexture("_MainTex", BuiltinRenderTextureType.Depth);
                //   m_AAMaterial.mainTexture = BuiltinRenderTextureType.Depth;

                //  var tempBuffer = BuiltinRenderTextureType.GBuffer2;
                //    m_buffer.Blit(BuiltinRenderTextureType.GBuffer2, master.ConvergedRT, m_AAMaterial);
                //    for (int i = 0; i < 1; i++)
                //    {
                m_buffer.Blit(master.Render(), master.ConvergedRT, m_AAMaterial);
                //      m_buffer.Blit(master.Render(), master.ConvergedRT, m_Depthmat);

                // }


                //blit to a converged render texture using the shader for integration
                //blit converged RT to screen RT
                if (m_Depthmat == null) Debug.Log("material is null!");
                else m_buffer.Blit(master.ConvergedRT, BuiltinRenderTextureType.RenderTexture);


                //RenderTexture r = RenderTexture.GetTemporary(master.ConvergedRT.width, master.ConvergedRT.height);
                //  Graphics.Blit(master.ConvergedRT, r);

                m_camInfo.SampleCount++;
            }
            else
            {
                m_buffer.Blit(master.Render(), BuiltinRenderTextureType.RenderTexture);
            }
        }
        private void ClearBuffer(CommandBuffer buffer)
        {
            m_context.SetupCameraProperties(m_cam);

            CameraClearFlags flags = m_cam.clearFlags;
            //Clear buffer based on camera flag
            buffer.ClearRenderTarget(
                flags == CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? m_cam.backgroundColor.linear : Color.clear);
        }
        private void BeginBuffer(CommandBuffer buffer)
        {
            //begin && execute buffer
            buffer.BeginSample(buffer.name);
            ExecuteBuffer(buffer);
        }
        private void ExecuteBuffer(CommandBuffer buffer)
        {
            m_context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
        private void submitBuffer(CommandBuffer buffer)
        {
            buffer.EndSample(buffer.name);
            ExecuteBuffer(buffer);
            m_context.Submit();
        }
        private SortingSettings GetSortingSettings()
        {
            SortingSettings sortingSettings = new SortingSettings(m_cam)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            return sortingSettings;
        }
        private DrawingSettings GetDrawingSettings(SortingSettings sortingSettings, string shaderTagName = "SRPDefaultUnlit")
        {
            DrawingSettings drawingsettings = new DrawingSettings(new ShaderTagId(shaderTagName), sortingSettings)
            {
                enableDynamicBatching = false,
                enableInstancing = false
            };
            return drawingsettings;
        }
        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            //get sorting settings from camera
            SortingSettings sortingSettings = GetSortingSettings();

            //configer draw settings with sorting settings and allowed shader
            DrawingSettings drawingsettings = GetDrawingSettings(sortingSettings);
            //DrawingSettings drawingsettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
            //{
            //    enableDynamicBatching = useDynamicBatching,
            //    enableInstancing = useGPUInstancing
            //};

            //specify that all render queues are allowed
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);


            //    m_context.DrawSkybox(m_cam);

            m_context.DrawRenderers(m_CullingResults, ref drawingsettings, ref filteringSettings);

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