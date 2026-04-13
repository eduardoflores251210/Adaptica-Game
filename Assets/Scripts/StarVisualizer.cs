using aaa;
using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities.Extentions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Debug = UnityEngine.Debug;
using EP = UnityEngine.ParticleSystem.EmitParams;
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

	public ConcurrentBag<quequeElement> QueQue; // bolsa concurrente para pasar estrellas desde el hilo de carga a la corutina de visualización sin bloquear
	public int Batch = 0;
	public int QueQUeLength = 0;
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
			main.maxParticles = int.MaxValue;
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
		catch (Exception e) { Debug.Log(e); yield break; }
		Debug.Log("Loaded ");
		if (data == null) yield break;
		galaxy = data;
		Bag = new ConcurrentBag<GalaxySector>();
		ParralelLOAD();
		while (!isDoneLoading)
		{
			yield return null; // esperar a que el hilo de carga termine
		}

		if (Bag.Count > 0)
		{
			yield return StartCoroutine(VisualizeStars(Bag, data));
		}
	}
	public GalaxyData galaxy = null;

	//deveria ser un poquito mas rapido
	public IEnumerator VisualizeStars(ConcurrentBag<GalaxySector> sectors, GalaxyData data = null)
	{
		Debug.Log("Visuzlize");
		List<EP> particleX = new();
		List<EP> particleO = new();
		List<EP> particleB = new();
		List<EP> particleA = new();
		List<EP> particleF = new();
		List<EP> particleG = new();
		List<EP> particleK = new();
		List<EP> particleM = new();
		List<EP> particleL = new();
		List<EP> particleT = new();
		List<EP> particleEB = new();
		List<EP> particleEN = new();
		List<EP> particleNS = new();
		Debug.Log("STart");
		QueQue = new();
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
		particleX = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.X, 0));
		particleO = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.O, 0));
		particleB = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.B, 0));
		particleA = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.A, 0));
		particleF = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.F, 0));
		particleG = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.G, 0));
		particleK = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.K, 0));
		particleM = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.M, 0));
		particleL = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.L, 0));
		particleT = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.T, 0));
		particleEB = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.EB, 0));
		particleEN = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.EN, 0));
		particleNS = new List<EP>(typeCounts.GetValueOrDefault(StarTypes.NS, 0));

		// --- Opciones de chunking/batching ---


		Debug.Log("STAR Counted " + totalStars.ToString());

		// 1. PRE-CREACIÓN DE SECTORES (Rápido y furioso)
		Dictionary<Vector2Int, Transform> sectorMap = new();
		if (needTemplates)
		{
			foreach (var sector in sectors)
			{
				GameObject sGO = Instantiate(sectorTemplate);
				sGO.name = $"Sector_{sector.Position}";
				sGO.transform.position = (((Vector3)sector.Position.To3DXZ()).Multiply3d(((Vector2)Generator.sectorSize).To3DXZ())) * GalaxyScale;
				sGO.transform.parent = starParent;
				sGO.GetComponent<MeshRenderer>().enabled = IsDebug;

				sectorMap[sector.Position] = sGO.transform;

				if (ChunckManager != null)
				{
					if (ChunckManager.Sectors == null) ChunckManager.Sectors = new();
					ChunckManager.Sectors.Add(sGO);
				}
			}
		}

		// 2. DISPARAR EL PROCESAMIENTO PARALELO
		SpawnedSector = false;
		ParralelMassiveSpawn(sectors, data); // La función masiva que creamos antes

		// 3. EL GRAN BUCLE DE VACIADO (Drenando la QueQue)
		Batch = 0;
		while (!SpawnedSector || !QueQue.IsEmpty)
		{
			// Mientras haya algo en la bolsa, lo sacamos sin piedad
			while (QueQue.TryTake(out quequeElement element))
			{
				Transform parent = sectorMap.ContainsKey(element.SectorPostion) ? sectorMap[element.SectorPostion] : starParent;

				// --- LÓGICA SEGÚN EL TIPO (Future Proofing) ---
				switch (element.Type)
				{
					case CelestialBodyType.Star:
						ProcessStar(element, parent, needTemplates, particleO, particleB, particleA, particleF, particleG, particleK, particleM, particleL, particleT, particleEB, particleNS, particleEN, particleX, starTemplate);
						break;

					case CelestialBodyType.Planet:
						ProcessPlanet(element, parent, needTemplates, particleEN, planetTemplate);
						break;

						// Aquí puedes meter Nebulas, Novas, etc. ¡ZAZ!
				}

				// Throttling para no matar al Nokia
				Batch++;
				if (Batch >= BatchSize && !Use1FramesPerSecondMode)
				{
					Batch = 0;
					yield return null;
				}
			}
			yield return null; // Esperar a que el hilo de carga meta más cosas
		}

		Debug.Log("¡GALAXIA COMPLETA!");

		Debug.Log("STAR LOADED");

		// --- Emitir partículas por tipo en chunks con yields ---
		foreach (var psKvp in Particles)
		{
			var ps = psKvp.Value;
			ps.Pause();
			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (Mats != null && Mats.ContainsKey(psKvp.Key)) renderer.material = Mats[psKvp.Key];

			List<EP> L = psKvp.Key switch
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
				_ => new List<EP>(),
			};

			// Emitir en chunks para no bloquear un frame entero
			Batch = 0;
			foreach (var l in L)
			{
				ps.Emit(l,1);
				Batch++;
				if (Batch >= BatchSize && !Use1FramesPerSecondMode)
				{
					Batch = 0;
					yield return null;
				}
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
	// --- Procesador de Estrellas ---
	private void ProcessStar(quequeElement el, Transform parent, bool needTemplates,
		List<EP> pO, List<EP> pB, List<EP> pA, List<EP> pF, List<EP> pG,
		List<EP> pK, List<EP> pM, List<EP> pL, List<EP> pT, List<EP> pEB,
		List<EP> pNS, List<EP> pEN, List<EP> pX, GameObject starTemplate)
	{
		var star = el.Star;
		if (star == null || star.IsNull()) return;

		// 1. Instanciar GameObject si es necesario
		if (needTemplates && star.ParentID[0] == 'S')
		{
			// Nota: starTemplate debe ser accesible aquí
			GameObject starGO = Instantiate(starTemplate);
			starGO.name = star.id;
			starGO.transform.position = star.transform.Pos * GalaxyScale;
			starGO.transform.parent = parent;

			var sps = starGO.GetComponent<SpaceStageStar>();
			if (sps != null) { sps.ID = star.id; sps.Type = star.type; sps.BinTransform = star.transform; sps.SectorPos = galaxy.SectorPositions[(int)BodyID.FromString(star.ParentID).GetID()]; }
			starGO.SetActive(true);
		}

		// 2. Clasificar Partícula (¡ZAZ! Directo a la lista correspondiente)
		switch (star.type)
		{
			case StarTypes.O: pO.Add(el.emit); break;
			case StarTypes.B: pB.Add(el.emit); break;
			case StarTypes.A: pA.Add(el.emit); break;
			case StarTypes.F: pF.Add(el.emit); break;
			case StarTypes.G: pG.Add(el.emit); break;
			case StarTypes.K: pK.Add(el.emit); break;
			case StarTypes.M: pM.Add(el.emit); break;
			case StarTypes.L: pL.Add(el.emit); break;
			case StarTypes.T: pT.Add(el.emit); break;
			case StarTypes.EB: pEB.Add(el.emit); break;
			case StarTypes.NS: pNS.Add(el.emit); break;
			case StarTypes.EN: pEN.Add(el.emit); break;
			case StarTypes.X:
			default: pX.Add(el.emit); break;
		}
	}

	// --- Procesador de Planetas ---
	private void ProcessPlanet(quequeElement el, Transform parent, bool needTemplates, List<EP> pEN, GameObject planetTemplate = null)
	{
		var planet = el.Planet;
		if (planet == null) return;

		if (needTemplates && !IsInMainMenu)
		{
			// Nota: planetTemplate debe ser accesible aquí
			GameObject planetGO = Instantiate(planetTemplate);
			planetGO.name = planet.id;
			planetGO.transform.position = planet.transform.Pos * GalaxyScale;
			planetGO.transform.rotation = Quaternion.Euler(planet.transform.Rot);
			planetGO.transform.parent = parent;

			var spp = planetGO.GetComponent<SpaceStageRouguePlanet>();
			if (spp != null) { spp.ID = planet.id; spp.Type = planet.type; }
			planetGO.SetActive(true);
		}

		// También le toca partícula de "Enana Negra/Planeta" para que brille en el mapa
		pEN.Add(el.emit);
	}
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
				GalaxyData data = galaxy;
				if (data == null) data = GalaxyData.LoadGalaxy();

				Parallel.ForEach(data.SectorPositions, (st) => {
					GalaxyData.LoadSector(st, out var Sec);
					if (Sec != null) Bag.Add(Sec);
				});
				isDoneLoading = true;
			}, token);
		}
		catch (OperationCanceledException) { /* Tarea cancelada con éxito */ }
	}
	bool SpawnedSector = false;
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

					EP emit = new();
					emit.position = star.transform.Pos * GalaxyScale;

					QueQue.Add(new quequeElement { star = star, emit = emit });
				}
				*/
				Parallel.ForEach(sector.Stars, (star) =>
				{
					if (token.IsCancellationRequested) return;
					if (star == null || star.IsNull()) return;
					EP emit = new();
					emit.position = star.transform.Pos * GalaxyScale;
					QueQue.Add(new quequeElement { Star = star, emit = emit });
				});
				SpawnedSector = true;
			}, token);
		}
		catch (Exception e) { Debug.LogError(e); }
	}
	async void ParralelMassiveSpawn(ConcurrentBag<GalaxySector> Sectors, GalaxyData data)
	{
		var token = _cts.Token;
		if (QueQue == null) QueQue = new();
		try
		{
			await Task.Run(() => {
				Parallel.ForEach(Sectors, (sector) => {
					if (token.IsCancellationRequested) return;

					// 1. Estrellas del sector
					foreach (var star in sector.Stars)
					{
						if (star == null || star.IsNull()) continue;
						EP emit = new EP { position = star.transform.Pos * GalaxyScale };
						QueQue.Add(new quequeElement
						{
							Star = star,
							emit = emit,
							SectorPostion = sector.Position,
							Type = CelestialBodyType.Star
						});
					}

					// 2. ¡EL TRUCO! Buscamos los planetas errantes AQUÍ, en el hilo secundario
					if (!HideRouguePlanets)
					{
						var collection = data.GetRougueStuffInThisSector(sector.Position);
						if (collection?.planets != null)
						{
							foreach (var planet in collection.planets)
							{
								EP emit = new EP { position = planet.transform.Pos * GalaxyScale };
								QueQue.Add(new quequeElement
								{
									Planet = planet,
									emit = emit,
									SectorPostion = sector.Position,
									Type = CelestialBodyType.Planet
								});
							}
						}
					}
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

		public EP emit;
		public CelestialBodyType Type;
		public StarData Star;
		public PlanetData Planet;
		public NebulaData Nebula;
		public NovaData Nova;
		public BaricenterData Baricenter;
		public Vector2Int SectorPostion;
	}
	private void Update()
	{
		if (QueQue != null) 
			QueQUeLength = QueQue.Count;
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
			if (aaaaaaaaaaaaaaaaaaa != double.MaxValue)
				aaaaaaaaaaaaaaaaaaa += a.x;
			else aaaaaaaaaaaaaaaaaaa = 0;
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
