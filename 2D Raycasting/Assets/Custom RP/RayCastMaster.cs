using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastMaster
{
    private ComputeShader m_RayTracingShader;

    private Texture m_SkyBox;
    private RenderTexture m_target;
    private Camera m_cam;
    private Color m_BackgroundColor;

    private float m_seed = 0;
    //  private bool m_useAA = false;

    private uint m_currentSample = 0;
    private LightContainerComponent m_dirLight;

    public RenderTexture ConvergedRT;
    // private Material m_AAMaterial;
    private Sphere[] m_Spheres;
    ComputeBuffer m_SphereBuffer;
    public void Init(ComputeShader newCS, Camera cam, Texture skybox, Color c, float seed)
    {
        m_seed = seed;
        GetLighting();

        // m_useAA = useAA;
        m_BackgroundColor = c;
        m_SkyBox = skybox;
        m_RayTracingShader = newCS;
        m_cam = cam;

        InitRayCastTargets();
    }
    private void InitRayCastTargets()
    {
        if (!Application.isPlaying) return;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("RayCastTarget");
        m_Spheres = new Sphere[targets.Length];
        int index = 0;
        foreach (GameObject target in targets)
        {
            Sphere s = GenerateSphere.Generate(target);
            m_Spheres[index] = s;
            index++;
        }
        m_SphereBuffer = new ComputeBuffer(index, 56);

        m_SphereBuffer.SetData(m_Spheres);
    }
    private void SetShaderParams()
    {
        m_RayTracingShader.SetMatrix("_CameraToWorld", m_cam.cameraToWorldMatrix);
        m_RayTracingShader.SetMatrix("_CameraInverseProjection", m_cam.projectionMatrix.inverse);
        m_RayTracingShader.SetTexture(0, "_SkyBoxTexture", m_SkyBox);
        m_RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
      //  m_RayTracingShader.SetVector("_PixelOffset", new Vector2(0.5f, 0.5f));

        m_RayTracingShader.SetVector("_DirLight", m_dirLight.Light);
        m_RayTracingShader.SetVector("_SkyColor", new Vector3(m_BackgroundColor.r, m_BackgroundColor.g, m_BackgroundColor.b));
        m_RayTracingShader.SetBuffer(0, "_Spheres", m_SphereBuffer);
        //    m_RayTracingShader.SetFloat("_Seed", m_seed);
           m_RayTracingShader.SetFloat("_Seed", Random.value);
      //  m_RayTracingShader.SetFloat("_Seed", 0);
    }

    private void GetLighting()
    {
        //find object tagged as light
        GameObject obj = GameObject.FindGameObjectWithTag("DirectionalLight");
        //return if none was found
        if (obj == null) return;
        //grab info container
        m_dirLight = obj.GetComponent<LightContainerComponent>();
        //add contaienr if container was null
        if (m_dirLight == null)
        {
            m_dirLight = obj.AddComponent<LightContainerComponent>();
        }
        //init info container
        m_dirLight.Init();
    }


    public RenderTexture Render()
    {
      //  Debug.Log("Render!");
        SetShaderParams();
        InitTexture();

        m_RayTracingShader.SetTexture(0, "Result", m_target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        m_RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        return m_target;
    }
    private void InitTexture()
    {
        //create Render texture if its null or does not match screen dimensions
        if (m_target == null || m_target.width != Screen.width || m_target.height != Screen.height)
        {
            if (m_target != null)
                m_target.Release();

            m_target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            m_target.enableRandomWrite = true;
            m_target.Create();
        }


        if (ConvergedRT == null || ConvergedRT.width != Screen.width || ConvergedRT.height != Screen.height)
        {

            Debug.Log("Creating Converged RT!");
            if (ConvergedRT != null)
                ConvergedRT.Release();

            ConvergedRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            ConvergedRT.enableRandomWrite = true;
            ConvergedRT.Create();
        }
    }
}
