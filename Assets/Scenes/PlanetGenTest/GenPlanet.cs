using ActualUtils;
using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetGen : MonoBehaviour
{
	[Header("Datos del planeta")]
	public PlanetData planetData;
	public static PlanetGen ActiveIns;

	// Diccionario para guardar los datos de los chunks y poder editarlos después
	// Si lo rellenaste por fuera antes de Start(), respetamos esos valores y no los limpiamos.
	public Dictionary<Vector3Int, AbstractGridPoint[,,]> WorldData = new Dictionary<Vector3Int, AbstractGridPoint[,,]>();

	// Lista de chunks activos (para inspección / iteración)
	public List<GameObject> Chuncks = new List<GameObject>();

	// Pool para reciclar GameObjects de chunks y evitar crear/destruir como si no hubiera mañana
	private Stack<GameObject> chunkPool = new Stack<GameObject>();

	[Header("Opciones de generación")]
	public bool useAbstract = true; // true = array (AbstractGridPoint), false = MonoBehaviour
	public Vector3Int chunkSize = new Vector3Int(16, 16, 16);
	public Vector3Int cubeSize = new Vector3Int(4, 4, 4);
	public float SurfaceLevel = 0.5f;
	public Material material;
	public byte Batches = 0x0F;
	[Header("Opciones de ruido")]
	public float noiseScale = 0.1f;
	public float noiseAmplitude = 5f;
	public float ExtraOffset = 0f;
	[Range(0f, 10f)]
	public float noiseIntensity = 0.5f;

	//Listas Temporales
	private List<Vector3> _tempVertices = new List<Vector3>();
	private List<int> _tempTriangles = new List<int>();
	private List<Vector2> _tempUVs = new List<Vector2>();
	private void Awake()
	{
		ActiveIns = this; // Aprovechamos para asignar la instancia
		InitializeNativeTables();
	}

	// ¡MUY IMPORTANTE! Si no haces esto, Unity explotará al cerrar el editor
	private void OnDestroy()
	{
		if (nativeEdgeTable.IsCreated) nativeEdgeTable.Dispose();
		if (nativeTriTable.IsCreated) nativeTriTable.Dispose();
	}
	private void Start()
	{
		if (planetData == null)
		{
			Debug.LogWarning("PlanetData no asignado, usando radio 1. (Porque la vida es así de generosa).");
			planetData = new PlanetData { radius = 1f, Seed = UnityEngine.Random.Range(-0xAAAA, 0xAAAA+0x1) };
		}

		float radius = planetData.radius * 100f;

		ActiveIns = this;

		if (useAbstract)
		{
			// Si WorldData ya viene preparado (tu caso: lo preparaste antes), no lo limpiamos.
			// Si está vacío, hacemos la generación completa y rellenamos WorldData.
			if (WorldData.Count == 0)
			{
				Debug.Log("WorldData vacío: generando planeta por primera vez y rellenando diccionario. (Momento LENTO)."); //decia momento epico pero No me gusto que chat GPT puseria eso asi que yo le puse Momento Lento por que es lentop 
				StartCoroutine(BuildPlanetAbstract(radius)); // esto rellenará WorldData
			}
			else
			{
				Debug.Log("WorldData ya existente: reutilizando datos y reconstruyendo mallas (regenerar malla, no el planeta entero).");
				RegenerateMeshesFromWorldData(radius);
			}

			foreach (Transform chunk in transform)
			{
				DrawChunkWireframe(chunk.gameObject); // se dibuja el wireframe de los chunks para verificar que está correcto.
			}
		}
		else
		{
			Debug.Log("YAn o existe el modo fisdico");
		}
	}

	// ------------------------------
	// Helpers y nombres
	// ------------------------------
	private string GetChunkName(Vector3Int idx) => $"Chunk_{idx.x}_{idx.y}_{idx.z}";

	private GameObject CreateNewChunkObject(string name, Vector3 position)
	{
		GameObject chunkObj;
		if (chunkPool.Count > 0)
		{
			// Reutilizamos un objeto inactivo del pool
			chunkObj = chunkPool.Pop();
			chunkObj.name = name;
			chunkObj.transform.parent = transform;
			chunkObj.transform.position = position;
			chunkObj.SetActive(true);
			// limpiamos componentes pesados (mesh) por seguridad
			var mf = chunkObj.GetComponent<MeshFilter>();
			if (mf != null) mf.sharedMesh = null;
			var mr = chunkObj.GetComponent<MeshRenderer>();
			if (mr != null) mr.sharedMaterial = material;
		}
		else
		{
			chunkObj = new GameObject(name);
			chunkObj.transform.parent = transform;
			chunkObj.transform.position = position;
			chunkObj.AddComponent<LineRenderer>();
		}

		// aseguramos que la lista de chunks lo contenga
		if (!Chuncks.Contains(chunkObj)) Chuncks.Add(chunkObj);

		return chunkObj;
	}

	private void PoolOrDestroyChunk(GameObject chunk)
	{
		// En lugar de destruir, lo desactivamos y lo metemos al pool para reciclarlo.
		// También limpiamos el mesh para liberar memoria.
		if (chunk == null) return;
		var mf = chunk.GetComponent<MeshFilter>();
		if (mf != null) mf.sharedMesh = null;
		chunk.SetActive(false);
		if (Chuncks.Contains(chunk)) Chuncks.Remove(chunk);
		chunkPool.Push(chunk);
	}

	// Utility para destruir o limpiar componentes según sea necesario (evita duplicar MeshFilter)
	private void EnsureMeshComponentsReplaced(GameObject parent, Mesh mesh)
	{
		// MeshFilter
		MeshFilter mf = parent.GetComponent<MeshFilter>();
		if (mf == null) mf = parent.AddComponent<MeshFilter>();
		mf.sharedMesh = mesh;

		// MeshRenderer
		MeshRenderer mr = parent.GetComponent<MeshRenderer>();
		if (mr == null) mr = parent.AddComponent<MeshRenderer>();
		mr.sharedMaterial = material;
	}

	#region Abstract Mode
	// ------------------------------
	// Abstract Mode (revisado)
	// ------------------------------
	private IEnumerator BuildPlanetAbstract(float radius)
	{
		// Rellenamos WORLD DATA en esta primera generación.
		// Si ya tenías datos en WorldData (preparados por fuera), no tocamos esa parte.
		float maxRadius = radius + noiseAmplitude;
		Vector3 planetCenter = Vector3.zero; // El planeta MANDATORIAMENTE en 0,0,0

		int chunksX = Mathf.CeilToInt((maxRadius * 2) / chunkSize.x);
		int chunksY = Mathf.CeilToInt((maxRadius * 2) / chunkSize.y);
		int chunksZ = Mathf.CeilToInt((maxRadius * 2) / chunkSize.z);

		Vector3 startOffset = new Vector3(
			chunksX * chunkSize.x / 2f,
			chunksY * chunkSize.y / 2f,
			chunksZ * chunkSize.z / 2f
		);

		// Si es la primera vez, es buena idea limpiar hijos antiguos (si existen).
		// Si WorldData ya venía relleno por ti, NO lo limpiamos arriba en Start().
		if (WorldData.Count == 0)
		{
			// destruir o poolear hijos previos (por si el objeto tenía restos)
			List<GameObject> childrenToPool = new List<GameObject>();
			foreach (Transform t in transform) childrenToPool.Add(t.gameObject);
			foreach (var c in childrenToPool) PoolOrDestroyChunk(c);
			Chuncks.Clear();
		}
		short bt = 0;
		for (int x = 0; x < chunksX; x++)
		{
			for (int y = 0; y < chunksY; y++)
			{
				for (int z = 0; z < chunksZ; z++)
				{
					Vector3 chunkOrigin = new Vector3(
						x * chunkSize.x,
						y * chunkSize.y,
						z * chunkSize.z
					) - startOffset;

					// --- OPTIMIZACIÓN: CULLING ---
					Vector3 chunkCenter = chunkOrigin + (Vector3)chunkSize / 2f;
					float distToPlanet = Vector3.Distance(chunkCenter, planetCenter);

					float margin = chunkSize.magnitude;
					if (distToPlanet > maxRadius + margin || distToPlanet < (radius - noiseAmplitude) - margin)
					{
						continue;
					}
					// -----------------------------

					BuildChunkArray(new Vector3Int(x, y, z), chunkOrigin, radius, planetCenter);
					if (bt >= Batches)
					{
						bt = 0;
						yield return null; //esperamos al proximo frame
					}
					else bt++;
				}
			}
		}
	}

	private void BuildChunkArray(Vector3Int chunkIndex, Vector3 chunkOrigin, float radius, Vector3 planetCenter)
	{
		string name = GetChunkName(chunkIndex);
		GameObject chunkObj = CreateNewChunkObject(name, chunkOrigin);

		int pointCount = (chunkSize.x + 1) * (chunkSize.y + 1) * (chunkSize.z + 1);
		int cellCount = chunkSize.x * chunkSize.y * chunkSize.z;

		// Usamos Persistent porque las corrutinas pueden durar más de 4 frames
		NativeArray<AbstractGridPointbit> gridPoints = new NativeArray<AbstractGridPointbit>(pointCount, Allocator.Persistent);
		NativeList<float3> outVerts = new NativeList<float3>(cellCount * 15, Allocator.Persistent);
		NativeList<float3> outNorms = new NativeList<float3>(cellCount * 15, Allocator.Persistent);

		// 1. Job de Puntos
		var pointsJob = new GeneratePointsJob
		{
			chunkSize = new int3(chunkSize.x, chunkSize.y, chunkSize.z),
			chunkOrigin = chunkOrigin,
			planetCenter = planetCenter,
			radius = radius,
			noiseScale = noiseScale,
			noiseAmplitude = noiseAmplitude,
			noiseIntensity = noiseIntensity,
			seed = planetData.Seed,
			gridPoints = gridPoints
		};

		// 2. Job de Malla
		var meshJob = new GenerateMeshJob
		{
			chunkSize = new int3(chunkSize.x, chunkSize.y, chunkSize.z),
			surfaceLevel = SurfaceLevel,
			gridPoints = gridPoints,
			edgeTableJ = nativeEdgeTable,
			triTableJ = nativeTriTable,
			outputVertices = outVerts.AsParallelWriter(),
			outputNormals = outNorms.AsParallelWriter()
		};

		JobHandle handle = meshJob.Schedule(cellCount, 32, pointsJob.Schedule(pointCount, 64));
		handle.Complete();

		// 3. WorldData (Opcional - solo si necesitas editarlo luego)
		if (!WorldData.ContainsKey(chunkIndex))
		{
			WorldData[chunkIndex] = ConvertNativeToAbstract(gridPoints);
		}

		// 4. Aplicar Malla
		UpdateMesh(chunkObj, outVerts, outNorms);

		// 5. Limpieza total
		gridPoints.Dispose();
		outVerts.Dispose();
		outNorms.Dispose();
	}
	private AbstractGridPoint[,,] ConvertNativeToAbstract(NativeArray<AbstractGridPointbit> nativePoints)
	{
		// 1. Crear el array de clases con las dimensiones del chunk
		// Usamos +1 porque el grid de puntos siempre es un paso más grande que el de celdas
		int resX = chunkSize.x + 1;
		int resY = chunkSize.y + 1;
		int resZ = chunkSize.z + 1;
		AbstractGridPoint[,,] grid = new AbstractGridPoint[resX, resY, resZ];

		// 2. Recorrer el NativeArray y reconstruir el array [,,]
		for (int i = 0; i < nativePoints.Length; i++)
		{
			// Extraer los datos del struct (bit)
			AbstractGridPointbit bitPoint = nativePoints[i];

			// Desglosar el índice lineal 'i' a coordenadas 3D
			// Debe coincidir EXACTAMENTE con la lógica de GeneratePointsJob
			int x = i % resX;
			int y = (i / resX) % resY;
			int z = i / (resX * resY);

			// 3. Conversión usando tu operador explícito (AbstractGridPoint)bitPoint
			grid[x, y, z] = (AbstractGridPoint)bitPoint;
		}

		return grid;
	}
	private NativeArray<int> nativeEdgeTable;
	private NativeArray<int> nativeTriTable;
	private void UpdateMesh(GameObject parent, NativeList<float3> verts, NativeList<float3> norms)
	{
		Mesh mesh = new Mesh();
		// Si el planeta es muy detallado, activamos índices de 32 bits
		if (verts.Length > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		mesh.SetVertices(verts.AsArray().Reinterpret<Vector3>());
		mesh.SetNormals(norms.AsArray().Reinterpret<Vector3>());

		// Como no estamos usando Indexing compartido, creamos un array secuencial [0, 1, 2, 3...]
		int[] indices = new int[verts.Length];
		for (int i = 0; i < indices.Length; i++) indices[i] = i;

		mesh.SetTriangles(indices, 0);
		EnsureMeshComponentsReplaced(parent, mesh);
	}

	void InitializeNativeTables()
	{
		nativeEdgeTable = new NativeArray<int>(MarchingCube.edgeTable, Allocator.Persistent);
		nativeTriTable = new NativeArray<int>(FlattenArray<int>(MarchingCube.triangleTable), Allocator.Persistent);

	}

	public static T[] FlattenArray<T>(T[,] twoDArray)
	{
		if (twoDArray == null) return new T[0]; // prevención de nulls

		int rows = twoDArray.GetLength(0);
		int cols = twoDArray.GetLength(1);
		T[] flat = new T[rows * cols];

		int index = 0;
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				flat[index++] = twoDArray[i, j];
			}
		}

		return flat;
	}
	private void BuildChunkMeshArray(AbstractGridPoint[,,] grid, GameObject parent)
	{
		_tempVertices.Clear();
		_tempTriangles.Clear();
		_tempUVs.Clear();

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
					BuildMeshCellData(ref cell, _tempVertices, _tempTriangles, _tempUVs);
				}

		Mesh mesh = new Mesh();
		mesh.vertices = _tempVertices.ToArray();
		mesh.triangles = _tempTriangles.ToArray();
		mesh.uv = _tempUVs.ToArray();
		mesh.RecalculateNormals();
		mesh.name = "ChunkMesh";

		// En lugar de añadir siempre componentes nuevos, reemplazamos/actualizamos los existentes.
		EnsureMeshComponentsReplaced(parent, mesh);
	}

	#endregion

	//-------------------------
	// BuildMeshCellData para Abstract
	// ------------------------------
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
		}
		else
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
		// vacío como la empatía de los políticos XD jajajajajaja
		// ok es broma pero de momento no se requiere nada aquí, el planeta se genera una vez al inicio y ya, no hay necesidad de actualizar nada cada frame.
	}

	// ------------------------------
	// Regeneración (solo malla)
	// ------------------------------
	[ConsoleCommand("regplt", false)] // false porque no es un truco
	public static void RegeneratePlanet()
	{
		if (ActiveIns != null)
		{
			ActiveIns.RegenerateMeshCommand();
		}
		else
		{
			Debug.LogError("No hay instancia activa de PlanetGen para regenerar.");
		}
	}

	// Método que invoca la regeneración de mallas según WorldData
	public void RegenerateMeshCommand()
	{
		// regenerar la malla (NO limpiar WorldData)
		if (WorldData == null || WorldData.Count == 0)
		{
			Debug.LogWarning("WorldData vacío — nada que regenerar (se hará generación completa en su lugar).");
			float radius = planetData.radius * 100f;
			BuildPlanetAbstract(radius);
			return;
		}

		float radiusCurrent = planetData.radius * 100f;
		RegenerateMeshesFromWorldData(radiusCurrent);

		Debug.Log("Regeneración de mallas completa. Tu planeta respira otra vez (la malla, no el planeta entero).");
	}

	// Reconstruye mallas a partir de WorldData y recicla/limpia objetos obsoletos.
	private void RegenerateMeshesFromWorldData(float radius)
	{
		float maxRadius = radius + noiseAmplitude;

		int chunksX = Mathf.CeilToInt((maxRadius * 2) / chunkSize.x);
		int chunksY = Mathf.CeilToInt((maxRadius * 2) / chunkSize.y);
		int chunksZ = Mathf.CeilToInt((maxRadius * 2) / chunkSize.z);

		Vector3 startOffset = new Vector3(
			chunksX * chunkSize.x / 2f,
			chunksY * chunkSize.y / 2f,
			chunksZ * chunkSize.z / 2f
		);

		// Marcar los nombres de chunks que necesitamos
		HashSet<string> requiredNames = new HashSet<string>();
		foreach (var kv in WorldData)
		{
			requiredNames.Add(GetChunkName(kv.Key));
		}

		// Poolear los hijos que NO estén en requiredNames (obsoletos)
		List<GameObject> toPool = new List<GameObject>();
		foreach (Transform child in transform)
		{
			if (!requiredNames.Contains(child.name))
			{
				toPool.Add(child.gameObject);
			}
		}
		foreach (var obsolete in toPool) PoolOrDestroyChunk(obsolete);

		// Para cada entry en WorldData, aseguramos un GameObject y reconstruimos su malla.
		foreach (var kv in WorldData)
		{
			Vector3Int idx = kv.Key;
			AbstractGridPoint[,,] grid = kv.Value;

			Vector3 chunkOrigin = new Vector3(
				idx.x * chunkSize.x,
				idx.y * chunkSize.y,
				idx.z * chunkSize.z
			) - startOffset;

			string name = GetChunkName(idx);
			Transform t = transform.Find(name);
			GameObject chunkObj;
			if (t != null)
			{
				chunkObj = t.gameObject;
				// si existe, dejamos su posición tal cual (por consistencia) o la reasignamos
				chunkObj.transform.position = chunkOrigin;
			}
			else
			{
				// No existe: crear (reutilizando del pool si hay)
				chunkObj = CreateNewChunkObject(name, chunkOrigin);
			}

			// Limpiar mesh viejo (si existiera) para evitar duplicados y luego regenerar
			var mf = chunkObj.GetComponent<MeshFilter>();
			if (mf != null) mf.sharedMesh = null;

			// Reconstruimos la malla con los valores en WorldData: RECUERDA: NO modificamos grid.Values aquí.
			BuildChunkMeshArray(grid, chunkObj);

			// Dibujamos wireframe si toca
			if (drawGrid)
			{
				DrawChunkWireframe(chunkObj);
			}
			else
			{
				// si no queremos wireframes, los podemos desactivar sin destruir el LR
				var lr = chunkObj.GetComponent<LineRenderer>();
				if (lr != null) lr.positionCount = 0;
			}
		}
	}

	[ConsoleCommand("togglewires", false)] // NO es un truco aunque llores y patalees 
	public static void ToggleWires()
	{
		if (ActiveIns != null)
		{
			ActiveIns.drawGrid = !ActiveIns.drawGrid;
			if (ActiveIns.drawGrid)
			{
				foreach (Transform chunk in ActiveIns.transform)
				{
					ActiveIns.DrawChunkWireframe(chunk.gameObject);
				}
			}
			else
			{
				foreach (Transform chunk in ActiveIns.transform)
				{
					LineRenderer lr = chunk.GetComponent<LineRenderer>();
					if (lr == null)
					{
						lr = chunk.gameObject.AddComponent<LineRenderer>();
						lr.material = ActiveIns.material; // o cualquier material de debug
						lr.widthMultiplier = 0.05f;
						lr.loop = false;
					}
					else
					{
						lr.positionCount = 0;
					}
				}
			}
		}
	}

	[ConsoleCommand("!resetplt", true, IsEgg = true)] //es un truco asi que el primer tru y IsEgg para que e comando de ayuda no lo muestrte
	public static void RESETPLANET()
	{
		if (ActiveIns == null)
		{
			Debug.Log("Instancia null :(");
			return;
		}

		// 1. Limpiar los datos lógicos
		ActiveIns.WorldData.Clear();

		// 2. Limpiar los objetos físicos (Los devolvemos al pool para no destruir memoria)
		// Usamos una lista temporal para evitar errores de "colección modificada"
		List<GameObject> toCleanup = new List<GameObject>(ActiveIns.Chuncks);
		foreach (GameObject chunk in toCleanup)
		{
			ActiveIns.PoolOrDestroyChunk(chunk);
		}
		ActiveIns.Chuncks.Clear();

		// 3. Reiniciar el proceso
		float radius = ActiveIns.planetData.radius * 100f;


		// ya no es un método void:
		ActiveIns.StartCoroutine(ActiveIns.BuildPlanetAbstract(radius));

		Debug.Log("¡Planeta reseteado! (Como la economía de mi país, pero este sí funciona).");
	}
	[ConsoleCommand("!loadpt", true, IsEgg = true)] //es un truco asi que el primer tru y IsEgg para que e comando de ayuda no lo muestrte
	public static void ldPT()
	{
		LoadWithLoadingScreen.LoadScene(7, Stages.Creature);
	}
	
}

public struct MeshStruct
{
	public NativeArray<Vector3> Verts;
	public NativeArray<int> Tris;
}
[BurstCompile]
public struct GeneratePointsJob : IJobParallelFor
{
	public int3 chunkSize;
	public float3 chunkOrigin;
	public float3 planetCenter;
	public float radius;
	public float noiseScale;
	public float noiseAmplitude;
	public float noiseIntensity;
	public float seed;

	public NativeArray<AbstractGridPointbit> gridPoints;

	public void Execute(int index)
	{
		// Convertir índice 1D a 3D
		int x = index % (chunkSize.x + 1);
		int y = (index / (chunkSize.x + 1)) % (chunkSize.y + 1);
		int z = index / ((chunkSize.x + 1) * (chunkSize.y + 1));

		// Dentro de Execute del GeneratePointsJob
		float3 posLocal = new float3(x, y, z);
		// Asegúrate de que chunkOrigin sea EXACTAMENTE múltiplo de chunkSize
		float3 posMundo = chunkOrigin + posLocal;

		float dist = math.distance(posMundo, planetCenter);

		// Ruido simple compatible con Burst
		float noiseVal = noise.snoise(new float4(posMundo * noiseScale, seed));
		float offset = noiseVal * noiseAmplitude * noiseIntensity;

		gridPoints[index] = new AbstractGridPointbit
		{
			Position = posLocal,
			Value = (radius + offset) - dist
		};
	}
}



[BurstCompile]
public struct GenerateMeshJob : IJobParallelFor
{
	// Datos del Grid
	[ReadOnly] public NativeArray<AbstractGridPointbit> gridPoints;
	[ReadOnly] public int3 chunkSize;
	public float surfaceLevel;

	// Tablas de Marching Cubes (Aplanadas a 1D)
	[ReadOnly] public NativeArray<int> triTableJ;
	[ReadOnly] public NativeArray<int> edgeTableJ;

	// Salida (Usa ParallelWriter para que varios hilos escriban a la vez)
	public NativeList<float3>.ParallelWriter outputVertices;
	public NativeList<float3>.ParallelWriter outputNormals;

	public void Execute(int index)
	{
		// 1. Calcular X, Y, Z de la celda actual
		int x = index % chunkSize.x;
		int y = (index / chunkSize.x) % chunkSize.y;
		int z = index / (chunkSize.x * chunkSize.y);

		// Omitir las celdas del borde para evitar salirnos del array de puntos
		if (x >= chunkSize.x || y >= chunkSize.y || z >= chunkSize.z) return;

		// 2. Obtener los 8 puntos usando TU mapeo de Metabolas
		// p0: (x, y, z+1), p1: (x+1, y, z+1), etc...
		AbstractGridCellBit cell = new AbstractGridCellBit();
		cell.p0 = GetPoint(x, y, z + 1);
		cell.p1 = GetPoint(x + 1, y, z + 1);
		cell.p2 = GetPoint(x + 1, y, z);
		cell.p3 = GetPoint(x, y, z);
		cell.p4 = GetPoint(x, y + 1, z + 1);
		cell.p5 = GetPoint(x + 1, y + 1, z + 1);
		cell.p6 = GetPoint(x + 1, y + 1, z);
		cell.p7 = GetPoint(x, y + 1, z);

		// 3. Ejecutar Lógica de IsoFaces (Portado)
		cell.config = 0;
		if (cell.p0.Value < surfaceLevel) cell.config |= 1;
		if (cell.p1.Value < surfaceLevel) cell.config |= 2;
		if (cell.p2.Value < surfaceLevel) cell.config |= 4;
		if (cell.p3.Value < surfaceLevel) cell.config |= 8;
		if (cell.p4.Value < surfaceLevel) cell.config |= 16;
		if (cell.p5.Value < surfaceLevel) cell.config |= 32;
		if (cell.p6.Value < surfaceLevel) cell.config |= 64;
		if (cell.p7.Value < surfaceLevel) cell.config |= 128;

		int edgeMask = edgeTableJ[cell.config];
		if (edgeMask == 0) return;

		// 4. Interpolación (Igual que Metabolas pero con float3)
		float3 e0 = float3.zero, e1 = float3.zero, e2 = float3.zero, e3 = float3.zero;
		float3 e4 = float3.zero, e5 = float3.zero, e6 = float3.zero, e7 = float3.zero;
		float3 e8 = float3.zero, e9 = float3.zero, e10 = float3.zero, e11 = float3.zero;

		if ((edgeMask & 1) != 0) e0 = Interpolate(cell.p0, cell.p1);
		if ((edgeMask & 2) != 0) e1 = Interpolate(cell.p1, cell.p2);
		if ((edgeMask & 4) != 0) e2 = Interpolate(cell.p2, cell.p3);
		if ((edgeMask & 8) != 0) e3 = Interpolate(cell.p3, cell.p0);
		if ((edgeMask & 16) != 0) e4 = Interpolate(cell.p4, cell.p5);
		if ((edgeMask & 32) != 0) e5 = Interpolate(cell.p5, cell.p6);
		if ((edgeMask & 64) != 0) e6 = Interpolate(cell.p6, cell.p7);
		if ((edgeMask & 128) != 0) e7 = Interpolate(cell.p7, cell.p4);
		if ((edgeMask & 256) != 0) e8 = Interpolate(cell.p0, cell.p4);
		if ((edgeMask & 512) != 0) e9 = Interpolate(cell.p1, cell.p5);
		if ((edgeMask & 1024) != 0) e10 = Interpolate(cell.p2, cell.p6);
		if ((edgeMask & 2048) != 0) e11 = Interpolate(cell.p3, cell.p7);

		// 5. Generar Triángulos
		int row = cell.config * 16;
		for (int i = 0; triTableJ[row + i] != -1; i += 3)
		{
			float3 v0 = GetEdgePoint(triTableJ[row + i], e0, e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11);
			float3 v1 = GetEdgePoint(triTableJ[row + i + 1], e0, e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11);
			float3 v2 = GetEdgePoint(triTableJ[row + i + 2], e0, e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11);

			outputVertices.AddNoResize(v0);
			outputVertices.AddNoResize(v1);
			outputVertices.AddNoResize(v2);

			// Normal por cara (Flat Shading)
			float3 normal = math.normalize(math.cross(v1 - v0, v2 - v0));
			outputNormals.AddNoResize(normal);
			outputNormals.AddNoResize(normal);
			outputNormals.AddNoResize(normal);
		}
	}

	// --- MÉTODOS AUXILIARES ---

	private AbstractGridPointbit GetPoint(int x, int y, int z)
	{
		// Importante: El mismo cálculo de índice que usaste al generar los puntos
		int idx = x + y * (chunkSize.x + 1) + z * (chunkSize.x + 1) * (chunkSize.y + 1);
		return gridPoints[idx];
	}

	private float3 Interpolate(AbstractGridPointbit v1, AbstractGridPointbit v2)
	{
		if (math.abs(surfaceLevel - v1.Value) < 0.00001f) return v1.Position;
		if (math.abs(surfaceLevel - v2.Value) < 0.00001f) return v2.Position;
		if (math.abs(v1.Value - v2.Value) < 0.00001f) return v1.Position;

		float mu = (surfaceLevel - v1.Value) / (v2.Value - v1.Value);
		return math.lerp(v1.Position, v2.Position, mu);
	}

	private float3 GetEdgePoint(int index, float3 e0, float3 e1, float3 e2, float3 e3, float3 e4, float3 e5, float3 e6, float3 e7, float3 e8, float3 e9, float3 e10, float3 e11)
	{
		switch (index)
		{
			case 0: return e0;
			case 1: return e1;
			case 2: return e2;
			case 3: return e3;
			case 4: return e4;
			case 5: return e5;
			case 6: return e6;
			case 7: return e7;
			case 8: return e8;
			case 9: return e9;
			case 10: return e10;
			case 11: return e11;
			default: return float3.zero;
		}
	}
}