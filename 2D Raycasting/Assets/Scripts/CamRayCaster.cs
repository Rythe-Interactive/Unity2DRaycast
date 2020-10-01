using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
[RequireComponent(typeof(Camera))]
public class CamRayCaster : MonoBehaviour
{

    public int2 ViewPort = new int2(600, 400);

    private Camera m_cam;
    private int m_originalWidth;
    private int m_originalHeight;

    private Ray[] m_Rays;
    private float3 m_direction = new float3(0, 0, 1);

    private RayCastTarget[] Targets;
    private void Start()

    {
        m_cam = GetComponent<Camera>();
        m_originalHeight = m_cam.scaledPixelHeight;
        m_originalWidth = m_cam.scaledPixelWidth;

        InitRayCastTargets();

        GenerateRays();
        CastRays();
    }
    private void CastRays()
    {
        int count = 0;
        //foreach (RayCastTarget target in Targets)
        //{
        //    Debug.Log("target" + target.CircleCollider.Center + target.CircleCollider.Radius);
        //}

        foreach (Ray ray in m_Rays)
        {
            foreach (RayCastTarget target in Targets)
            {
                if (PointCircleIntersection.Intersection(target.CircleCollider, ray.Origin))
                {
                    count++;
                }
            }
        }
        Debug.Log("count " + count);
    }
    private void InitRayCastTargets()
    {
        GameObject[] tempTargets = GameObject.FindGameObjectsWithTag("RayCastTarget");
        Targets = new RayCastTarget[tempTargets.Length];
        int index = 0;
        foreach (GameObject target in tempTargets)
        {
            RayCastTarget newTarget = target.AddComponent<RayCastTarget>();
            Targets[index] = newTarget;
            index++;
        }
    }
    private void GenerateRays()
    {
        m_Rays = new Ray[m_originalHeight * m_originalWidth];
        for (int x = 0; x < m_originalWidth; x++)
        {
            for (int y = 0; y < m_originalHeight; y++)
            {
                float2 pos = new float2(x / (float)ViewPort.x * 30, y / (float)ViewPort.y * 20);
                //    Debug.Log("position " + pos);
                CamRay newRay = new CamRay(pos, m_direction);
            }
        }
    }
    private void Update()
    {
        CastRays();
    }
}

