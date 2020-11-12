using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class RayTracingSphere : MonoBehaviour
{
    public bool NeedsRebuilding = false;
    public void OnEnable()
    {
        Debug.Log("on Enable!");
        NeedsRebuilding = true;
        RayCastMaster.SubscribeSphere(this);
    }
    private void OnDisable()
    {
        RayCastMaster.UnSubscribeSphere(this);
    }
    private void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            NeedsRebuilding = true;
        }
    }
}
