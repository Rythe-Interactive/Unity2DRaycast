using Unity;
namespace UnityEngine.Rendering
{
    [CreateAssetMenu(menuName = "Rendering/Custom Pipeline")]
    public class CustomPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private float m_Seed = 0;
        // [SerializeField]
        private bool m_useSRPBatcher = false;
        // [SerializeField]
        private bool m_useDynamicBatching = false;
        //  [SerializeField]
        private bool m_GPUInstancing = false;
        [SerializeField]
        private bool m_useComputeShader = false;
        [SerializeField]
        private ComputeShader m_ComputeShader = null;
        [SerializeField]
        private Texture m_SkyBoxTexture = null;
        [SerializeField]
        private bool m_UseAntiAliasing = false;
        [SerializeField]
        private Color m_SkyBoxColor = Color.blue;
        protected override RenderPipeline CreatePipeline()
        {
            return new CostumRenderPipeline(m_useDynamicBatching, m_GPUInstancing, m_useSRPBatcher,
                m_ComputeShader, m_useComputeShader, m_SkyBoxTexture, m_UseAntiAliasing, m_SkyBoxColor, m_Seed);
        }
    }
    public class CostumRenderPipeline : RenderPipeline
    {


        private bool m_useDynamicBatching = false;
        private bool m_GPUInstancing = false;
        private bool m_useCS = false;
        private bool m_UseAA = false;
        private ComputeShader m_computShader;
        private RayCastMaster m_RayCastMaster;
        CameraRenderer m_renderer;
        public CostumRenderPipeline(bool DynamicBatching, bool Instancing, bool batcher, ComputeShader cs, bool useCS, Texture skyboxTexture, bool useAA, Color color, float seed)
        {
            //set up variables
            m_UseAA = useAA;
            m_useCS = useCS;
            m_computShader = cs;
            m_useDynamicBatching = DynamicBatching;
            m_GPUInstancing = Instancing;
            GraphicsSettings.useScriptableRenderPipelineBatching = batcher;
            Debug.Log("Creating pipeline asset!");
            //cleanup camera info if new RP asset gets created // RP asset gets changed
            foreach (Camera cam in Camera.allCameras)
            {
                CamerInfoComponent info = cam.GetComponent<CamerInfoComponent>();
                //Create info if null
                if (info == null)
                {
                    info = cam.gameObject.AddComponent<CamerInfoComponent>();
                }
                //Init ray casting
                m_RayCastMaster = new RayCastMaster();
                m_RayCastMaster.Init(m_computShader, Camera.main, skyboxTexture, color, seed, Camera.main.GetComponent<CamerInfoComponent>());
                info.Init(m_UseAA, m_useCS, m_RayCastMaster);
            }

            //Init new renderer
            m_renderer = new CameraRenderer();
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            //iterate cameras && call actual render pass for the camera
            foreach (Camera cam in cameras)
            {
                CameraRenderer camR = new CameraRenderer();
                //get info from camera
                CamerInfoComponent info = cam.GetComponent<CamerInfoComponent>();
                //Create info if null
                if (info == null)
                {
                    info = cam.gameObject.AddComponent<CamerInfoComponent>();
                    //info.Init(false, m_useCS, m_computShader, m_RayCastMaster);
                }

                camR.Render(context, cam, m_useDynamicBatching, m_GPUInstancing, info);
            }

            OnRenderFinished();
        }
        private void OnRenderFinished()
        {

        }
    }

}