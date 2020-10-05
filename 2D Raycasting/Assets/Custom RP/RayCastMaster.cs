using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class RayCastMaster
{
    private ComputeShader m_RayTracingShader;
    private CamerInfoComponent m_camInfo;
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
    private ComputeBuffer m_SphereBuffer;
    private static bool m_meshObjectsNeedRebuild = true;
    private static List<RayTracingMesh> m_RayTracingObjects = new List<RayTracingMesh>();


    private static List<MeshObject> m_MeshObjects = new List<MeshObject>();
    private static List<Vector3> m_vertices = new List<Vector3>();
    private static List<int> m_indices = new List<int>();
    private ComputeBuffer m_meshBuffer;
    private ComputeBuffer m_vertexBuffer;
    private ComputeBuffer m_IndexBuffer;


  //  void Start() => RebuildMeshes();

    public void Init(ComputeShader newCS, Camera cam, Texture skybox, Color c, float seed, CamerInfoComponent newInfo)
    {
        Debug.Log("init Ray tracer!");
        m_camInfo = newInfo;
        m_seed = seed;
        GetLighting();

        // m_useAA = useAA;
        m_BackgroundColor = c;
        m_SkyBox = skybox;
        m_RayTracingShader = newCS;
        m_cam = cam;
        m_meshObjectsNeedRebuild = true;
        RebuildMeshes();
        InitRayCastTargets();
    }

    public static void SubscribeMesh(RayTracingMesh mesh)
    {
        m_RayTracingObjects.Add(mesh);
        m_meshObjectsNeedRebuild = true;
    }
    public static void Unsubscribe(RayTracingMesh mesh)
    {
        m_RayTracingObjects.Remove(mesh);
        m_meshObjectsNeedRebuild = true;
    }
    private void InitRayCastTargets()
    {
        Debug.Log("Init ray caster");
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
        //Debug.Log("setting params");
        m_RayTracingShader.SetMatrix("_CameraToWorld", m_cam.cameraToWorldMatrix);
        m_RayTracingShader.SetMatrix("_CameraInverseProjection", m_cam.projectionMatrix.inverse);
        m_RayTracingShader.SetTexture(0, "_SkyBoxTexture", m_SkyBox);
        m_RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        //  m_RayTracingShader.SetVector("_PixelOffset", new Vector2(0.5f, 0.5f));

        m_RayTracingShader.SetVector("_DirLight", m_dirLight.Light);
        m_RayTracingShader.SetVector("_SkyColor", new Vector3(m_BackgroundColor.r, m_BackgroundColor.g, m_BackgroundColor.b));
        m_RayTracingShader.SetBuffer(0, "_Spheres", m_SphereBuffer);
        //    m_RayTracingShader.SetFloat("_Seed", m_seed);
        //m_RayTracingShader.SetFloat("_Seed", Random.Range(-1000.0f, 1000.0f));
        m_RayTracingShader.SetFloat("_Seed", Random.value);

        //  m_RayTracingShader.SetFloat("_Seed", 0);
        RebuildMeshes();
        SetComputeBuffer("_MeshObjects", m_meshBuffer);
        SetComputeBuffer("_Vertices", m_vertexBuffer);
        SetComputeBuffer("_Indices", m_IndexBuffer);

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
            if (ConvergedRT != null)
                ConvergedRT.Release();

            ConvergedRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            ConvergedRT.enableRandomWrite = true;
            ConvergedRT.Create();
        }
    }


    private void RebuildMeshes()
    {
        if (!m_meshObjectsNeedRebuild) return;

        m_meshObjectsNeedRebuild = false;
        m_camInfo.SampleCount = 0;

        m_MeshObjects.Clear();
        m_vertices.Clear();
        m_indices.Clear();

        Debug.Log("building!");
        int indexer = 0;

        foreach (RayTracingMesh rtObj in m_RayTracingObjects)
        {
            Debug.Log(indexer++);
            Mesh currentMesh = rtObj.GetComponent<MeshFilter>().sharedMesh;

            int firstVertex = m_vertices.Count;
            m_vertices.AddRange(currentMesh.vertices);

            int firstIndex = m_indices.Count;
            var indices = currentMesh.GetIndices(0);
            m_indices.AddRange(indices.Select(index => index + firstVertex));

            m_MeshObjects.Add(new MeshObject()
            {
                localToWorldMat = rtObj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length
            });
        }
        CreateComputeBuffer(ref m_meshBuffer, m_MeshObjects, 72);
        CreateComputeBuffer(ref m_IndexBuffer, m_indices, 4);
        CreateComputeBuffer(ref m_vertexBuffer, m_vertices, 12);
    }


    private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride) where T : struct
    {
        if (buffer != null)
        {
            if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
        }
        if (data.Count != 0)
        {

            if (buffer == null)
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }
            buffer.SetData(data);
        }
    }

    private void SetComputeBuffer(string name, ComputeBuffer buffer)
    {

        if (buffer != null)
        {
            m_RayTracingShader.SetBuffer(0, name, buffer);
        }
    }
}
