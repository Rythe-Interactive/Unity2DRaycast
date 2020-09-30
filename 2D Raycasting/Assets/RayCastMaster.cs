using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastMaster
{
    private ComputeShader m_RayTracingShader;

    private Texture m_SkyBox;
    private RenderTexture m_target;
    private Camera m_cam;
    public void Init(ComputeShader newCS, Camera cam,Texture skybox)
    {
        m_SkyBox = skybox;
        m_RayTracingShader = newCS;
        m_cam = cam;
    }
    private void SetParameters()
    {
        m_RayTracingShader.SetMatrix("_CameraToWorld", m_cam.cameraToWorldMatrix);
        m_RayTracingShader.SetMatrix("_CameraInverseProjection", m_cam.projectionMatrix.inverse);
        m_RayTracingShader.SetTexture(0, "_SkyBoxTexture", m_SkyBox);
    }

    
    public RenderTexture Render()
    {
        Debug.Log("Render!");
        SetParameters();
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
    }
}
