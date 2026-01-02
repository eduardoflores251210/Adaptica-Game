using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigCube : MonoBehaviour
{
    /* vertex 8 (0-7)
          E4-------------F5         7654-3210
          |               |         HGFE-DCBA
          |               |
    H7-------------G6     |
    |     |         |     |
    |     |         |     |
    |     A0--------|----B1  
    |               |
    |               |
    D3-------------C2

    edge 12 (0-12)                  11
         +------E4-------+          1098-7654-3210 
       H7|            F5 |          LKJI-HGFE-DCBA
      /  |           /   |
    +-------G6------+    |
    |    |         |     |
    |    I8        |     J9
    L11  |         K10   |
    |    |         |     |
    |    +------A0-|-----+  
    |  D3          |   B1
    | /            |  /
    +-------C2------+               */

    public LineRenderer line = null;
    public GridPoint[] points = new GridPoint[8];
    public Material material = null;
    private Mesh mesh = null;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();
    private GridCell cell = new GridCell();
    private bool bGridReady = false;

    private void Start()
    {
        for (int i = 0; i < points.Length; i++)
        {
            cell.p[i] = points[i];
        }

        DrawLines();
    }
    private void OnEnable()
    {
        Mesh03.OnGridInitialized += OnGridInitialized;
    }
    private void OnDisable()
    {
        Mesh03.OnGridInitialized -= OnGridInitialized;
    }
    private void DrawLines()
    {
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

        float X = 1;
        float Y = 1;
        float Z = 1;

        line.positionCount = 16;
        line.SetPosition(0, new Vector3(0, 0, 0));  //D is the origin point of the cube
        line.SetPosition(1, new Vector3(0, Y, 0));   
        line.SetPosition(2, new Vector3(X, Y, 0));   
        line.SetPosition(3, new Vector3(X, 0, 0));   
        line.SetPosition(4, new Vector3(0, 0, 0));

        line.SetPosition(5, new Vector3(0, 0, Z));   
        line.SetPosition(6, new Vector3(0, Y, Z));   
        line.SetPosition(7, new Vector3(X, Y, Z));  
        line.SetPosition(8, new Vector3(X, 0, Z));   
        line.SetPosition(9, new Vector3(0, 0, Z));

        line.SetPosition(10, new Vector3(0, Y, Z)); 
        line.SetPosition(11, new Vector3(0, Y, 0));  
        line.SetPosition(12, new Vector3(X, Y, 0)); 
        line.SetPosition(13, new Vector3(X, Y, Z)); 

        line.SetPosition(14, new Vector3(X, 0, Z)); 
        line.SetPosition(15, new Vector3(X, 0, 0)); 
    }
    public void Visualize(GridCell celltoviz, float SurfaceLevel)
    {
        if (bGridReady == false)
            return;

        GameObject go = this.gameObject;
        MarchingCube.GetMesh(ref go, ref material, false);

        vertices.Clear();
        triangles.Clear();
        uv.Clear();

        for (int i = 0; i < celltoviz.p.Length; i++)
        {
            if (cell.p[i] == null)
                cell.p[i] = new GridPoint();
            cell.p[i].Value = celltoviz.p[i].Value;
            cell.p[i].Color = celltoviz.p[i].Color;
            cell.p[i].On = celltoviz.p[i].On;
            cell.p[i].Glow = celltoviz.p[i].Glow;
        }
        MarchingCube.IsoFaces(ref cell, SurfaceLevel);
        
        bool uvAlternate = false;
        for (int i = 0; i < cell.numtriangles; i++)
        {
            vertices.Add(cell.triangle[i].p[0]);
            vertices.Add(cell.triangle[i].p[1]);
            vertices.Add(cell.triangle[i].p[2]);

            triangles.Add(vertices.Count - 1);  //order of triangles = side rendered
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

        Vector3[] av = vertices.ToArray();
        int[] at = triangles.ToArray();
        Vector2[] au = uv.ToArray();
        MarchingCube.SetMesh(ref go, ref av, ref at, ref au);
    }
    private void OnGridInitialized(ref Mesh03 scr)
    {
        bGridReady = true;
    }
}
