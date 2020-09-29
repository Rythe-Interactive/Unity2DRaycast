using System.Collections.Generic;
using Unity.Mathematics;
/*
      This is the order of a quadrant
        -----------
        | q2 | q3 |
        | q0 | q1 |
        -----------
        o-------o-------o
        |       |       |
        |   2   |   3   |
        |       |       |
  my -- o-------o-------o
        |       |       |
        |   0   |   1   |
        |       |       |
  y0 -- o-------o-------o
        |       |
        x0      mx
*/

public class TraverseTree
{
    public float depthX = 0;
    public float depthY = 0;
    public QuadTree Tree;
    public Ray ray;
    private Ray modifiedRay;
    public Stack<TraversalInfo> InfoStack;
    public int Stepcount = 0;
    public byte a;
    private TraversalInfo m_info;

    public TraverseTree() { }
    public TraverseTree(QuadTree pTree, Ray pRay)
    {
        Tree = pTree;
        ray = pRay;
        initTraversal();
    }
    public void InitWithNewRay(Ray newRay)
    {
        ray = newRay;
        initTraversal();
    }

    private void initTraversal()
    {
        a = 0;
        modifiedRay = ray;
        InfoStack = new Stack<TraversalInfo>();
        float2 poi = float2.zero;

        //flip ray if x or y direction are negative 
        //store the flip as a byte in a 

        if (modifiedRay.Direction.x < 0.0f)
        {
            //Ray will never hit
            //  if (modifiedRay.Origin.x < Tree.Bounds.Max.x) return;
            modifiedRay.Origin.x = Tree.Size.x - modifiedRay.Origin.x;
            modifiedRay.Direction.x = -modifiedRay.Direction.x;
            a |= 1;
        }
        if (modifiedRay.Direction.y < 0.0f)
        {
            //   if (modifiedRay.Origin.y < Tree.Bounds.Max.y) return;

            modifiedRay.Origin.y = Tree.Size.y - modifiedRay.Origin.y;
            modifiedRay.Direction.y = -modifiedRay.Direction.y;
            a |= 2;
        }
        //Debug drawing rays

        //determine collision && tmin, tmax values for tree traversal
        double oodX = 1.0 / modifiedRay.Direction.x;
        double oodY = 1.0 / modifiedRay.Direction.y;

        double tx0 = (Tree.Bounds.Min.x - modifiedRay.Origin.x) * oodX;
        double tx1 = (Tree.Bounds.Max.x - modifiedRay.Origin.x) * oodX;
        //Debug.Log(tx0);
        //Debug.Log(tx1);
        double ty0 = (Tree.Bounds.Min.y - modifiedRay.Origin.y) * oodY;
        double ty1 = (Tree.Bounds.Max.y - modifiedRay.Origin.y) * oodY;
        //Debug.Log(ty0);
        //Debug.Log(ty1);

        //get min && max values
        double tmin = math.max(tx0, ty0);
        double tmax = math.min(tx1, ty1);
        //check if there was a collision
        if (tmin < tmax)
        {
            TraversalInfo newInfo = GenerateInfo(tx0, tx1, ty0, ty1, Tree);
            InfoStack.Push(newInfo);
            return;
        }
    }

    public bool Step(out float2 poi, out bool hit)
    {
        hit = false;
        poi = float2.zero;
        //check if the tree is not empty
        if (InfoStack.Count == 0)
        {
            //Return if tree is empty
            return true;
        }

        //POP 
        //if we are in a terminal tree / leaf node 
        //or we visited all children in that tree
        //pop current tree from stack

        //next child ==4 means all children visited; next child 0-3 are quadrants 0-3 
        if (InfoStack.Peek().nextChild == 4)
        {
            InfoStack.Pop();
            return false;
        }
        //check if tree is leaf node
        if (InfoStack.Peek().tree.IsLeaf())
        {
            TraversalInfo tempInfo = InfoStack.Pop();
            //try to find ray collision with colliders in leaf node
            foreach (AABB aabb in tempInfo.tree.AABBs)
            {
                if (RayAABBIntersection.Intersection(ray.Origin, ray.Direction, aabb, out poi))
                {
                    hit = true;
                    return true;
                }
            }
            return false;
        }

        //PUSH

        //get info
        TraversalInfo info = InfoStack.Peek();
        //rewrite variables for readability 
        double2 t0 = info.t0;
        double2 t1 = info.t1;
        double2 tm = info.tm;


        //check if quadrant has already been visited 
        //if visiting quadrant for the first time the next child has not been determined & set to -1
        if (InfoStack.Peek().nextChild == -1)
        {
            //find the firstQuadrant && store it
            InfoStack.Peek().nextChild = firstNode(t0.x, t0.y, tm.x, tm.y);
        }

        //ADVANCE
        //find next child in voxel to advance to
        //define variables
        QuadTree getTree;
        TraversalInfo newInfo;
        //switch case to determine what to do for each quadrant
        switch (InfoStack.Peek().nextChild)
        {
            case (0):
                //get next child
                InfoStack.Peek().nextChild = NewNode(tm.x, 1, tm.y, 2);
                //get current child                        
                getTree = InfoStack.Peek().tree.GetChild(0 ^ a);
                //create new info
                newInfo = GenerateInfo(t0.x, tm.x, t0.y, tm.y, getTree);
                //push new info to stack
                InfoStack.Push(newInfo);
                break;

            case (1):
                InfoStack.Peek().nextChild = NewNode(t1.x, 4, tm.y, 3);
                getTree = InfoStack.Peek().tree.GetChild(1 ^ a);
                newInfo = GenerateInfo(tm.x, t1.x, t0.y, tm.y, getTree);
                InfoStack.Push(newInfo);
                break;

            case (2):
                InfoStack.Peek().nextChild = NewNode(tm.x, 3, t1.y, 4);
                getTree = InfoStack.Peek().tree.GetChild(2 ^ a);
                newInfo = GenerateInfo(t0.x, tm.x, tm.y, t1.y, getTree);
                InfoStack.Push(newInfo);
                break;

            case (3):
                InfoStack.Peek().nextChild = NewNode(tm.x, 4, tm.y, 4);
                getTree = InfoStack.Peek().tree.GetChild(3 ^ a);
                newInfo = GenerateInfo(tm.x, t1.x, tm.y, t1.y, getTree);
                InfoStack.Push(newInfo);
                break;
        }
        return false;
    }
    //creates new info 
    private TraversalInfo GenerateInfo(double tx0, double tx1, double ty0, double ty1, QuadTree tree)
    {
        TraversalInfo info = new TraversalInfo();
        info.tree = tree;
        info.t0 = new double2(tx0, ty0);
        info.t1 = new double2(tx1, ty1);
        info.tm = 0.5 * (info.t0 + info.t1);
        info.nextChild = -1;
        return info;
    }
    //initialize tree traversal

    //picks the first child depending on t0 
    private int firstNode(double tx0, double ty0, double txm, double tym)
    {
        //x slab hits first
        if (ty0 > tx0)
        {
            if (txm < ty0)
            {
                return 1;
                //hits right half
            }
            else
            {
                return 0;
                //hits left half
            }
        }
        //y slab hit first
        else
        {
            if (tym < tx0)
            {
                return 2;
                //hits upper half
            }
            else
            {
                return 0;
                //hits lower half
            }
        }
    }
    //returns x or y value depending on which axis is collided with first
    private int NewNode(double tmx, int x, double tmy, int y)
    {
        //if ty is smaller than tx the ray collides with with y plane next 
        if (tmy < tmx)
        {
            return y;
        }
        //else tx is smaller, ray collides with x plane next
        else
        {
            return x;
        }
    }
}


public class TraversalInfo
{
    public QuadTree tree;
    public double2 poi;
    public double2 t0;
    public double2 t1;
    public double2 tm;
    public int nextChild;
}



