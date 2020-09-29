using System.Collections.Generic;
using Unity.Mathematics;
public class QuadTree
{
    const int QT_NODE_CAPACITY = 6;
    public CircleCollider BoundsCircle;
    public AABB Bounds;
    public List<AABB> AABBs = new List<AABB>();
    public float2 Size;
    public QuadTree m_NorthWest;        //2         | 2 | 3 |
    private QuadTree m_NorthEast;       //3         | 0 | 1 |
    private QuadTree m_SouthWest;       //0
    private QuadTree m_SouthEast;       //1
    private bool isFull() => AABBs.Count >= QT_NODE_CAPACITY;
    public bool HasBeenSubdivided() => m_NorthWest != null;

    public int Depth = 0;
    public bool IsLeaf() => m_NorthWest == null;
    public QuadTree() { }

    public QuadTree GetChild(int index)
    {
        switch (index)
        {
            case 0:
                return m_SouthWest;

            case 1:
                return m_SouthEast;

            case 2:
                return m_NorthWest;

            case 3:
                return m_NorthEast;

            default: return null;
        }
    }
    public QuadTree(AABB bounds, bool isRootTree = false, int index = 0, float xIndex = 0, float yIndex = 0)
    {

        AABBs = new List<AABB>();
        index++;
        Depth = index;
        Bounds = bounds;
        Size = bounds.Min + bounds.Max;
        if (isRootTree)
            GenerateCircleCollider();
    }
    public QuadTree[] GetTrees()
    {
        QuadTree[] trees = new QuadTree[4];
        trees[0] = m_NorthEast;
        trees[1] = m_NorthWest;
        trees[2] = m_SouthEast;
        trees[3] = m_SouthWest;
        return trees;
    }
    public bool Insert(AABB newAABB)
    {
        //AABB does not overlap with the quadtree, you should not add it 
        if (!AABBExtension.OverlapsAABB(Bounds, newAABB))
        {
            return false;
        }

        //check if there is space to insert left && check if tree has been subdivided yet
        if (!isFull() && !HasBeenSubdivided())
        {
            AABBs.Add(newAABB);
            return false;
        }

        //else this tree needs to be subdivided
        if (m_NorthWest == null)
            Subdivide();

        //insert in subdivisions, return true if you succesfully inserted
        bool succesufull = false;
        if (m_NorthWest.Insert(newAABB)) succesufull = true;
        if (m_NorthEast.Insert(newAABB)) succesufull = true;
        if (m_SouthEast.Insert(newAABB)) succesufull = true;
        if (m_SouthWest.Insert(newAABB)) succesufull = true;
        return succesufull;
    }
    private void Subdivide()
    {
        //calculate center && dimensions
        float xDim = Bounds.Max.x - Bounds.Min.x;
        xDim *= 0.5f;
        float yDim = Bounds.Max.y - Bounds.Min.y;
        yDim *= 0.5f;

        float2 centerPoint = Bounds.Min + new float2(xDim, yDim);
        //generate new quadtrees
        m_NorthWest = new QuadTree(new AABB(new float2(Bounds.Min.x, centerPoint.y), new float2(centerPoint.x, Bounds.Max.y)), false, Depth);

        m_NorthEast = new QuadTree(new AABB(centerPoint, Bounds.Max), false, Depth);

        m_SouthWest = new QuadTree(new AABB(Bounds.Min, centerPoint), false, Depth);

        m_SouthEast = new QuadTree(new AABB(new float2(centerPoint.x, Bounds.Min.y), new float2(Bounds.Max.x, centerPoint.y)), false, Depth);

        foreach (AABB currentAABB in AABBs)
        {
            Insert(currentAABB);
        }

        //overwrite aabbs with leaf aabbs
        AABBs = new List<AABB>();
        AABBs.Add(m_NorthEast.Bounds);
        AABBs.Add(m_NorthWest.Bounds);
        AABBs.Add(m_SouthEast.Bounds);
        AABBs.Add(m_SouthWest.Bounds);
    }
    private void GenerateCircleCollider()
    {
        float2 center = (Bounds.Min + Bounds.Max) * 0.5f;
        float r = math.distance(Bounds.Min, center);

        BoundsCircle = new CircleCollider(center, r);
    }

    public void DebugDraw()
    {
        AABBExtension.DebugDrawAABB(Bounds, RayConfig.QUAD_TREE_COLOR);
        foreach (AABB aabb in AABBs) AABBExtension.DebugDrawAABB(aabb);
        if (m_NorthEast == null) return;
        else
        {
            m_NorthWest.DebugDraw();
            m_NorthEast.DebugDraw();
            m_SouthWest.DebugDraw();
            m_SouthEast.DebugDraw();
        }
    }


    public override string ToString()
    {
        string r;
        r = "Bounds :" + Bounds.ToString();
        r += ", Depth: " + Depth.ToString();
        return r;
    }
}
