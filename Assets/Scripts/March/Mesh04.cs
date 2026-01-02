using System.Collections.Generic;
using UnityEngine;

public class Mesh04 : MonoBehaviour
{
    public Vector3 GridSize = new Vector3(3, 3, 3);
    public float SurfaceLevel = 0.5f;
    public LineRenderer line = null;
    public Material material = null;
    private GridPoint[,,] p = null;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();
    private GridCell cell = new GridCell();
    public bool bRequestBuild = false;

    private void Start()
    {
        p = new GridPoint[(int) GridSize.x + 1, (int) GridSize.y + 1, (int) GridSize.z + 1];
        InitGrid();
    }
    private void Update()
    {
        if ((true) == true)
        {
            BuildMesh();
        }

        if (bRequestBuild == true)
        {
            BuildMesh();
            bRequestBuild = false;
        }
    }
    private void OnEnable()
    {
        GridPoint.OnPointValueChange += OnPointValueChange;
    }
    private void OnDisable()
    {
        GridPoint.OnPointValueChange -= OnPointValueChange;
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
                    gp.Size = 0.1f;
                    gp.Value = SurfaceLevel + 0.05f;
                    p[(int)x, (int)y, (int)z] = gp;
                }
            }
        }
    }
    public void DrawLines()
    {
        float X = GridSize.x;
        float Y = GridSize.y;
        float Z = GridSize.z;

        line.positionCount = 16;
        line.SetPosition(0, new Vector3(0, 0, 0));   //C
        line.SetPosition(1, new Vector3(0, Y, 0));   //A
        line.SetPosition(2, new Vector3(X, Y, 0));   //B
        line.SetPosition(3, new Vector3(X, 0, 0));   //D
        line.SetPosition(4, new Vector3(0, 0, 0));

        line.SetPosition(5, new Vector3(0, 0, Z));   //G
        line.SetPosition(6, new Vector3(0, Y, Z));   //E
        line.SetPosition(7, new Vector3(X, Y, Z));   //F
        line.SetPosition(8, new Vector3(X, 0, Z));   //H
        line.SetPosition(9, new Vector3(0, 0, Z));

        line.SetPosition(10, new Vector3(0, Y, Z));  //E
        line.SetPosition(11, new Vector3(0, Y, 0));  //A
        line.SetPosition(12, new Vector3(X, Y, 0));  //B
        line.SetPosition(13, new Vector3(X, Y, Z));  //F

        line.SetPosition(14, new Vector3(X, 0, Z));  //H
        line.SetPosition(15, new Vector3(X, 0, 0));  //D
    }
    private void BuildMesh()
    {
        float mark = Time.time;

        GameObject go = this.gameObject;
        MarchingCube.GetMesh(ref go, ref material, false);

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

        vertices.Clear();
        triangles.Clear();
        uv.Clear();

        for (int z = 0; z < GridSize.z; z++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    cell.p[0] = p[x, y, z + 1];         //A0
                    cell.p[1] = p[x + 1, y, z + 1];     //B1
                    cell.p[2] = p[x + 1, y, z];         //C2
                    cell.p[3] = p[x, y, z];             //D3
                    cell.p[4] = p[x, y + 1, z + 1];     //E4
                    cell.p[5] = p[x + 1, y + 1, z + 1]; //F5
                    cell.p[6] = p[x + 1, y + 1, z];     //G6
                    cell.p[7] = p[x, y + 1, z];         //H7

                    MarchingCube.IsoFaces(ref cell, SurfaceLevel);

                    BuildCellMeshData(ref cell);
                }
            }
        }

        Vector3[] av = vertices.ToArray();
        int[] at = triangles.ToArray();
        Vector2[] au = uv.ToArray();
        MarchingCube.SetMesh(ref go, ref av, ref at, ref au);

        float timeTaken = mark - Time.time;
        if (Mathf.Approximately(timeTaken, 0) == false)
        {
            Debug.Log(string.Format("mesh time = {0}", timeTaken));
        }        
    }
    private void BuildCellMeshData(ref GridCell cell)
    {
        bool uvAlternate = false;
        for (int i = 0; i < cell.numtriangles; i++)
        {
            vertices.Add(cell.triangle[i].p[0]);
            vertices.Add(cell.triangle[i].p[1]);
            vertices.Add(cell.triangle[i].p[2]);

            triangles.Add(vertices.Count - 1);
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
    private void OnPointValueChange(ref GridPoint gp)
    {
        bRequestBuild = true;
    }
}
