using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.Rendering
{
    public class PostProcessingDispatcher : ComputeDispatcher
    {
        private RenderTexture m_RTXrt;
        private RenderTexture m_NormalRT;
        private RenderTexture m_DetphRT;
        private RenderTexture m_PosRT;
        private CommandBuffer m_frameHistoryBuffer;
        private List<RenderTexture> m_previosTextures = new List<RenderTexture>();
        //exlude .compute
        private const string shaderName = "RTPostProcessing";
        public PostProcessingDispatcher() : base()
        {
            Init();
        }
        private void Init()
        {
            m_CS = FindeShader(shaderName);
            if (m_CS == null)
                Debug.LogWarning("Could not find compute shader");
        }
        protected override void SetData()
        {
            //_frameHistory
            int index = 0;
            foreach (RenderTexture rt in m_previosTextures)
            {
                m_CS.SetTexture(0, "_frameHistory" + index.ToString(), rt);
                index++;
            }
            //m_CS.SetTexture(0, "_frameHistory", m_previosTextures[0]);
            //CreateComputeBuffer(m_frameHistoryBuffer,m_previosTextures)
            m_CS.SetTexture(0, "_MainTex", m_RTXrt);
            m_CS.SetTexture(0, "_DepthTex", m_DetphRT);
            //   m_CS.SetTexture(0, "_NormalTex", m_NormalRT);
            m_CS.SetTexture(0, "_PosTex", m_PosRT);
            //m_CS.SetTexture(0, "_Frame1", m_frame1);
            //m_CS.SetTexture(0, "_Frame2", m_frame2);

        }

        protected override void SetThreadGroupSize()
        {
            m_threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
            m_threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        }
        public void SetRenderTextures(RenderTexture rtxRT, RenderTexture depthRT, RenderTexture position, List<RenderTexture> newTexArray)
        {
            m_RTXrt = rtxRT;
            //  m_NormalRT = normalRT;
            m_DetphRT = depthRT;
            m_PosRT = position;
            m_previosTextures = newTexArray;
        }
    }

}
