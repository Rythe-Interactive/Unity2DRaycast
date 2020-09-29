using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public class RayCaster : MonoBehaviour
{
    public enum RayCastMode
    {
        LINE_X_RAY,
        AABB_X_RAY,
        AABB_QUADTREE_RAY,
        NONE
    }
    public bool RotateSource = true;
    public float rotationSpeed = 0.001f;
    public bool reflect = true;
    private int bounces = 2;
    public RayCastMode RayMode = RayCastMode.AABB_QUADTREE_RAY;
    public bool isStatic = false;

    public LineSegmentData Data = null;
    [SerializeField] private Color m_LineColor = Color.cyan;
    [SerializeField] private Color m_RayColor = Color.red;
    [SerializeField] private Color m_RayHitColor = Color.green;
    [SerializeField] private float m_rayDensity = 8.0f;

    private List<Ray> m_Rays;

    private int m_messageID;
    private int m_CollisionTypeID;
    int index = 0;
    private TraverseTree traverser;
    private float angleOffset = 0;
    void Start()
    {

    }
    private void GenerateRays()
    {
        Vector2 origin = new Vector2(this.transform.position.x, this.transform.position.y);

        m_Rays = new List<Ray>();
        for (int i = 0; i < m_rayDensity; i++)
        {
            Vector2 Dir = new Vector2(0, 1).normalized;
            float angle = 360 / m_rayDensity * i + angleOffset;

            Dir = Dir.RotateVecByAngle(angle);
            Ray newRay = new Ray(origin, Dir);

            m_Rays.Add(newRay);
        }
    }
    public void Update()
    {
        if (RotateSource)
        {
            angleOffset += rotationSpeed * Time.deltaTime;
            if (angleOffset > 360) angleOffset = 0;
        }
        if (Input.GetKeyDown(KeyCode.Q)) m_rayDensity *= 2.0f;
        if (Input.GetKeyDown(KeyCode.E)) m_rayDensity *= 0.5f;
        GenerateRays();


        if (Data == null) return;
        float startTime = Time.realtimeSinceStartup;

        //if (RayMode == RayCastMode.LINE_X_RAY)
        //{
        //    foreach (Ray currentRay in m_Rays)
        //    {
        //        CastRayLines(currentRay);
        //    }
        //}
        //else if (RayMode == RayCastMode.AABB_X_RAY)
        //{
        //    //Debug.Log("casting aabbs!");
        //    foreach (Ray currentRay in m_Rays)
        //    {
        //        bounces = RayConfig.RayBounces;
        //        CastRayAABB(currentRay);
        //    }
        //}
        //else if (RayMode == RayCastMode.AABB_QUADTREE_RAY)
        //{

        //            Debug.Log(Data.Trees.Count);
        //iterate trees
        foreach (QuadTree currenTree in Data.Trees)
        {
            //iterate rays
            for (int i = 0; i < m_Rays.Count; i++)
            {
                //if ray intersects with circle bounds, try traversing the tree
                if (RayCircleIntersection.Intersection(currenTree.BoundsCircle, m_Rays[i]))
                {
                    traverser = new TraverseTree(currenTree, m_Rays[i]);
                    Step();
                }
            }
        }
        //   }

    }

    private void Step()
    {
        float2 poi;
        bool hit;
        if (!traverser.Step(out poi, out hit))
        {
            Step();
        }
        else
        {
            if (hit)
            {
                Debug.DrawLine(traverser.ray.Origin.ConvertFloat2ToVec2(), (poi.ConvertFloat2ToVec2()), m_RayHitColor);
            }
            else Debug.DrawRay(traverser.ray.Origin.ConvertFloat2ToVec2(), math.normalize(traverser.ray.Direction).ConvertFloat2ToVec2() * RayConfig.RayDistance, m_RayColor);
        }

    }


    private bool FirstAABBCollision(AABB[] aabbs, Ray currentRay, out float2 firstPOI)
    {
        //setup some vars
        bool hit = false;
        float distance = float.MaxValue;
        firstPOI = new float2(0, 0);
        float2 rayHit;
        //iterate aabbs
        foreach (AABB currentAABB in aabbs)
        {
            if (RayAABBIntersection.Intersection(currentRay.Origin, currentRay.Direction, currentAABB, out rayHit))
            {
                float otherDist = math.length((rayHit - currentRay.Origin));

                if (otherDist < RayConfig.RayDistance)
                {
                    hit = true;
                    if (distance > otherDist)
                    {
                        firstPOI = rayHit;
                        distance = otherDist;
                    }
                }
            }
        }
        return hit;
    }
    private bool FirstAABBCollision(AABB[] aabbs, Ray currentRay, out float2 firstPOI, out float2 Normal)
    {
        //setup some vars
        bool hit = false;
        float distance = float.MaxValue;
        firstPOI = new float2(0, 0);
        Normal = new float2(0, 0);
        AABB collisionAABB = new AABB();
        float2 rayHit;
        //iterate aabbs
        foreach (AABB currentAABB in aabbs)
        {
            if (RayAABBIntersection.Intersection(currentRay.Origin, currentRay.Direction, currentAABB, out rayHit))
            {
                float otherDist = math.length((rayHit - currentRay.Origin));

                if (otherDist < RayConfig.RayDistance)
                {
                    hit = true;
                    if (distance > otherDist)
                    {
                        collisionAABB = currentAABB;
                        firstPOI = rayHit;
                        distance = otherDist;
                    }
                }
            }
        }
        //return early, dont calc normals if not collision was found
        if (!hit) return false;
        Normal = AABBExtension.findNormal(collisionAABB, firstPOI);
        return true;
    }

    private void CastRayAABB(Ray currentRay)
    {
        //check first if
        bool hit = false;
        float2 RayHit = float2.zero;
        float distance = float.MaxValue;
        float2 outVec = float2.zero;
        float2 normal = float2.zero;
        AABB collisionAABB = new AABB();
        foreach (TextureData currentTexture in Data.textures)
        {
            //skip if not colliding with texture bounds
            if (!RayAABBIntersection.Intersection(currentRay.Origin, currentRay.Direction, currentTexture.TextureAABBBounds))
            {
                Debug.DrawRay(currentRay.Origin.ConvertFloat2ToVec2(), math.normalize(currentRay.Direction).ConvertFloat2ToVec2() * RayConfig.RayDistance, m_RayColor);
                continue;
            }
            if (FirstAABBCollision(currentTexture.OutlineAABBBounds, currentRay, out RayHit, out normal))
            {
                Debug.DrawLine(currentRay.Origin.ConvertFloat2ToVec2(), (RayHit.ConvertFloat2ToVec2()), m_RayHitColor);
                if (bounces > 0 && reflect)
                {
                    Ray newRay = currentRay.ReflectRay(RayHit, normal);

                    --bounces;
                    CastRayAABB(newRay);
                }
            }
            else
            {
                Debug.DrawRay(currentRay.Origin.ConvertFloat2ToVec2(), math.normalize(currentRay.Direction).ConvertFloat2ToVec2() * RayConfig.RayDistance, m_RayColor);
            }
        }
    }
    private void CastRayLines(Ray currentRay)
    {
        bool hit = false;
        float2 RayHit = float2.zero;
        float hitDist = float.MaxValue;
        float2 POI = float2.zero;
        foreach (List<LineSegment> currentTexture in Data.TextureEdgesList)
        {
            foreach (LineSegment currnetline in currentTexture)
            {
                if (RayLineIntersection.Intersection
                    (currnetline.startPos, currnetline.endPos, currentRay.Origin, currentRay.Origin + currentRay.Direction * RayConfig.RayDistance, out POI))
                {
                    hit = true;
                    float dist = math.length(POI - currentRay.Origin);
                    if (dist < hitDist)
                    {
                        hitDist = dist;
                        RayHit = POI;
                    }
                }
            }
        }
        if (hit)
        {
            // RayDrawer.DrawLine(currentRay.Origin, RayHit, m_RayHitColor);
            //Debug.DrawRay(currentRay.Origin.ConvertFloat2ToVec2(), RayHit.ConvertFloat2ToVec2() - currentRay.Origin.ConvertFloat2ToVec2(), m_RayHitColor);
        }
        else
        {
            // RayDrawer.DrawLine(currentRay.Origin, currentRay.Direction * RayConfig.RayDistance + currentRay.Origin, m_RayHitColor);

            //Debug.DrawRay(currentRay.Origin.ConvertFloat2ToVec2(), currentRay.Direction.ConvertFloat2ToVec2() * RayConfig.RayDistance, m_RayColor);
        }

    }
    private void DrawLines()
    {
        if (Data == null) return;
        Debug.Log("drawing lines");
        foreach (List<LineSegment> currentTexture in Data.TextureEdgesList)
        {
            foreach (LineSegment currnetline in currentTexture)
            {
                Debug.DrawLine(
               new Vector3(currnetline.startPos.x, currnetline.startPos.y, 0),
               new Vector3(currnetline.endPos.x, currnetline.endPos.y, 0),
               m_LineColor, 100.0f
               );
            }
        }
    }
}



