using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightContainerComponent : MonoBehaviour
{
    public Vector4 Light;
    public void Init()
    {
        Vector3 forward = transform.forward;
        Light = new Vector4(forward.x, forward.y, forward.z, GetComponent<Light>().intensity);
    }
    void Update()
    {
        if (transform.hasChanged)
        {
            Init();
            transform.hasChanged = false;
        }
    }
}
