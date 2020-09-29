using System.Collections.Generic;
using UnityEngine;

public class LineSegmentData : MonoBehaviour
{
    public List<List<LineSegment>> TextureEdgesList = new List<List<LineSegment>>();
    public LightDirections directions;
    public List<TextureData> textures = new List<TextureData>();
    public List<QuadTree> Trees = new List<QuadTree>();

    private void Start()
    {
        //print amount of aabbs 
     //   int id = FrameTimePrinter.Instance().CreateNewFramePrinter("AABBs :");
        int aabbAmount = 0;
        foreach (TextureData texture in textures)
        {
            Trees.Add(TreeFactory.CreateNewTree(texture));
            aabbAmount += texture.OutlineAABBBounds.Length;
        }
       // FrameTimePrinter.Instance().GetUpdate(id, aabbAmount);

        //print amount of aabbs 
      //  int otherID = FrameTimePrinter.Instance().CreateNewFramePrinter("Line Segments :");
        int lineAmounts = 0;
        foreach (List<LineSegment> lines in TextureEdgesList)
        {
            lineAmounts += lines.Count;
        }
      //  FrameTimePrinter.Instance().GetUpdate(otherID, lineAmounts);
    }
}
public class LightDirections
{

    Vector2[] vector2s;

    Vector2[] oldData;

    public void InsertVec2(Vector2 dir)
    {
        vector2s[vector2s.Length] = dir;
    }
    public Vector2 AverageOutDIr()
    {
        Vector2 averageDir = Vector2.zero;

        for (int i = 0; i < vector2s.Length; i++)
        {
            averageDir += vector2s[i];
        }

        averageDir = new Vector2(averageDir.x / vector2s.Length, averageDir.y / vector2s.Length);

        return averageDir;

    }
}