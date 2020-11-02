using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.Rendering
{
    public abstract class ComputeDispatcher
    {
        protected int m_threadGroupsX;
        protected int m_threadGroupsY;

        protected RenderTexture m_RT;
        protected ComputeShader m_CS;
        public ComputeDispatcher() { }
        public RenderTexture Render()
        {
            //return if compute shader is null
            if (m_CS == null) return null;
            //setup render texture
            InitTexture();
            //update data
            SetData();
            //set the result render texture
            m_CS.SetTexture(0, "Result", m_RT);

            //set thread groups size
            SetThreadGroupSize();
            //dispatch compute shader
            m_CS.Dispatch(0, m_threadGroupsX, m_threadGroupsY, 1);
            //return the render texture
            return m_RT;
        }

        protected abstract void SetData();
        protected abstract void SetThreadGroupSize();
        protected void InitTexture()
        {
            //if render texture is null or screen size has been updated create new Render texture
            if (m_RT == null || m_RT.width != Screen.width || m_RT.height != Screen.height)
            {
                if (m_RT != null)
                    m_RT.Release();

                m_RT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

                m_RT.enableRandomWrite = true;
                m_RT.Create();
            }
        }
        protected void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride) where T : struct
        {
            //check if buffer already exists
            if (buffer != null)
            {
                //reset buffer
                if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
                {
                    buffer.Release();
                    buffer = null;
                }
            }
            //set data if input data is not empty
            if (data.Count != 0)
            {
                //if buffer is null create new buffer
                if (buffer == null)
                {
                    buffer = new ComputeBuffer(data.Count, stride);
                }
                buffer.SetData(data);
            }
        }

        protected void SetComputeBuffer(string name, ComputeBuffer buffer)
        {
            if (buffer != null)
            {
                m_CS.SetBuffer(0, name, buffer);
            }
        }
        protected ComputeShader FindeShader(string name)
        {
            ComputeShader[] foundObjects = Resources.LoadAll<ComputeShader>("");
            for (int i = 0; i < foundObjects.Length; i++)
            {
                if (foundObjects[i].name == name)
                {
                    return foundObjects[i];
                }
            }
            return null;
        }
    }
}