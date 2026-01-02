using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mesh03 : MonoBehaviour
{
    // Purpose:
    //  grid of points to sampling a 3D volume

    #region --- events ---
    public delegate void GridInitialized(ref Mesh03 g);
    public static event GridInitialized OnGridInitialized;
    #endregion

    /*  vertex 8 (0-7)
          E4-------------F5         7654-3210
          |               |         HGFE-DCBA
          |               |
    H7-------------G6     |
    |     |         |     |
    |     |         |     |
    |     A0--------|----B1  
    |               |
    |               |
    D3-------------C2               */

    public Vector3 GridSize = new Vector3(3, 3, 3);
    [Range(0f, 1f)]
    public float PointSize = 0.25f;   
    [Range(0.1f, 10.0f)]
    public float Zoom = 3f;
    [Range(0f, 1f)]
    public float SurfaceLevel = 0.5f;
    public Material material = null;
    public bool Noise2D = true;
    public LineRenderer lineOutline = null;
    public LineRenderer lineHighlight = null;
    public LineRenderer lineExtend = null;
    public Text txtDebug = null;
    public Vector3 D = Vector3.zero;   //D is the (0,0,0) origin point of a cell
    public GridPoint[,,] p = null;
    public ConfigCube configcube = null;
    private float? pointMinValue = null;
    private float? pointMaxValue = null;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();
    private bool bGridReady = false;

    private void Start()
    {
        p = new GridPoint[(int)GridSize.x + 1, (int)GridSize.y + 1, (int)GridSize.z + 1];
        InitGrid();
        OutlineGrid();
        SetCurrentCell();        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) == true && bGridReady == true)
        {
            BuildMesh();
        }

        // UP ARROW
        if (Input.GetKeyDown(KeyCode.UpArrow) == true) 
        {
            if (Input.GetKey(KeyCode.LeftShift) == true)
            {
                D = new Vector3(D.x, Mathf.Clamp(D.y + 1, 0, GridSize.y), D.z); // [shift] up
                SetCurrentCell();
            }
            else
            {
                D = new Vector3(D.x, D.y, Mathf.Clamp(D.z + 1, 0, GridSize.z)); // forward
                SetCurrentCell();
            }
        }

        // DOWN ARROW
        if (Input.GetKeyDown(KeyCode.DownArrow) == true)
        {
            if (Input.GetKey(KeyCode.LeftShift) == true)
            {
                D = new Vector3(D.x, Mathf.Clamp(D.y - 1, 0, GridSize.y), D.z); // [shift] down
                SetCurrentCell();
            }
            else
            {
                D = new Vector3(D.x, D.y, Mathf.Clamp(D.z - 1, 0, GridSize.z)); // back
                SetCurrentCell();
            }
        }

        // LEFT ARROW
        if (Input.GetKeyDown(KeyCode.LeftArrow) == true)
        {
            D = new Vector3(Mathf.Clamp(D.x - 1, 0, GridSize.x), D.y, D.z); // left
            SetCurrentCell();
        }

        // RIGHT ARROW
        if (Input.GetKeyDown(KeyCode.RightArrow) == true)
        {
            D = new Vector3(Mathf.Clamp(D.x + 1, 0, GridSize.x), D.y, D.z); // right
            SetCurrentCell();
        }
    }
    private void InitGrid()
    {
        for (float z = 0; z <= GridSize.z; z++)
        {
            for (float y = 0; y <= GridSize.y; y++)
            {
                for (float x = 0; x <= GridSize.x; x++)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.parent = this.transform;
                    go.GetComponent<Collider>().isTrigger = true;
                    Rigidbody rb = go.gameObject.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    GridPoint gp = go.gameObject.AddComponent<GridPoint>();
                    gp.Position = new Vector3(x, y, z);
                    p[(int)x, (int)y, (int)z] = gp;
                }
            }
        }

        UpdateGridDisplay();

        bGridReady = true;
        if (OnGridInitialized != null)
        {
            Mesh03 scr = this.GetComponent<Mesh03>();
            OnGridInitialized(ref scr);
        }
    }
    public void UpdateGridDisplay()
    {
        if (p == null)
            return;

        //apply noise to points
        float nx = 0;
        float ny = 0;
        float nz = 0;
        for (int z = 0; z <= GridSize.z; z++)
        {
            for (int y = 0; y <= GridSize.y; y++)
            {
                for (int x = 0; x <= GridSize.x; x++)
                {
                    GridPoint gp = p[x, y, z];
                    nx = (x / GridSize.x) * Zoom;
                    ny = (y / GridSize.y) * Zoom;
                    nz = (z / GridSize.z) * Zoom;
                    if (Noise2D == true)
                    {
                        gp.Value = (Mathf.PerlinNoise(nx, nz) * ny);
                    }
                    else
                    {
                        gp.Value = MarchingCube.Perlin3D(nx, ny, nz);
                    }
                    if (pointMinValue == null || pointMinValue.Value > gp.Value)
                    {
                        pointMinValue = gp.Value;
                    }
                    if (pointMaxValue == null || pointMaxValue.Value < gp.Value)
                    {
                        pointMaxValue = gp.Value;
                    }
                }
            }
        }

        //set color, size, on
        for (int z = 0; z <= GridSize.z; z++)
        {
            for (int y = 0; y <= GridSize.y; y++)
            {
                for (int x = 0; x <= GridSize.x; x++)
                {
                    GridPoint gp = p[x, y, z];
                    gp.Size = PointSize;
                    gp.Color = 1.0f - ((pointMaxValue.Value - gp.Value) / (pointMaxValue.Value - pointMinValue.Value));
                    gp.Value = gp.Color;    //convert all the values from 0 to %100
                    gp.Glow = false;
                    gp.On = (gp.Color >= SurfaceLevel) ? false : true;
                }
            }
        }
    }
    public void OutlineGrid()
    {
        DrawLines(ref lineOutline, this.transform.localPosition, GridSize.x, GridSize.y, GridSize.z);
    }
    public GridCell SetCurrentCell()
    {
        // Note: 
        //  D is the 0,0,0 point of the current grid cell. Other points are found as offsets from D corner.
        //  Set D = Vector3 to specify highlight cell.

        DrawLines(ref lineHighlight, D, 1f, 1f, 1f);

        // extension lines
        float elinex = D.x + 0.5f;
        float eliney = D.y + 0.5f;
        float elinez = D.z + 0.5f;
        lineExtend.positionCount = 8;
        lineExtend.SetPosition(0, new Vector3(0, eliney, elinez));          //A =0
        lineExtend.SetPosition(1, new Vector3(GridSize.x, eliney, elinez)); //B =1
        lineExtend.SetPosition(2, new Vector3(elinex, eliney, elinez));     //C =2 
        lineExtend.SetPosition(3, new Vector3(elinex, 0, elinez));          //D =3
        lineExtend.SetPosition(4, new Vector3(elinex, GridSize.y, elinez)); //E =4
        lineExtend.SetPosition(5, new Vector3(elinex, eliney, elinez));     //F =5
        lineExtend.SetPosition(6, new Vector3(elinex, eliney, 0));          //G =6
        lineExtend.SetPosition(7, new Vector3(elinex, eliney, GridSize.z)); //H =7

        UpdateGridDisplay();
        GridCell cell = DefineGridCell(D);
        if (cell != null)
        {
            //highlight grid cell points
            cell.p[0].Glow = true;
            cell.p[1].Glow = true;
            cell.p[2].Glow = true;
            cell.p[3].Glow = true;
            cell.p[4].Glow = true;
            cell.p[5].Glow = true;
            cell.p[6].Glow = true;
            cell.p[7].Glow = true;

            // cube triangles
            MarchingCube.IsoFaces(ref cell, SurfaceLevel);
            txtDebug.text = string.Format("{0} {1}", cell.config, Bits.BinaryFormNumbers(cell.config));
            configcube.Visualize(cell, SurfaceLevel);
        }        

        return cell;
    }
    private void DrawLines(ref LineRenderer line, Vector3 C, float X, float Y, float Z)
    {
        line.positionCount = 16;
        line.SetPosition(0, C + new Vector3(0, 0, 0));   //C
        line.SetPosition(1, C + new Vector3(0, Y, 0));   //A
        line.SetPosition(2, C + new Vector3(X, Y, 0));   //B
        line.SetPosition(3, C + new Vector3(X, 0, 0));   //D
        line.SetPosition(4, C + new Vector3(0, 0, 0));

        line.SetPosition(5, C + new Vector3(0, 0, Z));   //G
        line.SetPosition(6, C + new Vector3(0, Y, Z));   //E
        line.SetPosition(7, C + new Vector3(X, Y, Z));   //F
        line.SetPosition(8, C + new Vector3(X, 0, Z));   //H
        line.SetPosition(9, C + new Vector3(0, 0, Z));

        line.SetPosition(10, C + new Vector3(0, Y, Z));  //E
        line.SetPosition(11, C + new Vector3(0, Y, 0));  //A
        line.SetPosition(12, C + new Vector3(X, Y, 0));  //B
        line.SetPosition(13, C + new Vector3(X, Y, Z));  //F

        line.SetPosition(14, C + new Vector3(X, 0, Z));  //H
        line.SetPosition(15, C + new Vector3(X, 0, 0));  //D
    }
    public GridCell DefineGridCell(Vector3 D)
    {
        if (p == null)
            return null;

        GridCell cell = new GridCell();
        int x = (int)D.x;
        int y = (int)D.y;
        int z = (int)D.z;

        /*  vertex 8 (0-7)
              E4-------------F5         7654-3210
              |               |         HGFE-DCBA
              |               |
        H7-------------G6     |
        |     |         |     |
        |     |         |     |
        |     A0--------|----B1  
        |               |
        |               |
        D3-------------C2               */

        cell.p[0] = p[x, y, z + 1];           // A =0
        cell.p[1] = p[x + 1, y, z + 1];       // B =1
        cell.p[2] = p[x + 1, y, z];           // C =2
        cell.p[3] = p[x, y, z];               // D =3
        cell.p[4] = p[x, y + 1, z + 1];       // E =4
        cell.p[5] = p[x + 1, y + 1, z + 1];   // F =5
        cell.p[6] = p[x + 1, y + 1, z];       // G =6
        cell.p[7] = p[x, y + 1, z];           // H =7

        return cell;
    }
    private void BuildMesh()
    {
        GameObject go = this.gameObject;
        MarchingCube.GetMesh(ref go, ref material, true);

        vertices.Clear();
        triangles.Clear();
        uv.Clear();

        GridCell cell = null;
        for (int z = 0; z < GridSize.z; z++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    D = p[x, y, z].Position;
                    cell = SetCurrentCell();
                    BuildCellMeshData(ref cell);
                }
            }
        }

        Vector3[] av = vertices.ToArray();
        int[] at = triangles.ToArray();
        Vector2[] au = uv.ToArray();
        MarchingCube.SetMesh(ref go, ref av, ref at, ref au);
    }
    private void BuildCellMeshData(ref GridCell cell)
    {
        bool uvAlternate = false;
        for (int i = 0; i < cell.numtriangles; i++)
        {
            vertices.Add(cell.triangle[i].p[0]);
            vertices.Add(cell.triangle[i].p[1]);
            vertices.Add(cell.triangle[i].p[2]);

            triangles.Add(vertices.Count - 1);  //clockwise order for side you want to render
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 3);

            if (uvAlternate == true)
            {
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.D);
            }
            else
            {
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.C);
            }
            uvAlternate = !uvAlternate;
        }
    }
}
