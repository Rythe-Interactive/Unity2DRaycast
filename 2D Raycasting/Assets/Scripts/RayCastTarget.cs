using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class RayCastTarget : MonoBehaviour
{
    public AABB BoxCollider;
    public CircleCollider CircleCollider;


    public Texture2D m_Texture;
    private void Awake()
    {
        m_Texture = GetComponent<SpriteRenderer>().sprite.texture;
        float2 pos = new Vector2(transform.position.x, transform.position.y);
        float2 dimension = new Vector2(m_Texture.width * transform.lossyScale.x / 100, m_Texture.height * transform.lossyScale.x / 100);
        BoxCollider = new AABB(pos, pos + dimension);

        float2 center = (pos + dimension) * 0.5f;
        CircleCollider = new CircleCollider(BoxCollider);
    }
}
