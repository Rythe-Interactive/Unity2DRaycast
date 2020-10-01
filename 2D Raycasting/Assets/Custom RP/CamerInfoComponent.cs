using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerInfoComponent : MonoBehaviour
{
    public int SampleCount = 0;
    public bool UseAA = false;
    public bool UseRayTracing = false;
    public RayCastMaster RayCaster = null;
    public void Init(bool useAntiAliasing, bool useRT, RayCastMaster raycaster)
    {
        RayCaster = raycaster;
        UseAA = useAntiAliasing;
        UseRayTracing = useRT;
    }

    public void Update()
    {
        if (UseAA)
        {
            //reset sample if the camera has moved
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                SampleCount = 0;
            }
        }
    }
}
