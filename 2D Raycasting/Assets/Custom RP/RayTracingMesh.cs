using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RayTracingMesh : MonoBehaviour
{
   // private static bool _MeshNeedsRebuilding = false;

    public void OnEnable()
    {
        RayCastMaster.SubscribeMesh(this);
    }
    private void OnDisable()
    {
        RayCastMaster.Unsubscribe(this);
    }
    //public void Update()
    //{
    //    if (transform.hasChanged)
    //    {
    //        _MeshNeedsRebuilding = true;
    //        transform.hasChanged = false;
    //    }
    //}
}


struct MeshObject
{
    public Matrix4x4 localToWorldMat;
    public int indices_offset;
    public int indices_count;

}