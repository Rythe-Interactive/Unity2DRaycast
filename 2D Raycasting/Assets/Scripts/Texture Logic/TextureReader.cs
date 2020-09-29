using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public enum Direction
{
    NORTH = 0,
    EAST = 1,
    SOUTH = 2,
    WEST = 3
}
public enum MipmapLevel
{
    NORMAL,
    MIP_MAP_1,
    MIP_MAP_2,
    MIP_MAP_3,
}
public class TextureReader : MonoBehaviour
{

    [SerializeField] private bool m_GenerateBoxes = true;
    [SerializeField] private bool m_GenerateLines = true;

    [SerializeField] private bool m_drawBoxes = true;
    [Header("--- you probably should not go below 1---")]
    [Header("--- Mip Map Level for bound generation---")]
    public MipmapLevel m_CollisionDetail = MipmapLevel.NORMAL;

    [SerializeField] private Color debugColor = Color.red;
    private float m_Scale = 1.0f;

    private Sprite m_Texture;
    [SerializeField] Texture2D m_Normal;
    private List<LineSegment> m_BoundingLineSegments;
    private List<AABB> m_AABBs;

    private List<Cell> m_Cells;
    private Direction m_currentDirCheck;
    private int m_xDim = 0;
    private int m_yDim = 0;

    private Vector2 m_origin;
    [SerializeField] private LineSegmentData Data;
    private TextureData m_textureData;
    private Color[] m_NormalColor;
    private void Awake()
    {
        m_AABBs = new List<AABB>();
        m_Scale = transform.lossyScale.x / 100;
        Generate();
        if (m_GenerateBoxes)
        {
            GenerateAABBs();
            if (m_drawBoxes) DrawBoxes();
        }

        if (Data)
        {
            Data.TextureEdgesList.Add(m_BoundingLineSegments);

            //generate texture data
            m_textureData = new TextureData
                (m_origin,
                new Vector2(m_Texture.texture.width * transform.lossyScale.x / 100, m_Texture.texture.height * transform.lossyScale.x / 100),
                m_AABBs);

            //   AABBExtension.DebugDrawAABB(m_textureData.TextureAABBBounds);
            Data.textures.Add(m_textureData);
          //  int id = FrameTimePrinter.Instance().CreateNewFramePrinter("AABB Count: ");
         //   FrameTimePrinter.Instance().GetUpdate(id, m_AABBs.Count);
            //  int id FrameTimePrinter.Instance().CreateNewFramePrinter("AABB Count:");
        }


    }
    public void Generate()
    {

        m_Texture = GetComponent<SpriteRenderer>().sprite;
        Debug.Log("(" + (m_Texture.texture.width * -0.005f) + ", " + (m_Texture.texture.height * -0.005f) + ")");
        m_origin = new Vector2(m_Texture.texture.width * -0.005f, m_Texture.texture.height * -0.005f);
        m_origin *= transform.localScale.x;
        m_origin += new Vector2(transform.position.x, transform.position.y);

        Debug.Log("Reading texture!");
        if (m_Texture == null)
        {
            Debug.LogWarning("Please assign Texture!");
            return;
        }
        Debug.Log(m_Texture.texture.mipmapCount);
        m_BoundingLineSegments = new List<LineSegment>();
        Color[] textureData = GetTexture(m_Texture.texture);
        m_NormalColor = GetTexture(m_Normal);
        m_Cells = new List<Cell>();
        int xDim = m_Texture.texture.width;
        int yDim = m_Texture.texture.height;
        float ratio = xDim / yDim;

        if (m_CollisionDetail != MipmapLevel.NORMAL)
        {
            int divident = 1;
            if (m_CollisionDetail == MipmapLevel.MIP_MAP_1) divident = 2;
            else if (m_CollisionDetail == MipmapLevel.MIP_MAP_2) divident = 4;
            else if (m_CollisionDetail == MipmapLevel.MIP_MAP_3) divident = 8;

            xDim = xDim / divident;
            yDim = yDim / divident;
            m_Scale *= divident;
        }
        m_xDim = xDim;
        m_yDim = yDim;



        //generate cells
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                int index = x + xDim * y;
                bool exists = textureData[index].a != 0;
                float4 c = new float4(textureData[index].a, textureData[index].g, textureData[index].b, textureData[index].a);
                m_Cells.Add(new Cell(exists, index, xDim, yDim, x, y, new float2(m_NormalColor[index].r, m_NormalColor[index].g), c));
            }
        }
        //  Debug.Log("Generated cells :" + m_Cells.Count);
        if (!m_GenerateLines) return;
        //  Debug.Log("Generating Lines");
        foreach (Cell currentCell in m_Cells)
        {
            //skip cells that do not exist
            if (!currentCell.exists)
            {
                continue;
            }
            //      Debug.Log("Current Cell" + currentCell.ID);
            //init some bools
            bool edgeNeeded = false, canExtend = false;
            //check for all 4 directions
            ///
            ///-------------Western border-----------------
            ///
            //check if you are at the right border
            if (currentCell.X == m_xDim - 1) edgeNeeded = true;
            //check if there is a right to you, if not edge is needed
            else if (!m_Cells[currentCell.ID + 1].exists) edgeNeeded = true;
            //generate new edge if to the south bounds 
            if (currentCell.Y == 0)
            {
                if (edgeNeeded)
                    GenerateEdge(currentCell, Direction.WEST);
            }
            //else continue
            else
            {
                //check if the neighbour has a border you can extend
                canExtend = (neighbourHasBorder(m_Cells[currentCell.ID - m_xDim], Direction.WEST));
                if (edgeNeeded)
                {
                    if (canExtend) ExtendLine(currentCell, Direction.WEST);
                    else GenerateEdge(currentCell, Direction.WEST);
                }
            }

            ///
            ///-------------Easter border-----------------
            ///
            edgeNeeded = false;
            canExtend = false;
            //check if you are at the left border
            if (currentCell.X == 0) edgeNeeded = true;
            //check if there is a left to you, if not edge is needed
            else if (!m_Cells[currentCell.ID - 1].exists) edgeNeeded = true;
            //generate new edge if to the south bounds 
            if (currentCell.Y == 0)
            {
                if (edgeNeeded)
                    GenerateEdge(currentCell, Direction.EAST);
            }
            //else continue
            else
            {
                //check if the neighbour has a border you can extend
                canExtend = (neighbourHasBorder(m_Cells[currentCell.ID - m_xDim], Direction.EAST));
                if (edgeNeeded)
                {
                    if (canExtend) ExtendLine(currentCell, Direction.EAST);
                    else GenerateEdge(currentCell, Direction.EAST);
                }
            }

            ///
            ///-------------Southern border-----------------
            ///
            edgeNeeded = false;
            canExtend = false;
            //check if you are at the bottom border
            if (currentCell.Y == 0) edgeNeeded = true;
            //check if there is a cell below you, if not edge is needed
            else if (!m_Cells[currentCell.ID - m_xDim].exists) edgeNeeded = true;
            //generate new edge if to the left bounds 
            if (currentCell.X == 0)
            {
                if (edgeNeeded)
                    GenerateEdge(currentCell, Direction.SOUTH);
            }
            //else continue
            else
            {
                //check if the neighbour has a border you can extend
                canExtend = (neighbourHasBorder(m_Cells[currentCell.ID - 1], Direction.SOUTH));
                if (edgeNeeded)
                {
                    if (canExtend) ExtendLine(currentCell, Direction.SOUTH);
                    else GenerateEdge(currentCell, Direction.SOUTH);
                }
            }

            ///
            ///-------------Norhtern border-----------------
            ///
            edgeNeeded = false;
            canExtend = false;
            //check if you are at the top border
            if (currentCell.Y == m_yDim - 1) edgeNeeded = true;
            //check if there is a cell above you, if not edge is needed
            else if (!m_Cells[currentCell.ID + m_xDim].exists) edgeNeeded = true;
            //generate new edge if to the left bounds 
            if (currentCell.X == 0)
            {
                if (edgeNeeded)
                    GenerateEdge(currentCell, Direction.NORTH);
            }
            //else continue
            else
            {
                //check if the neighbour has a border you can extend
                canExtend = (neighbourHasBorder(m_Cells[currentCell.ID - 1], Direction.NORTH));
                if (edgeNeeded)
                {
                    if (canExtend) ExtendLine(currentCell, Direction.NORTH);
                    else GenerateEdge(currentCell, Direction.NORTH);
                }
            }
        }


        MoveToOrigin();
    }
    //  private Color[] GetTexture(out int dimX, out int dimY)
    private Color[] GetTexture(Texture2D texture)
    {
        int dimX = 0;
        int dimY = 0;
        Color[] rColor = new Color[0];
        if (m_CollisionDetail == MipmapLevel.NORMAL)
            rColor = texture.GetPixels();
        dimX = texture.width;
        dimY = texture.height;

        if (m_CollisionDetail == MipmapLevel.MIP_MAP_1)
            rColor = texture.GetPixels(1);

        if (m_CollisionDetail == MipmapLevel.MIP_MAP_2)
            rColor = texture.GetPixels(2);

        if (m_CollisionDetail == MipmapLevel.MIP_MAP_3)
            rColor = texture.GetPixels(3);

        return rColor;
    }
    private void DrawBoxes()
    {
        int index = 0;
        Debug.Log("drawing boxes");
        foreach (AABB aabb in m_AABBs)
        {
            Debug.Log("drawing boxe :" + index++);

            Vector2 min = aabb.Min;
            Vector2 max = aabb.Max;
            Vector2 TopLeft = new Vector2(min.x, max.y);
            Vector2 botRight = new Vector2(max.x, min.y);
            Debug.DrawLine(min, TopLeft, debugColor, 100.0f);
            Debug.DrawLine(min, botRight, debugColor, 100.0f);
            Debug.DrawLine(TopLeft, max, debugColor, 100.0f);
            Debug.DrawLine(botRight, max, debugColor, 100.0f);
            //Debug.Log(aabb.ToString());
        }
    }
    private void GenerateAABBs()
    {

        //iterate throgh cells
        foreach (Cell currentCell in m_Cells)
        {
            if (!currentCell.exists) continue;
            //if adjacent to texture border or if not surrounded by coloured pixels generate AABB for intersecting
            //check north
            if (currentCell.Y == (m_yDim - 1))
            {
                GenerateBox(currentCell);
                continue;
            }
            else if (!m_Cells[currentCell.ID + m_xDim].exists)
            {
                GenerateBox(currentCell);
                continue;
            }

            //check south
            if (currentCell.Y == 0)
            {
                GenerateBox(currentCell);
                continue;
            }
            else if (!m_Cells[currentCell.ID - m_xDim].exists)
            {
                GenerateBox(currentCell);
                continue;
            }
            //check east
            if (currentCell.X == 0)
            {
                GenerateBox(currentCell);
                continue;
            }
            else if (!m_Cells[currentCell.ID - 1].exists)
            {
                GenerateBox(currentCell);
                continue;
            }
            //check west
            if (currentCell.X == (m_xDim - 1))
            {
                GenerateBox(currentCell);
                continue;
            }
            else if (!m_Cells[currentCell.ID + 1].exists)
            {
                GenerateBox(currentCell);
                continue;
            }

        }
        Debug.Log("Generated " + m_AABBs.Count + " AABBs");
    }
    private void GenerateBox(Cell currentCell)
    {
        // Debug.Log("Generating aabb:" + currentCell.ID);

        Vector2 min = new Vector2(currentCell.X * m_Scale, currentCell.Y * m_Scale);
        min += m_origin;
        //   AABB newBox = new AABB(min, min + new Vector2(m_Scale, m_Scale));
        AABB newBox = new AABB(min, min + new Vector2(m_Scale, m_Scale), currentCell.Color);
        m_AABBs.Add(newBox);
    }
    private Cell GetSouthernCell(Cell currentCell)
    {

        int cellID = currentCell.ID - m_xDim;
        //cell is out of bound return empty cell
        if (cellID < 0 || cellID > m_Cells.Count) Debug.LogError("TRYING TO ACCES CELL OUT OF BOUNDS!!!");

        return m_Cells[cellID];
    }
    private Cell GetWesternCell(Cell currentCell)
    {

        int cellID = currentCell.ID - 1;
        //cell is out of bound return empty cell
        if (cellID < 0 || cellID > m_Cells.Count) Debug.LogError("TRYING TO ACCES CELL OUT OF BOUNDS!!!");

        return m_Cells[cellID];
    }
    private void ExtendLine(Cell currentCell, Direction dir)
    {
        Direction extensionDir = 0;
        if (dir == Direction.EAST || dir == Direction.WEST) extensionDir = Direction.SOUTH;
        if (dir == Direction.SOUTH || dir == Direction.NORTH) extensionDir = Direction.WEST;

        //Grab cell to extend from
        Cell extensionCell = new Cell();
        if (extensionDir == Direction.SOUTH) extensionCell = GetSouthernCell(currentCell);
        if (extensionDir == Direction.WEST) extensionCell = GetWesternCell(currentCell);

        //grab line from cell
        LineSegment line = m_BoundingLineSegments[extensionCell.edgeID[(int)dir]];

        //generate the offset 
        Vector2 offset = Vector2.zero;
        if (dir == Direction.SOUTH) offset = new Vector2(m_Scale, 0);
        if (dir == Direction.NORTH || dir == Direction.WEST) offset = new Vector2(m_Scale, m_Scale);
        if (dir == Direction.EAST) offset = new Vector2(0, m_Scale);
        // if (dir == Direction.WEST) offset = new Vector2(m_Scale, 0);

        //update Line 
        line.endPos = (offset + new Vector2(currentCell.X * m_Scale, currentCell.Y * m_Scale));

        //UPDATE CELL
        currentCell.edgeID[(int)dir] = extensionCell.edgeID[(int)dir];
        currentCell.edgeExists[(int)dir] = true;
    }
    private void MoveToOrigin()
    {

        foreach (LineSegment currentLine in m_BoundingLineSegments)
        {
            currentLine.startPos += m_origin;
            currentLine.endPos += m_origin;
        }
    }

    private void GenerateEdge(Cell currentCell, Direction edgeDirection)
    {
        Vector2 direction = Vector2.zero;
        Vector2 offset = Vector2.zero;

        //border goes up
        if (edgeDirection == Direction.EAST || edgeDirection == Direction.WEST) direction = new Vector2(0, m_Scale);
        //border goes to the right
        else direction = new Vector2(m_Scale, 0);


        if (edgeDirection == Direction.NORTH) offset = new Vector2(0, m_Scale);
        if (edgeDirection == Direction.WEST) offset = new Vector2(m_Scale, 0);


        //actually generate the edge
        LineSegment line = new LineSegment();

        line.startPos = new Vector2(currentCell.X * m_Scale, currentCell.Y * m_Scale) + offset;
        line.endPos = line.startPos + direction;
        m_BoundingLineSegments.Add(line);

        int id = m_BoundingLineSegments.Count - 1;

        //store information
        currentCell.edgeID[(int)edgeDirection] = id;
        currentCell.edgeExists[(int)edgeDirection] = true;
    }
    private bool neighbourHasBorder(Cell currentCell, Direction dir) => currentCell.edgeExists[(int)dir];

    private void VisualizeNormals()
    {
    }
}
struct Cell
{

    //init cell
    public Cell(bool doesExist, int id, int dimX, int dimY, int xCoord, int yCoord, float2 sampleValue, float4 ColorSampleVal)
    {
        Color = ColorSampleVal;
        Normal = math.normalize(sampleValue);
        ID = id;
        X = xCoord;
        Y = yCoord;
        exists = doesExist;

        edgeID = new List<int>(4);
        for (int i = 0; i < 4; i++)
        {
            edgeID.Add(-1);
        }

        edgeExists = new List<bool>();
        neighbour = new List<bool>();
        for (int i = 0; i < 4; i++)
        {
            edgeExists.Add(false);
            neighbour.Add(true);
        }

    }
    //0==north
    //1==east
    //2=south
    //3=east
    public float2 Normal;
    public List<int> edgeID;
    //0 is north
    //1 is east
    //2 is south
    //3 is west
    public List<bool> edgeExists;
    public List<bool> neighbour;
    public float4 Color;
    public bool exists;
    public readonly int X;
    public readonly int Y;
    public readonly int ID;
}
public class LineSegment
{
    public Vector2 startPos;
    public Vector2 endPos;

    public LineSegment() { }
    public LineSegment(Vector2 a, Vector2 b)
    {
        startPos = a;
        endPos = b;
    }

}