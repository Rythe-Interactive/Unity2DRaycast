using Unity.Mathematics;
public class TreeNode
{
    // const char[] LEAF = new char[4] { NOCHILD, NOCHILD, NOCHILD, NOCHILD };


    public int Size;
    public int childrenBase;
    byte[] ChildrenOffset = null;
    public bool HasChildren(uint i)
    {
        return (ChildrenOffset[i] != 0);
    }
    float2 GetChildPos(uint i)
    {
        if (ChildrenOffset[i] == 0)
            return float2.zero;
        else
            return float2.zero;
    }

    public bool IsLeaf()
    {
        return (ChildrenOffset.Length == 0);
    }
    public bool HasData() { return false; }
    public bool IsNull() { return false; }
    public TreeNode()
    { }



}
