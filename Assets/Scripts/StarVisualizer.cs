using aaa;
using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities.Extentions;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "UNT0022:Inefficient position/rotation assignment", Justification = "<pendiente>")]
public class StarVisualizer : MonoBehaviour
{
	[Header("Opciones de visualización")]
	public float GalaxyScale = 1f;
	public Material BasMat; // SI O SI DEBE USAR EL SHADER StarShader
	public Material BholMat; //este material brilla por que es el del disco de acrecion
	public Material OpaceMat; //este material es opaco por que planetas y enanas negras
	public Transform starParent; // Objeto padre opcional
	public GalaxyGenerator Generator;
	public bool IsDebug = false;
	Mesh Sphere = null;
	// Materiales por tipo de estrella
	public Dictionary<StarTypes, Material> Mats = new Dictionary<StarTypes, Material>();
	// ParticleSystems compartidos por tipo
	public Dictionary<StarTypes, ParticleSystem> Particles = new Dictionary<StarTypes, ParticleSystem>();
	public bool Done = false;
	[HideInInspector]
	[NonSerialized]
	public ConcurrentBag<GalaxySector> Bag;
	[HideInInspector]
	[NonSerialized]
	public List<GalaxySector> list;
	public ConcurrentBag<quequeElement> QueQue; // bolsa concurrente para pasar estrellas desde el hilo de carga a la corutina de visualización sin bloquear
	public int BatchSize = 50;
	public bool IsInMainMenu = false;
	public bool HideRouguePlanets = true;
	[Tooltip("Usado en el menu principal para cargar las estrellas antes de hacer el fundido de negro a vista normal")]
	public bool Use1FramesPerSecondMode = false; //es mas rapido pero se ve mas lento por que el juego se alenta
	[Header("Opcional")]
	public SectorTurnOnOffEr ChunckManager;

	void Start()
	{

		if (Sphere == null)
		{
			GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Sphere = tmp.GetComponent<MeshFilter>().mesh.CopyMesh();
			Destroy(tmp);
		}
		// Crear materiales por tipo
		foreach (StarTypes st in Enum.GetValues(typeof(StarTypes)))
		{
			Material material = new Material(BasMat);
			material.SetFloat("_Temp_K", StarData.Temperatures[st]);
			Mats[st] = material;
			if (st == StarTypes.X)
			{
				Mats[st] = BholMat;
			}
			else if (st == StarTypes.EN)
			{
				Mats[st] = OpaceMat;
			}

			// Crear ParticleSystem compartido para este tipo
			GameObject psGO = new GameObject($"Particles_{st}");
			psGO.transform.parent = this.transform;
			ParticleSystem ps = psGO.AddComponent<ParticleSystem>();
			var main = ps.main;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.loop = true;
			main.playOnAwake = true;
			main.startSize = 1f;
			main.startLifetime = Mathf.Infinity;
			ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
			ps.startSpeed = 0;
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
			Particles[st] = ps;

		}
		try
		{
			StartCoroutine(LookCoroutine());
		}
		catch (Exception e) { Debug.Log(e); }
	}

	public IEnumerator LookCoroutine()
	{
		Debug.Log("LOOK");
		ResetVisualizationIfNeeded();
#pragma warning disable IDE0059 // Asignación innecesaria de un valor
		GalaxyData data = null;
#pragma warning restore IDE0059 // Asignación innecesaria de un valor
		try { data = GalaxyData.LoadGalaxy(); }
		catch (Exception ) {/* Debug.Log(e);*/ yield break; }

		if (data == null) yield break;

		list = new List<GalaxySector>();
		ParralelLOAD();
		while (!isDoneLoading)
		{
			yield return null; // esperar a que el hilo de carga termine
		}
		if (list.Count == 0 && Bag.Count > 0)
		{
			list = Bag.ToList();
		}
		if (list.Count > 0 || Bag.Count > 0)
		{
			yield return StartCoroutine(NEWPARARELVisualizeStars(Bag,data));
		}
	}
	public GalaxyData galaxy = null;

	//deveria ser un poquito mas rapido
	public IEnumerator NEWPARARELVisualizeStars(ConcurrentBag<GalaxySector> sectors, GalaxyData data = null)
	{
		Debug.Log("Visuzlize");
		List<ParticleSystem.EmitParams> particleX = new();
		List<ParticleSystem.EmitParams> particleO = new();
		List<ParticleSystem.EmitParams> particleB = new();
		List<ParticleSystem.EmitParams> particleA = new();
		List<ParticleSystem.EmitParams> particleF = new();
		List<ParticleSystem.EmitParams> particleG = new();
		List<ParticleSystem.EmitParams> particleK = new();
		List<ParticleSystem.EmitParams> particleM = new();
		List<ParticleSystem.EmitParams> particleL = new();
		List<ParticleSystem.EmitParams> particleT = new();
		List<ParticleSystem.EmitParams> particleEB = new();
		List<ParticleSystem.EmitParams> particleEN = new();
		List<ParticleSystem.EmitParams> particleNS = new();
		Debug.Log("STart");

		if (starParent == null) starParent = this.transform;
		galaxy = data;

		// --- Decide si necesitaremos crear GameObjects en este run ---
		bool menuMode = IsInMainMenu && !IsDebug; // en menú y sin debug -> modo ligero (no crear GOs)
		bool needTemplates = !menuMode; // si no estamos en modo ligero, necesitamos plantillas para clonar

		// --- Templates locales (se crean UNA sola vez al inicio de la corutina si se requieren) ---
		GameObject sectorTemplate = null;
		GameObject starTemplate = null;
		GameObject planetTemplate = null;

		if (needTemplates)
		{
			// Sector template: un plane básico, sin collider para plantilla
			sectorTemplate = GameObject.CreatePrimitive(PrimitiveType.Plane);
			sectorTemplate.name = "TEMPLATE_Sector";
			var sectorRenderer = sectorTemplate.GetComponent<MeshRenderer>();
			if (sectorRenderer != null) sectorRenderer.enabled = false; // plantilla apagada
			var sectorCollider = sectorTemplate.GetComponent<Collider>();
			if (sectorCollider != null) DestroyImmediate(sectorCollider); // quitar collider en plantilla

			// Star template: GameObject con SpaceStageStar y SphereCollider, inactivo
			starTemplate = new GameObject("TEMPLATE_Star");
			var sComp = starTemplate.AddComponent<SpaceStageStar>();
			starTemplate.AddComponent<SphereCollider>();
			starTemplate.SetActive(false);

			// Planet template: GameObject con SpaceStageRouguePlanet y SphereCollider, inactivo
			planetTemplate = new GameObject("TEMPLATE_Planet");
			planetTemplate.AddComponent<SpaceStageRouguePlanet>();
			planetTemplate.AddComponent<SphereCollider>();
			planetTemplate.SetActive(false);
		}

		// --- Precontar estrellas por tipo para reservar listas (reduce realocaciones) ---
		var typeCounts = new Dictionary<StarTypes, int>();
		foreach (StarTypes t in System.Enum.GetValues(typeof(StarTypes))) typeCounts[t] = 0;
		int totalStars = 0;
		foreach (var s in sectors)
		{
			if (s?.Stars == null) continue;
			foreach (var st in s.Stars)
			{
				if (st == null || st.IsNull()) continue;
				totalStars++;
				if (typeCounts.ContainsKey(st.type)) typeCounts[st.type]++; else typeCounts[st.type] = 1;
			}
		}

		// Reservar capacidad basada en conteos
		particleX = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.X, 0));
		particleO = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.O, 0));
		particleB = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.B, 0));
		particleA = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.A, 0));
		particleF = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.F, 0));
		particleG = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.G, 0));
		particleK = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.K, 0));
		particleM = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.M, 0));
		particleL = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.L, 0));
		particleT = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.T, 0));
		particleEB = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.EB, 0));
		particleEN = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.EN, 0));
		particleNS = new List<ParticleSystem.EmitParams>(typeCounts.GetValueOrDefault(StarTypes.NS, 0));

		// --- Opciones de chunking/batching ---
		int Batch = 0;
		int emitChunk = 1024; // ajustar según memoria/frametime objetivo
		Debug.Log("STAR Counted " + totalStars.ToString());

		foreach (var sector in sectors)
		{
			if (sector == null) continue;

			GameObject SectorGO = null;
			if (needTemplates)
			{
				// Clonar plantilla sector (más barato que CreatePrimitive por sector repetido)
				SectorGO = Instantiate(sectorTemplate);
				SectorGO.name = $"Sector_{sector.Position}";
				SectorGO.transform.position = (((Vector3)sector.Position.To3DXZ()).Multiply3d(((Vector2)Generator.sectorSize).To3DXZ())) * GalaxyScale;
				SectorGO.transform.localScale = Vector3.one;
				SectorGO.GetComponent<MeshRenderer>().enabled = IsDebug;
				if (ChunckManager != null)
				{
					if (ChunckManager.Sectors == null) ChunckManager.Sectors = new List<GameObject>();
					ChunckManager.Sectors.Add(SectorGO);
				}
			}
			SpawnedSector = false;
			ParralelStarSpawn(sector);
			while (!SpawnedSector)
			{
				if (QueQue == null) yield return null; // esperar a que el hilo de carga inicialice la fila
				if (QueQue.Count == 0) yield return null; // esperar a que el hilo de carga ponga elementos en la fila
				int i = 0;
				while (!QueQue.IsEmpty)
				{
					

					// En lugar de foreach + Clear, usamos un while que extrae
					// Esto garantiza que no borramos estrellas que acaban de entrar
					while (QueQue.TryTake(out quequeElement element))
					{
						var star = element.star;
						if (star == null || star.IsNull()) continue;

						// 1. Instanciar (Tu lógica de templates)
						if (needTemplates)
						{
							GameObject starGO = Instantiate(starTemplate);
							starGO.name = star.id;
							starGO.transform.position = star.transform.Pos * GalaxyScale;
							starGO.transform.parent = SectorGO != null ? SectorGO.transform : starParent;

							var sps = starGO.GetComponent<SpaceStageStar>();
							if (sps != null)
							{
								sps.ID = star.id;
								sps.Type = star.type;
							}
							starGO.SetActive(true);
						}

						// 2. Partículas
						switch (star.type)
						{
							case StarTypes.O: particleO.Add(element.emit); break;
							case StarTypes.B: particleB.Add(element.emit); break;
							case StarTypes.A: particleA.Add(element.emit); break;
							case StarTypes.F: particleF.Add(element.emit); break;
							case StarTypes.G: particleG.Add( element.emit); break; //usamos directamente el emit que viene del hilo de carga para evitar crear uno nuevo en cada iteracion
							case StarTypes.K: particleK.Add( element.emit); break;
							case StarTypes.M: particleM.Add( element.emit); break;
							case StarTypes.L: particleL.Add( element.emit); break;
							case StarTypes.T: particleT.Add(element.emit); break;
							case StarTypes.EB: particleEB.Add( element.emit); break;
							case StarTypes.NS: particleNS.Add( element.emit); break;
							case StarTypes.EN: particleEN.Add(element.emit); break;
							case StarTypes.X:
							default: particleX.Add(element.emit); break;
						}

						bool T = false;
						if (Use1FramesPerSecondMode)
						{
							// mantengo tu lógica original de throttling temporal si está activada
							// (aquí no usamos stopwatch global para no agregar overhead por iteración)
						}

						Batch++;
						if ((Batch >= BatchSize && !Use1FramesPerSecondMode) || T)
						{
							Batch = 0;
							yield return null;
						}
						i++;
					}
					yield return null;
				}


				




			}
			yield return null;

			if (!HideRouguePlanets)
			{
				// Esta llamada era marcada como costosa; la dejamos sólo si no estamos en modo menú ligero
				if (!menuMode)
				{
					GalObjCollection collection = data.GetRougueStuffInThisSector(sector.Position);

					if (collection != null && collection.planets != null && collection.planets.Count != 0)
					{
						foreach (var planet in collection.planets)
						{
							if (!IsInMainMenu)
							{
								GameObject PlanetGO = Instantiate(planetTemplate);
								PlanetGO.name = planet.id;
								PlanetGO.transform.position = planet.transform.Pos * GalaxyScale;
								PlanetGO.transform.rotation = Quaternion.Euler(planet.transform.Rot);
								PlanetGO.transform.parent = SectorGO != null ? SectorGO.transform : starParent;
								PlanetGO.SetActive(true);
								var SPPDATA = PlanetGO.GetComponent<SpaceStageRouguePlanet>();
								if (SPPDATA != null)
								{
									SPPDATA.ID = planet.id;
									SPPDATA.BinTransform = planet.transform;
									SPPDATA.Type = planet.type;
								}
							}

							if (Particles.ContainsKey(StarTypes.EN))
							{
								ParticleSystem.EmitParams emit = new();
								emit.position = planet.transform.Pos * GalaxyScale;
								particleEN.Add(emit);
							}

							Batch++;
							if ((Batch >= BatchSize) && !Use1FramesPerSecondMode)
							{
								Batch = 0;
								yield return null;
							}
						}
					}
				}
			}

			if ((IsDebug || !IsInMainMenu) && SectorGO != null)
			{
				SectorGO.transform.parent = starParent;
			}
			if (ChunckManager != null && SectorGO != null)
			{
				SectorGO.SetActive(false);
			}

			yield return null;
		} // end foreach sector

		Debug.Log("STAR LOADED");

		// --- Emitir partículas por tipo en chunks con yields ---
		foreach (var psKvp in Particles)
		{
			var ps = psKvp.Value;
			ps.Pause();
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (Mats != null && Mats.ContainsKey(psKvp.Key)) renderer.material = Mats[psKvp.Key];

			List<ParticleSystem.EmitParams> L = psKvp.Key switch
			{
				StarTypes.X => particleX,
				StarTypes.O => particleO,
				StarTypes.B => particleB,
				StarTypes.A => particleA,
				StarTypes.F => particleF,
				StarTypes.G => particleG,
				StarTypes.K => particleK,
				StarTypes.M => particleM,
				StarTypes.L => particleL,
				StarTypes.T => particleT,
				StarTypes.EB => particleEB,
				StarTypes.NS => particleNS,
				StarTypes.EN => particleEN,
				_ => new List<ParticleSystem.EmitParams>(),
			};

			// Emitir en chunks para no bloquear un frame entero
			for (int i = 0; i < L.Count; i += emitChunk)
			{
				int c = Mathf.Min(emitChunk, L.Count - i);
				for (int j = 0; j < c; j++)
				{
					ps.Emit(L[i + j], 1);
				}
				yield return null;
			}

			if (psKvp.Key == StarTypes.X)
			{
				renderer.material = BholMat;
			}
		}

		Done = true;
		Debug.Log("STAR SPAWNED");

		// --- limpiar plantillas locales para no dejar basura en escena ---
		if (sectorTemplate != null) Destroy(sectorTemplate);
		if (starTemplate != null) Destroy(starTemplate);
		if (planetTemplate != null) Destroy(planetTemplate);

		yield break;
	}
	bool isDoneLoading = false;
	async void ParralelLOAD()
	{
		_cts = new CancellationTokenSource(); // Inicializar antes de empezar
		var token = _cts.Token;
		if (Bag == null)
		{
			Bag = new();
		}
		try
		{
			await System.Threading.Tasks.Task.Run(() =>
			{
				GalaxyData data = GalaxyData.LoadGalaxy();
				if (data == null) return;

				Parallel.ForEach(data.SectorPositions, (st) => {
					GalaxyData.LoadSector(st, out var Sec);
					if (Sec != null) Bag.Add(Sec); // 'list' debe ser ConcurrentBag
				});
				isDoneLoading = true;
			}, token);
		}
		catch (OperationCanceledException) { /* Tarea cancelada con éxito */ }
	}
	bool SpawnedSector= false;
	private CancellationTokenSource _cts;
	async void ParralelStarSpawn(GalaxySector sector)
	{
		var token = _cts.Token;
		if (QueQue == null) QueQue = new();

		try
		{
			await System.Threading.Tasks.Task.Run(() =>
			{
				if (sector == null)
				{
					throw new NullReferenceException("SECTOR IS NULL");
				}
				// 1. Validar que el sector y su lista de estrellas existan
				if (sector == null || sector.Stars == null) return;
				//y si hago esto un parralel for each? no se si es buena idea pero podria ser divertido y mas rapido, aunque no se si el overhead de crear tareas por cada estrella lo haria mas lento, pero bueno, podria ser divertido intentarlo
				/*
				foreach (var star in sector.Stars)
				{
					if (token.IsCancellationRequested) return;

					// 2. ¡AQUÍ ESTÁ EL TRUCO! 
					// Primero checamos si la referencia 'star' es nula.
					// Si es nula, el "||" hace que pase a la siguiente sin ejecutar IsNull()
					if (star == null || star.IsNull()) continue;

					ParticleSystem.EmitParams emit = new();
					emit.position = star.transform.Pos * GalaxyScale;

					QueQue.Add(new quequeElement { star = star, emit = emit });
				}
				*/
				Parallel.ForEach(sector.Stars, (star) =>
				{
					if (token.IsCancellationRequested) return;
					if (star == null || star.IsNull()) return;
					ParticleSystem.EmitParams emit = new();
					emit.position = star.transform.Pos * GalaxyScale;
					QueQue.Add(new quequeElement { star = star, emit = emit });
				});
				SpawnedSector = true;
			}, token);
		}
		catch (Exception e) { Debug.LogError(e); }
	}
	private void OnDisable()
	{
		if (_cts != null)
		{
			_cts.Cancel(); // Notifica a todas las tareas que deben parar
			_cts.Dispose();
			_cts = null;
		}

		// Opcional: Detener corrutinas si quieres ser extra seguro
		StopAllCoroutines();
	}
	public struct quequeElement
	{
		public StarData star;
		public ParticleSystem.EmitParams emit;
	}
	public void ResetVisualizationIfNeeded()
	{
		if (!Done) return;

		// Detener corrutinas activas
		StopAllCoroutines();

		// Cancelar tareas async
		if (_cts != null)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = null;
		}

		// Limpiar estructuras de datos
		Bag = new ConcurrentBag<GalaxySector>();
		list = new List<GalaxySector>();
		QueQue = new ConcurrentBag<quequeElement>();

		// Reset flags
		isDoneLoading = false;
		SpawnedSector = false;
		Done = false;

		// Limpiar partículas
		foreach (var ps in Particles.Values)
		{
			if (ps == null) continue;
			ps.Clear(true);
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}

		// Destruir hijos visuales (estrellas, sectores, etc.)
		if (starParent != null)
		{
			for (int i = starParent.childCount - 1; i >= 0; i--)
			{
				Destroy(starParent.GetChild(i).gameObject);
			}
		}

		Debug.Log("StarVisualizer reset: listo para visualizar otra vez");
	}
}



// Namespace con nombre raro
namespace aaa
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Convertir el miembro en 'readonly'", Justification = "<pendiente>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Estilos de nombres", Justification = "<pendiente>")]
	public static class aaaa
	{
		static double aaaaaaaaaaaaaaaaaaa = 3.14; // No no lo uso 
		public static Vector3 To3DXZ(this Vector2 a)
		{
			return new Vector3(a.x, 0, a.y); //float 
		}
		public static Vector3 To3DXY(this Vector2 a)
		{
#pragma warning disable UNT0035 // A Vector3 can be converted into a Vector2.
			return new Vector3(a.x, a.y, 0);//float                     //DEVO DE SER CONSSITENTE UNITY
#pragma warning restore UNT0035 // A Vector3 can be converted into a Vector2.
		}
		public static Vector3Int To3DXZ(this Vector2Int a)
		{
			return new Vector3Int(a.x, 0, a.y); //int 
		}
		public static Vector3Int To3DXY(this Vector2Int a)
		{
			return new Vector3Int(a.x, a.y, 0);//int
		}
		/// <summary>
		/// Escala la malla directamente modificando sus vértices.
		/// Esto NO requiere cambiar el Transform del GameObject.
		/// </summary>
		/// <param name="mesh">La malla a escalar.</param>
		/// <param name="scale">Vector de escala por eje.</param>
		public static Mesh ScaleMesh(this Mesh mesa, Vector3 scale) //ORIGINALMENTE SE LLAMABA Masha pero decidi que no era buena idea referenciar a la niña que molesta al pobre de oso (PD si MASHA y EL OSO)
		{
			if (mesa == null) throw new Exception("W");

			Vector3[] verts = mesa.vertices;

			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = Vector3.Scale(verts[i], scale);
			}
			Mesh mesh = new Mesh();
			mesh.vertices = verts;
			mesh.triangles = mesa.triangles;
			mesa.uv = mesh.uv;
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			return mesh;
		}
		public static Mesh CopyMesh(this Mesh mesh) //Ctrl C, Ctrl V
		{
			var colors = mesh.colors;
			var colors32 = mesh.colors32;
			var tris = mesh.triangles;
			var vers = mesh.vertices;
			var uvs = mesh.uv;
			Mesh Mesh2 = new Mesh();
			Mesh2.vertices = vers; Mesh2.triangles = tris;
			Mesh2.uv = uvs;
			Mesh2.colors = colors;
			Mesh2.colors32 = colors32;
			Mesh2.RecalculateBounds();
			Mesh2.RecalculateNormals();
			Mesh2.RecalculateTangents();
			return Mesh2;
		}
	}
}
