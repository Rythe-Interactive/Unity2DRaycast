using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenerateSphere
{
    public static Sphere Generate(GameObject targetObj)
    {
        Sphere newSphere = new Sphere();
        newSphere.position = targetObj.transform.position;
        //I assume objects are scaled uniformly for simplicity sake
        newSphere.radius = targetObj.transform.localScale.x * 0.5f;

        RayTracingMaterial mat = targetObj.GetComponent<RayTracingMaterial>();
        if (mat) return mat.sphere;
        //else generate sphere

        //random specualrity &&albedo
        Color color = Random.ColorHSV();
        bool metal = Random.value < 0.5f;
        if (metal)
        {
            newSphere.specular = new Vector3(color.r, color.g, color.b);
        }
        else
        {
            newSphere.specular = Vector3.one * 0.04f;
        }
        newSphere.albedo = new Vector3(color.r, color.g, color.b);
        Color emission = Random.ColorHSV();

        newSphere.emission = new Vector3(emission.r, emission.g, emission.b);
        newSphere.emission = new Vector3(1, 0, 0);
        newSphere.smoothness = UnityEngine.Random.value;
        return newSphere;
    }
}
