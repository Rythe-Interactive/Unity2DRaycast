using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class RayTracingSprite : MonoBehaviour
{
    [SerializeField]
    private Texture2D m_NormalMap;
    public enum TextureMode
    {
        TEX512,
        TEX256,
        TEX128,
        TEX64,
    }
    public TextureMode TexSize = TextureMode.TEX256;
    public bool Rebuild = false;
    private SpriteRenderer m_sr;
    private Texture2D m_tex;
    public int Dimensions = 0;
    public void OnEnable()
    {
        m_sr = GetComponent<SpriteRenderer>();
        m_tex = m_sr?.sprite.texture;
        float max = Mathf.Max(m_tex.width, m_tex.height);
        float pow = Mathf.Ceil(max / 64.0f);
        if (pow == 1)
        {
            TexSize = TextureMode.TEX64;
            Dimensions = 64;
        }
        if (pow == 2)
        {
            TexSize = TextureMode.TEX128;
            Dimensions = 128;
        }
        if (pow == 3 || pow == 4)
        {
            TexSize = TextureMode.TEX256;
            Dimensions = 256;
        }
        if (pow >= 5 && pow <= 8)
        {
            TexSize = TextureMode.TEX512;
            Dimensions = 512;
        }


        Rebuild = true;
        RayCastMaster.SubscribeTexture(this);
        m_sr = GetComponent<SpriteRenderer>();
    }
    public void OnDisable()
    {
        RayCastMaster.UnSubscribeTexture(this);
    }
    public void Update()
    {
        if (transform.hasChanged)
        {
            Rebuild = true;
            transform.hasChanged = false;
        }
    }
    public SpriteRT GenSprite()
    {
        SpriteRT sRT = new SpriteRT();
        if (m_sr == null) return sRT;
        sRT.posMin = m_sr.bounds.min;
        sRT.posMax = m_sr.bounds.max;
        sRT.depth = transform.position.z;
        sRT.TextureDimensions = Dimensions;
        sRT.Width = sRT.posMax.x - sRT.posMin.x;
        sRT.Height = sRT.posMax.y - sRT.posMin.y;
        return sRT;
    }
    public Texture2D GetTexture() => m_tex;
    public Texture2D GetNormal() => m_NormalMap;
}
public struct SpriteRT
{
    public Vector2 posMin;
    public Vector2 posMax;
    public float depth;
    public int TextureIndex;
    public int TextureDimensions;
    public float Width;
    public float Height;
}