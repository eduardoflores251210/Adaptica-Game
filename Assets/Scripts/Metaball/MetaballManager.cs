using StandartUtilities;// antes se usaba IsEven aqui pero ya noa si que quedo como remanente esto
using System.Collections.Generic;
using UnityEngine;
using Sys = System;

public class MetaballManager : MonoBehaviour //No confundir con MeatBallManager
{
    public static MetaballManager Instance;
    public List<Metaball> metaballs = new List<Metaball>();
    public LineRenderer line = null;
    public GameObject crt; //No no e suna referencia a los tv de tubo
    public bool isInDebug;
    // Datos de la malla
    public Vector3 GridSize = new Vector3(16, 16, 16); 
    public float SurfaceLevel = 0.5f;
    public Material material;

    private GridPoint[,,] gridPoints;
    private GridCell cell = new GridCell();

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();

    public bool bRequestBuild = false;

    public Mesh mesh;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        if (line != null) 
        DrawLines();
        if (crt == null) crt = new GameObject("AAA"); //AAA por que no se me ocuruo otro nombre
        float X, Y, Z; //define 3 variables
        X = GridSize.x; Y = GridSize.y; Z = GridSize.z;
        crt.transform.position = new Vector3(X / 2, Y / 2, Z / 2);
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
    private void OnEnable()
    {
        GridPoint.OnPointValueChange += OnPointValueChange;
    }

    private void OnDisable()
    {
        GridPoint.OnPointValueChange -= OnPointValueChange;
    }

    private void Start()
    {
        gridPoints = new GridPoint[(int)GridSize.x + 1, (int)GridSize.y + 1, (int)GridSize.z + 1];
        InitGrid();
        BuildMesh();
    }
    public void RegisterMetaball(Metaball mb)
    {
        if (!metaballs.Contains(mb))
        {
            metaballs.Add(mb);
            bRequestBuild = true; // Solicitar reconstrucción porque hay una nueva metaball
        }
    }

    public void UnregisterMetaball(Metaball mb)
    {
        if (metaballs.Contains(mb))
        {
            metaballs.Remove(mb);
            bRequestBuild = true; // Solicitar reconstrucción porque se eliminó una metaball
        }
    }
    public void DestroyAllMetaballs()
    {
        if (metaballs.Count > 0)
        {
            foreach (Metaball item in metaballs)
            {
                Destroy(item.gameObject);
            }
        }
    }
    private void Update()
    {
        if (bRequestBuild)
        {
            BuildMesh();
            bRequestBuild = false;
        }

        UpdateMetaballDensities();
        mesh = GetComponent<MeshFilter>().mesh;
    }

    private void OnPointValueChange(ref GridPoint gp)
    {
        bRequestBuild = true;
    }

    //copiado directamente del ejemplo 4
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
                    gridPoints[(int)x, (int)y, (int)z] = gp;
                    if (!isInDebug)
                    {
                        if (go.TryGetComponent<MeshRenderer>(out var Pe))
                        {
                            Destroy(Pe);
                            
                        }
                    }
                }
            }
        }
    }

    private void UpdateMetaballDensities()
    {
        // Primero ponemos todos los valores en 0 para evitar acumulaciones viejas
        int gx = (int)GridSize.x;
        int gy = (int)GridSize.y;
        int gz = (int)GridSize.z;

        for (int z = 0; z <= gz; z++)
        {
            for (int y = 0; y <= gy; y++)
            {
                for (int x = 0; x <= gx; x++)
                {
                    // Reiniciamos el valor para que el evento se dispare si cambia
                    gridPoints[x, y, z].Value = 0f;
                }
            }
        }

        // Ahora sumamos las densidades de cada metabola y actualizamos el gridPoint
        foreach (var mb in metaballs)
        {
            Vector3 mbPos = mb.transform.position;
            float radius = mb.Radius;
            float rSqr = radius * radius;

            // Para optimizar, solo modificamos puntos dentro del rango del metaball
            int startX = Mathf.Max(0, Mathf.FloorToInt(mbPos.x - radius));
            int endX = Mathf.Min(gx, Mathf.CeilToInt(mbPos.x + radius));
            int startY = Mathf.Max(0, Mathf.FloorToInt(mbPos.y - radius));
            int endY = Mathf.Min(gy, Mathf.CeilToInt(mbPos.y + radius));
            int startZ = Mathf.Max(0, Mathf.FloorToInt(mbPos.z - radius));
            int endZ = Mathf.Min(gz, Mathf.CeilToInt(mbPos.z + radius));

            for (int z = startZ; z <= endZ; z++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        Vector3 ptPos = gridPoints[x, y, z].Position;
                        Vector3 diff = ptPos - mbPos;
                        float distSqr = diff.sqrMagnitude;

                        if (distSqr < rSqr)
                        {
                            float density = (rSqr) / (distSqr + 0.0001f);
                            // Acumula la densidad y dispara el evento solo si cambia realmente el valor
                            gridPoints[x, y, z].Value += density;
                        }
                    }
                }
            }
        }
    }

    private void BuildMesh()
    {
        GameObject go = gameObject;
        float mark = Time.time;

       
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
                    cell.p[0] = gridPoints[x, y, z + 1];         //A0
                    cell.p[1] = gridPoints[x + 1, y, z + 1];     //B1
                    cell.p[2] = gridPoints[x + 1, y, z];         //C2
                    cell.p[3] = gridPoints[x, y, z];             //D3
                    cell.p[4] = gridPoints[x, y + 1, z + 1];     //E4
                    cell.p[5] = gridPoints[x + 1, y + 1, z + 1]; //F5
                    cell.p[6] = gridPoints[x + 1, y + 1, z];     //G6
                    cell.p[7] = gridPoints[x, y + 1, z];         //H7

                    MarchingCube.IsoFaces(ref cell, SurfaceLevel);

                    BuildMeshCellData(ref cell);
                }
            }
        }
        try
        {
            Vector3[] av = vertices.ToArray();
            int[] at = triangles.ToArray();
            Vector2[] au = uv.ToArray();
            MarchingCube.SetMesh(ref go, ref av, ref at, ref au);

            float timeTaken = mark - Time.time;
            if (Mathf.Approximately(timeTaken, 0) == false)
            {
                Debug.Log(string.Format("mesh time = {0}", timeTaken));
            }
        } catch (Sys.Exception e)
        {
            Debug.Log(e);
        }
    }

    private void BuildMeshCellData(ref GridCell cell) //hace algo 
    {
        bool uvAlternate = false;
        for (int i = 0; i < cell.numtriangles; i++)
        {
            vertices.Add(cell.triangle[i].p[0]);
            vertices.Add(cell.triangle[i].p[1]);
            vertices.Add(cell.triangle[i].p[2]);

            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);

            if (uvAlternate)
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
