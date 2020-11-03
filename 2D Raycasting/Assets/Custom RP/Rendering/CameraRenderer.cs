using UnityEditor;
using System.Collections.Generic;
namespace UnityEngine.Rendering
{
    public class CameraRenderer
    {

        private List<RenderTexture> m_previousTextures;

        private PostProcessingDispatcher m_RTPostProcessing;
        private RenderTexture m_albedo;
        private RenderTexture m_DepthRT;
        private RenderTexture m_NormalRT;
        private RenderTexture m_MeshId;
        private RenderTexture m_previousDepth;
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

        private bool m_firstDraw = false;
        //shaders to use
        private static Material errorMaterial;
        private static ShaderTagId standarSpriteShader = new ShaderTagId("Sprites/Default");
        private static ShaderTagId diffuseSprite = new ShaderTagId("Sprites/Diffuse");

        private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private Texture2D previousFrame;
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
        public CameraRenderer()
        {
            Init();
        }
        private void Init()
        {
            Debug.Log("init RTS");
            //    m_PositionRT.filterMode = FilterMode.Bilinear;
            //     m_PositionRT.wrapMode = TextureWrapMode.Clamp;

            m_NormalRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            m_NormalRT.filterMode = FilterMode.Bilinear;
            m_NormalRT.wrapMode = TextureWrapMode.Clamp;

            m_DepthRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.Depth);
            m_albedo = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            //    m_VelocityBuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            m_MeshId = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            m_previousDepth = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);

            m_RTPostProcessing = new PostProcessingDispatcher();
            m_previousTextures = new List<RenderTexture>();
            //create 10 empty textures
            for (int i = 0; i < 10; i++)
            {
                RenderTexture rt = new RenderTexture(0, 0, 0);
                m_previousTextures.Add(rt);
            }
            m_firstDraw = true;
            //  m_previousFrames = new Texture2DArray(Screen.width,Screen.height,);
            // m_frame1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            //   m_frame2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        }
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
            ClearBuffer(m_buffer);

            BeginBuffer(m_buffer);



            //Do "normal" rendering if cam should not use ray tracing, cam is scene view cam or game view is not ingame
            if (!m_camInfo.UseRayTracing || m_cam.cameraType == CameraType.SceneView || !Application.isPlaying)
            {
                DrawVisibleGeometry(useDynmaicBatching, useGPUInstancing);
                DrawUnsupportedShaders();
                //Draw Gizmos, only executes if in editor
                DrawGizmos();
            }
            //else run comput shader
            else
            {
                ExecuteCustomPass(m_DepthRT, "DepthOnly");
                ExecuteCustomPass(m_NormalRT, "NormalPass");
                //        ExecuteCustomPass(m_MeshId, "PositionPass");
                ExecuteCustomPass(m_albedo, "SRPDefaultUnlit");

                ExecuteComputeShader(info.RayCaster);
                ExecuteCustomPass(m_previousDepth, "DepthOnly");

            }
            //Submit buffer
            submitBuffer(m_buffer);

        }


        private void ExecuteCustomPass(RenderTexture targetRT, string passName)
        {
            //setup buffer
            CommandBuffer buffer = new CommandBuffer { name = passName };
            //set render target to the input Render texture, If render texture is null, the target will stay the camera
            if (targetRT != null)
            {
                CoreUtils.SetRenderTarget(buffer, targetRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.All);
            }
            //clear & begin buffer
            ClearBuffer(buffer);
            BeginBuffer(buffer);
            //get rendering settings
            SortingSettings sortingSettings = GetSortingSettings();
            DrawingSettings drawingSettings = GetDrawingSettings(sortingSettings, passName);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            //render
            m_context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);

            //end Render pass 
            submitBuffer(buffer);


        }
        private void InitPostProcessing()
        {
            if (!m_camInfo.UseAA) return;
            //Create new material if null
            if (m_AAMaterial == null)
                m_AAMaterial = new Material(Shader.Find("Hidden/AntiAliasing"));

            m_AAMaterial.SetFloat("TextureHeight", Camera.main.scaledPixelHeight);
            m_AAMaterial.SetFloat("_Sample", m_camInfo.SampleCount);

        }
        private void ExecuteComputeShader(RayCastMaster master)
        {
            if (master == null)
            {
                Debug.LogError("Ray casting master is null!");
                return;
            }
            InitPostProcessing();
            if (m_firstDraw)
            {
                m_previousDepth = m_DepthRT;
                m_firstDraw = false;
            }



            RenderTexture rt = master.Render();


            m_RTPostProcessing.SetRenderTextures(rt, m_DepthRT, m_previousDepth, m_previousTextures, m_albedo);

            rt = m_RTPostProcessing.Render();
            m_previousTextures.Insert(0, rt);
            m_buffer.Blit(rt, master.ConvergedRT, m_AAMaterial);
            //m_buffer.Blit(m_RTPostProcessing.Render(), master.ConvergedRT);

            //     m_buffer.Blit(rt, master.ConvergedRT);
            //m_buffer.Blit(rt, master.ConvergedRT, m_AAMaterial);

            m_buffer.Blit(master.ConvergedRT, BuiltinRenderTextureType.RenderTexture);

            //m_AAMaterial);
            //m_buffer.Blit(rt, BuiltinRenderTextureType.RenderTexture);





            //m_previousDepth.Create();

            //Trimm last list element 
            while (m_previousTextures.Count > 10)
                m_previousTextures.RemoveAt(m_previousTextures.Count - 1);


            m_camInfo.SampleCount++;

        }
        private void DisplayRenderTexture(RenderTexture rt)
        {
            if (rt == null) return;
            m_buffer.Blit(rt, BuiltinRenderTextureType.RenderTexture);
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