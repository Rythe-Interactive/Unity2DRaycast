namespace UnityEngine.Rendering
{
    [CreateAssetMenu(menuName = "Rendering/Custom Pipeline")]
    public class CustomPipelineAsset : RenderPipelineAsset
    {

        protected override RenderPipeline CreatePipeline()
        {
            return new CostumRenderPipeline();
        }
    }
    public class CostumRenderPipeline : RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            CameraRenderer renderer = new CameraRenderer();
            //iterate cameras && call actual render pass for the camera
            foreach (Camera cam in cameras)
            {
                renderer.Render(context, cam);
            }
        }
    }
}
