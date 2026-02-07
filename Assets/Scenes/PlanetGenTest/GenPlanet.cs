using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGen : MonoBehaviour
{
	[Header("Datos del planeta")]
	public PlanetData planetData;

	[Header("Opciones de generación")]
	public bool useAbstract = true; // true = array (AbstractGridPoint), false = MonoBehaviour
	public Vector3Int chunkSize = new Vector3Int(16, 16, 16);
	public Vector3Int cubeSize = new Vector3Int(4, 4, 4);
	public float SurfaceLevel = 0.5f;
	public Material material;

	[Header("Opciones de ruido")]
	public float noiseScale = 0.1f;
	public float noiseAmplitude = 5f;
	public float ExtraOffset = 0f;
	[Range(0f, 1f)]
	public float noiseIntensity = 0.5f;

	private void Start()
	{
		if (planetData == null)
		{
			Debug.LogWarning("PlanetData no asignado, usando radio 1.");
			planetData = new PlanetData { radius = 1f };
		}

		float radius = planetData.radius * 100f;

		if (useAbstract)
		{
			BuildPlanetAbstract(radius);
			foreach (Transform chunk in transform)
			{
				DrawChunkWireframe(chunk.gameObject); //se dibujae el wireframe de los chunks para vererificar que esta correcto.
				//Debug.Log(chunk.name)			 // de momento no se requiere 																										;
			}
		}
		else
		{
			BuildPlanetPhysical(radius);
			foreach (Transform chunk in transform)
			{
				DrawChunkWireframe(chunk.gameObject); //se dibujae el wireframe de los chunks para vererificar que esta correcto.
													  //Debug.Log(chunk.name)			 // de momento no se requiere 																										;
			}
		}
	}

	#region Abstract Mode
	private void BuildPlanetAbstract(float radius)
	{
		Vector3 planetCenter = new Vector3(radius, radius, radius);
		Vector3Int chunksCount = new Vector3Int(
			Mathf.CeilToInt((radius * 2 / chunkSize.x) + noiseAmplitude + ExtraOffset),
			Mathf.CeilToInt((radius * 2 / chunkSize.y) + noiseAmplitude + ExtraOffset),
			Mathf.CeilToInt((radius * 2 / chunkSize.z) + noiseAmplitude + ExtraOffset)
		);

		for (int x = 0; x < chunksCount.x; x++)
			for (int y = 0; y < chunksCount.y; y++)
				for (int z = 0; z < chunksCount.z; z++)
					BuildChunkArray(new Vector3Int(x, y, z), radius, planetCenter);
	}

	private void BuildChunkArray(Vector3Int chunkIndex, float radius, Vector3 planetCenter)
	{
		GameObject chunkObj = new GameObject($"Chunk_{chunkIndex.x}_{chunkIndex.y}_{chunkIndex.z}");
		chunkObj.transform.parent = transform;
		chunkObj.transform.position = new Vector3(
			chunkIndex.x * chunkSize.x,
			chunkIndex.y * chunkSize.y,
			chunkIndex.z * chunkSize.z
		);
		chunkObj.AddComponent<LineRenderer>();

		AbstractGridPoint[,,] grid = new AbstractGridPoint[chunkSize.x + 1, chunkSize.y + 1, chunkSize.z + 1];
		Vector3 chunkOrigin = new Vector3(chunkIndex.x * chunkSize.x, chunkIndex.y * chunkSize.y, chunkIndex.z * chunkSize.z);

		for (int z = 0; z <= chunkSize.z; z++)
			for (int y = 0; y <= chunkSize.y; y++)
				for (int x = 0; x <= chunkSize.x; x++)
				{
					// 1. Posición relativa al CHUNK (0 a 16)
					// Esto es lo que Marching Cubes usa para crear la malla local.
					Vector3 posLocal = new Vector3(x, y, z);

					// 2. Posición en el MUNDO (donde realmente está el punto en el espacio)
					// Esto solo se usa para calcular la distancia al centro y el ruido.
					Vector3 posMundo = chunkOrigin + posLocal;

					float dist = Vector3.Distance(posMundo, planetCenter);

					// 3. El Ruido (Usando la posición de mundo para que sea continuo)
					float offset = (
						Mathf.PerlinNoise((posMundo.x) * noiseScale, (posMundo.y) * noiseScale) +
						Mathf.PerlinNoise((posMundo.y) * noiseScale, (posMundo.z) * noiseScale) +
						Mathf.PerlinNoise((posMundo.x) * noiseScale, (posMundo.z) * noiseScale)
					) / 3f * noiseAmplitude * noiseIntensity;

					// 4. EL CAMBIO CLAVE:
					grid[x, y, z] = new AbstractGridPoint
					{
						Position = posLocal, // <-- AQUÍ: Usa posLocal, NO posMundo
						Value = (radius + offset) - dist // <-- SUAVIZADO: (R + Ruido) - Distancia
					};
				}

		BuildChunkMeshArray(grid, chunkObj);
	}

	private void BuildChunkMeshArray(AbstractGridPoint[,,] grid, GameObject parent)
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();

		AbstractGridCell cell = new AbstractGridCell();
		int gx = grid.GetLength(0) - 1;
		int gy = grid.GetLength(1) - 1;
		int gz = grid.GetLength(2) - 1;

		for (int z = 0; z < gz; z++)
			for (int y = 0; y < gy; y++)
				for (int x = 0; x < gx; x++)
				{
					cell.p[0] = grid[x, y, z + 1];
					cell.p[1] = grid[x + 1, y, z + 1];
					cell.p[2] = grid[x + 1, y, z];
					cell.p[3] = grid[x, y, z];
					cell.p[4] = grid[x, y + 1, z + 1];
					cell.p[5] = grid[x + 1, y + 1, z + 1];
					cell.p[6] = grid[x + 1, y + 1, z];
					cell.p[7] = grid[x, y + 1, z];

					MarchingCube.IsoFaces(ref cell, SurfaceLevel);
					BuildMeshCellData(ref cell, vertices, triangles, uv);
				}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uv.ToArray();
		mesh.RecalculateNormals();

		MeshFilter mf = parent.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		MeshRenderer mr = parent.AddComponent<MeshRenderer>();
		mr.material = material;
	}
	#endregion

	#region Physical Mode
	private void BuildPlanetPhysical(float radius)
	{
		Vector3 planetCenter = new Vector3(radius, radius, radius);
		Vector3Int chunksCount = new Vector3Int(
			Mathf.CeilToInt((radius * 2 / chunkSize.x) + noiseAmplitude + ExtraOffset), //si no se suma tenemos montañas cortadas Xd xdxdxdxdxdxdx
			Mathf.CeilToInt((radius * 2 / chunkSize.y) + noiseAmplitude + ExtraOffset),
			Mathf.CeilToInt((radius * 2 / chunkSize.z) + noiseAmplitude + ExtraOffset)
		);

		for (int x = 0; x < chunksCount.x; x++)
			for (int y = 0; y < chunksCount.y; y++)
				for (int z = 0; z < chunksCount.z; z++)
					BuildChunkPhysical(new Vector3Int(x, y, z), radius, planetCenter);
	}

	private void BuildChunkPhysical(Vector3Int chunkIndex, float radius, Vector3 planetCenter)
	{
		GameObject chunkObj = new GameObject($"Chunk_{chunkIndex.x}_{chunkIndex.y}_{chunkIndex.z}");
		chunkObj.transform.parent = transform;
		chunkObj.transform.position = new Vector3(
			chunkIndex.x * chunkSize.x,
			chunkIndex.y * chunkSize.y,
			chunkIndex.z * chunkSize.z
		);
		chunkObj.AddComponent<LineRenderer>();

		GridPoint[,,] grid = new GridPoint[chunkSize.x + 1, chunkSize.y + 1, chunkSize.z + 1];
		Vector3 chunkOrigin = new Vector3(chunkIndex.x * chunkSize.x, chunkIndex.y * chunkSize.y, chunkIndex.z * chunkSize.z);

		for (int z = 0; z <= chunkSize.z; z++)
			for (int y = 0; y <= chunkSize.y; y++)
				for (int x = 0; x <= chunkSize.x; x++)
				{
					Vector3 pos = chunkOrigin + new Vector3(x, y, z);
					float dist = Vector3.Distance(pos, planetCenter);
					Vector3 posLocal = new Vector3(x, y, z);
					float offset = (
						Mathf.PerlinNoise((pos.x + z) * noiseScale, (pos.y + z) * noiseScale) +
						Mathf.PerlinNoise((pos.y + z) * noiseScale, (pos.z + z) * noiseScale) +
						Mathf.PerlinNoise((pos.x + z) * noiseScale, (pos.z + z) * noiseScale)
					) / 3f * noiseAmplitude * noiseIntensity;

					GameObject gpObj = new GameObject($"GP_{x}_{y}_{z}");
					gpObj.transform.parent = chunkObj.transform;
					gpObj.transform.localPosition = new Vector3(x, y, z);

					GridPoint gp = gpObj.AddComponent<GridPoint>();
					gp.Position = posLocal;
					gp.Size = 0.1f;
					gp.Value = dist <= radius + offset ? 1f : 0f;

					grid[x, y, z] = gp;
				}

		BuildChunkMesh(grid, chunkObj);
	}

	private void BuildChunkMesh(GridPoint[,,] grid, GameObject parent)
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();

		GridCell cell = new GridCell();
		int gx = grid.GetLength(0) - 1;
		int gy = grid.GetLength(1) - 1;
		int gz = grid.GetLength(2) - 1;

		for (int z = 0; z < gz; z++)
			for (int y = 0; y < gy; y++)
				for (int x = 0; x < gx; x++)
				{
					cell.p[0] = grid[x, y, z + 1];
					cell.p[1] = grid[x + 1, y, z + 1];
					cell.p[2] = grid[x + 1, y, z];
					cell.p[3] = grid[x, y, z];
					cell.p[4] = grid[x, y + 1, z + 1];
					cell.p[5] = grid[x + 1, y + 1, z + 1];
					cell.p[6] = grid[x + 1, y + 1, z];
					cell.p[7] = grid[x, y + 1, z];

					MarchingCube.IsoFaces(ref cell, SurfaceLevel);
					BuildMeshCellData(ref cell, vertices, triangles, uv);
				}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uv.ToArray();
		mesh.RecalculateNormals();

		MeshFilter mf = parent.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		MeshRenderer mr = parent.AddComponent<MeshRenderer>();
		mr.material = material;
	}
	#endregion

	private void BuildMeshCellData(ref GridCell cell, List<Vector3> vertices, List<int> triangles, List<Vector2> uv)
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

	private void BuildMeshCellData(ref AbstractGridCell cell, List<Vector3> vertices, List<int> triangles, List<Vector2> uv)
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
	public bool drawGrid = true;
	public Material debugLineMaterial;

	private void DrawChunkWireframe(GameObject chunk)
	{
		LineRenderer lr = chunk.GetComponent<LineRenderer>();
		if (lr == null)
		{
			lr = chunk.AddComponent<LineRenderer>();
			lr.material = material; // o cualquier material de debug
			lr.widthMultiplier = 0.05f;
			lr.positionCount = 16; // 12 aristas + 4 para cerrar bucle
			lr.loop = false;
		}else
		{
			lr.material = material; // o cualquier material de debug
			lr.widthMultiplier = 0.05f;
			lr.positionCount = 16; // 12 aristas + 4 para cerrar bucle
			lr.loop = false;
		}
		
		Vector3 origin = chunk.transform.position;
		Vector3 size = new Vector3(chunkSize.x, chunkSize.y, chunkSize.z);

		Vector3[] corners = new Vector3[8];
		corners[0] = origin;
		corners[1] = origin + new Vector3(size.x, 0, 0);
		corners[2] = origin + new Vector3(size.x, size.y, 0);
		corners[3] = origin + new Vector3(0, size.y, 0);
		corners[4] = origin + new Vector3(0, 0, size.z);
		corners[5] = origin + new Vector3(size.x, 0, size.z);
		corners[6] = origin + new Vector3(size.x, size.y, size.z);
		corners[7] = origin + new Vector3(0, size.y, size.z);

		Vector3[] positions = new Vector3[16]
		{
		corners[0], corners[1], corners[2], corners[3], corners[0], // base
        corners[4], corners[5], corners[6], corners[7], corners[4], // top
        corners[7], corners[3], corners[6], corners[2], corners[5], corners[1] // verticales
		};

		lr.SetPositions(positions);
	}


	private void Update()
	{


	}


}

