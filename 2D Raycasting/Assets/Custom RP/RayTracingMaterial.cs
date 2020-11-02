using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaterial : MonoBehaviour
{
    public Color Albedo;
    public Color Emission;
    public Color Specular;
    [Range(0, 1)]
    public float Smoothness;
    public float LightStrenght = 1.0f;
    public Sphere sphere;

    private void OnValidate()
    {
        UpdateStats();
    }
    private void UpdateStats()
    {
        sphere.radius = this.transform.lossyScale.x * 0.5f;
        sphere.position = this.transform.position;
        Vector3 c = new Vector3(Albedo.r, Albedo.g, Albedo.b);
        sphere.albedo = c;

        Vector3 E = new Vector3(Emission.r, Emission.g, Emission.b);
        sphere.emission = E * LightStrenght;

        Vector3 S = new Vector3(Specular.r, Specular.g, Specular.b);
        sphere.specular = S;
        sphere.smoothness = Smoothness;

    }
}
