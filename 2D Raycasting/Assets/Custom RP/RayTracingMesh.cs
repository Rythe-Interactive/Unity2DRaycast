using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class RayTracingMesh : MonoBehaviour
{
    public bool NeedsRebuilding = false;
    public void OnEnable()
    {
        NeedsRebuilding = true;
        RayCastMaster.SubscribeMesh(this);
    }
    private void OnDisable()
    {
        RayCastMaster.UnsubscribeMesh(this);
    }
    private void Update()
    {
        if (transform.hasChanged)
        {
            Debug.Log("transform has changed!");
            transform.hasChanged = false;
            NeedsRebuilding = true;
        }
    }
}


struct MeshObject
{
    public Matrix4x4 localToWorldMat;
    public int indices_offset;
    public int indices_count;

}