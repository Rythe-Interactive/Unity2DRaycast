/// <summary>
/// static class that creates a quadtree based on the provided texture data
/// </summary>
public static class TreeFactory
{
    public static QuadTree CreateNewTree(TextureData data)
    {
        //create tree
        QuadTree newTree = new QuadTree(data.TextureAABBBounds, true);
        //insert all texture outlineColliders
        foreach (AABB currentAABB in data.OutlineAABBBounds)
            newTree.Insert(currentAABB);
        //draw tree
        //   newTree.DebugDraw();
        return newTree;
    }
}
