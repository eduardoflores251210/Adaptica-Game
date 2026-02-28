using aaa;
using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
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
	public List<GalaxySector> list;

	public int BatchSize = 50;
	public bool IsInMainMenu = false;
	public bool HideRouguePlanets = true;
	[Tooltip("Usado en el menu principal para cargar las estrellas antes de hacer el fundido de negro a vista normal")]
	public bool Use1FramesPerSecondMode = false;
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
#pragma warning disable IDE0059 // Asignación innecesaria de un valor
		GalaxyData data = null;
#pragma warning restore IDE0059 // Asignación innecesaria de un valor
		try { data = GalaxyData.LoadGalaxy(); }
		catch (Exception e) { Debug.Log(e); yield break; }

		if (data == null) yield break;

		list = new List<GalaxySector>();
		foreach (var st in data.SectorPositions)
		{
			GalaxyData.LoadSector(st, out var Sec);
			if (Sec != null)
			{
				list.Add(Sec);
				//Debug.Log("LD SEC " + Sec.Position.ToString());
			}

			yield return null;
		}

		if (list.Count > 0) yield return StartCoroutine(VisualizeStars(list, data));
	}
	public GalaxyData galaxy = null;

	public IEnumerator VisualizeStars(List<GalaxySector> sectors, GalaxyData data = null)
	{
		List<ParticleSystem.EmitParams> particleX = new  ();
		List<ParticleSystem.EmitParams> particleO = new  ();
		List<ParticleSystem.EmitParams> particleB = new  ();
		List<ParticleSystem.EmitParams> particleA = new  ();
		List<ParticleSystem.EmitParams> particleF = new  ();
		List<ParticleSystem.EmitParams> particleG = new  ();
		List<ParticleSystem.EmitParams> particleK = new  ();
		List<ParticleSystem.EmitParams> particleM = new  ();
		List<ParticleSystem.EmitParams> particleL = new  ();
		List<ParticleSystem.EmitParams> particleT = new  ();
		List<ParticleSystem.EmitParams> particleEB = new ();
		List<ParticleSystem.EmitParams> particleEN = new ();
		List<ParticleSystem.EmitParams> particleNS = new ();
		Debug.Log("STart");
		if (starParent == null) starParent = this.transform;
		galaxy= data;
		GameObject SectorGO = null;
		foreach (var sector in sectors)
		{
			if (IsDebug || !IsInMainMenu)
			{ 
			SectorGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
			SectorGO.name = ($"Sector_{sector.Position}");
			SectorGO.transform.position = (((Vector3)sector.Position.To3DXZ()).Multiply3d(((Vector2)Generator.sectorSize).To3DXZ())) * GalaxyScale;
			SectorGO.transform.localScale = Vector3.one;
			var MeshFiltaaa = SectorGO.GetComponent<MeshFilter>();
			MeshFiltaaa.mesh = MeshFiltaaa.mesh.ScaleMesh(((Vector2)Generator.sectorSize).To3DXZ());
			SectorGO.GetComponent<MeshRenderer>().enabled = IsDebug;
			if (ChunckManager != null)
			{
				if (ChunckManager.Sectors == null)
					ChunckManager.Sectors = new List<GameObject>();
				else
					ChunckManager.Sectors.Add(SectorGO);
			}
			}
			int Batch = 0;
			System.Diagnostics.Stopwatch sw = null;
			if (Use1FramesPerSecondMode) sw = System.Diagnostics.Stopwatch.StartNew();
			foreach (var star in sector.Stars)
			{
				if(star == null) continue;
				if (star.IsNull()) continue;
				if (!IsInMainMenu && SectorGO != null) 
				{

					GameObject starGO = new GameObject(star.id);
					starGO.transform.position = star.transform.Pos * GalaxyScale;
					starGO.transform.rotation = Quaternion.Euler(star.transform.Rot);
					starGO.transform.parent = SectorGO.transform;
					var SPSDATA = starGO.AddComponent<SpaceStageStar>();
					starGO.AddComponent<SphereCollider>();
					SPSDATA.ID = star.id;
					SPSDATA.BinTransform = star.transform;
					SPSDATA.Type = star.type;
					SPSDATA.SectorPos =sector.Position;
				}
				// Partículas compartidas por tipo
				if (Particles.ContainsKey(star.type))
				{

					ParticleSystem.EmitParams emit = new();
					emit.position = star.transform.Pos * GalaxyScale;

					switch (star.type)
					{
						case StarTypes.O:
							particleO.Add(emit);
							break;
						case StarTypes.B:
							particleB.Add(emit);
							break;
						case StarTypes.A:
							particleA.Add(emit);
							break;
						case StarTypes.F:
							particleF.Add(emit);
							break;
						case StarTypes.G:
							particleG.Add(emit);
							break;
						case StarTypes.K:
							particleK.Add(emit);
							break;
						case StarTypes.M:
							particleM.Add(emit);
							break;
						case StarTypes.L:
							particleL.Add(emit);
							break;
						case StarTypes.T:
							particleT.Add(emit);
							break;
						case StarTypes.EB:
							particleEB.Add(emit);
							break;
						case StarTypes.NS:
							particleNS.Add(emit);
							break;
						case StarTypes.EN:
							particleEN.Add(emit);
							break;
						case StarTypes.X:
						default:
							particleX.Add(emit);
							break;
					}


				}
				else
				{

				}
				bool T = false;
				if (Use1FramesPerSecondMode)
				{
					if (sw != null)
					{
						if (sw.Elapsed > new TimeSpan(0, 0, 1))
						{
							T = true;
							sw.Restart();
						}
					}
				}
				Batch++;
				if (((Batch >= BatchSize ) && !Use1FramesPerSecondMode ) || T)
				{
					Batch = 0;
					yield return null;
				}
			}

			yield return null;
			if (!HideRouguePlanets)
			{
				// no queremos llenar la ram con basura en el modo solo estrella 
				GalObjCollection collection = data.GetRougueStuffInThisSector(sector.Position); ; //esto tarda demasiado tiempo en salir 

				if (Use1FramesPerSecondMode && sw == null) sw = System.Diagnostics.Stopwatch.StartNew(); //por si alguien cambia la configuracion a mitad de corutina 
				if (collection != null)
				{
					if (collection.planets != null)
					{
						if (collection.planets.Count != 0)
						{
							foreach (var planet in collection.planets)
							{
								if (!IsInMainMenu)
								{

									GameObject PlanetGO = new GameObject(planet.id);
									PlanetGO.transform.position = planet.transform.Pos * GalaxyScale;
									PlanetGO.transform.rotation = Quaternion.Euler(planet.transform.Rot);
									PlanetGO.transform.parent = SectorGO.transform;
									var SPPDATA = PlanetGO.AddComponent<SpaceStageRouguePlanet>();
									PlanetGO.AddComponent<SphereCollider>();
									SPPDATA.ID = planet.id;
									SPPDATA.BinTransform = planet.transform;
									SPPDATA.Type = planet.type;
								}
								// Partículas compartidas por tipo
								if (Particles.ContainsKey(StarTypes.EN))
								{
									ParticleSystem.EmitParams emit = new();
									emit.position = planet.transform.Pos * GalaxyScale;
									particleEN.Add(emit);
								}
								bool T = false;
								if (Use1FramesPerSecondMode)
								{
									if (sw != null)
									{
										if (sw.Elapsed > new TimeSpan(0, 0, 1))
										{
											T = true;
											sw.Restart();
										}
									}
								}
								Batch++;
								if (((Batch >= BatchSize) && !Use1FramesPerSecondMode) || T)
								{
									Batch = 0;
									yield return null;
								}
							}
						}
					}
				}
			}
			if (IsDebug || !IsInMainMenu)
			{
				SectorGO.transform.parent = starParent;
			}
			if (ChunckManager != null)
			{
				SectorGO.SetActive(false);
			}
			yield return null;
		}
		foreach (var ps in Particles)
		{
			ps.Value.Pause();
			var renderer = ps.Value.GetComponent<ParticleSystemRenderer>();
			renderer.material = Mats[ps.Key];
			List<ParticleSystem.EmitParams> L = ps.Key switch
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

			foreach (var p in L)
				ps.Value.Emit(p, 1);

			if (ps.Key == StarTypes.X)
			{
				
				renderer.material = BholMat;
			}
		}
		Done = true;
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
