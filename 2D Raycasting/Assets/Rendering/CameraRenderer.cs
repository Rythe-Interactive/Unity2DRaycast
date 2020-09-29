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

        //shaders to use
        private static Material errorMaterial;
        private static ShaderTagId standarSpriteShader = new ShaderTagId("Sprites/Default");
        private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

        static ShaderTagId[] legacyShaderTagIds =
        {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
        };
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            //setup camera && context
            this.m_context = context;
            this.m_cam = camera;

            //grab culling parameters & return if no were found
            if (!Cull()) return;

            //get culling results based on culling parameters
            m_CullingResults = context.Cull(ref m_cullingParams);


            //create command buffer && set name
            m_buffer = new CommandBuffer();
            m_buffer.name = m_cam.name + " Buffer";

            //Setup buffer
            Setup();
            //Draw stuff
            DrawVisibleGeometry();
            //only exectues if in editor
            DrawUnsupportedShaders();
            //Draw Gizmos, only executes if in editor
            DrawGizmos();

            //Submit buffer
            Submit();
        }

        private void Setup()
        {
            m_context.SetupCameraProperties(m_cam);

            CameraClearFlags flag = m_cam.clearFlags;
            //Clear buffer based on camera flag
            m_buffer.ClearRenderTarget((flag & CameraClearFlags.Depth) != 0, (flag & CameraClearFlags.Color) != 0, m_cam.backgroundColor);

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

        private void DrawVisibleGeometry()
        {
            //get sorting settings from camera
            SortingSettings sortingSettings = new SortingSettings(m_cam)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            //configer draw settings with sorting settings and allowed shader
            DrawingSettings drawingsettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
            //specify that all render queues are allowed
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);


            m_context.DrawSkybox(m_cam);

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
    }
}