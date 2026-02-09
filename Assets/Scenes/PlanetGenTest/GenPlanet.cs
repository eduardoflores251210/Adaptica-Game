using ActualUtils;
using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
// Añadir estos usings junto al resto (arriba del fichero)
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh;

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
	public Byte Batches = 0xF;
	[Header("Opciones de ruido")]
	public float noiseScale = 0.1f;
	public float noiseAmplitude = 5f;
	public float ExtraOffset = 0f;
	[Range(0f, 10f)]
	public float noiseIntensity = 0.5f;
	// Campos nuevos (añadir en la clase PlanetGen)
	private CancellationTokenSource meshCts = new CancellationTokenSource();
	private readonly ConcurrentBag<Task> backgroundTasks = new ConcurrentBag<Task>();
	//Listas Temporales
	private List<Vector3> _tempVertices = new List<Vector3>();
	private List<int> _tempTriangles = new List<int>();
	private List<Vector2> _tempUVs = new List<Vector2>();

	private void Start()
	{
		if (planetData == null)
		{
			Debug.LogWarning("PlanetData no asignado, usando radio 1. (Porque la vida es así de generosa).");
			planetData = new PlanetData { radius = 1f, Seed = UnityEngine.Random.Range(-0xAAAA, 0xAAAA + 0x1) };
		}

		float radius = planetData.radius * 100f;

		ActiveIns = this;

		if (useAbstract)
		{
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
			Debug.LogWarning("Modo físico en uso, puede ser ineficiente. además está deprecado/obsoleto");
			BuildPlanetPhysical(radius);
			Debug.LogWarning("Se generó el planeta ya puedes descansar pues la parte pesada ya pasó, ahora solo queda dibujar el wireframe de los chunks para verificar que está correcto.");
			foreach (Transform chunk in transform)
			{
				DrawChunkWireframe(chunk.gameObject);
			}
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
	private void EnsureMeshComponentsReplaced(GameObject parent, UnityEngine.Mesh mesh)
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
					// 1. Calculamos posición y Culling
					Vector3 chunkOrigin = new Vector3(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z) - startOffset;
					Vector3 chunkCenter = chunkOrigin + (Vector3)chunkSize / 2f;
					float distToPlanet = Vector3.Distance(chunkCenter, planetCenter);

					float margin = chunkSize.magnitude;

					// Si el chunk se descarta, pasamos al siguiente inmediatamente
					if (distToPlanet > maxRadius + margin || distToPlanet < (radius - noiseAmplitude) - margin)
					{
						continue;
					}

					// 2. Si llegamos aquí, es un chunk que SÍ se va a procesar
					BuildChunkArray(new Vector3Int(x, y, z), chunkOrigin, radius, planetCenter);

					// 3. Control de batches (PAUSA)
					bt++;
					if (bt >= Batches) 
					{
						bt = 0;
						yield return null;
					}
				}
			}
		}
		Debug.Log("Generación terminada.");
	}

	// Reemplaza tu método BuildChunkArray por esta versión que SCHEDULEA la generación de malla en background
	private void BuildChunkArray(Vector3Int chunkIndex, Vector3 chunkOrigin, float radius, Vector3 planetCenter)
	{
		var to = System.Diagnostics.Stopwatch.StartNew();
		string name = GetChunkName(chunkIndex);
		GameObject chunkObj = CreateNewChunkObject(name, chunkOrigin);

		// Aseguramos que tenga LineRenderer (para dibujar wireframe)
		LineRenderer lr = chunkObj.GetComponent<LineRenderer>();
		if (lr == null) lr = chunkObj.AddComponent<LineRenderer>();

		// 1) Generar grid lógico en main thread (como hacías)
		AbstractGridPoint[,,] grid = new AbstractGridPoint[chunkSize.x + 1, chunkSize.y + 1, chunkSize.z + 1];

		for (int z = 0; z <= chunkSize.z; z++)
			for (int y = 0; y <= chunkSize.y; y++)
				for (int x = 0; x <= chunkSize.x; x++)
				{
					Vector3 posLocal = new Vector3(x, y, z);
					Vector3 posMundo = chunkOrigin + posLocal;
					float dist = Vector3.Distance(posMundo, planetCenter);

					float offset = Noise4D(
						new Vector4(posMundo.x, posMundo.y, posMundo.z, planetData.Seed),
						noiseScale,
						noiseAmplitude,
						noiseIntensity
					);

					grid[x, y, z] = new AbstractGridPoint
					{
						Position = posLocal,
						Value = (radius + offset) - dist
					};
				}

		// Guardar WorldData (si corresponde)
		if (!WorldData.ContainsKey(chunkIndex))
		{
			WorldData[chunkIndex] = grid;
		}

		// 2) Lanzar tarea en background para construir el StandartUtilities mesh
		var gridCopy = grid; // referencia inmutable mientras no modifiques grid después
		var chunkName = name;
		var origin = chunkOrigin;
		// Reemplaza la llamada a Task.Run(...) por esta versión (dentro de BuildChunkArray)
		var token = meshCts.Token;
		var task = Task.Run(() =>
		{
			// Comprueba al inicio
			token.ThrowIfCancellationRequested();

			// Genera un mesh serializable puro (sin tocar Unity API)
			var serialMesh = GenerateSerializableMesh(gridCopy);

			// Comprobar cancelación antes de encolar
			if (token.IsCancellationRequested) return;

			// Encolar resultado para procesar en main thread
			meshResults.Enqueue((chunkIndex, serialMesh, origin, chunkName));
		}, token);

		// Guardar referencia al Task (opcional, para esperar/inspección)
		backgroundTasks.Add(task);

		to.Stop();
		Debug.Log($"Queued mesh generation for {chunkIndex}. (Init time MS:{to.ElapsedMilliseconds})");
	}
	// Método que GENERA la malla como StandartUtilities.StdUtils.Serializable.Mesh (se ejecuta en background).
	// IMPORTANTE: no usar Unity API dentro de este método.
	private Mesh GenerateSerializableMesh(AbstractGridPoint[,,] grid)
	{
		// Listas locales puras
		var verts = new List<Vector3>();
		var tris = new List<int>();
		var uv = new List<Vector2>(); // si quieres UVs, pero el Serial Mesh no usa UVs en su clase actual

		int gx = grid.GetLength(0) - 1;
		int gy = grid.GetLength(1) - 1;
		int gz = grid.GetLength(2) - 1;

		AbstractGridCell cell = new AbstractGridCell();

		for (int z = 0; z < gz; z++)
		{
			for (int y = 0; y < gy; y++)
			{
				for (int x = 0; x < gx; x++)
				{
					// Asignar los 8 puntos de la celda
					cell.p[0] = grid[x, y, z + 1];
					cell.p[1] = grid[x + 1, y, z + 1];
					cell.p[2] = grid[x + 1, y, z];
					cell.p[3] = grid[x, y, z];
					cell.p[4] = grid[x, y + 1, z + 1];
					cell.p[5] = grid[x + 1, y + 1, z + 1];
					cell.p[6] = grid[x + 1, y + 1, z];
					cell.p[7] = grid[x, y + 1, z];

					// Calcula triángulos (usa tu implementación pura)
					MarchingCube.IsoFaces(ref cell, SurfaceLevel);

					// Si no hay triángulos, saltar
					if (cell.numtriangles <= 0) continue;

					// Añadir cada triángulo: copiamos vértices (tu pipeline actual duplica vértices por cara)
					for (int t = 0; t < cell.numtriangles; t++)
					{
						// cada cell.triangle[t].p[0..2] son Vector3
						verts.Add(cell.triangle[t].p[0]);
						verts.Add(cell.triangle[t].p[1]);
						verts.Add(cell.triangle[t].p[2]);

						// índices secuenciales
						int baseIdx = verts.Count - 3;
						tris.Add(baseIdx);
						tris.Add(baseIdx + 1);
						tris.Add(baseIdx + 2);
					}
				}
			}
		}

		// Convertir tris int-list a lista de TRIANGLE que espera tu serial Mesh
		var triObjs = new List<StandartUtilities.StdUtils.Serializable.TRIANGLE>(tris.Count / 3);
		for (int i = 0; i + 2 < tris.Count; i += 3)
		{
			triObjs.Add(new StandartUtilities.StdUtils.Serializable.TRIANGLE(tris[i], tris[i + 1], tris[i + 2]));
		}

		// Construir el Mesh serializable
		var serialMesh = new StandartUtilities.StdUtils.Serializable.Mesh(verts, triObjs);
		return serialMesh;
	}
	// Función auxiliar para ruido 4D (puedes ponerla al final de tu clase)
	private static float Noise4D(Vector4 Pos, float noise_scale, float noiseAmplitud, float noiseIntensity)
	{
		// La escala DEBE multiplicar a las coordenadas antes de entrar al ruido
		// Usamos Pos.w (tu semilla) como un offset que desplaza la "realidad" del ruido
		float x = Pos.x * noise_scale;
		float y = Pos.y * noise_scale;
		float z = Pos.z * noise_scale;
		float w = Pos.w; // La semilla no suele escalarse, es un offset puro

		float ab = Mathf.PerlinNoise(x + w, y);
		float bc = Mathf.PerlinNoise(y + w, z);
		float ac = Mathf.PerlinNoise(x + w, z);

		float ba = Mathf.PerlinNoise(y - w, x);
		float cb = Mathf.PerlinNoise(z - w, y);
		float ca = Mathf.PerlinNoise(z - w, x);

		// El promedio se multiplica por la amplitud y la intensidad al final
		return ((ab + bc + ac + ba + cb + ca) / 6f) * noiseAmplitud * noiseIntensity;
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

		UnityEngine.Mesh mesh = new UnityEngine.Mesh();
		mesh.vertices = _tempVertices.ToArray();
		mesh.triangles = _tempTriangles.ToArray();
		mesh.uv = _tempUVs.ToArray();
		mesh.RecalculateNormals();
		mesh.name = "ChunkMesh";

		// En lugar de añadir siempre componentes nuevos, reemplazamos/actualizamos los existentes.
		EnsureMeshComponentsReplaced(parent, mesh);
	}

	#endregion
	// Añade OnDisable/OnDestroy para cancelar y limpiar (añadir en la clase)
	private void OnDisable()
	{
		CancelBackgroundWork();
	}

	private void OnDestroy()
	{
		CancelBackgroundWork();
	}

	// Método helper
	private void CancelBackgroundWork()
	{
		// Evitar llamada repetida
		if (meshCts == null || meshCts.IsCancellationRequested) return;

		// Pedimos cancelación cooperativa
		meshCts.Cancel();

		// Opcional: intentar esperar un pequeño tiempo por las tareas para terminar ordenadamente.
		// No es obligatorio — bloquear el hilo principal mucho tiempo es indeseable.
		try
		{
			Task[] tasks = backgroundTasks.ToArray();
			if (tasks.Length > 0)
			{
				// Espera corta: p.ej. 200 ms para cerrar ordenadamente
				Task.WaitAll(tasks, 200);
			}
		}
		catch (Exception)
		{
			// ignorar: si fallan, ya se cancelaron
		}

		// Limpiar cola resultante si no quieres aplicar meshes pendientes
		while (meshResults.TryDequeue(out _)) { }
	}
	#region Pysical Mode
	// ------------------------------
	// Physical Mode (obsoleto, sin cambios mayores)
	// ------------------------------
	[Obsolete("POR favor usa el modo abstracto, es mucho más eficiente y fácil de manejar. Este modo físico es solo para referencia histórica.")]
	private void BuildPlanetPhysical(float radius)
	{
		ActiveIns = this;
		Vector3 planetCenter = new Vector3(radius, radius, radius);
		Vector3Int chunksCount = new Vector3Int(
			Mathf.CeilToInt((radius * 2 / chunkSize.x) + noiseAmplitude + ExtraOffset),
			Mathf.CeilToInt((radius * 2 / chunkSize.y) + noiseAmplitude + ExtraOffset),
			Mathf.CeilToInt((radius * 2 / chunkSize.z) + noiseAmplitude + ExtraOffset)
		);

		for (int x = 0; x < chunksCount.x; x++)
			for (int y = 0; y < chunksCount.y; y++)
				for (int z = 0; z < chunksCount.z; z++)
					BuildChunkPhysical(new Vector3Int(x, y, z), radius, planetCenter);
	}

	[Obsolete("POR favor usa el modo abstracto, es mucho más eficiente y fácil de manejar. Este modo físico es solo para referencia histórica.")]
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

	[Obsolete("POR favor usa el modo abstracto, es mucho más eficiente y fácil de manejar. Este modo físico es solo para referencia histórica.")]
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

		UnityEngine.Mesh mesh = new UnityEngine.Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uv.ToArray();
		mesh.RecalculateNormals();

		MeshFilter mf = parent.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		MeshRenderer mr = parent.AddComponent<MeshRenderer>();
		mr.material = material;
	}

	[Obsolete("POR favor usa el modo abstracto, es mucho más eficiente y fácil de manejar. Este modo físico es solo para referencia histórica.")]
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
	#endregion
	// ------------------------------
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
	// Campos de clase: (añádelos dentro de la clase PlanetGen)
	private readonly ConcurrentQueue<(Vector3Int idx, StandartUtilities.StdUtils.Serializable.Mesh serialMesh, Vector3 origin, string chunkName)> meshResults
					= new ConcurrentQueue<(Vector3Int, StandartUtilities.StdUtils.Serializable.Mesh, Vector3, string)>();

	// Cuántos meshes aplicar por frame para evitar picos de trabajo en main thread
	private int maxApplyPerFrame = 4;
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
		// ya no vacío como la empatía de los políticos XD jajajajajaja
		// ok es broma 
		int applied = 0;
		while (applied < maxApplyPerFrame && meshResults.TryDequeue(out var result))
		{
			// result.serialMesh es puro, la conversión a UnityEngine.Mesh debe hacerse en main thread.
			try
			{
				UnityEngine.Mesh unityMesh = (UnityEngine.Mesh)result.serialMesh; // usa el operador explícito que ya tienes
																				  // Busca o crea el GameObject/chunk
				Transform t = transform.Find(result.chunkName);
				GameObject chunkObj = t != null ? t.gameObject : CreateNewChunkObject(result.chunkName, result.origin);

				// Asignar mesh en main thread
				EnsureMeshComponentsReplaced(chunkObj, unityMesh);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error applying serial mesh on main thread: " + ex);
			}

			applied++;
		}
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

		ActiveIns.StartCoroutine(ActiveIns.BuildPlanetAbstract(radius)); // esto rellenará WorldData

		Debug.Log("¡Planeta reseteado! (Como la economía de mi país, pero este sí funciona).");
	}
	[ConsoleCommand("!loadpt", true, IsEgg = true)] //es un truco asi que el primer tru y IsEgg para que e comando de ayuda no lo muestrte
	public static void ldPT()
	{
		LoadWithLoadingScreen.LoadScene(7, Stages.Creature);
	}

}