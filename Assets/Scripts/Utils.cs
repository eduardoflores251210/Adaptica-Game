//advertencia hay MUCHO comentario XD y NO hay que tocar mucho de este archivo por que es el corazon del juego
//un momento de silencio por el pobre SHA512 que se queda sin su gloria de nombrar galaxias y ahora solo nombra guardados XD
using ActualUtils;
using FixedMath;
using SerializableTypes;
using SerializableTypes.Biology;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; //ignorar este remanete 
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;


public static class SpaceUtils
{

	/// <summary>
	/// 🪐 NOMBRADORES GALÁCTICOS
	/// </summary>
	public static class Naming
	{
		public static System.Random Random = new();
		public static string GenerateGalaxyName()
		{
			string[] Catalogues = {
				"GCA",
				"EGC",
				"SGC",
				"GCGC",
				"VGE",
				"G",
				"S5GC"
			};
			// significados de Los catalogos:
			//Galactic Catalogue A,      //NGC
			//Extra Galactic Catalogue, //inspíracion LEDA y ESO a la vez
			//SGC Super Galactic Catalogue, //isnpiracion PGC
			//Global Clusters and Galaxies Catalogue,    //inspiracion CGCG
			//VGE = Virgo Galactic Extention //inspiracion  VCC fusionado con NGC 
			//Guadalupe             //insperacion Messier. si, MESSIER pero es una señora que amaba ver cometas pero solo encontró galaxias XD
			//SHA512 Galactic Catalogue	//inspiracion NINGUNO, originalidad 100% XD 
			string Catalogo = Catalogues[Random.Range(0, Catalogues.Length)];
			int number1 = Random.Range(100, 9999);
			int number2 = Random.Range(10, 999);
			int number3 = Random.Range(1, 125);
			int number4 = Random.Range(1, 9999);

			string ShaIn = "" + Random.Range(int.MinValue, int.MaxValue);
			string SHAOUT = "";

			using SHA512 SHA512 = SHA512.Create();
			{
				byte[] hashBytes = SHA512.ComputeHash(Encoding.UTF8.GetBytes(ShaIn));

				// Convertir a hexadecimal solo Los primeros  128
				StringBuilder sb = new StringBuilder();
				int i = 1;
				foreach (byte b in hashBytes)
				{
					sb.Append(b.ToString("x2"));
					i++;
					if (i >= 128)
						break;
				}
				SHAOUT = sb.ToString();
			}
			switch (Catalogo)
			{
				case "VGE":

					return $"{Catalogo} {number3}-{number4}";
				case "G":
					return $"{Catalogo} {number3}";
				case "GCGC":
					return $"{Catalogo} {number3}:{number1}";
				case "SGC":
					return $"{Catalogo} {number4}";
				case "S5GC":
					return "S5GC " + SHAOUT ;
				default:
					return $"{Catalogo} {number1}-{number2}";
					
			}

			//funfact: NGC es el catalogo de galaxias mas famoso y usado en la vida real, pero no lo uso por que es muy obvio XD
			//2 VGE se iba a llamar VGA pero VGA es un cable 
			//3 S5GC era algo random que se me ocurio despues de tener la decimo cuarta crisis creativa del año 2026
		}

		public static string GenerateStarName_NASAStyle()
		{
			int catalogNumber = Random.Range(10000, 999999);
			List<string> Catalogues = new List<string>()
			{
				"SC",//Clasico 1 aka Star Catalog
				"SL",//Clasico 2 //Star List // oh wow Comentario en un comentario X3
				"Krumpler", //si Keppler pero inspirado en krampus... si el de la navidad XD
				"HUP", //Hipparcos pero ahora es Hupparcus XD
				"FHD", //jaja FHD en vez de HD // que significa Full HD  pregintaras? pues Flores-Hernandez-Diaz catalogo de estrellas       si es un nombre largo XD
				"DESS" //ups referencia implicita accidental a deltarune (dess la hermana mayor de Noelle la que esta desaparecida) auque originamente esto era referencia a TESS. pero aqui DESS significa Deep Extra Stellar Survey no December Holiday (aka la Hermana de Noelle XD)
			};
			//favor de ignorar el infodump de DESS pls, no quiero cambiar el nombre por que ya lo use en varios lados XD
			string catalogPrefix = Catalogues[Random.Range(0,Catalogues.Count)];
			return $"{catalogPrefix} {catalogNumber}";
		}

		public static string GeneratePlanetName_NASAStyle(string systemName, int idx)
		{
			//Debug.Log(idx.ToString());
			char suffix = (char)('b' + idx); // b, c, d, etc.
			return $"{systemName}{suffix}";
		}
		
		public static string GenerateMoonName_NASAStyle(string PlanetName, int idx)
		{
			//Debug.Log(idx.ToString());

			return $"{PlanetName} {idx.ToRoman()}";
		}

	}
	public static void AddTooltipManipulators(UIDocument uiDocument)
	{
		if (uiDocument == null || uiDocument.rootVisualElement == null)
			return;

		System.Action<VisualElement> walk = null;
		walk = (ve) =>
		{
			if (!string.IsNullOrEmpty(ve.tooltip))
				ve.AddManipulator(new ToolTipManipulator());

			foreach (var child in ve.Children())
				walk(child);
		};

		walk(uiDocument.rootVisualElement);
	}

	public static class UnitConversion
	{
		public static float LightYearToParsec(float value) 
		{
			return value * 3.26156f;

		}
		public static float ParsecToLightYear(float value)
		{
			return (value / 3.26156f);
		}
		public static float LightYearToAU(float value)
		{
			return value * 63241.1f;
		}
		public static float AuToLightYear(float value)
		{
			return ((value / 63241.1f));

		}
		/// <summary>
		/// convierte kilometros a unidades astronomicas 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float KmToAU(float value)
		{
			return value / 149597870.7f; 
		}
		/// <summary>
		/// convierte Unidades astronomicas a Kilometros
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float AuToKm(float value)
		{
			return value * 149597870.7f;
		}
		/// <summary>
		/// convierte kilometros a metros
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float KmToM(float value)
		{
			return value * 1000;
		}

		public static float MeterToKm(float value)
		{
			return value / 1000;
		}

	}
	//easter egg recursivo originalmentre pense que era un coinflip pero uego me di cuenta que era recursivo XD
	static void a(bool f)
	{
		if (f) 
		a(Random.Range(0,2)==0);
	}
	/// <summary>
	/// Convierte un numero entero a numero romano
	/// </summary>
	/// <param name="number"> el numero</param>
	/// <returns></returns>
	public static string ToRoman(this int number)
	{
		if (number > 3999) return "0"; // límites clásicos del sistema romano
		if (number == 0)
			return "O";
		bool isNEg = false;
		if (number< 0 )
			isNEg = true;
		var romanNumerals = new[]
		{
		new { Value = 1000, Symbol = "M" },
		new { Value = 900, Symbol = "CM" },
		new { Value = 500, Symbol = "D" },
		new { Value = 400, Symbol = "CD" },
		new { Value = 100, Symbol = "C" },
		new { Value = 90, Symbol = "XC" },
		new { Value = 50, Symbol = "L" },
		new { Value = 40, Symbol = "XL" },
		new { Value = 10, Symbol = "X" },
		new { Value = 9, Symbol = "IX" },
		new { Value = 5, Symbol = "V" },
		new { Value = 4, Symbol = "IV" },
		new { Value = 1, Symbol = "I" }
	};

		string result = "";
		foreach (var item in romanNumerals)
		{
			while (number >= item.Value)
			{
				result += item.Symbol;
				number -= item.Value;
			}
		}
		if (isNEg)
			result = '-' + result;

		return result;
	}
	/// <summary>
	/// instancia un sistema solar... POSICIONES y DISCOS DE ACRECIÓN NO INCLUIDOS TODO SE GENERA EN 0.0
	/// </summary>
	public static class SystemObjectsBuilder
	{
		public static Material BaseMat;
		public static Material BholMat;
		public static Material BaseGasMaterial;
		public static Material OpaceMat;
		public static Dictionary<StarTypes, Material> Mats = new Dictionary<StarTypes, Material>();
		public static GalaxyData galaxyData;
		public static InstantiatedSystemData systemData;
		public static Mesh SphereMesh; //esto es Mesh serializable de mi libreria estandar dem is projecctos unity no un Mesh de unity pero hay conversiones imple,entadas
		public static void InitStuf()
		{
			foreach (StarTypes st in Enum.GetValues(typeof(StarTypes)))
			{
				Material material = new Material(BaseMat);
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
			}
			SphereMesh = Mesh.FromObjString(PrimitivesOBJ.Sphere);
			Inited = true;

		}
		static UnityEngine.Mesh CacheSphere;
		static void GiveSphere(GameObject @object, Material mat)
		{
			if (CacheSphere == null)
				CacheSphere = (UnityEngine.Mesh)SphereMesh;
			@object.AddComponent<MeshFilter>().mesh = CacheSphere;
			@object.AddComponent<MeshRenderer>().material = mat;
			@object.AddComponent<MeshCollider>();
		}
		public static GameObject InstantiateStar(StarData starData)
		{
			GameObject starGO = new GameObject();
			starGO.name = starData.Name;
			GiveSphere(starGO, Mats[starData.type]);

			starGO.SetActive(true);
			starGO.transform.localScale = Vector3.one; //iba a multiplicar por radio pero olvide que no hay StarData.radius 
			systemData.IDS.Stars.Add(BodyID.FromString(starData.id));
			systemData.Datas.Stars.Add(starData);
			systemData.ObjAndIDS.Add(starGO, starData.id);
			return starGO;
		}
		public static GameObject InstantiateBaricenter(BaricenterData baricenterData)
		{
			var gol = new GameObject(baricenterData.Name);
			systemData.IDS.baricenters.Add(BodyID.FromString(baricenterData.id));
			systemData.Datas.baricenters.Add(baricenterData);
			systemData.ObjAndIDS.Add(gol, baricenterData.id);
			return gol; //literalmente son invisibles
		}
		public static GameObject InstantiatePlanet(PlanetData planetData)
		{
			GameObject planetGO = new GameObject(planetData.Name);
			Material Mat = null;
			if (planetData.type == PlanetTypes.BasicGas || planetData.type == PlanetTypes.IceGas)
			{
				if (planetData.GasColors == null || planetData.GasColors.Count < 5)
				{
					Debug.LogWarning($"Planeta gaseoso sin suficientes colores: {planetData.Name ?? "Unnamed"}");
					// Asignamos material básico como fallback para evitar crash
					if (OpaceMat != null)
						Mat = OpaceMat;
				}
				else if (BaseGasMaterial != null)
				{
					Material gasMatInstance = new Material(BaseGasMaterial);
					gasMatInstance.SetColor("_PoloNorte", planetData.GasColors[0]);
					gasMatInstance.SetColor("_Arriba", planetData.GasColors[1]);
					gasMatInstance.SetColor("_Ecuador", planetData.GasColors[2]);
					gasMatInstance.SetColor("_Abajo", planetData.GasColors[3]);
					gasMatInstance.SetColor("_PoloSur", planetData.GasColors[4]);
					Mat = gasMatInstance;
				}
			}
			else
				if (OpaceMat != null)
				Mat = OpaceMat;

			GiveSphere(planetGO, Mat);
			planetGO.transform.localScale = Vector3.one*planetData.radius;
			systemData.IDS.planets.Add(BodyID.FromString(planetData.id));
			systemData.Datas.planets.Add(planetData);
			systemData.ObjAndIDS.Add(planetGO,planetData.id);
			return planetGO;
		}
		public static void InstantiateBody(string StartID, UnityEngine.Transform parent)
		{
			if (StartID[0] == 'S')
				throw new ArgumentException("NO PUEDES HACER ESTO EN UN SECTOR ENTERO");
			if (galaxyData == null)
				if (!GalaxyData.TryToLoadGalaxy(out galaxyData))
				{
					throw new Exception("ERROR AL CARGAR GALAXIA");
				}

			CelestialBody body = null;
			BodyID bodyID = BodyID.FromString(StartID);
			if (!TryToLoadABody(bodyID, out body, out var celestialBodyType))
				throw new Exception("ERROR CARGANDO");
			GameObject gameObject = null;
			switch (bodyID.GetCelestialBodyType())
			{
				case CelestialBodyType.None:
					break;
				case CelestialBodyType.Planet:
					gameObject=InstantiatePlanet((PlanetData)body);
					break;
				case CelestialBodyType.Star:
					gameObject=InstantiateStar((StarData)body);
					break;
				case CelestialBodyType.Baricenter:
					gameObject=InstantiateBaricenter((BaricenterData)body);
					break;
				case CelestialBodyType.Nova:
					gameObject = InstantiateNova((NovaData)body);
					break;
				case CelestialBodyType.Nebula:
					gameObject = InstantiateNebula((NebulaData)body);
					break;
				case CelestialBodyType.Sector:
					Debug.Log("NO");
					throw new ArgumentException("NO SECTORES");
				default:
					break;
			}
			if (parent !=null)
			gameObject.transform.parent = parent;
			if (body.Children == null)
			{

			}
			else if (body.Children.Count == 0)
			{

			}
			else
			{
				foreach (var child in body.Children)
				{
					InstantiateBody(child,gameObject.transform);
				}
			}
		}

		private static GameObject InstantiateNebula(NebulaData body)
		{
			throw new NotImplementedException();//ni tengo forma de renderizar Nebulosas 
		}

		private static GameObject InstantiateNova(NovaData body)
		{
			throw new NotImplementedException();//ni tengo forma de renderizar Novas
		}
		static bool Inited;
		public static void InstantiateSystem(string ParentId)
		{
			if (!Inited)
			{
				InitStuf();	
			}
			systemData = new();
			systemData.Datas = new();
			systemData.IDS = new();
			systemData.ObjAndIDS = new();//des-optimización
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			systemData.ObjAndIDS = new();
			if (ParentId[0] == 'S')
				throw new ArgumentException("NO PUEDES HACER ESTO EN UN SECTOR ENTERO");
			if (galaxyData == null)
				if (!GalaxyData.TryToLoadGalaxy(out galaxyData))
				{
					throw new Exception("ERROR AL CARGAR GALAXIA");
				}

			InstantiateBody(ParentId, null);
		}
		static bool TryToLoadABody(BodyID bodyID,out CelestialBody body, out CelestialBodyType d)
		{
			try
			{
				d = bodyID.GetCelestialBodyType();
				body = d
					switch
				{
					CelestialBodyType.Planet => galaxyData.LoadPlanet(bodyID.GetID()),
					CelestialBodyType.Star => galaxyData.LookForStar(bodyID.GetID()),
					CelestialBodyType.Baricenter => galaxyData.LookForBaricenter(bodyID.GetID()),
					CelestialBodyType.Nova => galaxyData.LookForNova(bodyID.GetID()),
					CelestialBodyType.Nebula => galaxyData.LookForNebula(bodyID.GetID()),
					CelestialBodyType.Sector => throw new ArgumentException("TIPO INVALIDO"),
					_ => throw new Exception("TIPO DESCONOCIDO"),
				};
				return true;
			}
			catch (Exception)
			{
				d = CelestialBodyType.None;
				body = null;
				return false;
			}
		}
		public struct InstantiatedSystemData
		{
			public GalObjCollection Datas;
			public GalObjCollectionID IDS;
			public Dictionary<GameObject, string> ObjAndIDS;

		}
	}
}
//si Seria mas facil con Flags pero bueno no sabia de su existencia cuando hice el enum y no quiero cambiarlo por ahora
public enum MultiEje 
{
	X,
	Y,
	Z,
	XY,
	XZ,
	YZ,
	XYZ
}
// el corazon del juego aqui esta todo dato un enum una estrutrua cualquier cosa usada ya sea en guardado o en runtime para manejar y representar datos del juego esta aqui, ademas de funciones utiles para el espacio como conversiones de unidades y generadores de nombres galacticos
namespace SerializableTypes
{
	/// <summary>
	/// El enum de los estadios del juego
	/// Microbio (cof cof Celúla de spore)
	/// criatura (tiene el mismo nombre que en spore)
	/// tribal (no ha empezado desarollo pero tiene el mismo nombre que en spore)
	/// city  (no ha empesado el desarollo pero tiene el mismo nombre que su equivalente descartado en spore) aunque actualmente a menudo se le llama feudal
	/// Civilization (no ha empesado desarollo pero tiene el mismo nombre que en spore) // ademas se le llama actualmente NACION
	/// Space (estructuras de datos en cosntrucción aunque ya puedes visitar sistemas pero no planetas) tiene el mismo nombre que en spore
	/// Main Menu         Es solo un stage para simplificar  y facilitar el manejo de escenas pero como puedes notar en el codigo de guardado en PauseMenuManager no se guarda nada en este estadio
	/// </summary>
	public enum Stages
	{
		Microbe = 0, //en construccion aunque ya es Jugable
		Creature, //en construccion, NO, ni ha empezado desarollo
		tribal, //No ha empesado desarollos 
		City, //No ha empesado desarollos       ademas se le llama acvtualmente feudal
		Civilization, //No ha empesado desarollo		ademas se le llama actualmente NACION
		Space, //estructuras de datos en cosntrucción  aunque ya puedes visitar sistemas pero no planetas
		MainMenu //Funciona desde alpha 1 
				 //ah claro porque todos definitivamernte sabemos que el Menu principal es el estadio MAS Avanzado del juego XD 
	}

	public enum CreatureTypes
	{
		Microbe,
		Animal,
		TribeMember,
		FeudalCitizen,
		Citizen,
		SpaceCitizen,
	}
	//advertencia: muchos nombres de editores son largos y feos por que asi se evitan conflictos de nombres con otras clases
	//2 algunos editores podrian NO usarse nunca pero estan aqui por si acaso y si son 43... Casi 42 ,, la respuesta a la vida el universo y todo lo demas XD
	public enum Editors
	{
		Microbe,
		Animal,
		TribeDresser,
		FeudalCitizenDresser,
		CitizenDresser,
		SpaceCitizenDresser,
		Plant,//tambien puedes no añadir ramas y crear un Prototaxites en vez de un árbol/planta/arbusto normal
		Planet,
		Vehicles_Car,
		Vehicles_Car_Religius,
		Vehicles_Car_Economic,
		Vehicles_Car_Military,
		Vehicles_Car_Civilian,//hasta aqui van los posiblemente usados
		Vehicles_Car_BUS,
		Vehicles_Train_Steam_Civilian,
		Vehicles_Train_Steam_Military,
		Vehicles_Train_Steam_Economic,
		Vehicles_Train_Electrical_Metro,// [dun dundun] llegando a Martin Carrera. Puerta abierta, Cuidado al salir// Si una referencia a la CDMX XD y al metrobus por la voz de la estación XD
		Vehicles_Train_Electrical_Tram,
		Vehicles_Train_Electrical_Monorail,
		Vehicles_Train_Electrical_MagLev, //oh levita
		Vehicles_Train_Electrical_LightTrain,
		Vehicles_Train_Electrical_Suburban,
		Vehicles_Train_Electrical_Bullet,
		Vehicles_Train_TrainLike_CableCar,
		Vehicles_Plane_Civilian,//ok estos aviones de abajo a lo mejor si se usan
		Vehicles_Plane_Military,
		Vehicles_Plane_Economic,
		Vehicles_Plane_Religous,
		Vehicles_Boat_Civilian,//ok estos barcos a lo mejor si se usan
		Vehicles_Boat_Military,
		Vehicles_Boat_Economic,
		Vehicles_Boat_Religous,
		Vehicles_Boat_Canoe, //si el barco mas basico
	}

	[Serializable]
	public class SavedGame
	{
		public ulong PlanetID;
		public bool isCPUEmpire; //para los imperios de CPU en estadio del espacio
		public Stages CurentStage;
		public string CreatureName;
		public List<HistoryActions> Actions;
		public Diets CreatureDiet = Diets.Omnivore;
		public double ingameTime = 0;
		public CellGameData CellGameData;

		public SavedGame(ulong planetID, bool isCPUEmpire, Stages curentStage, string creatureName, List<HistoryActions> actions, Diets creatureDiet, double ingameTime = 0)
		{
			PlanetID = planetID;
			this.isCPUEmpire = isCPUEmpire;
			CurentStage = curentStage;
			CreatureName = creatureName ?? throw new ArgumentNullException(nameof(creatureName) + "ES NULL!!!!!");
			Actions = actions?? throw new ArgumentNullException(nameof(actions) + "ES NULL!!!!!!!!!"); 
			CreatureDiet = creatureDiet;
			this.ingameTime = ingameTime;
		}
		public SavedGame() { }
		public SavedGame(string planetID, bool isCPUEmpire, Stages curentStage, string creatureName, List<HistoryActions> actions, Diets creatureDiet, double ingameTime = 0)
		{
			PlanetID = BodyID.FromString(planetID).GetID();
			this.isCPUEmpire = isCPUEmpire;
			CurentStage = curentStage;
			CreatureName = creatureName ?? throw new ArgumentNullException(nameof(creatureName) + "ES NULL!!!!!");
			Actions = actions ?? throw new ArgumentNullException(nameof(actions) + "ES NULL!!!!!");
			CreatureDiet = creatureDiet;
			this.ingameTime = ingameTime;
		}
	}
	[Serializable]
	public struct CellGameData : IEquatable<CellGameData> 
	{
		public float DNA_Amount;
		public float MaxDNA_Got;
		public float Progress;
		public float PlayerHealth;
		public GéneroBiológico Gender;
		public bool Finished;
		public CellGameData(float dNA_Amount, float maxDNA_Got, float progress, float playerHealth, GéneroBiológico gender)
		{
			DNA_Amount = dNA_Amount;
			MaxDNA_Got = maxDNA_Got;
			Progress = progress;
			PlayerHealth = playerHealth;
			Gender = gender;
			Finished = false;
		}

		public bool Equals(CellGameData other)
		{
			return DNA_Amount == other.DNA_Amount && Progress == other.Progress &&  PlayerHealth == other.PlayerHealth && Gender == other.Gender && MaxDNA_Got == other.MaxDNA_Got ;
		}
		public override bool Equals (object o)
		{
			if (ReferenceEquals(this,o)) return true;
			if (o is null ) return false;

			if (o is CellGameData Cell)
			{
				return Equals((CellGameData)o);
			}else return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(DNA_Amount, Progress, PlayerHealth, Gender);
		}
		public override string ToString()
		{
			return $"DNA {DNA_Amount}, progresss {Progress}, HP {PlayerHealth}, Gender {Gender}, Done {Finished}";
		}
	}
	[Serializable]
	public struct SpaceStageData
	{
		public long MONEY_Amount;// capitalismo
		public List<PopulatedPlanetData> PlanetData;
	}
	[Serializable]
	public struct PopulatedPlanetData
	{
		public List<CityData> Cities;
	}
	[Serializable]
	public struct CityData
	{
		public Mesh RoadPathMesh;
		public Mesh TrackMesh;
		public List<BuildingData> Buildings;
	}
	[Serializable]
	public struct BuildingData
	{
		public Buiding_Type Type;
		public int Level;
		public Transform transform;
	}
	public enum GovermentType
	{
		Tribal = 0,
		Feudal = 1,
		DirectDemocracy = 2,
		RepresentativeDemocracy = 3,
		Dictatorship = 4,
		Monarchies_Absolute = 5,
		Monarchies_Constitutional = 6,
		Monarchies_Parliamentary = 7, // god Save the Queen ... OH ups King XD aun no he superado lo de Elizabeth II. (1936-2022) RIP
		Monarchies_Unspecified = 8,
		Communism = 9,
		Socialism = 10,
		Anarchy = 11,
		Oligarchy = 12,
		Theocracy = 13,
		JuntaMilitary = 14,// [Mira fijamente a [insetar pais con junta militar]]
		Spamton = 99999// easter egg XD... hello do you wanto buy [GOVERMENT TYPE] for only 1 easy payment of 9.99 kromers?
	}
	[Flags]
	public enum GovermentPowerSeparation
	{
		None = 0,
		Executive = 1 << 0,//presidente primer ministro etc
		Legislative = 1 << 1,//congreso parlamento etc
		Judicial = 1 << 2,//corte suprema tribunales etc
		TieneMaximoPoderConservador = 1 << 3, //si inspirado en Mexico  pero en e tiempo de republivas centralistas XD
	}


	/// <summary>
	/// otro enum largo pero necesario
	/// a lo mejor no se usan todos los tipos de edificios pero estan aqui por si acaso
	/// </summary>
	public enum Buiding_Type
	{
		None = 0,
		House = 1,
		Factory = 2,
		Water_Pump = 3,
		ElectricalGenerator = 4,
		Farm = 5,
		Schools_Kinder = 6,
		Schools_Elementary = 7,
		Schools_Middle = 8,
		Schools_FarmHigh = 9,
		Schools_TechnicalHigh = 10,
		Schools_IndustrialHigh = 11,
		Schools_GeneralHigh = 12,
		Schools_University_Medical = 13,
		Schools_University_Farm = 14,
		Schools_University_Technical = 15,
		Schools_University_Industrial = 16,
		Schools_University_General = 17,
		Schools_University_Space = 18,
		Schools_DayCare = 19,
		Schools_AllInOne = 20,
		Schools_Unknown = 21,
		Hospital = 22,
		FireFighter_Station = 23,
		FireFighter_Command = 24,
		Police_Comisary = 25,
		Police_Prison = 26,
		Police_SherifOfice = 27,
		Police_Command = 28,
		Park_Small = 29,
		Park_Medium = 30,
		Park_Large = 31,
		Park_Amusesment = 32,
		Holy_Church = 33,
		Holy_Temple = 34,
		Holy_Generic = 35,
		Transport_Bus_Stop = 36,
		Transport_Bus_BigStation = 37,
		Transport_Bus_Station = 38,
		Transport_Taxi_Stop =39,
		Transport_Taxi_Base = 40,
		Transport_Metro_Station = 41,
		Transport_Metro_BigStation = 42,
		Transport_Train_Station = 43,
		Transport_Train_BigStation = 44,
		Transport_Train_StationFarm = 45,
		Transport_Teleferic_Station = 46,
		Transport_Teleferic_Depo = 47,
		Transport_Generic_CETRAM_Small = 48,
		Transport_Generic_CETRAM_Big = 49,
		Transport_Generic_DepartamentOfMovility = 50,

	}
	[Serializable]
	/// <summary>
	/// Accion hecha por el jugador
	/// z</summary>
	public class HistoryActions
	{
		public HistoryPaths Path;
		public ActionType Tipo;
		public string[] Tags;
		public StatList propieties; //para propiedades Key Value
	}
	public enum HistoryPaths
	{
		Friendly =1,
		Neutral =0,
		Agressive = -1
	}

	/// <summary>
	/// Tipo de acción realizada por el jugador
	/// no he decidido bien cuales van a estar en el juego
	/// asi que por ahora es una lista random de acciones posibles
	/// </summary>
	public enum ActionType
	{
		// Microbio y criatura clásica
		None = -1,
		Eat,                // microbios comen, microbios evolucionan
		Evolve,             // evoluciona y sorprende a tus enemigos
		Die,                // RIP, la naturaleza es cruel
		ChangeStage,        // metamorfosis nivel dios
		LayEgg,             // reproduccion adorable
		GetPregnant,        // biología manda
		Miscarry,           // biología también golpea duro

		// Interacciones generales

		AdvanceStage,
		BuildBuilding,
		Gift,
		FindBean,           // [bean es la criatura mas adorable del spore de Maxis]
		unlockAchievement,  // desbloquea un logro
		DestroyBuilding,
		DestroySettlement,
		SignPeaceTreaty,
		BuyCity,
		Trade,
		GotoAdventure,
		Aliance,            // las alianzas ocuren desde tribu en adelante
		DoSymbiosis,        // solo microbio por endosimbiosis
		DiscoverThing,
		ResearchTechnology,
		FindEasterEgg,
		 FightBoss,           //dudo que haya bosses en el juego pero bueno
		DestroyAllColonies,
		BuySystem,
		ConquerSystem,
		FindSomeUnusalGalacticObject,
		GoToTheGalacticCore,
		DomesticateSomething,
		TerraformPlanet,
		CreateColony,
		PlayMusic,			//una de las fromas de imcrementar lreación en tribu
		MakeAthemn,			
		SPORE,                                              //esto deveria ser un logro no una acción
		CrashGAME,                      //COMO LO LOGRASTE???      [sarcasmo]
		Respuesta,
		Suerte,
		DessignClothesForCreature		//diseñar una nueva ropa
	}
	/// <summary>
	/// aun no estoy seguro de si habra
	/// logros pero por si acaso aqui esta el enum de logros, a lo mejor algunos logros se desbloquean al hacer ciertas acciones o al alcanzar ciertos hitos en el juego, pero por ahora no hay ninguno definido
	/// y si los logro temporales son graciosos pero no se guardan en el guardado del juego, asi que no se si valen la pena o si solo deberian ser logros permanentes que se guardan en el guardado del juego, pero bueno por si acaso aqui esta el enum de logros
	/// </summary>
	public enum Logros
	{
		ninguno = 0,
		AprendesRapido = 1, // si copie el nombre del logro de Woobledogs pero EJEM esto seri ala temrinalr el tutorial inexistente del juego, o sea hacer algo por primera vez en el juego
		ÑamÑam = 2, // si comes por primera vez
		OhNoSoyUnPez = 3, // Avanza al estadio de criatura por primera vez
		TribuTribu = 4, // Avanza al estadio de tribu por primera vez
		OhSuMajestad = 5, // Avanza al estadio Feudal por primera vez
		Presidente = 6, // Avanza al estadio Nacion por primera vez
		DanzandoPorLasEstrellas = 7, // Avanza al estadio del espacio por primera vez
		PrimerContacto = 8, // Interactua con tu primera especie alienigena

		//logros no numerados por flojera de escribir numeros tan grandes XD
		OHHHHNOOOOO, //muere por primera vez
		OHHHHHHHHHHHHHHHH_NOOOOOOOOOOOOOOOOOOOOO, //muere 100 veces
		PEOOOOOORRRRRRRRRRRRRR, //encuentra la BSOD por algun error en el creador de partidas
		QUEEEEEEEEEEEEEEEEEEEEEHICISTE, // crashea el juego.
		OOOOOOOOOOOOOOOOOOHHHHHHHHHHHHHHHHHHHHH_NOOOOOOOOOOOOOOOOOO, //muere 1000 veces
		FavorDeNoHacerPantitlan, //desbloquea el metro en estadio de Nacion
		FavorDeNoHacerMartinCarrera, //desbloquea el metrobus en estadio de Nacion
									 //¿QUE TIENE DE MALO MARTIN CARRERA? ay no la IA rara pantilan lo entiendo esta lleno siempre pero Martin carrera eso esta mas vacio que mis ideas para logros XD
		HELLO_WORLD, //inventa el Ordenador y el código, desbloquea la informática en estadio de Nacion
		
		PorFavorNoAruinesTODO, // privatiza una empresa en estadio de Nacion
		ABCDEF, // inventa un alfabeto, desbloquea la escritura en estadio tribal
		HLL, //inventa un abjad, desbloquea la escritura en estadio de tribal
		TSU, //inventa un silabario, desbloquea la escritura en estadio de tribal
		T_A, //Inventa un Abugida, desbloquea la escritura en estadio de tribal   //la A se supone que es un SUperinidice pegado a la T pero por cosas de C# no se puede hacer eso asi que es un guion bajo pero bueno la idea es esa XD
		標誌, //inventa un sistema de escritura logografico, desbloquea la escritura en estadio de tribal
		Feautural, //inventa un sistema de escritura featural, desbloquea la escritura en estadio de tribal

		//si no se me ocurio un nombre para el logro de sistam featural, pues se llama feautural XD devi haber hecho un juego de palabras como hize cone l resto	

		IDEEEA, //inventa un sistema de escritura ideografico, desbloquea la escritura en estadio de tribal

		unDosTres, //inventa una nuemración posicional, desbloquea las matematicas en estadio tribal
		LIV, //inventa un sistema de numeración no posicional, desbloquea las matematicas en estadio tribal
		A_2ⵜB_2,// si el ⵜ es un +, asi que es A^2 + B^2, o sea el teorema de pitagoras, desbloquea la geometria en estadio tribal

		imaginario, //inventa los numeros imaginarios, desbloquea la matematica avanzada en estadio de nacion
		e, //inventa el numero e, desbloquea la matematica avanzada en estadio de nacion
		π, //inventa el numero pi, desbloquea la matematica avanzada en estadio de nacion
		


		ArtistaMaestro, //diseña una bandera.
		Agricultor, //descubre la agricultura, desbloquea la agricultura en estadio tribal
		Tribunal, // desbloquea el sistema judicial en estadio de Nacion //o escribe mal tribal
		UnPequeñoPasoParaAlguienUnGranSaltoPara_TU_ESPECIE_AQUI, //llega a la luna de tu planeta o a otro planeta cercano por primera vez
		FINALMENTE_NO_HUMORES, //desbloquea la biologia en el estadio nacion,      si es el fin de la teoria de los humores pero no el fin de la biologia XD
		HmmmTraicionero, //traiciona a una alianza, si es que hay alianzas en el juego XD
		HMMMM_Pizza, // traiciona 2 alianzas con el mismo pais,  (si referencia a italia en ambas guerras mundiales XD)
		P_A_N, // descubre el pan, desbloquea la panaderia en estadio tribal
		IPN, // crea una universidad tecnica en un lugar dodne habia una granja de burros, (si referencia al IPN de mexico y su mascota la burra blanca XD)



		Spore, //Completa el juego, desbloquea el reinicio de la galaxia y el modo New Game Plus
			   //NO LE  CREAN A LA IA no hay modo New Game Plus solo regenerar la galaxia con una nueva semilla pero bueno el logro de Spore es para eso, para completar el juego y luego reiniciar la galaxia con una nueva semilla para empezar de nuevo pero con la experiencia de haber jugado antes, aunque no hay un modo New Game Plus como tal, si puedes reiniciar la galaxia y empezar de nuevo con una nueva semilla pero sin perder tu progreso en cuanto a logros y cosas desbloqueadas, asi que es como un New Game Plus pero sin llamarse así XD







		//acciones movidas a logros por Que eran easter eggs o cosas asi y no acciones que el jugador pueda hacer a proposito, aunque algunos de estos logros son tan absurdos que probablemente nunca se desbloqueen XD
		InteractWithCreature, // hace amigos, pelea o ignora según el humor del alien
		InteractWithSpecies,  // alianzas o exterminios interestelares, todo en uno
		UseSuperThing,      //usar una super habilidad como Frenesi de compras
		DETERMINATION,      // Undertale mode ON
		FindChara,          // Si estaba EN una hiperfijación de UNDERTALE cuando hice el Enum
		Hope,
		Dream,
		HopeAndDream,       // combo Asriel que te da DETERMINACIÓN


		DELTARUNE,                      //si también estaba en una hiperfijación de DELTARUNE cuando hice el Enum
		UNDERTALE,                     //si también estaba en una hiperfijación de UNDERTALE cuando hice el Enum




		MLP, // Twilight Sparkle es la mejor pony, y si no te gusta eso, pues... no se que decirte XD
			 // NO Twilightés.
			 //No le hagan caso a la IA, no es una guerra de fandoms, es solo un logro tonto que puse por diversión XD
			 // ademas Twilight Sparkle es la mejor pony, y si no te gusta eso, pues... no se que decirte XD

		//DIJE QU EN OLE CRASN, la mejor pony es Apple JAck



















		FriskMODE, // haz la danza de frisk



		//osea mira hacia arriba y rapidamente abajo repetidamente, si lo haces bien, desbloqueas el modo Frisk, que te da la habilidad de esquivar ataques y hacer amigos con los enemigos, aunque no hay enemigos ni ataques en el juego XD


		أدابتيكا,   //si creo que asi se llmaria el juego en arabe pues en español es Adaptica, asi que en arabe es أدابتيكا, y el logro de أدابتيكا es para completar el juego en arabe, aunque no se si voy a poner un idioma arabe en el juego o no, pero bueno, si lo pongo, este logro seria para completar el juego en arabe XD
		アダプティカ, // si es adaptica en japones, aunque no se si voy a poner un idioma japones en el juego o no, pero bueno, si lo pongo, este logro seria para completar el juego en japones XD

		QUE_NO_ES_Ádaptica, //escribe mal adaptica en un guardado.	  // este logro es para escribir mal adaptica en un guardado, aunque no se si voy a poner esta opción de escribir mal adaptica en un guardado o no, pero bueno, si lo pongo, este logro seria para eso XD











		الجبر, // si es algebra en arabe.      y si tambien lo puse para romper el cursor, y confundir.









		Furby, //Oh dala unai.      //ups hable como furby XD, bueno este logro es para encontrar un furby escondido en el juego, aunque no se si lo voy a poner o no, pero bueno, si lo pongo, este logro seria para encontrarlo y desbloquear un mini juego de furby dentro del juego XD


		chara, //encontrar a Chara, el personaje de Undertale, por alguna razón en el juego XD
	}
	//estos logros son temporales y llenos de easter eggs si llegan a añadirse logros reales algunos cambiraran de nombre o seran eliminados pero por ahora estos son los logros del juego, algunos de ellos son referencias a cosas reales o a otras obras de ficción, pero todos ellos son parte del mundo del juego y pueden ser desbloqueados por el jugador al alcanzar ciertos hitos o realizar ciertas acciones en el juego, aunque algunos de ellos son tan absurdos que probablemente nunca se desbloqueen XD

	[Serializable]
	public class HourTime
	{
		public int Hour;
		public int Minute;
		public int Second;
		public HourTime(int value)
		{
			Hour = value;
			Minute = 0;
			Second = 0;
		}
		public HourTime(int H, int Min)
		{
			Hour = H;
			Minute = Min;
			Second = 0;
		}
		public HourTime(int H, int M, int S)
		{
			Hour = H;
			Minute = M;
			Second = S;
		}
		public HourTime(DateTime time)
		{
			Hour = time.Hour;
			Minute = time.Minute;
			Second = time.Second;
		}
		public HourTime(DayTime Copy)
		{

			this.Hour = (int)Copy.Hour;
			this.Minute = (int)Copy.Minute;
			this.Second = (int)Copy.Second;

		}
		public HourTime(HourTime Copy)
		{

			this.Hour = (int)Copy.Hour;
			this.Minute = (int)Copy.Minute;
			this.Second = (int)Copy.Second;

		}
		public string To12Hour()
		{
			if (Hour < 12) return this.ToString() + "AM";
			var v = Hour - 12;
			return $"{new HourTime(v, Minute).ToString()}PM";
		}
		public override string ToString()
		{
			if (Minute < 10)
			{
				return $"{Hour}:0{Minute}";

			}
			return $"{Hour}:{Minute}";
		}
		public override int GetHashCode()
		{
			return Hour ^ Minute;
		}
		public void AddSeconds(int secondsToAdd)
		{
			// Paso 1: sumar los segundos al total actual
			Second += secondsToAdd;

			// Paso 2: normalizar segundos a minutos
			if (Second >= 60 || Second < 0)
			{
				int deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
				Second = Second % 60;                // los segundos "sobrantes"
				if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
				Minute += deltaMinutes;              // agregamos los minutos "extra"
			}

			// Paso 3: normalizar minutos a horas
			if (Minute >= 60 || Minute < 0)
			{
				int deltaHours = Minute / 60;        // cuántas horas completas
				Minute = Minute % 60;                // los minutos restantes
				if (Minute < 0) Minute += 60;        // ajustar si es negativo
				Hour += deltaHours;                  // agregamos las horas "extra"
			}

			// Paso 4: normalizar horas a un ciclo de 24h
			if (Hour >= 24 || Hour < 0)
			{
				Hour = Hour % 24;                    // rollover en 24h
				if (Hour < 0) Hour += 24;            // ajustar si es negativo
			}

			// 🎉 Hora final: tu HourTime ya no explota ni viaja en el tiempo
			// Ejemplo divertido: 25:75:80 ya no existe, ahora es 02:16:20
		}
		public void AddSeconds(int secondsToAdd, bool OverFlowHours, bool OverFlowMinutes = true, bool OverFlowSeconds = true)
		{
			// Paso 1: sumar los segundos al total actual
			Second += secondsToAdd;

			if (OverFlowSeconds)
			{
				// Paso 2: normalizar segundos a minutos
				if (Second >= 60 || Second < 0)
				{
					int deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
					Second = Second % 60;                // los segundos "sobrantes"
					if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
					Minute += deltaMinutes;              // agregamos los minutos "extra"
				}
			}
			if (OverFlowMinutes)
			{
				// Paso 3: normalizar minutos a horas
				if (Minute >= 60 || Minute < 0)
				{
					int deltaHours = Minute / 60;        // cuántas horas completas
					Minute = Minute % 60;                // los minutos restantes
					if (Minute < 0) Minute += 60;        // ajustar si es negativo
					Hour += deltaHours;                  // agregamos las horas "extra"
				}
			}
			if (OverFlowHours)
			{
				// Paso 4: normalizar horas a un ciclo de 24h
				if (Hour >= 24 || Hour < 0)
				{
					Hour = Hour % 24;                    // rollover en 24h
					if (Hour < 0) Hour += 24;            // ajustar si es negativo
				}
			}

			// 🎉 AddSeconds edición OverFlow:
			// Aquí decides si tu reloj es un ciudadano modelo o un anarquista del tiempo.
			//
			// 🔧 Con OverFlowSeconds/Minutes/Hours en true:
			//   - El tiempo se normaliza como un reloj cuerdo.
			//   - Ejemplo: 25:75:80 → 02:16:20
			//
			// 🚀 Con alguno en false:
			//   - El reloj acumula valores como si fueran puntos de experiencia.
			//   - Ejemplo: (OverFlowHours = false) 25:75:80 → 26:16:20
			//
			// 🤪 Con todos en false:
			//   - Bienvenido al modo “reloj mutante”.
			//   - Ejemplo: 25:75:80 → 25:75:80
			//   - Sí, ahora existen los 80 segundos y nadie puede detenerlos.
			//

		}

		public static bool operator ==(HourTime a, HourTime b)
		{
			if (b is null && a is not null) return false;
			if (b is null && a is null) return true;
			return (a.Hour == b.Hour) && (a.Minute == b.Minute);
		}
		public static bool operator !=(HourTime a, HourTime b)
		{
			return !(a == b);
		}
		public static bool operator ==(HourTime a, DateTime b)
		{
			return (a.Hour == b.Hour) && (a.Minute == b.Minute);
		}
		public static bool operator !=(HourTime a, DateTime b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{

			if (obj == null) return false;
			if (obj is HourTime)
			{
				return this == (HourTime)obj;
			}
			else if (obj is DateTime dt)
			{
				return (dt.Hour == Hour) && (dt.Minute == Minute);
			}
			else return false;
		}
		public string TimeOfDayString()
		{

			if (Hour >= 5 && Hour < 8) return "Morning";
			else if (Hour < 12 && Hour > 7) return "Day";
			else if (Hour == 12) return "Noon";
			else if (Hour > 12 && Hour < 17) return "Afternoon";
			else if (Hour >= 17 && Hour < 20) return "Evening";
			else if (Hour >= 20 && Hour < 23) return "Night";
			else return "MidNight";
		}
		public TimeOfDay TimeOfDayEnum()
		{
			if (Hour >= 5 && Hour < 8) return TimeOfDay.Morning;
			else if (Hour < 12 && Hour > 7) return TimeOfDay.Day;
			else if (Hour == 12) return TimeOfDay.Noon;
			else if (Hour > 12 && Hour < 17) return TimeOfDay.afternoon;
			else if (Hour >= 17 && Hour < 20) return TimeOfDay.evening;
			else if (Hour >= 20 && Hour < 23) return TimeOfDay.Night;
			else return TimeOfDay.MidNight;
		}


		public long TotalSeconds()
		{
			return Second + (Minute * 60) + (Hour * 60 * 60);
		}
	}
	[Serializable]
	public class FixedPointDayTime
	{
		public Fixed128 Year;
		public Fixed128 Month;
		public Fixed128 Week;
		public Fixed128 Day;
		public Fixed128 Hour;
		public Fixed128 Minute;
		public Fixed128 Second;
		public Fixed128 WeekLenght = 7;
		public Fixed128 MonthLenghtInWeeks = 4;
		public Fixed128 YearLenghtInMonths = 12;
		public FixedPointDayTime(Fixed128 value)
		{
			Year = 0;
			Month = 0;
			Week = 0;
			Day = value;
			Hour = 0;
			Minute = 0;
			Second = 0;
		}


		public FixedPointDayTime(DateTime time)
		{
			Year = time.Year;
			Month = time.Month;
			Week = time.Day / 4;
			Day = (Fixed128)(int)time.DayOfWeek;
			Hour = time.Hour;
			Minute = time.Minute;
			Second = time.Second;
		}

		public FixedPointDayTime(Fixed128 year, Fixed128 month, Fixed128 week, Fixed128 day, Fixed128 hour, Fixed128 minute, Fixed128 second) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = second;
		}
		public FixedPointDayTime(Fixed128 year, Fixed128 month, Fixed128 week, Fixed128 day, Fixed128 hour, Fixed128 minute) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = 0;
		}
		public FixedPointDayTime(Fixed128 year, Fixed128 month, Fixed128 week, Fixed128 day, Fixed128 hour) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = hour;
			Minute = 0;
			Second = 0;
		}
		public FixedPointDayTime(Fixed128 year, Fixed128 month, Fixed128 week, Fixed128 day) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = 0;
			Minute = 0;
			Second = 0;
		}
		public FixedPointDayTime(FixedPointDayTime Copy)
		{
			this.Year = Copy.Year;
			this.Month = Copy.Month;
			this.Week = Copy.Week;
			this.Day = Copy.Day;
			this.Hour = Copy.Hour;
			this.Minute = Copy.Minute;
			this.Second = Copy.Second;
			WeekLenght = Copy.WeekLenght;
			MonthLenghtInWeeks = Copy.MonthLenghtInWeeks;
			YearLenghtInMonths = Copy.YearLenghtInMonths;
		}

		public bool IsCompatible(FixedPointDayTime Other)
		{
			bool MW = this.MonthLenghtInWeeks == Other.MonthLenghtInWeeks;
			bool WD = this.WeekLenght == Other.WeekLenght;
			bool YM = this.YearLenghtInMonths == Other.YearLenghtInMonths;
			return MW && WD && YM;
		}
		public string To12Hour()
		{
			if (Hour < 12) return this.ToString() + "AM";
			var v = Hour - 12;
			return $"{new FixedPointDayTime(Year, Month, Week, Day, v, Minute).ToString()}PM";
		}
		public override string ToString()
		{
			if (Minute < 10)
			{
				return $"{Year}-{Month}-{Week}-{Day} {Hour}:0{Minute}";

			}
			return $"{Year}-{Month}-{Week}-{Day} {Hour}:{Minute}";
		}
		public override int GetHashCode()
		{
			return Hour.GetHashCode() ^ Minute.GetHashCode();
		}
		public void AddSeconds(Fixed128 secondsToAdd)
		{
			// Paso 1: sumar los segundos al total actual
			Second += secondsToAdd;

			// Paso 2: normalizar segundos a minutos
			if (Second >= 60 || Second < 0)
			{
				Fixed128 deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
				Second = Second % 60;                // los segundos "sobrantes"
				if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
				Minute += deltaMinutes;              // agregamos los minutos "extra"
			}

			// Paso 3: normalizar minutos a horas
			if (Minute >= 60 || Minute < 0)
			{
				Fixed128 deltaHours = Minute / 60;        // cuántas horas completas
				Minute = Minute % 60;                // los minutos restantes
				if (Minute < 0) Minute += 60;        // ajustar si es negativo
				Hour += deltaHours;                  // agregamos las horas "extra"
			}

			// Paso 4: normalizar horas a un ciclo de 24h
			if (Hour >= 24 || Hour < 0)
			{
				Fixed128 DeltaDays = Hour / 24;
				Hour = Hour % 24;
				if (Hour < 0) Hour += 24;
				Day += DeltaDays;
			}
			if (WeekLenght <= 0) WeekLenght = 7;
			// Paso 5 normalizar Dias a una semana
			if (Day >= WeekLenght || Day < 0)
			{
				Fixed128 DeltaWeeks = Day / WeekLenght;
				Day = Day % WeekLenght;
				if (Day < 0) Day += WeekLenght;
				Week += DeltaWeeks;
			}
			if (MonthLenghtInWeeks <= 0) MonthLenghtInWeeks = 4;
			// Paso 6 Normalizar Semanas a meses
			if (Week >= MonthLenghtInWeeks || Week < 0)
			{
				Fixed128 DeltaMonths = Week / MonthLenghtInWeeks;
				Week = Week % MonthLenghtInWeeks;
				if (Week < 0) Week += MonthLenghtInWeeks;
				Month += DeltaMonths;
			}
			if (YearLenghtInMonths <= 0) YearLenghtInMonths = 12;
			// Paso 7 Normalizar meses a años
			if (Month >= YearLenghtInMonths || Month < 0)
			{
				Fixed128 DeltaMonths = Month / YearLenghtInMonths;
				Month = Month % YearLenghtInMonths;
				if (Month < 0) Month += YearLenghtInMonths;
				Year += DeltaMonths;
			}


		}

		public static bool operator ==(FixedPointDayTime a, FixedPointDayTime b)
		{
			if (b is null && a is not null) return false;
			if (b is null && a is null) return true;
			return (a.Hour == b.Hour) && (a.Minute == b.Minute)
				&& (a.Day == b.Day) //day equivale a day of the week
				&& (a.Week == b.Week)
				&& (a.Month == b.Month)
				&& (a.Year == b.Year)
				&& (a.WeekLenght == b.WeekLenght)
				&& (a.MonthLenghtInWeeks == b.MonthLenghtInWeeks)
				&& (a.YearLenghtInMonths == b.YearLenghtInMonths);
		}
		public static bool operator !=(FixedPointDayTime a, FixedPointDayTime b)
		{
			return !(a == b);
		}
		public static bool operator ==(FixedPointDayTime a, DateTime b)
		{
			return (a.Hour == b.Hour) && (a.Minute == b.Minute)
				&& (a.Day == (int)b.DayOfWeek) //day equivale a day of the week
				&& (a.Week == b.Day / 7)
				&& (a.Month == b.Month)
				&& (a.Year == b.Year)
				&& (a.WeekLenght == 7)
				&& (a.MonthLenghtInWeeks == 4)
				&& (a.YearLenghtInMonths == 12);
		}
		public static bool operator !=(FixedPointDayTime a, DateTime b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{
			return obj switch
			{
				FixedPointDayTime => this == (FixedPointDayTime)obj,
				DateTime dt => this == dt,
				_ => false
			};
		}
		public string TimeOfDayString()
		{

			if (Hour >= 5 && Hour < 8) return "Morning";
			else if (Hour < 12 && Hour > 7) return "Day";
			else if (Hour == 12) return "Noon";
			else if (Hour > 12 && Hour < 17) return "Afternoon";
			else if (Hour >= 17 && Hour < 20) return "Evening";
			else if (Hour >= 20 && Hour < 23) return "Night";
			else return "MidNight";
		}
		public TimeOfDay TimeOfDayEnum()
		{
			if (Hour >= 5 && Hour < 8) return TimeOfDay.Morning;
			else if (Hour < 12 && Hour > 7) return TimeOfDay.Day;
			else if (Hour == 12) return TimeOfDay.Noon;
			else if (Hour > 12 && Hour < 17) return TimeOfDay.afternoon;
			else if (Hour >= 17 && Hour < 20) return TimeOfDay.evening;
			else if (Hour >= 20 && Hour < 23) return TimeOfDay.Night;
			else return TimeOfDay.MidNight;
		}

		public double TotalSeconds()
		{
			Fixed128 secs = 0;
			secs += Year * YearLenghtInMonths * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Month * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Week * WeekLenght * 24 * 60 * 60;
			secs += Day * 24 * 60 * 60;
			secs += Hour * 3600 + Minute * 60 + Second;
			return secs.ToDouble();
		}
		public Fixed128 TotalSecondsFixed128()
		{
			Fixed128 secs = 0;
			secs += Year * YearLenghtInMonths * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Month * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Week * WeekLenght * 24 * 60 * 60;
			secs += Day * 24 * 60 * 60;
			secs += Hour * 3600 + Minute * 60 + Second;
			return secs;
		}

	}
	//código hecho por ChatGPT probablemente es de pésima calidad
	public static class StageLoader
	{
		public static void LoadStageFromSavePath(string PAth)
		{
			var Filepath = /*Path.Join(*/PAth/*)*/; //si  no
			SavedGame game = JsonUtility.FromJson<SavedGame>(File.ReadAllText(Filepath));
			Stages stage = game.CurentStage;
			string creatureName = game.CreatureName;

			CrossScenePackageSender mailMan = CrossScenePackageSender.Instance;

			switch (stage)
			{
				case Stages.Microbe:
					if (!string.IsNullOrEmpty(creatureName))
						LoadMicrobeStage(mailMan, creatureName);
					else
						LoadEmptyMicrobe(mailMan);
					break;

				case Stages.Creature:
				case Stages.tribal:
				case Stages.City:
				case Stages.Civilization:
					Debug.Log($"Stage {stage} aún no implementado. Solo carga Space o Microbe por ahora.");
					break;

				case Stages.Space:
					Debug.Log("Cargando Space Stage...");
					SceneManager.LoadScene(6);
					break;
				case Stages.MainMenu:
					LoadWithLoadingScreen.LoadScene(0,Stages.MainMenu);
					break;
				default:
					Debug.LogWarning($"Stage {stage} no tiene escena asignada");
					break;
			}
		}
		public static void LoadStage(Stages stage, string creatureName = null)
		{
			CrossScenePackageSender mailMan = CrossScenePackageSender.Instance;

			switch (stage)
			{
				case Stages.Microbe:
					if (!string.IsNullOrEmpty(creatureName))
						LoadMicrobeStage(mailMan, creatureName);
					else
						LoadEmptyMicrobe(mailMan);
					break;

				case Stages.Creature:
				case Stages.tribal:
				case Stages.City:
				case Stages.Civilization:
					Debug.Log($"Stage {stage} aún no implementado. Solo carga Space o Microbe por ahora.");
					break;

				case Stages.Space:
					Debug.Log("Cargando Space Stage...");
					LoadWithLoadingScreen.LoadScene(6, stage);
					break;
				case Stages.MainMenu:
					LoadWithLoadingScreen.LoadScene(0, stage);
					break;
				default:
					Debug.LogWarning($"Stage {stage} no tiene escena asignada");
					break;
			}
		}

		private static void LoadEmptyMicrobe(CrossScenePackageSender mailMan)
		{
			if (mailMan == null)
			{
				Debug.LogError("No se pudo obtener CrossScenePackageSender");
				return;
			}

			MicrobeData emptyMicrobe = MicrobeData.GetDefaultMicrobe();

			mailMan.SendTypedPackage("StageLoader", "Player", emptyMicrobe, new string[] { nameof(MicrobeData) });
			LoadWithLoadingScreen.LoadScene(4, Stages.Microbe); // Microbe stage
		}

		public static void LoadMicrobeStage(CrossScenePackageSender mailMan, string creatureName)
		{
			if (mailMan == null)
			{
				Debug.LogError("No se pudo obtener CrossScenePackageSender");
				return;
			}
			string filePath;
			if (string.IsNullOrEmpty(Saver.CurrentSaveName))

				filePath = Path.Combine(Paths.Cells, $"{creatureName}.json");
			else
				filePath = null;

			if (!File.Exists(filePath) && string.IsNullOrEmpty(Saver.CurrentSaveName))
			{
				Debug.LogWarning($"Archivo no encontrado: {filePath} y no se esta cargando desde Saver, cargando microbio vacío");
				LoadEmptyMicrobe(mailMan);
				return;
			}
			MicrobeData microbe;
			try
			{
				if (filePath != null)
				{
					string json = File.ReadAllText(filePath);

					microbe = JsonUtility.FromJson<MicrobeData>(json);
					Debug.Log("cargado el microbio " + filePath);
				}
				else
					Saver.TryToLoadLastMicrobeRevision(Saver.CurrentSaveName, out microbe);
				if (microbe == null )
				{
					Debug.LogWarning("SavedGame no tiene criatura válida, cargando microbio vacío");
					LoadEmptyMicrobe(mailMan);
					return;
				}
				microbe.CenterMicrobe();
				microbe.RotateMicrobeEuler(new(0, 90, 0)); 

				mailMan.SendTypedPackage("StageLoader", "Player", microbe, new string[] { nameof(MicrobeData) });
				LoadWithLoadingScreen.LoadScene(4,Stages.Microbe); // Microbe stage
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error al cargar microbio: {ex.Message}, cargando microbio vacío");
				LoadEmptyMicrobe(mailMan);
			}
		}
		/// <summary>
		/// cargara elestado correspondiente a la partida cargada en saver
		/// </summary>
		
		public static void LoadCurrentStage()
		{
			if (Saver.CurrentSaveName == null|| Saver.CurrentGame == null)
			{
				Debug.LogError("No hay partida cargada en Saver");
				return;
			}
			switch(Saver.CurrentGame.CurentStage)
			{
				case Stages.Microbe:
					LoadMicrobeStage(CrossScenePackageSender.Instance, Saver.CurrentGame.CreatureName);
					break;
				case Stages.Creature:
					Debug.Log("por favor espera un momento aun no esta lo sufuciente desarrollado");
					break;
				case Stages.tribal:
				case Stages.City:
				case Stages.Civilization:
					Debug.Log("no implementado aun");
					break;
				case Stages.Space:
					Debug.Log("Cargando Space Stage...");
					LoadWithLoadingScreen.LoadScene(6, Stages.Space);
					break;
				default:
					throw new NotImplementedException($"Carga de estado {Saver.CurrentGame.CurentStage} no implementada");
			}
		}
	}
	public static class EditorLoader
	{
		public static void EditMicrobe(string Name, bool loadStage)
		{
			if (CreationLoader.TryToLoadMicrobe(Name, out var data))
			{
				CreationLoader.BackUpMicrobe(Name);
				CrossScenePackageSender Mailman = CrossScenePackageSender.Instance;//No  puedo cambiar esos nombres de destinatario de MC yMain Camera CS por que el cartero No tiene codigo postal solo nombre de destinatario :(
				Mailman.SendTypedPackage("EnterEdit", "CellSaver", loadStage, new string[2] { nameof(Boolean), "LodStg" }); //avisarle a cellsaver QUE AL GUARDAR ENTRAREMOS AL ESTADIO CELULA DIGO MICROBIO SIN CREAR NUEVA PARTIDA
				Mailman.SendTypedPackage("EditorLoader", "MC.SegmentManager", data, new string[2] { nameof(MicrobeData), "LodMic" }); //avisarle al segment manager que TIENE QUE CARGAR UNA CRIATRURA
				SceneManager.LoadScene(1); // Microbe Editor
			}
		}

		[ConsoleCommand(Name ="!editarmic")]
		public static void EditMicrobeCommand()
		{
			//asume que estas en el estadio celula 
			if (Saver.HasLoadedAnySave())
			{
				Debug.Log("ENTRANDO AL EDITOR, ADVERTENCIA ESTO NO ESTA PROVADO ASI QUE PODRIA CORROMPER TU HERMOSA CREACIÓN");
				EditMicrobe();
			}else
			{
				Debug.Log("ENTORNO INVALIDO");

			}

		}

		public static void EditMicrobe()
		{
			if (!Saver.HasLoadedAnySave())
			{
				CrossScenePackageSender Mailman = CrossScenePackageSender.Instance;//No  puedo cambiar esos nombres de destinatario de MC yMain Camera CS por que el cartero No tiene codigo postal solo nombre de destinatario :(
				Mailman.SendTypedPackage("EnterEdit", "CellSaver", false, new string[2] { nameof(Boolean), "LodStg" }); //avisarle a cellsaver QUE AL GUARDAR NO ENTRAREMOS AL ESTADIO CELULA DIGO MICROBIO
				LoadWithLoadingScreen.LoadScene(1, Stages.Microbe); // Microbe Editor
				return;
			}
			if (Saver.TryToLoadLastMicrobeRevision(Saver.CurrentSaveName, out var data)) //por error usaba el nombre de microbio no del archivo de guardado(eso es SHA) el nombre del microbio es lo que sea que escribio el Jugador;
			{
				CrossScenePackageSender Mailman = CrossScenePackageSender.Instance;//No  puedo cambiar esos nombres de destinatario de MC yMain Camera CS por que el cartero No tiene codigo postal solo nombre de destinatario :(
				Mailman.SendTypedPackage("EnterEdit", "CellSaver", true, new string[2] { nameof(Boolean), "LodStg" }); //avisarle a cellsaver QUE AL GUARDAR ENTRAREMOS AL ESTADIO CELULA DIGO MICROBIO SIN CREAR NUEVA PARTIDA
				Mailman.SendTypedPackage("EditorLoader", "MC.SegmentManager", data, new string[2] { nameof(MicrobeData), "LodMic" }); //avisarle al segment manager que TIENE QUE CARGAR UNA CRIATRURA
				LoadWithLoadingScreen.LoadScene(1,Stages.Microbe); // Microbe Editor
				return;
			}
		}
		//simbologia:
		//MC					: Marching Cubes
		//Main Camera CS		: Basurero con componentes distintos que tambien renderiza la escena y guarda el microbio
		//"EnterEdit"			: El Sender que le avisa a MicrobeSaver QUE YA HAY UNA PARTIDA Y NO TIENE QUE CREAR OTRA
		//"CellSaver"			: Se añadio la capacidad que el Mailman te de paquetes basándote en un string en vez de game objects asi que ya no tengo que escribir el nombre de la cámara solo "CellSaver"
		//"MC.SegmentManager"	: se especifica que es el segment manager de MC y no el componente de marching cubes
		public static void EnterEditor(Editors editor)
		{
			switch (editor)
			{
				case Editors.Microbe:
					EditMicrobe();
					break;
				case Editors.Animal:
					break;
				case Editors.TribeDresser:
					break;
				case Editors.FeudalCitizenDresser:
					break;
				case Editors.CitizenDresser:
					break;
				case Editors.SpaceCitizenDresser:
					break;
				case Editors.Plant:
					SceneManager.LoadSceneAsync(5);
					break;
				case Editors.Planet:
					break;
				case Editors.Vehicles_Car:
					break;
				case Editors.Vehicles_Car_Religius:
					break;
				case Editors.Vehicles_Car_Economic:
					break;
				case Editors.Vehicles_Car_Military:
					break;
				case Editors.Vehicles_Car_Civilian:
					break;
				case Editors.Vehicles_Car_BUS:
					break;
				case Editors.Vehicles_Train_Steam_Civilian:
					break;
				case Editors.Vehicles_Train_Steam_Military:
					break;
				case Editors.Vehicles_Train_Steam_Economic:
					break;
				case Editors.Vehicles_Train_Electrical_Metro:
					break;
				case Editors.Vehicles_Train_Electrical_Tram:
					break;
				case Editors.Vehicles_Train_Electrical_Monorail:
					break;
				case Editors.Vehicles_Train_Electrical_MagLev:
					break;
				case Editors.Vehicles_Train_Electrical_LightTrain:
					break;
				case Editors.Vehicles_Train_Electrical_Suburban:
					break;
				case Editors.Vehicles_Train_Electrical_Bullet:
					break;
				case Editors.Vehicles_Train_TrainLike_CableCar:
					break;
				case Editors.Vehicles_Plane_Civilian:
					break;
				case Editors.Vehicles_Plane_Military:
					break;
				case Editors.Vehicles_Plane_Economic:
					break;
				case Editors.Vehicles_Plane_Religous:
					break;
				case Editors.Vehicles_Boat_Civilian:
					break;
				case Editors.Vehicles_Boat_Military:
					break;
				case Editors.Vehicles_Boat_Economic:
					break;
				case Editors.Vehicles_Boat_Religous:
					break;
				case Editors.Vehicles_Boat_Canoe:
					break;
				default:
					break;
			}
		}

		[ConsoleCommand(Name = "entereditor")] //esto no es debug solo es para que los creadores de contenido puedan entrar al editor sin necesidad de cargar una partida o algo asi, es un comando para facilitar la creación de contenido
		public static void EnterEditorCommand(int editor)
		{
			if (Enum.IsDefined(typeof(Editors), editor))
			{
				EnterEditor((Editors)editor);
			}
			else
			{
				Debug.LogError($"Editor con ID {editor} no existe.");
			}
		}

	}
	public static class CreationLoader
	{
		public static bool TryToLoadMicrobe(string name,out MicrobeData data)
		{
			var Jsons = Directory.GetFiles(Paths.Cells);
			foreach (var json in Jsons)
			{
				string Jsontex = File.ReadAllText(json);
				try
				{
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(Jsontex);
					if (microbe.Name == name)
					{
						data = microbe;
						return true;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			data = MicrobeData.GetDefaultMicrobe();
			return false;
		}
		public static bool TryToLoadMicrobe(string name, int revission, out MicrobeData data)
		{
			var files = Directory.GetFiles(Paths.BackUPCells);
			// Ordenar por fecha de modificación ascendente (más antiguos primero)
			var sortedFiles = files.OrderBy(f => File.GetLastWriteTime(f)).ToList();

			List<MicrobeData> Revisions = new();
			foreach (var json in sortedFiles)
			{
				string Jsontex = File.ReadAllText(json);
				try
				{
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(Jsontex);
					if (microbe.Name == name)
						Revisions.Add(microbe);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			if (Revisions.Count > 0)
			{
				data = Revisions[revission];
				return true;
			}
			else { 
				data = MicrobeData.GetDefaultMicrobe();
				return false;

			}
		}
		public static MicrobeData LoadMicrobe(string name)
		{
			var Jsons = Directory.GetFiles(Paths.Cells);
			foreach (var json in Jsons)
			{
				string Jsontex = File.ReadAllText(json);
				try
				{
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(Jsontex);
					if (microbe.Name == name)
						return microbe;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			return MicrobeData.GetDefaultMicrobe();
		}
		public static MicrobeData LoadMicrobe(string name, int revission)
		{
			var files = Directory.GetFiles(Paths.BackUPCells);
			// Ordenar por fecha de modificación ascendente (más antiguos primero)
			var sortedFiles = files.OrderBy(f => File.GetLastWriteTime(f)).ToList();

			List<MicrobeData> Revisions = new();
			foreach (var json in sortedFiles)
			{
				string Jsontex = File.ReadAllText(json);
				try
				{
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(Jsontex);
					if (microbe.Name == name)
						Revisions.Add(microbe);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			if (Revisions.Count > 0)
				return Revisions[revission];
			else return MicrobeData.GetDefaultMicrobe();
		}
		public static MicrobeData LoadLastMicrobeRevision(string name)
		{
			var files = Directory.GetFiles(Paths.BackUPCells);
			// Ordenar por fecha de modificación ascendente (más antiguos primero)
			var sortedFiles = files.OrderBy(f => File.GetLastWriteTime(f)).ToList();

			List<MicrobeData> Revisions = new();
			foreach (var json in sortedFiles)
			{
				string Jsontex = File.ReadAllText(json);
				try
				{
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(Jsontex);
					if (microbe.Name == name)
						Revisions.Add(microbe);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			if (Revisions.Count > 0)
			{
				int revission = Revisions.Count - 1;
				return Revisions[revission];
			}
			else return MicrobeData.GetDefaultMicrobe();

		}
		public static bool TryToLoadLastMicrobeRevision(string name, out MicrobeData data)
		{
			var files = Directory.GetFiles(Paths.BackUPCells);
			// Ordenar por fecha de modificación ascendente (más antiguos primero)
			var sortedFiles = files.OrderBy(f => File.GetLastWriteTime(f)).ToList();

			List<MicrobeData> Revisions = new();
			foreach (var json in sortedFiles)
			{
				string Jsontex = File.ReadAllText(json);
				try
				{
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(Jsontex);
					if (microbe.Name == name)
						Revisions.Add(microbe);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			if (Revisions.Count > 0)
			{
				int revission = Revisions.Count - 1;
				data = Revisions[revission];
				return true;
			}
			else {
				data =
				 MicrobeData.GetDefaultMicrobe();
				return false;
			}

		}
		public static void BackUpMicrobe(string name)
		{
			if (TryToLoadMicrobe(name, out MicrobeData microbe))
			{
				try
				{
					var steam = File.CreateText(Path.Combine(MicrobeData.GenerateMicrobeID(microbe) + ".json"));
					steam.Write(JsonUtility.ToJson(microbe));
					
				} catch(Exception ex) { Debug.LogError(ex);}
			}else
			{ }
		}
		public static bool TryToBackUpMicrobe(string name)
		{
			if (TryToLoadMicrobe(name, out MicrobeData microbe))
			{
				try
				{
					var steam = File.CreateText(Path.Combine(MicrobeData.GenerateMicrobeID(microbe) + ".json"));
					steam.Write(JsonUtility.ToJson(microbe));
					return true;
				} catch(Exception ex) { Debug.LogError(ex);return false;}
			}else
			{ return false; }
		}
	}

	public static class LoadWithLoadingScreen
	{
		public static async void LoadScene(int id, Stages Stage)
		{
			Debug.Log("Loading " + id + " related to " + Stage);
			await CreateLoadingScreen(Stage);
			await SceneManager.LoadSceneAsync(id);
		}

		public static async void LoadScene(string id, Stages Stage)
		{
			await CreateLoadingScreen(Stage);
			await SceneManager.LoadSceneAsync(id);
		}

		private static async System.Threading.Tasks.Task CreateLoadingScreen(Stages Stage)
		{
			// 🔹 Cargar ConfigLoadScreen desde Addressables
			var handle = Addressables.LoadAssetAsync<ConfigLoadScreen>("Assets/GLSS"); // "GLSS" es el Address que yo le puse
			await handle.Task;
			ConfigLoadScreen loadScreenConfig = handle.Result;

			if (loadScreenConfig == null)
			{
				Debug.LogError("No se pudo cargar ConfigLoadScreen desde Addressables.");
				return;
			}

			// 🔹 Crear Canvas
			GameObject LoadSc = new("LoadingScreen");
			Canvas C = LoadSc.AddComponent<Canvas>();
			C.renderMode = RenderMode.ScreenSpaceOverlay;
			C.sortingOrder = 999;
			LoadSc.layer = 5;

			// 🔹 Crear Image
			GameObject ImageGO = new("IMG");
			UnityEngine.UI.Image IMG = ImageGO.AddComponent<UnityEngine.UI.Image>();

			IMG.sprite = Stage switch
			{
				Stages.Microbe => loadScreenConfig.CellLoadImg,
				Stages.Creature => loadScreenConfig.CreatureLoadImg,
				Stages.tribal => loadScreenConfig.TribeLoadImg,
				Stages.City => loadScreenConfig.FeudalLoadImg,
				Stages.Civilization => loadScreenConfig.NationLoadImg,
				Stages.Space => loadScreenConfig.SpaceLoadImg,
				Stages.MainMenu => loadScreenConfig.MainMenuLoadImg,
				_ => loadScreenConfig.CellLoadImg,
			};

			IMG.color = Color.white;
			IMG.rectTransform.SetParent(LoadSc.transform, false);
			IMG.rectTransform.anchorMin = Vector2.zero;
			IMG.rectTransform.anchorMax = Vector2.one;
			IMG.rectTransform.offsetMin = Vector2.zero;
			IMG.rectTransform.offsetMax = Vector2.zero;
			if (Stage == Stages.MainMenu)
				GameObject.DontDestroyOnLoad(LoadSc);
			// 🔹 Liberar handle cuando ya no lo necesitamos
			Addressables.Release(handle); // para no saturar la ram
		}
	}



	[Serializable]
	public class DayTime
	{
		public long Year;
		public long Month;
		public long Week;
		public long Day;
		public long Hour;
		public long Minute;
		public long Second;
		public long WeekLenght = 7;
		public long MonthLenghtInWeeks = 4;
		public long YearLenghtInMonths = 12;
		public DayTime(long value)
		{
			Year = 0;
			Month = 0;
			Week = 0;
			Day = value;
			Hour = 0;
			Minute = 0;
			Second = 0;
		}


		public DayTime(DateTime time)
		{
			Year = time.Year;
			Month = time.Month;
			Week = time.Day / 4;
			Day = (long)time.DayOfWeek;
			Hour = time.Hour;
			Minute = time.Minute;
			Second = time.Second;
		}

		public DayTime(long year, long month, long week, long day, long hour, long minute, long second) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = second;
		}
		public DayTime(long year, long month, long week, long day, long hour, long minute) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = 0;
		}
		public DayTime(long year, long month, long week, long day, long hour) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = hour;
			Minute = 0;
			Second = 0;
		}
		public DayTime(long year, long month, long week, long day) : this(year)
		{
			Month = month;
			Week = week;
			Day = day;
			Hour = 0;
			Minute = 0;
			Second = 0;
		}
		public DayTime(DayTime Copy)
		{
			this.Year = Copy.Year;
			this.Month = Copy.Month;
			this.Week = Copy.Week;
			this.Day = Copy.Day;
			this.Hour = Copy.Hour;
			this.Minute = Copy.Minute;
			this.Second = Copy.Second;
			WeekLenght = Copy.WeekLenght;
			MonthLenghtInWeeks = Copy.MonthLenghtInWeeks;
			YearLenghtInMonths = Copy.YearLenghtInMonths;
		}

		public bool IsCompatible(DayTime Other)
		{
			bool MW = this.MonthLenghtInWeeks == Other.MonthLenghtInWeeks;
			bool WD = this.WeekLenght == Other.WeekLenght;
			bool YM = this.YearLenghtInMonths == Other.YearLenghtInMonths;
			return MW && WD && YM;
		}
		public string To12Hour()
		{
			if (Hour < 12) return this.ToString() + "AM";
			var v = Hour - 12;
			return $"{new DayTime(Year, Month, Week, Day, v, Minute).ToString()}PM";
		}
		public override string ToString()
		{
			if (Minute < 10)
			{
				return $"{Year}-{Month}-{Week}-{Day} {Hour}:0{Minute}";

			}
			return $"{Year}-{Month}-{Week}-{Day} {Hour}:{Minute}";
		}
		public override int GetHashCode()
		{
			return Hour.GetHashCode() ^ Minute.GetHashCode();
		}
		public void AddSeconds(long secondsToAdd)
		{
			// Paso 1: sumar los segundos al total actual
			Second += secondsToAdd;

			// Paso 2: normalizar segundos a minutos
			if (Second >= 60 || Second < 0)
			{
				long deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
				Second = Second % 60;                // los segundos "sobrantes"
				if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
				Minute += deltaMinutes;              // agregamos los minutos "extra"
			}

			// Paso 3: normalizar minutos a horas
			if (Minute >= 60 || Minute < 0)
			{
				long deltaHours = Minute / 60;        // cuántas horas completas
				Minute = Minute % 60;                // los minutos restantes
				if (Minute < 0) Minute += 60;        // ajustar si es negativo
				Hour += deltaHours;                  // agregamos las horas "extra"
			}

			// Paso 4: normalizar horas a un ciclo de 24h
			if (Hour >= 24 || Hour < 0)
			{
				long DeltaDays = Hour / 24;
				Hour = Hour % 24;
				if (Hour < 0) Hour += 24;
				Day += DeltaDays;
			}
			if (WeekLenght <= 0) WeekLenght = 7;
			// Paso 5 normalizar Dias a una semana
			if (Day >= WeekLenght || Day < 0)
			{
				long DeltaWeeks = Day / WeekLenght;
				Day = Day % WeekLenght;
				if (Day < 0) Day += WeekLenght;
				Week += DeltaWeeks;
			}
			if (MonthLenghtInWeeks <= 0) MonthLenghtInWeeks = 4;
			// Paso 6 Normalizar Semanas a meses
			if (Week >= MonthLenghtInWeeks || Week < 0)
			{
				long DeltaMonths = Week / MonthLenghtInWeeks;
				Week = Week % MonthLenghtInWeeks;
				if (Week < 0) Week += MonthLenghtInWeeks;
				Month += DeltaMonths;
			}
			if (YearLenghtInMonths <= 0) YearLenghtInMonths = 12;
			// Paso 7 Normalizar meses a años
			if (Month >= YearLenghtInMonths || Month < 0)
			{
				long DeltaMonths = Month / YearLenghtInMonths;
				Month = Month % YearLenghtInMonths;
				if (Month < 0) Month += YearLenghtInMonths;
				Year += DeltaMonths;
			}


		}

		public static bool operator ==(DayTime a, DayTime b)
		{
			if (b is null && a is not null) return false;
			if (b is null && a is null) return true;
			return (a.Hour == b.Hour) && (a.Minute == b.Minute)
				&& (a.Day == b.Day) //day equivale a day of the week
				&& (a.Week == b.Week)
				&& (a.Month == b.Month)
				&& (a.Year == b.Year)
				&& (a.WeekLenght == b.WeekLenght)
				&& (a.MonthLenghtInWeeks == b.MonthLenghtInWeeks)
				&& (a.YearLenghtInMonths == b.YearLenghtInMonths);
		}
		public static bool operator !=(DayTime a, DayTime b)
		{
			return !(a == b);
		}
		public static bool operator ==(DayTime a, DateTime b)
		{
			return (a.Hour == b.Hour) && (a.Minute == b.Minute)
				&& (a.Day == (int)b.DayOfWeek) //day equivale a day of the week
				&& (a.Week == b.Day / 7)
				&& (a.Month == b.Month)
				&& (a.Year == b.Year)
				&& (a.WeekLenght == 7)
				&& (a.MonthLenghtInWeeks == 4)
				&& (a.YearLenghtInMonths == 12);
		}
		public static bool operator !=(DayTime a, DateTime b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{
			return obj switch
			{
				DayTime => this == (DayTime)obj,
				DateTime dt => this == dt,
				_ => false
			};
		}
		public string TimeOfDayString()
		{

			if (Hour >= 5 && Hour < 8) return "Morning";
			else if (Hour < 12 && Hour > 7) return "Day";
			else if (Hour == 12) return "Noon";
			else if (Hour > 12 && Hour < 17) return "Afternoon";
			else if (Hour >= 17 && Hour < 20) return "Evening";
			else if (Hour >= 20 && Hour < 23) return "Night";
			else return "MidNight";
		}
		public TimeOfDay TimeOfDayEnum()
		{
			if (Hour >= 5 && Hour < 8) return TimeOfDay.Morning;
			else if (Hour < 12 && Hour > 7) return TimeOfDay.Day;
			else if (Hour == 12) return TimeOfDay.Noon;
			else if (Hour > 12 && Hour < 17) return TimeOfDay.afternoon;
			else if (Hour >= 17 && Hour < 20) return TimeOfDay.evening;
			else if (Hour >= 20 && Hour < 23) return TimeOfDay.Night;
			else return TimeOfDay.MidNight;
		}

		public double TotalSeconds()
		{
			double secs = 0;
			secs += Year * YearLenghtInMonths * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Month * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Week * WeekLenght * 24 * 60 * 60;
			secs += Day * 24 * 60 * 60;
			secs += Hour * 3600 + Minute * 60 + Second;
			return secs;
		}
		public long TotalSecondsLong()
		{
			long secs = 0;
			secs += Year * YearLenghtInMonths * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Month * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
			secs += Week * WeekLenght * 24 * 60 * 60;
			secs += Day * 24 * 60 * 60;
			secs += Hour * 3600 + Minute * 60 + Second;
			return secs;
		}

	}
	public enum TimeOfDay
	{
		None = -1,
		Morning,
		Day,
		Noon,
		afternoon,
		evening,
		Night,
		MidNight
	}

	[Serializable]
	public struct Resolution
	{
		public ulong Width;
		public ulong Height;
		public Resolution(ulong height, ulong width)
		{
			Height = height;
			Width = width;
		}

		public static Resolution FromUnityRes(UnityEngine.Resolution resolution)
		{
			return new((ulong)resolution.height, (ulong)resolution.width);
		}
		public UnityEngine.Resolution ToUnityRes()
		{
			return new UnityEngine.Resolution() { width = (int)Width, height = (int)Height };
		}
	}
	[Serializable]
	public class Settings
	{
		public string LAST_VERSION;
		public bool UseFullScreen;
		public bool UseAniwayControlls;
		public Resolution Res;


		public static Settings Loaded;
		public static bool TryToload()
		{
			string path = Path.Join(Application.persistentDataPath, "STGS.JSON");
			try
			{
				Settings a = JsonUtility.FromJson<Settings>(path);
				Loaded = a;
				return true;
			} catch
			{
				return false;
			}
		}

		public static void Save()
		{
			string path = Path.Join(Application.persistentDataPath, "STGS.JSON");
			string JSON = JsonUtility.ToJson(Loaded);
			File.WriteAllText(path, JSON);
		}
	}
}
//utilidades de guardado y carga de partidas
namespace ActualUtils
{
	using pt = Paths;
	/// <summary>
	/// Creara y Cargara partidas en el nuevo sistema de guardado 
	/// </summary>
	public static class Saver
	{
		/// <summary>
		/// Crea una nueva partida guardada con la siguiente estructura
		/// pt.Savefiles
		///		[partida nombre en SHA 512]
		///			CreationPrivate
		///				Creatures
		///				Microbe
		///				TribalClothes
		///				FeudalCLothes 
		///				NationClothes
		///			Save.json
		/// </summary>
		/// <param name="CreatureName">Nombre de la ciratura</param>
		/// <param name="PlanetID">ID del planeta </param>
		/// <returns></returns>
		public static SavedGame CreateSavefile(string CreatureName, ulong PlanetID, out string NAME)
		{
			if (!Directory.Exists(pt.SaveFiles))
			{
				Directory.CreateDirectory(pt.SaveFiles);
			}
			string SHA = "";
			using SHA512 sHA = SHA512.Create();
			{
				string inp = DateTime.Now.ToString("o") + Random.ColorHSV().ToHexString();
				byte[] AA = Encoding.UTF8.GetBytes(inp);
				byte[] BB = sHA.ComputeHash(AA);
				// Convertir a hexadecimal
				StringBuilder sb = new StringBuilder();
				foreach (byte b in BB)
					sb.Append(b.ToString("x2"));
				SHA = sb.ToString();
			}
			NAME = SHA;
			string fil = J(pt.SaveFiles, SHA);
			string dir4;
			if (Application.platform == RuntimePlatform.WindowsPlayer ||
	Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
				dir4 = @"\\?\" + fil; // para rutas largas en windows
			else
				dir4 = fil;
			fil = dir4;
			Directory.CreateDirectory(fil);
			string CC = J(fil, "CreationPrivate");
			Directory.CreateDirectory(CC);
			Directory.CreateDirectory(J(CC, "Microbe"));
			Directory.CreateDirectory(J(CC, "Creatures"));
			Directory.CreateDirectory(J(CC, "TribalClothes"));
			Directory.CreateDirectory(J(CC, "FeudalClothes"));
			Directory.CreateDirectory(J(CC, "NationClothes"));
			MicrobeData microbeData = null;
			try
			{
				microbeData = JsonUtility.FromJson<MicrobeData>(File.ReadAllText(Path.Combine(Paths.Cells, CreatureName)));
			}
			catch { }
			SavedGame game = new()
			{
				CurentStage = Stages.Microbe,
				CreatureName = CreatureName,
				Actions = new(),
				CreatureDiet = Diets.none,
				ingameTime = 0,
				isCPUEmpire = false,
				PlanetID = PlanetID
			};
			if (microbeData != null)
			{
				game.CellGameData = new()
				{
					DNA_Amount = 0,
					MaxDNA_Got = 0,
					Gender = GéneroBiológico.Female,
					PlayerHealth = 100,
					Progress = 0,
				};
			}

			string SAV = J(fil, "Save.Json");

			string JAV = JsonUtility.ToJson(game, true);
			File.WriteAllText(SAV, JAV);

			return game;
		}
		public static string J(string a, string b) => Path.Combine(a, b); //si me da peresa escribir Path.Join

		/// <summary>
		/// List all save folders (names)
		/// </summary>
		public static List<string> ListSavefiles()
		{
			if (!Directory.Exists(pt.SaveFiles)) return new List<string>();
			string dir = Paths.SaveFiles;
			string dir4;
			if (Application.platform == RuntimePlatform.WindowsPlayer ||
	Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
				dir4 = @"\\?\" + dir; // para rutas largas en windows
			else
				dir4 = dir;
			dir = dir4;
			return Directory.GetDirectories(dir).Select(d => Path.GetFileName(d)).ToList(); //ay no no entiendo Linq
		}


		/// <summary>
		/// Try to load Save.Json for a savefolder
		/// </summary>
		public static bool TryLoadSave(string savefile, out SavedGame game)
		{
			game = null;
			string dir = J(pt.SaveFiles, savefile);
			string dir4;
			if (Application.platform == RuntimePlatform.WindowsPlayer ||
	Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
				dir4 = @"\\?\" + dir; // para rutas largas en windows
			else
				dir4 = dir;
			dir = dir4;
			string sav = J(dir, "Save.Json");
			if (!File.Exists(sav)) return false;
			try
			{
				string json = File.ReadAllText(sav);
				game = JsonUtility.FromJson<SavedGame>(json);
				return game != null;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				return false;
			}
		}

		/// <summary>
		/// Delete a savefile folder
		/// </summary>
		public static bool DeleteSavefile(string savefile)
		{
			string dir = J(pt.SaveFiles, savefile);
			string dir4;
			if (Application.platform == RuntimePlatform.WindowsPlayer ||
	Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
				dir4 = @"\\?\" + dir; // para rutas largas en windows
			else
				dir4 = dir;
			dir = dir4;
			if (!Directory.Exists(dir)) return false;
			try
			{
				Directory.Delete(dir, true);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				return false;
			}
		}

		/// <summary>
		/// Save a microbe revision into the save's Microbe folder (timestamped)
		/// </summary>
		public static bool SaveMicrobeRevision(string savefile, MicrobeData microbe)
		{
			if (microbe == null) return false;
			string dir1 = J(pt.SaveFiles, savefile);
			string dir2 = J(dir1, "CreationPrivate");
			string dir3 = J(dir2, "Microbe");
			string dir4;
			if (Application.platform == RuntimePlatform.WindowsPlayer ||
				Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
				dir4 = @"\\?\" + dir3; // para rutas largas en windows
			else
				dir4 = dir3;
			dir3 = dir4; 
			try
			{
				if (!Directory.Exists(dir3)) Directory.CreateDirectory(dir3);
				string nameSafe;
				if (string.IsNullOrEmpty(microbe.Name)) nameSafe = "microbe";
				else
				{
					var invalid = Path.GetInvalidFileNameChars();
					nameSafe = new string(microbe.Name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
				}
				string filename = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{nameSafe}.json";
				string full = Path.Combine(dir3, filename);
				File.WriteAllText(full, JsonUtility.ToJson(microbe, true));
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				return false;
			}
		}

		/// <summary>
		/// Get list of microbe revision file names ordered by modification time (ascending)
		/// </summary>
		public static List<string> GetMicrobeRevisionFiles(string savefile)
		{
			string dir1 = J(pt.SaveFiles, savefile);
			string dir2 = J(dir1, "CreationPrivate");
			string dir3 = J(dir2, "Microbe");
			if (!Directory.Exists(dir3)) return new List<string>();
			var files = Directory.GetFiles(dir3).OrderBy(f => File.GetLastWriteTime(f)).Select(Path.GetFileName).ToList();
			return files;
		}

		/// <summary>
		/// intentra cargar la ultima revision del microbio de la partida 
		/// </summary>
		public static bool TryToLoadLastMicrobeRevision(string savefile, out MicrobeData data)
		{
			data = MicrobeData.GetDefaultMicrobe();
			string dir1 = J(pt.SaveFiles, savefile);
			string dir2 = J(dir1, "CreationPrivate");
			string dir3 = J(dir2, "Microbe");

			string dir4;
			if (Application.platform == RuntimePlatform.WindowsPlayer ||
				Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
				dir4 = @"\\?\" + dir3; // para rutas largas en windows
			else
			dir4 = dir3;
			//ahora a copiarlo A OTROS METODOS 

			if (!Directory.Exists(dir4)) return false;

			var files = Directory.GetFiles(dir4);
			var sortedFiles = files.OrderBy(f => File.GetLastWriteTime(f)).ToList();

			if (sortedFiles.Count == 0) return false;
			for (int i = 0; i < sortedFiles.Count; i++)
			{
				try
				{
					string json = File.ReadAllText(sortedFiles[i]);
					MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(json);
					if (microbe != null)
						data = microbe;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					continue;
				}
			}
			// return last revision
			data = JsonUtility.FromJson<MicrobeData>(File.ReadAllText(sortedFiles.Last()));
			return data != null;
		}

		/// <summary>
		/// intenta cargar el microbio de la partida con X revision  
		/// </summary>
		public static bool TryLoadMicrobeRevission(string savefile, int revission, out MicrobeData data)
		{
			data = MicrobeData.GetDefaultMicrobe();
			string dir1 = J(pt.SaveFiles, savefile);
			string dir2 = J(dir1, "CreationPrivate");
			string dir3 = J(dir2, "Microbe");

			if (!Directory.Exists(dir2)) return false;

			var files = Directory.GetFiles(dir2);
			var sortedFiles = files.OrderBy(f => File.GetLastWriteTime(f)).ToList();
			if (sortedFiles.Count == 0) return false;
			if (revission < 0 || revission >= sortedFiles.Count) return false;
			try
			{
				string json = File.ReadAllText(sortedFiles[revission]);
				MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(json);
				if (microbe != null)
				{
					data = microbe;
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
			return false;
		}
		public static SavedGame CurrentGame;
		public static string CurrentSaveName;
		public static void LoadGameComplete(string name)
		{
			SavedGame a;
			if (!TryLoadSave(name, out a))
			{
				Debug.LogError($"No se pudo cargar la partida '{name}'");
				return;
			}
			CurrentGame = a;
			CurrentSaveName = name; //1742 lineas estoy seguro que este archivo es el mas largo del proyecto y que paso algo en ese año 1742  ah si Anders Celsius inventa la escala de temperatura que lleva su nombre.      (fuente wikipedia) XD 
			string aa = J(Paths.SaveFiles, name);
			string bb = J(aa, "Save.json");
			StageLoader.LoadStageFromSavePath(bb);

		}
		public static void UnloadCurrentGame(bool Save = false, SavedGame NewData = null)
		{
			CurrentGame = null;
			CurrentSaveName = null;
			if (Save)
			{
				CurrentGame = NewData;
				SaveCurrentGame();
			}
			LoadWithLoadingScreen.LoadScene(0, Stages.MainMenu);
		}
		public static void SaveCurrentGame()
		{
			if (CurrentGame == null || string.IsNullOrEmpty(CurrentSaveName))
			{
				Debug.LogError("No hay partida cargada para guardar");
				return;
			}
			string dir = J(Paths.SaveFiles, CurrentSaveName);
			string sav = J(dir, "Save.Json");
			try
			{
				string json = JsonUtility.ToJson(CurrentGame, true);
				File.WriteAllText(sav, json);
			}
				// 1773 lineas  segun wikipedia en 1773 17 de enero: el capitán James Cook se convierte en el primer explorador europeo en cruzar el círculo polar ártico.  (fuente: wikipedia) no no hablare del museo estadounidense que se inauguro ese año  XD
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
		}
		public static void SaveGame(SavedGame game, string saveName)
		{
			if (game == null || string.IsNullOrEmpty(saveName))
			{
				Debug.LogError("No hay partida válida para guardar");
				return;
			}
			string dir = J(Paths.SaveFiles, saveName);
			string sav = J(dir, "Save.Json");
			try
			{
				string json = JsonUtility.ToJson(game, true);
				File.WriteAllText(sav, json);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
		}
		public static void SaveCreature(string json, CreatureTypes creatureType)
		{
			if (CurrentGame == null || string.IsNullOrEmpty(CurrentSaveName))
			{
				Debug.LogError("No hay partida cargada para guardar criatura");
				return;
			}
			string dir = J(Paths.SaveFiles, CurrentSaveName);
			string creDir = J(dir, "CreationPrivate");
			string creatureFolder = creatureType switch
			{
				CreatureTypes.Microbe => "Microbe", // [insertar chiste de microbios aqui]
				CreatureTypes.Animal => "Creatures", // [insertar chiste de animales aqui]
				CreatureTypes.TribeMember => "TribalClothes", //si aunque dice Clothes se refiere a las criaturas vestidas no a la ropa en si  ¿ENTENDIDO?
				CreatureTypes.FeudalCitizen => "FeudalClothes", // [insetar chiste sobre Feudalismo aqui]
				CreatureTypes.Citizen => "NationClothes",//si es raro pero es asi
				_ => null
			};
			if (creatureFolder == null)
			{
				Debug.LogError("Tipo de criatura no válido para guardar");
				return;
			}
					// 1821 Mexico se inependizo
			string fullDir = J(creDir, creatureFolder);
			try
			{
				if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);
				string nameSafe;
				using SHA512 sHA = SHA512.Create();
				{
					string inp = DateTime.Now.ToString("o") + Random.ColorHSV().ToHexString();
					byte[] AA = Encoding.UTF8.GetBytes(inp);
					byte[] BB = sHA.ComputeHash(AA);
					// Convertir a hexadecimal
					StringBuilder sb = new StringBuilder();
					foreach (byte b in BB)
						sb.Append(b.ToString("x2"));
					nameSafe = sb.ToString();
				}
				string filename = $"{nameSafe}.json"; //nombre basado en hash de tiempo y random para evitar colisiones
				string full = Path.Combine(fullDir, filename);
				File.WriteAllText(full, json); 
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
		}

		public static string GetSaveNameForFolder(string SaveFolder)
		{
			string dir = J(Paths.SaveFiles, SaveFolder);
			string sav = J(dir, "Save.Json");
			if (!File.Exists(sav)) return null;
			try
			{
				string json = File.ReadAllText(sav);
				SavedGame game = JsonUtility.FromJson<SavedGame>(json);
				if (game != null)
					return game.CreatureName;
				else
					return null;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				return null;
			}
		}
		public static string GetFolderForSaveName(string name)
		{
			var d = ListSavefiles();
			foreach (var sav in d)
			{
				string fil2 = Path.Combine(Paths.SaveFiles, sav);
				string file = Path.Combine(fil2, "Save.json");
				if (Application.platform == RuntimePlatform.WindowsPlayer ||
Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsServer)
					file = @"\\?\" + file;
				SavedGame saved = JsonUtility.FromJson<SavedGame>(File.ReadAllText(file));
				if (saved.CreatureName ==name )
				{
					return fil2;
				}

			}
			return null;
		}
		public static bool HasLoadedAnySave() => !(CurrentGame == null || CurrentSaveName == null);
		
	}
	[System.Serializable]
	public class GameSavingException : System.Exception
	{
		public GameSavingException() { }
		public GameSavingException(string message) : base(message) { }
		public GameSavingException(string message, System.Exception inner) : base(message, inner) { }
		protected GameSavingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Atributo para comandos de la consola debug/cheats
	/// XDdddd
	/// 
	/// SIMBOLOGIA DE LOS COMANDOS DE LA CONSOLA:
	/// ! -comando debug o de una funcion en pruebas ej
	///		!editmic - abre el editor de micrboios desde el juego en el futuro cuando esta funcion sea estable se eliminara el ! y quedara solo EditMic o el comando desaparecera y el boton del menu se encargara de abrir el editor pero por ahora esta asi para evitar que alguien use un comando que no esta
	///		!loadpt -carga la generacion test de planeta es MUY debug
	/// si no hay ! o es truco o es un comando normal que no es ni truco ni debug, por ejemplo:
	///		fbxexport -exportaria o (a lo mejor jamas llegue) el modelo del jugador en formato fbx para usarlo en otros programas de modelado o lo que sea, este comando no es ni truco ni debug es solo una utilidad que podria ser util para los jugadores pero no es algo que se quiera que se use sin saber lo que hace por eso no es un comando normal sin simbolos ni nada
	///		help - muestra una lista de comandos disponibles y su descripcion, este comando es un comando normal que no es ni truco ni debug por que no hace nada malo ni es algo que se quiera ocultar a los jugadores pero tampoco es algo que se quiera que se use sin saber lo que hace por eso no es un comando normal sin simbolos ni nada
	///		
	///  no mayusculas solo minusculas por favor, gracias por su coomprension :D
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]

	public class ConsoleCommandAttribute : Attribute
	{
		public string Name;
		public string Description;
		public bool IsCheat;
		public bool IsEgg;
		public ConsoleCommandAttribute(string name) 
		{
			Name = name;
			IsCheat = false; 
		}
		public ConsoleCommandAttribute(string name, bool isCheat) : this(name)
		{
			Name = name;
			IsCheat = isCheat;
		}
		public ConsoleCommandAttribute() 
		{
			Name = "MyCommand";
			IsCheat = false;
		}
	}
	public static class PlayerManager
	{
		public static UnityEngine.MonoBehaviour Player;// el controlador del jugador actual, actualmente solo hay 2 pero en el futuro podria haber mas asi que lo dejo como MonoBehaviour para no limitarlo a un tipo especifico
		public static Stages Current_Stage;
		public static Editors Current_Editor;
		public static bool isInEditor;
		public static void RegisterPlayer(MonoBehaviour @object, Stages stage)
		{
			bool Correct = stage switch
			{
				Stages.Microbe => @object is CellController,
				_ => false,
			};
			if (Correct)
			{
				Player = @object;
				Current_Stage = stage;
			}
			else
				throw new ArgumentException("ERROR jugador no es del tipo correcto");
		}
		public static void RegisterEditor(MonoBehaviour	 @object,Editors editor)
		{
			bool Correct = editor switch
			{
				Editors.Microbe => @object is PartManager,
				Editors.Plant => @object is PlantStemGenerator,
				_ => false,
			};
			if (Correct)
			{
				Player = @object;
				Current_Editor = editor;
				isInEditor = true;
			}
			else
				throw new ArgumentException("ERROR jugador no es del tipo correcto");
		}
		public static void UnRegisterPlayer()
		{
			if (!isInEditor)
				Player = null;

		}
		public static void UnregisterEditor()
		{
			if(isInEditor) Player = null;
		}
	}
	public static class PrimitivesOBJ
	{
		public static readonly string Sphere = "# Blender 5.1.0\n# www.blender.org\nmtllib Sphere.mtl\no E\nv -0.287827 -0.287827 -0.287827\nv -0.287827 -0.287827 0.287827\nv 0.287827 -0.287827 -0.287827\nv 0.287827 -0.287827 0.287827\nv -0.288835 0.287900 -0.288835\nv -0.288835 0.287900 0.288835\nv 0.288835 0.287900 -0.288835\nv 0.288835 0.287900 0.288835\nv 0.000000 -0.500000 0.000000\nv -0.000000 0.500000 -0.000000\nv -0.500000 -0.000000 -0.000000\nv -0.000000 0.000000 0.500000\nv 0.500000 0.000000 -0.000000\nv 0.000000 -0.000000 -0.500000\nv 0.000000 -0.352515 -0.352515\nv 0.352515 -0.352515 0.000000\nv -0.000000 -0.352515 0.352515\nv -0.352515 -0.352515 0.000000\nv -0.353691 0.352788 -0.000000\nv -0.000000 0.352788 0.353691\nv 0.353691 0.352788 -0.000000\nv 0.000000 0.352788 -0.353691\nv -0.353552 0.000000 0.353552\nv -0.353691 -0.000884 -0.353691\nv 0.353552 0.000000 0.353552\nv 0.353691 -0.000884 -0.353691\nv -0.240193 -0.308901 -0.308901\nv -0.170142 -0.331348 -0.331348\nv -0.088060 -0.346970 -0.346970\nv 0.240193 -0.308901 -0.308901\nv 0.170142 -0.331348 -0.331348\nv 0.088060 -0.346970 -0.346970\nv 0.308901 -0.308901 -0.240193\nv 0.331348 -0.331348 -0.170142\nv 0.346970 -0.346970 -0.088060\nv 0.308901 -0.308901 0.240193\nv 0.331348 -0.331348 0.170142\nv 0.346970 -0.346970 0.088060\nv 0.240193 -0.308901 0.308901\nv 0.170142 -0.331348 0.331348\nv 0.088060 -0.346970 0.346970\nv -0.240193 -0.308901 0.308901\nv -0.170142 -0.331348 0.331348\nv -0.088060 -0.346970 0.346970\nv -0.308901 -0.308901 0.240193\nv -0.331348 -0.331348 0.170142\nv -0.346970 -0.346970 0.088060\nv -0.308901 -0.308901 -0.240193\nv -0.331348 -0.331348 -0.170142\nv -0.346970 -0.346970 -0.088060\nv -0.309951 0.309089 -0.240906\nv -0.332473 0.331570 -0.170713\nv -0.348127 0.347227 -0.088355\nv -0.309951 0.309089 0.240906\nv -0.332473 0.331570 0.170713\nv -0.348127 0.347227 0.088354\nv -0.240906 0.309089 0.309951\nv -0.170713 0.331570 0.332473\nv -0.088354 0.347227 0.348127\nv 0.240906 0.309089 0.309951\nv 0.170713 0.331570 0.332473\nv 0.088354 0.347227 0.348127\nv 0.309951 0.309089 0.240906\nv 0.332473 0.331570 0.170713\nv 0.348127 0.347227 0.088354\nv 0.309951 0.309089 -0.240906\nv 0.332473 0.331570 -0.170713\nv 0.348127 0.347227 -0.088355\nv 0.240906 0.309089 -0.309951\nv 0.170713 0.331570 -0.332473\nv 0.088354 0.347227 -0.348127\nv -0.240906 0.309089 -0.309951\nv -0.170713 0.331570 -0.332473\nv -0.088354 0.347227 -0.348127\nv -0.308901 -0.240193 0.308901\nv -0.331348 -0.170142 0.331348\nv -0.346970 -0.088060 0.346970\nv -0.309951 0.240173 0.309951\nv -0.332473 0.169893 0.332473\nv -0.348127 0.087507 0.348127\nv -0.309951 0.240173 -0.309951\nv -0.332473 0.169893 -0.332473\nv -0.348127 0.087507 -0.348127\nv -0.308901 -0.240193 -0.308901\nv -0.331348 -0.170142 -0.331348\nv -0.346970 -0.088060 -0.346970\nv 0.308901 -0.240193 0.308901\nv 0.331348 -0.170142 0.331348\nv 0.346970 -0.088060 0.346970\nv 0.309951 0.240173 0.309951\nv 0.332473 0.169893 0.332473\nv 0.348127 0.087507 0.348127\nv 0.308901 -0.240193 -0.308901\nv 0.331348 -0.170142 -0.331348\nv 0.346970 -0.088060 -0.346970\nv 0.309951 0.240173 -0.309951\nv 0.332473 0.169893 -0.332473\nv 0.348127 0.087507 -0.348127\nv 0.000000 -0.412363 -0.280159\nv 0.000000 -0.458555 -0.195605\nv 0.000000 -0.488232 -0.100795\nv 0.280159 -0.412363 0.000000\nv 0.195605 -0.458555 0.000000\nv 0.100795 -0.488232 0.000000\nv -0.000000 -0.412363 0.280159\nv 0.000000 -0.458555 0.195606\nv 0.000000 -0.488232 0.100795\nv -0.280159 -0.412363 0.000000\nv -0.195605 -0.458555 0.000000\nv -0.100795 -0.488232 0.000000\nv -0.281150 0.412861 -0.000000\nv -0.196276 0.459102 -0.000000\nv -0.101136 0.488857 -0.000000\nv -0.000000 0.412861 0.281150\nv -0.000000 0.459102 0.196276\nv -0.000000 0.488857 0.101136\nv 0.281150 0.412861 -0.000000\nv 0.196276 0.459102 -0.000000\nv 0.101136 0.488857 -0.000000\nv 0.000000 0.412861 -0.281150\nv 0.000000 0.459102 -0.196276\nv 0.000000 0.488857 -0.101136\nv -0.412363 -0.280159 -0.000000\nv -0.458555 -0.195605 -0.000000\nv -0.488232 -0.100795 -0.000000\nv -0.415735 0.000000 0.277786\nv -0.461938 0.000000 0.191342\nv -0.490391 0.000000 0.097545\nv -0.413748 0.280222 -0.000000\nv -0.459844 0.195459 -0.000000\nv -0.489658 0.100297 -0.000000\nv -0.413748 -0.000884 -0.281150\nv -0.459844 -0.000884 -0.196276\nv -0.489658 -0.000884 -0.101136\nv -0.000000 -0.280159 0.412363\nv -0.000000 -0.195605 0.458555\nv -0.000000 -0.100795 0.488232\nv 0.277786 0.000000 0.415735\nv 0.191342 0.000000 0.461938\nv 0.097545 0.000000 0.490391\nv -0.000000 0.280222 0.413748\nv -0.000000 0.195459 0.459844\nv -0.000000 0.100297 0.489658\nv -0.277786 0.000000 0.415735\nv -0.191342 0.000000 0.461938\nv -0.097545 0.000000 0.490391\nv 0.412363 -0.280159 0.000000\nv 0.458555 -0.195605 0.000000\nv 0.488232 -0.100795 0.000000\nv 0.413748 -0.000884 -0.281150\nv 0.459844 -0.000884 -0.196276\nv 0.489658 -0.000884 -0.101136\nv 0.413748 0.280222 -0.000000\nv 0.459844 0.195459 -0.000000\nv 0.489658 0.100297 -0.000000\nv 0.415735 0.000000 0.277786\nv 0.461938 0.000000 0.191342\nv 0.490391 0.000000 0.097545\nv -0.000000 -0.280159 -0.412363\nv 0.000000 -0.195606 -0.458555\nv -0.000000 -0.100795 -0.488232\nv -0.281150 -0.000884 -0.413748\nv -0.196276 -0.000884 -0.459844\nv -0.101136 -0.000884 -0.489658\nv 0.000000 0.280222 -0.413748\nv 0.000000 0.195459 -0.459844\nv 0.000000 0.100297 -0.489658\nv 0.281150 -0.000884 -0.413748\nv 0.196276 -0.000884 -0.459844\nv 0.101136 -0.000884 -0.489658\nv -0.248503 -0.353586 -0.248503\nv -0.174878 -0.385237 -0.263708\nv -0.090212 -0.405581 -0.275496\nv -0.263709 -0.385237 -0.174878\nv -0.183943 -0.425278 -0.183943\nv -0.094507 -0.450264 -0.191999\nv -0.275496 -0.405581 -0.090212\nv -0.192000 -0.450264 -0.094507\nv -0.098752 -0.478569 -0.098752\nv 0.248503 -0.353586 -0.248503\nv 0.263709 -0.385237 -0.174878\nv 0.275496 -0.405581 -0.090212\nv 0.174878 -0.385237 -0.263708\nv 0.183943 -0.425278 -0.183943\nv 0.192000 -0.450264 -0.094507\nv 0.090212 -0.405581 -0.275496\nv 0.094507 -0.450264 -0.191999\nv 0.098752 -0.478569 -0.098752\nv 0.248503 -0.353586 0.248503\nv 0.174878 -0.385237 0.263709\nv 0.090212 -0.405581 0.275496\nv 0.263709 -0.385237 0.174878\nv 0.183943 -0.425278 0.183943\nv 0.094507 -0.450264 0.192000\nv 0.275496 -0.405581 0.090212\nv 0.192000 -0.450264 0.094507\nv 0.098752 -0.478569 0.098752\nv -0.248503 -0.353586 0.248503\nv -0.263709 -0.385237 0.174878\nv -0.275496 -0.405581 0.090212\nv -0.174878 -0.385237 0.263709\nv -0.183943 -0.425278 0.183943\nv -0.192000 -0.450264 0.094507\nv -0.090212 -0.405581 0.275496\nv -0.094507 -0.450264 0.192000\nv -0.098752 -0.478569 0.098752\nv -0.249236 0.353860 -0.249236\nv -0.264644 0.385637 -0.175457\nv -0.276475 0.406079 -0.090519\nv -0.175457 0.385637 -0.264644\nv -0.184567 0.425781 -0.184567\nv -0.192635 0.450811 -0.094821\nv -0.090519 0.406079 -0.276475\nv -0.094821 0.450811 -0.192636\nv -0.099088 0.479199 -0.099088\nv -0.249236 0.353860 0.249236\nv -0.175457 0.385637 0.264644\nv -0.090519 0.406079 0.276475\nv -0.264644 0.385637 0.175457\nv -0.184567 0.425781 0.184567\nv -0.094821 0.450811 0.192635\nv -0.276475 0.406079 0.090519\nv -0.192635 0.450811 0.094821\nv -0.099088 0.479199 0.099087\nv 0.249236 0.353860 0.249236\nv 0.264644 0.385637 0.175457\nv 0.276475 0.406079 0.090519\nv 0.175457 0.385637 0.264644\nv 0.184567 0.425781 0.184567\nv 0.192635 0.450811 0.094821\nv 0.090519 0.406079 0.276475\nv 0.094821 0.450811 0.192635\nv 0.099088 0.479199 0.099087\nv 0.249236 0.353860 -0.249236\nv 0.175457 0.385637 -0.264644\nv 0.090519 0.406079 -0.276475\nv 0.264644 0.385637 -0.175457\nv 0.184567 0.425781 -0.184567\nv 0.094821 0.450811 -0.192636\nv 0.276475 0.406079 -0.090519\nv 0.192635 0.450811 -0.094821\nv 0.099088 0.479199 -0.099088\nv -0.353586 -0.248503 -0.248503\nv -0.385237 -0.263709 -0.174878\nv -0.405581 -0.275496 -0.090212\nv -0.385237 -0.174879 -0.263709\nv -0.425278 -0.183943 -0.183943\nv -0.450264 -0.192000 -0.094507\nv -0.405581 -0.090212 -0.275496\nv -0.450264 -0.094507 -0.192000\nv -0.478569 -0.098752 -0.098752\nv -0.353586 -0.248503 0.248503\nv -0.385237 -0.174879 0.263709\nv -0.405581 -0.090212 0.275496\nv -0.385237 -0.263709 0.174878\nv -0.425278 -0.183943 0.183943\nv -0.450264 -0.094507 0.192000\nv -0.405581 -0.275496 0.090212\nv -0.450264 -0.192000 0.094507\nv -0.478569 -0.098752 0.098752\nv -0.354751 0.248503 0.249236\nv -0.386509 0.263748 0.175457\nv -0.406968 0.275552 0.090519\nv -0.386509 0.174652 0.264644\nv -0.426504 0.183743 0.184567\nv -0.451553 0.191842 0.094821\nv -0.406968 0.089677 0.276475\nv -0.451553 0.093983 0.192635\nv -0.479956 0.098238 0.099088\nv -0.354751 0.248503 -0.249236\nv -0.386509 0.174652 -0.264644\nv -0.406968 0.089677 -0.276475\nv -0.386509 0.263748 -0.175457\nv -0.426504 0.183743 -0.184567\nv -0.451553 0.093983 -0.192635\nv -0.406968 0.275552 -0.090519\nv -0.451553 0.191842 -0.094821\nv -0.479956 0.098238 -0.099088\nv -0.248503 -0.248503 0.353586\nv -0.174879 -0.263708 0.385237\nv -0.090212 -0.275496 0.405581\nv -0.263709 -0.174879 0.385237\nv -0.183943 -0.183943 0.425278\nv -0.094507 -0.191999 0.450264\nv -0.275496 -0.090212 0.405581\nv -0.192000 -0.094507 0.450264\nv -0.098752 -0.098752 0.478569\nv 0.248503 -0.248503 0.353586\nv 0.263709 -0.174878 0.385237\nv 0.275496 -0.090212 0.405581\nv 0.174878 -0.263708 0.385237\nv 0.183943 -0.183943 0.425278\nv 0.192000 -0.094507 0.450264\nv 0.090212 -0.275496 0.405581\nv 0.094507 -0.191999 0.450264\nv 0.098752 -0.098752 0.478569\nv 0.249236 0.248503 0.354751\nv 0.175457 0.263748 0.386509\nv 0.090519 0.275552 0.406968\nv 0.264644 0.174653 0.386509\nv 0.184567 0.183743 0.426504\nv 0.094821 0.191842 0.451553\nv 0.276475 0.089677 0.406968\nv 0.192635 0.093983 0.451553\nv 0.099088 0.098238 0.479956\nv -0.249236 0.248503 0.354751\nv -0.264644 0.174653 0.386509\nv -0.276475 0.089677 0.406968\nv -0.175457 0.263748 0.386509\nv -0.184567 0.183743 0.426504\nv -0.192635 0.093983 0.451553\nv -0.090519 0.275552 0.406968\nv -0.094821 0.191842 0.451553\nv -0.099088 0.098238 0.479956\nv 0.353586 -0.248503 0.248503\nv 0.385237 -0.263709 0.174879\nv 0.405581 -0.275496 0.090212\nv 0.385237 -0.174878 0.263709\nv 0.425278 -0.183943 0.183943\nv 0.450264 -0.192000 0.094507\nv 0.405581 -0.090212 0.275496\nv 0.450264 -0.094507 0.192000\nv 0.478569 -0.098752 0.098752\nv 0.353586 -0.248503 -0.248503\nv 0.385237 -0.174878 -0.263709\nv 0.405581 -0.090212 -0.275496\nv 0.385237 -0.263709 -0.174878\nv 0.425278 -0.183943 -0.183943\nv 0.450264 -0.094507 -0.192000\nv 0.405581 -0.275496 -0.090212\nv 0.450264 -0.192000 -0.094507\nv 0.478569 -0.098752 -0.098752\nv 0.354751 0.248503 -0.249236\nv 0.386509 0.263748 -0.175457\nv 0.406968 0.275552 -0.090519\nv 0.386509 0.174653 -0.264644\nv 0.426504 0.183743 -0.184567\nv 0.451553 0.191842 -0.094821\nv 0.406968 0.089677 -0.276475\nv 0.451553 0.093983 -0.192635\nv 0.479956 0.098238 -0.099088\nv 0.354751 0.248503 0.249236\nv 0.386509 0.174653 0.264644\nv 0.406968 0.089677 0.276475\nv 0.386509 0.263748 0.175457\nv 0.426504 0.183743 0.184567\nv 0.451553 0.093983 0.192635\nv 0.406968 0.275552 0.090519\nv 0.451553 0.191842 0.094821\nv 0.479956 0.098238 0.099088\nv 0.248503 -0.248503 -0.353586\nv 0.174879 -0.263709 -0.385237\nv 0.090212 -0.275496 -0.405581\nv 0.263709 -0.174878 -0.385237\nv 0.183943 -0.183943 -0.425278\nv 0.094507 -0.192000 -0.450264\nv 0.275496 -0.090212 -0.405581\nv 0.192000 -0.094507 -0.450264\nv 0.098752 -0.098752 -0.478569\nv -0.248503 -0.248503 -0.353586\nv -0.263709 -0.174879 -0.385237\nv -0.275496 -0.090212 -0.405581\nv -0.174878 -0.263709 -0.385237\nv -0.183943 -0.183943 -0.425278\nv -0.192000 -0.094507 -0.450264\nv -0.090212 -0.275496 -0.405581\nv -0.094507 -0.192000 -0.450264\nv -0.098752 -0.098752 -0.478569\nv -0.249236 0.248503 -0.354751\nv -0.175457 0.263748 -0.386509\nv -0.090519 0.275552 -0.406968\nv -0.264644 0.174653 -0.386509\nv -0.184567 0.183743 -0.426504\nv -0.094821 0.191842 -0.451553\nv -0.276475 0.089677 -0.406968\nv -0.192635 0.093983 -0.451553\nv -0.099088 0.098238 -0.479956\nv 0.249236 0.248503 -0.354751\nv 0.264644 0.174653 -0.386509\nv 0.276475 0.089677 -0.406968\nv 0.175457 0.263748 -0.386509\nv 0.184567 0.183743 -0.426504\nv 0.192635 0.093983 -0.451553\nv 0.090519 0.275552 -0.406968\nv 0.094821 0.191842 -0.451553\nv 0.099088 0.098238 -0.479956\nvn -0.3828 -0.9044 -0.1887\nvn -0.1981 -0.9600 -0.1981\nvn -0.2033 -0.9791 -0.0000\nvn -0.3670 -0.8548 -0.3670\nvn -0.5273 -0.7746 -0.3492\nvn -0.5509 -0.8148 -0.1805\nvn -0.3890 -0.9212 -0.0000\nvn -0.0000 -1.0000 -0.0000\nvn -0.1982 -0.9599 0.1982\nvn -0.1887 -0.9043 -0.3828\nvn -0.0000 -0.9791 -0.2033\nvn -0.4964 -0.7122 -0.4964\nvn -0.3492 -0.7746 -0.5273\nvn -0.6647 -0.6647 -0.3411\nvn -0.6201 -0.6201 -0.4805\nvn -0.5773 -0.5773 -0.5774\nvn -0.4805 -0.6201 -0.6201\nvn -0.6960 -0.6960 -0.1769\nvn -0.3411 -0.6647 -0.6647\nvn -0.5597 -0.8287 0.0001\nvn -0.7071 -0.7071 -0.0000\nvn -0.1805 -0.8148 -0.5509\nvn -0.1769 -0.6959 -0.6960\nvn -0.5508 -0.8149 0.1805\nvn -0.6960 -0.6960 0.1769\nvn -0.3828 -0.9044 0.1887\nvn -0.0000 -0.9212 -0.3890\nvn 0.0001 -0.8287 -0.5597\nvn -0.0000 -0.7071 -0.7071\nvn -0.5273 -0.7746 0.3492\nvn -0.6647 -0.6647 0.3411\nvn -0.3670 -0.8548 0.3670\nvn 0.1805 -0.8149 -0.5508\nvn 0.1769 -0.6960 -0.6959\nvn 0.1887 -0.9044 -0.3828\nvn 0.1982 -0.9599 -0.1982\nvn -0.4963 -0.7122 0.4963\nvn -0.6201 -0.6201 0.4805\nvn 0.3492 -0.7746 -0.5273\nvn 0.3411 -0.6647 -0.6647\nvn 0.2033 -0.9791 -0.0000\nvn 0.3890 -0.9212 -0.0001\nvn 0.3827 -0.9044 -0.1886\nvn 0.5597 -0.8287 -0.0001\nvn 0.3670 -0.8548 -0.3670\nvn 0.5508 -0.8149 -0.1805\nvn 0.7071 -0.7071 0.0001\nvn 0.6959 -0.6959 -0.1770\nvn 0.5273 -0.7746 -0.3492\nvn 0.6647 -0.6647 -0.3411\nvn 0.4963 -0.7122 -0.4963\nvn 0.6201 -0.6201 -0.4805\nvn 0.4805 -0.6201 -0.6201\nvn 0.5774 -0.5774 -0.5773\nvn -0.0000 -0.9791 0.2033\nvn 0.1981 -0.9600 0.1981\nvn 0.1888 -0.9043 0.3828\nvn -0.0000 -0.9212 0.3890\nvn -0.1887 -0.9044 0.3828\nvn -0.0001 -0.8287 0.5597\nvn -0.1805 -0.8149 0.5508\nvn 0.1805 -0.8148 0.5509\nvn -0.3492 -0.7746 0.5273\nvn -0.0000 -0.7071 0.7071\nvn 0.1769 -0.6959 0.6960\nvn -0.1769 -0.6960 0.6959\nvn -0.3411 -0.6647 0.6647\nvn -0.4805 -0.6201 0.6201\nvn -0.5774 -0.5774 0.5773\nvn 0.3411 -0.6647 0.6647\nvn 0.3492 -0.7746 0.5273\nvn 0.4805 -0.6201 0.6201\nvn 0.4964 -0.7122 0.4964\nvn 0.5773 -0.5773 0.5774\nvn 0.6201 -0.6201 0.4805\nvn 0.3670 -0.8548 0.3670\nvn 0.5273 -0.7746 0.3492\nvn 0.6647 -0.6647 0.3411\nvn 0.3828 -0.9044 0.1887\nvn 0.5509 -0.8148 0.1805\nvn 0.6960 -0.6960 0.1769\nvn -0.1972 0.9603 -0.1972\nvn -0.0000 1.0000 -0.0000\nvn -0.0000 0.9795 -0.2015\nvn -0.1885 0.9046 -0.3823\nvn -0.3668 0.8549 -0.3668\nvn -0.3823 0.9046 -0.1885\nvn -0.2015 0.9795 -0.0000\nvn -0.0000 0.9215 -0.3884\nvn -0.3496 0.7746 -0.5271\nvn -0.1805 0.8150 -0.5507\nvn -0.5271 0.7746 -0.3495\nvn -0.4962 0.7124 -0.4962\nvn -0.3414 0.6646 -0.6646\nvn -0.4803 0.6205 -0.6199\nvn -0.5771 0.5779 -0.5770\nvn -0.6199 0.6205 -0.4803\nvn -0.1769 0.6959 -0.6961\nvn -0.6646 0.6647 -0.3414\nvn 0.0001 0.8289 -0.5594\nvn -0.0000 0.7070 -0.7072\nvn -0.5507 0.8150 -0.1805\nvn -0.6961 0.6959 -0.1768\nvn 0.1804 0.8150 -0.5507\nvn 0.1768 0.6959 -0.6961\nvn 0.1885 0.9046 -0.3823\nvn -0.3884 0.9215 -0.0000\nvn -0.5594 0.8289 0.0001\nvn -0.7072 0.7070 -0.0000\nvn 0.3495 0.7746 -0.5271\nvn 0.3414 0.6647 -0.6646\nvn -0.5507 0.8150 0.1804\nvn -0.6961 0.6959 0.1769\nvn -0.3822 0.9047 0.1884\nvn -0.1972 0.9603 0.1972\nvn -0.5271 0.7746 0.3495\nvn -0.6646 0.6647 0.3414\nvn -0.3668 0.8549 0.3668\nvn -0.4962 0.7124 0.4962\nvn -0.6199 0.6205 0.4804\nvn -0.4804 0.6205 0.6199\nvn -0.5770 0.5779 0.5771\nvn -0.3414 0.6647 0.6646\nvn -0.3495 0.7746 0.5271\nvn -0.1768 0.6959 0.6961\nvn -0.1804 0.8150 0.5507\nvn -0.0000 0.7070 0.7072\nvn -0.1885 0.9046 0.3822\nvn -0.0001 0.8288 0.5595\nvn 0.1769 0.6959 0.6961\nvn -0.0000 0.9215 0.3884\nvn 0.1805 0.8150 0.5507\nvn 0.3414 0.6647 0.6646\nvn -0.0001 0.9795 0.2015\nvn 0.3496 0.7746 0.5271\nvn 0.4803 0.6205 0.6199\nvn 0.1885 0.9046 0.3822\nvn 0.4962 0.7124 0.4962\nvn 0.5771 0.5779 0.5770\nvn 0.6199 0.6205 0.4803\nvn 0.1972 0.9603 0.1972\nvn 0.3668 0.8549 0.3668\nvn 0.5271 0.7746 0.3495\nvn 0.6646 0.6647 0.3414\nvn 0.3823 0.9046 0.1885\nvn 0.5507 0.8150 0.1805\nvn 0.6961 0.6959 0.1768\nvn 0.2015 0.9795 -0.0000\nvn 0.3884 0.9215 -0.0000\nvn 0.5595 0.8288 -0.0001\nvn 0.7072 0.7070 -0.0000\nvn 0.6199 0.6205 -0.4804\nvn 0.5770 0.5779 -0.5771\nvn 0.4804 0.6205 -0.6199\nvn 0.4962 0.7124 -0.4962\nvn 0.6646 0.6647 -0.3414\nvn 0.5271 0.7746 -0.3495\nvn 0.6960 0.6959 -0.1768\nvn 0.3669 0.8549 -0.3669\nvn 0.5507 0.8150 -0.1804\nvn 0.3822 0.9047 -0.1884\nvn 0.1972 0.9603 -0.1972\nvn -0.6201 -0.4805 0.6201\nvn -0.7122 -0.4963 0.4963\nvn -0.6647 -0.3411 0.6647\nvn -0.7747 -0.5272 0.3492\nvn -0.7746 -0.3492 0.5273\nvn -0.6950 -0.1840 0.6950\nvn -0.8149 -0.5508 0.1805\nvn -0.8145 -0.1878 0.5489\nvn -0.7071 -0.0089 0.7071\nvn -0.8547 -0.3670 0.3670\nvn -0.8287 -0.5597 0.0001\nvn -0.8306 -0.0086 0.5568\nvn -0.6962 0.1751 0.6962\nvn -0.9038 -0.1963 0.3803\nvn -0.9044 -0.3827 0.1886\nvn -0.8148 -0.5509 -0.1805\nvn -0.8163 0.1794 0.5490\nvn -0.6646 0.3414 0.6646\nvn -0.9212 -0.3891 0.0001\nvn -0.9230 -0.0076 0.3847\nvn -0.9593 -0.2044 0.1948\nvn -0.7746 -0.5273 -0.3493\nvn -0.7750 0.3494 0.5266\nvn -0.6200 0.4809 0.6200\nvn -0.9044 -0.3828 -0.1887\nvn -0.9783 -0.2073 -0.0004\nvn -0.9804 -0.0069 0.1967\nvn -0.9055 0.1889 0.3799\nvn -0.7123 -0.4963 -0.4963\nvn -0.6201 -0.4805 -0.6201\nvn -0.8547 -0.3670 -0.3670\nvn -0.7746 -0.3492 -0.5273\nvn -0.6647 -0.3411 -0.6647\nvn -0.9591 -0.2041 -0.1962\nvn -1.0000 -0.0069 -0.0010\nvn -0.9607 0.1976 0.1949\nvn -0.8134 -0.1898 -0.5498\nvn -0.6947 -0.1866 -0.6947\nvn -0.9029 -0.1969 -0.3821\nvn -0.9798 -0.0076 -0.1999\nvn -0.8290 -0.0095 -0.5591\nvn -0.7071 -0.0099 -0.7071\nvn -0.9215 -0.0086 -0.3883\nvn -0.9797 0.2004 -0.0005\nvn -0.8553 0.3665 0.3663\nvn -0.8154 0.1805 -0.5501\nvn -0.6960 0.1768 -0.6960\nvn -0.9049 0.1884 -0.3817\nvn -0.9605 0.1967 -0.1968\nvn -0.9047 0.3819 0.1886\nvn -0.7125 0.4964 0.4959\nvn -0.7749 0.5269 0.3492\nvn -0.8153 0.5502 0.1805\nvn -0.9216 0.3881 -0.0000\nvn -0.8291 0.5590 -0.0001\nvn -0.9047 0.3819 -0.1886\nvn -0.8153 0.5502 -0.1805\nvn -0.8553 0.3665 -0.3663\nvn -0.7749 0.5269 -0.3492\nvn -0.7750 0.3494 -0.5266\nvn -0.6646 0.3414 -0.6646\nvn -0.7126 0.4964 -0.4958\nvn -0.6200 0.4809 -0.6200\nvn 0.6201 -0.4805 0.6201\nvn 0.4963 -0.4963 0.7122\nvn 0.6647 -0.3411 0.6647\nvn 0.3492 -0.5273 0.7746\nvn 0.5273 -0.3492 0.7746\nvn 0.6950 -0.1840 0.6950\nvn 0.1806 -0.5509 0.8148\nvn 0.5489 -0.1878 0.8146\nvn 0.7071 -0.0089 0.7071\nvn 0.3670 -0.3670 0.8548\nvn 0.0001 -0.5597 0.8287\nvn 0.5568 -0.0086 0.8306\nvn 0.6962 0.1751 0.6962\nvn 0.3803 -0.1963 0.9038\nvn 0.1882 -0.3826 0.9045\nvn -0.1805 -0.5509 0.8148\nvn 0.5490 0.1794 0.8163\nvn 0.6646 0.3414 0.6646\nvn -0.0000 -0.3890 0.9212\nvn 0.3847 -0.0076 0.9230\nvn 0.1948 -0.2044 0.9593\nvn -0.3493 -0.5273 0.7746\nvn 0.5266 0.3493 0.7750\nvn 0.6200 0.4809 0.6200\nvn -0.1888 -0.3828 0.9043\nvn -0.0000 -0.2076 0.9782\nvn 0.1967 -0.0069 0.9804\nvn 0.3799 0.1889 0.9055\nvn -0.4963 -0.4963 0.7123\nvn -0.3670 -0.3670 0.8548\nvn -0.5273 -0.3492 0.7746\nvn -0.1948 -0.2043 0.9593\nvn -0.0000 -0.0067 1.0000\nvn 0.1949 0.1976 0.9607\nvn -0.5489 -0.1878 0.8146\nvn -0.3803 -0.1963 0.9038\nvn -0.1967 -0.0069 0.9804\nvn -0.5568 -0.0086 0.8306\nvn -0.3846 -0.0076 0.9230\nvn -0.0000 0.2010 0.9796\nvn 0.3663 0.3665 0.8553\nvn -0.5491 0.1794 0.8163\nvn -0.3799 0.1889 0.9056\nvn -0.1949 0.1976 0.9607\nvn 0.1887 0.3820 0.9047\nvn 0.4959 0.4964 0.7125\nvn 0.3492 0.5269 0.7749\nvn 0.1805 0.5502 0.8153\nvn -0.0000 0.3881 0.9216\nvn -0.0000 0.5590 0.8292\nvn -0.1886 0.3819 0.9047\nvn -0.1805 0.5502 0.8153\nvn -0.3663 0.3665 0.8553\nvn -0.3492 0.5269 0.7749\nvn -0.5266 0.3494 0.7750\nvn -0.4958 0.4964 0.7126\nvn 0.8287 -0.5597 -0.0001\nvn 0.8148 -0.5509 0.1805\nvn 0.9212 -0.3890 -0.0000\nvn 0.7746 -0.5273 0.3492\nvn 0.9043 -0.3828 0.1887\nvn 0.9782 -0.2076 -0.0007\nvn 0.7122 -0.4964 0.4964\nvn 0.8548 -0.3670 0.3670\nvn 0.9593 -0.2044 0.1947\nvn 1.0000 -0.0070 -0.0008\nvn 0.7746 -0.3492 0.5273\nvn 0.9038 -0.1963 0.3803\nvn 0.9804 -0.0069 0.1967\nvn 0.9797 0.2005 -0.0006\nvn 0.8146 -0.1878 0.5489\nvn 0.9607 0.1976 0.1949\nvn 0.9216 0.3881 -0.0000\nvn 0.9230 -0.0076 0.3846\nvn 0.8306 -0.0086 0.5568\nvn 0.9047 0.3819 0.1886\nvn 0.8292 0.5590 -0.0000\nvn 0.9055 0.1889 0.3799\nvn 0.8164 0.1794 0.5490\nvn 0.8153 0.5502 0.1805\nvn 0.8553 0.3665 0.3663\nvn 0.7750 0.3494 0.5266\nvn 0.7749 0.5269 0.3492\nvn 0.7126 0.4964 0.4958\nvn 0.6201 -0.4805 -0.6201\nvn 0.7122 -0.4963 -0.4963\nvn 0.6647 -0.3411 -0.6647\nvn 0.7746 -0.5273 -0.3492\nvn 0.7746 -0.3492 -0.5273\nvn 0.6947 -0.1866 -0.6947\nvn 0.8148 -0.5509 -0.1806\nvn 0.8135 -0.1897 -0.5498\nvn 0.7071 -0.0099 -0.7071\nvn 0.8548 -0.3670 -0.3670\nvn 0.9044 -0.3828 -0.1887\nvn 0.8290 -0.0096 -0.5592\nvn 0.6960 0.1768 -0.6960\nvn 0.9028 -0.1971 -0.3822\nvn 0.9591 -0.2040 -0.1962\nvn 0.8154 0.1805 -0.5501\nvn 0.6646 0.3414 -0.6646\nvn 0.9215 -0.0086 -0.3884\nvn 0.9798 -0.0076 -0.1998\nvn 0.7750 0.3494 -0.5266\nvn 0.6200 0.4809 -0.6200\nvn 0.9048 0.1885 -0.3817\nvn 0.9606 0.1966 -0.1966\nvn 0.7125 0.4964 -0.4959\nvn 0.8553 0.3665 -0.3663\nvn 0.9047 0.3820 -0.1887\nvn 0.7749 0.5269 -0.3491\nvn 0.8153 0.5502 -0.1805\nvn -0.4963 -0.4963 -0.7122\nvn -0.3492 -0.5272 -0.7747\nvn -0.5273 -0.3492 -0.7746\nvn -0.1804 -0.5508 -0.8149\nvn -0.5498 -0.1897 -0.8135\nvn -0.3670 -0.3670 -0.8547\nvn -0.0001 -0.5597 -0.8287\nvn -0.5592 -0.0096 -0.8290\nvn -0.3819 -0.1966 -0.9030\nvn -0.1886 -0.3827 -0.9044\nvn 0.1805 -0.5509 -0.8148\nvn -0.5501 0.1805 -0.8154\nvn -0.0001 -0.3891 -0.9212\nvn -0.3884 -0.0086 -0.9215\nvn -0.1964 -0.2042 -0.9590\nvn 0.3492 -0.5273 -0.7746\nvn -0.5266 0.3493 -0.7750\nvn 0.1887 -0.3828 -0.9043\nvn -0.0000 -0.2070 -0.9783\nvn -0.1998 -0.0076 -0.9798\nvn -0.3817 0.1886 -0.9048\nvn 0.4964 -0.4964 -0.7122\nvn 0.3670 -0.3670 -0.8547\nvn 0.5273 -0.3492 -0.7746\nvn 0.1962 -0.2040 -0.9591\nvn -0.0000 -0.0072 -1.0000\nvn -0.1966 0.1965 -0.9606\nvn 0.5498 -0.1898 -0.8135\nvn 0.3821 -0.1970 -0.9029\nvn 0.1999 -0.0076 -0.9798\nvn 0.5594 -0.0091 -0.8288\nvn 0.3883 -0.0086 -0.9215\nvn -0.0000 0.1999 -0.9798\nvn -0.3663 0.3665 -0.8553\nvn 0.5501 0.1805 -0.8154\nvn 0.3817 0.1886 -0.9048\nvn 0.1968 0.1967 -0.9605\nvn -0.1886 0.3819 -0.9047\nvn -0.4959 0.4964 -0.7125\nvn -0.3492 0.5269 -0.7749\nvn -0.1805 0.5502 -0.8153\nvn -0.0000 0.3881 -0.9216\nvn -0.0000 0.5590 -0.8292\nvn 0.1885 0.3819 -0.9048\nvn 0.1805 0.5502 -0.8153\nvn 0.3663 0.3665 -0.8553\nvn 0.3492 0.5269 -0.7749\nvn 0.5266 0.3494 -0.7750\nvn 0.4958 0.4964 -0.7126\nvt 0.425961 0.175268\nvt 0.372580 0.133626\nvt 0.498706 0.097569\nvt 0.373799 0.214219\nvt 0.405580 0.250794\nvt 0.448379 0.237880\nvt 0.498706 0.169569\nvt 0.373458 0.021756\nvt 0.615080 0.132108\nvt 0.321637 0.185627\nvt 0.248892 0.102047\nvt 0.373799 0.283400\nvt 0.342019 0.250794\nvt 0.423263 0.290564\nvt 0.393596 0.304814\nvt 0.373799 0.318191\nvt 0.354002 0.304814\nvt 0.459178 0.280647\nvt 0.324335 0.290564\nvt 0.498706 0.233575\nvt 0.498706 0.277129\nvt 0.299219 0.237880\nvt 0.288421 0.280647\nvt 0.549033 0.237880\nvt 0.538235 0.280647\nvt 0.571451 0.180770\nvt 0.248892 0.174340\nvt 0.248892 0.233575\nvt 0.248892 0.277129\nvt 0.591833 0.250794\nvt 0.574149 0.290564\nvt 0.623613 0.214219\nvt 0.198565 0.237880\nvt 0.209364 0.280647\nvt 0.176148 0.184810\nvt 0.125205 0.142171\nvt 0.623613 0.283400\nvt 0.603816 0.304814\nvt 0.155766 0.250794\nvt 0.173450 0.290564\nvt -0.000921 0.111119\nvt 0.119418 0.021756\nvt -0.000921 0.173344\nvt 0.071823 0.185920\nvt -0.000921 0.233575\nvt 0.123986 0.209653\nvt 0.049406 0.237880\nvt -0.000921 0.277129\nvt 0.038607 0.280647\nvt 0.092205 0.250794\nvt 0.074521 0.290564\nvt 0.123986 0.283400\nvt 0.104189 0.304814\nvt 0.143782 0.304814\nvt 0.123986 0.318191\nvt 0.748520 0.104690\nvt 0.622417 0.017629\nvt 0.881960 0.139528\nvt 0.873917 0.020380\nvt 0.821265 0.181853\nvt 0.748520 0.174340\nvt 0.675775 0.183082\nvt 0.748520 0.233575\nvt 0.698193 0.237880\nvt 0.798847 0.237880\nvt 0.655394 0.250794\nvt 0.748520 0.277129\nvt 0.788048 0.280647\nvt 0.708991 0.280647\nvt 0.673077 0.290564\nvt 0.643410 0.304814\nvt 0.623613 0.318191\nvt 0.823963 0.290564\nvt 0.841646 0.250794\nvt 0.853630 0.304814\nvt 0.873427 0.283400\nvt 0.873427 0.318191\nvt 0.893224 0.304814\nvt 0.873427 0.209653\nvt 0.905207 0.250794\nvt 0.922891 0.290564\nvt 0.925589 0.185920\nvt 0.948007 0.237880\nvt 0.958805 0.280647\nvt 0.998334 0.103510\nvt 0.998334 0.173344\nvt 0.998334 0.233575\nvt 0.998334 0.277129\nvt 0.373799 0.870621\nvt 0.384783 0.971765\nvt 0.248892 0.902040\nvt 0.321637 0.810544\nvt 0.373799 0.770862\nvt 0.425961 0.810544\nvt 0.498706 0.902040\nvt 0.248892 0.816243\nvt 0.342019 0.745444\nvt 0.299219 0.758358\nvt 0.405580 0.745444\nvt 0.373799 0.725353\nvt 0.324335 0.711236\nvt 0.354002 0.696987\nvt 0.373799 0.683609\nvt 0.393596 0.696987\nvt 0.288421 0.721153\nvt 0.423263 0.711236\nvt 0.248892 0.762664\nvt 0.248892 0.724672\nvt 0.448379 0.758358\nvt 0.459178 0.721153\nvt 0.198565 0.758358\nvt 0.209364 0.721153\nvt 0.176148 0.810544\nvt 0.498706 0.816243\nvt 0.498706 0.762664\nvt 0.498706 0.724672\nvt 0.155766 0.745444\nvt 0.173450 0.711236\nvt 0.549033 0.758358\nvt 0.538235 0.721153\nvt 0.571451 0.810544\nvt 0.623613 0.870621\nvt 0.591833 0.745444\nvt 0.574149 0.711236\nvt 0.623613 0.770862\nvt 0.623613 0.725353\nvt 0.603816 0.696987\nvt 0.643410 0.696987\nvt 0.623613 0.683609\nvt 0.673077 0.711236\nvt 0.655394 0.745444\nvt 0.708991 0.721153\nvt 0.698193 0.758358\nvt 0.748520 0.724672\nvt 0.675775 0.810544\nvt 0.748520 0.762664\nvt 0.788048 0.721153\nvt 0.748520 0.816243\nvt 0.798847 0.758358\nvt 0.823963 0.711236\nvt 0.748657 0.901956\nvt 0.619770 0.975891\nvt 0.841646 0.745444\nvt 0.853630 0.696987\nvt 0.821265 0.810544\nvt 0.873427 0.725353\nvt 0.873427 0.683609\nvt 0.893224 0.696987\nvt 0.873427 0.876644\nvt 0.875080 0.973140\nvt 0.873427 0.770862\nvt 0.905207 0.745444\nvt 0.922891 0.711236\nvt 0.925589 0.810544\nvt 0.948007 0.758358\nvt 0.958805 0.721153\nvt 0.997860 0.899095\nvt 0.998334 0.816243\nvt 0.998334 0.762664\nvt 0.998334 0.724672\nvt 0.104189 0.696987\nvt 0.123986 0.683609\nvt 0.143782 0.696987\nvt 0.123986 0.725353\nvt 0.074521 0.711236\nvt 0.092205 0.745444\nvt 0.038607 0.721153\nvt 0.123986 0.770862\nvt 0.049406 0.758358\nvt -0.000921 0.724672\nvt -0.000921 0.762664\nvt 0.071823 0.810544\nvt -0.000921 0.816243\nvt 0.123986 0.870621\nvt -0.000921 0.902040\nvt 0.129473 0.971765\nvt 0.623613 0.348428\nvt 0.596133 0.343153\nvt 0.623613 0.392897\nvt 0.566477 0.333502\nvt 0.594171 0.389889\nvt 0.623613 0.445000\nvt 0.533514 0.326019\nvt 0.593599 0.443635\nvt 0.623613 0.500900\nvt 0.563629 0.384136\nvt 0.498706 0.323059\nvt 0.593613 0.500900\nvt 0.623613 0.556800\nvt 0.562809 0.440908\nvt 0.531609 0.379021\nvt 0.463899 0.326019\nvt 0.593599 0.558166\nvt 0.623613 0.608904\nvt 0.498706 0.376732\nvt 0.562830 0.500900\nvt 0.531069 0.438213\nvt 0.430935 0.333502\nvt 0.594171 0.611911\nvt 0.623613 0.653372\nvt 0.465803 0.379021\nvt 0.498706 0.436917\nvt 0.531084 0.500900\nvt 0.562809 0.560893\nvt 0.401279 0.343153\nvt 0.373799 0.348428\nvt 0.433784 0.384136\nvt 0.403241 0.389889\nvt 0.373799 0.392897\nvt 0.466343 0.438213\nvt 0.498706 0.500900\nvt 0.531069 0.563587\nvt 0.403814 0.443635\nvt 0.373799 0.445000\nvt 0.434603 0.440908\nvt 0.466328 0.500900\nvt 0.403799 0.500900\nvt 0.373799 0.500900\nvt 0.434582 0.500900\nvt 0.498706 0.564884\nvt 0.563629 0.617665\nvt 0.403814 0.558166\nvt 0.373799 0.556800\nvt 0.434603 0.560893\nvt 0.466343 0.563587\nvt 0.531609 0.622780\nvt 0.596133 0.658648\nvt 0.566477 0.668299\nvt 0.533514 0.675782\nvt 0.498706 0.625069\nvt 0.498706 0.678741\nvt 0.465803 0.622780\nvt 0.463899 0.675782\nvt 0.433784 0.617665\nvt 0.430935 0.668299\nvt 0.403241 0.611911\nvt 0.373799 0.608904\nvt 0.401279 0.658648\nvt 0.373799 0.653372\nvt 0.873427 0.348428\nvt 0.845947 0.343153\nvt 0.873427 0.392897\nvt 0.816291 0.333502\nvt 0.843985 0.389889\nvt 0.873427 0.445000\nvt 0.783327 0.326019\nvt 0.843413 0.443635\nvt 0.873427 0.500900\nvt 0.813443 0.384136\nvt 0.748520 0.323059\nvt 0.843427 0.500900\nvt 0.873427 0.556800\nvt 0.812623 0.440908\nvt 0.781423 0.379021\nvt 0.713713 0.326019\nvt 0.843413 0.558166\nvt 0.873427 0.608904\nvt 0.748520 0.376732\nvt 0.812644 0.500900\nvt 0.780883 0.438213\nvt 0.680749 0.333502\nvt 0.843985 0.611911\nvt 0.873427 0.653372\nvt 0.715617 0.379021\nvt 0.748520 0.436917\nvt 0.780898 0.500900\nvt 0.812623 0.560893\nvt 0.651093 0.343153\nvt 0.683597 0.384136\nvt 0.653055 0.389889\nvt 0.716157 0.438213\nvt 0.748520 0.500900\nvt 0.780883 0.563587\nvt 0.653627 0.443635\nvt 0.684417 0.440908\nvt 0.716142 0.500900\nvt 0.653613 0.500900\nvt 0.684396 0.500900\nvt 0.748520 0.564884\nvt 0.813443 0.617665\nvt 0.653627 0.558166\nvt 0.684417 0.560893\nvt 0.716157 0.563587\nvt 0.781423 0.622780\nvt 0.845947 0.658648\nvt 0.816291 0.668299\nvt 0.783327 0.675782\nvt 0.748520 0.625069\nvt 0.748520 0.678741\nvt 0.715617 0.622780\nvt 0.713713 0.675782\nvt 0.683597 0.617665\nvt 0.680749 0.668299\nvt 0.653055 0.611911\nvt 0.651093 0.658648\nvt 0.998334 0.323059\nvt 0.963526 0.326019\nvt 0.998334 0.376732\nvt 0.930563 0.333502\nvt 0.965430 0.379021\nvt 0.998334 0.436917\nvt 0.900906 0.343153\nvt 0.933411 0.384136\nvt 0.965971 0.438213\nvt 0.998334 0.500900\nvt 0.902869 0.389889\nvt 0.934231 0.440908\nvt 0.965956 0.500900\nvt 0.998334 0.564884\nvt 0.903441 0.443635\nvt 0.965971 0.563587\nvt 0.998334 0.625069\nvt 0.934210 0.500900\nvt 0.903427 0.500900\nvt 0.965430 0.622780\nvt 0.998334 0.678741\nvt 0.934231 0.560893\nvt 0.903441 0.558166\nvt 0.963526 0.675782\nvt 0.933411 0.617665\nvt 0.902869 0.611911\nvt 0.930563 0.668299\nvt 0.900906 0.658648\nvt 0.123986 0.348428\nvt 0.096506 0.343153\nvt 0.123986 0.392897\nvt 0.066850 0.333502\nvt 0.094543 0.389889\nvt 0.123986 0.445000\nvt 0.033886 0.326019\nvt -0.000921 0.323059\nvt 0.093971 0.443635\nvt 0.123986 0.500900\nvt 0.064001 0.384136\nvt 0.031982 0.379021\nvt -0.000921 0.376732\nvt 0.093986 0.500900\nvt 0.123986 0.556800\nvt 0.063182 0.440908\nvt 0.031441 0.438213\nvt -0.000921 0.436917\nvt 0.093971 0.558166\nvt 0.123986 0.608904\nvt 0.063202 0.500900\nvt 0.031457 0.500900\nvt -0.000921 0.500900\nvt 0.094543 0.611911\nvt 0.123986 0.653372\nvt 0.063182 0.560893\nvt 0.031441 0.563587\nvt -0.000921 0.564884\nvt 0.096506 0.658648\nvt 0.064001 0.617665\nvt 0.031982 0.622780\nvt -0.000921 0.625069\nvt 0.066850 0.668299\nvt 0.033886 0.675782\nvt -0.000921 0.678741\nvt 0.346320 0.343153\nvt 0.316663 0.333502\nvt 0.344357 0.389889\nvt 0.283700 0.326019\nvt 0.343785 0.443635\nvt 0.313815 0.384136\nvt 0.248892 0.323059\nvt 0.343800 0.500900\nvt 0.312995 0.440908\nvt 0.281796 0.379021\nvt 0.214085 0.326019\nvt 0.343785 0.558166\nvt 0.248892 0.376732\nvt 0.313016 0.500900\nvt 0.281255 0.438213\nvt 0.181121 0.333502\nvt 0.344357 0.611911\nvt 0.215989 0.379021\nvt 0.248892 0.436917\nvt 0.281270 0.500900\nvt 0.312995 0.560893\nvt 0.151465 0.343153\nvt 0.183970 0.384136\nvt 0.153428 0.389889\nvt 0.216530 0.438213\nvt 0.248892 0.500900\nvt 0.281255 0.563587\nvt 0.154000 0.443635\nvt 0.184789 0.440908\nvt 0.216514 0.500900\nvt 0.153985 0.500900\nvt 0.184769 0.500900\nvt 0.248892 0.564884\nvt 0.313815 0.617665\nvt 0.154000 0.558166\nvt 0.184789 0.560893\nvt 0.216530 0.563587\nvt 0.281796 0.622780\nvt 0.346320 0.658648\nvt 0.316663 0.668299\nvt 0.283700 0.675782\nvt 0.248892 0.625069\nvt 0.248892 0.678741\nvt 0.215989 0.622780\nvt 0.214085 0.675782\nvt 0.183970 0.617665\nvt 0.181121 0.668299\nvt 0.153428 0.611911\nvt 0.151465 0.658648\ns 1\nusemtl Lit\nf 178/1/1 179/2/2 110/3/3\nf 175/4/4 179/2/2 178/1/1\nf 174/5/5 175/4/4 178/1/1\nf 174/5/5 178/1/1 177/6/6\nf 177/6/6 178/1/1 109/7/7\nf 178/1/1 110/3/3 109/7/7\nf 179/2/2 9/8/8 110/3/3\nf 206/9/9 109/7/7 110/3/3\nf 175/4/4 176/10/10 179/2/2\nf 176/10/10 101/11/11 179/2/2\nf 179/2/2 101/11/11 9/8/8\nf 171/12/12 175/4/4 174/5/5\nf 172/13/13 176/10/10 175/4/4\nf 171/12/12 172/13/13 175/4/4\nf 49/14/14 174/5/5 177/6/6\nf 48/15/15 174/5/5 49/14/14\nf 48/15/15 171/12/12 174/5/5\nf 1/16/16 171/12/12 48/15/15\nf 1/16/16 27/17/17 171/12/12\nf 27/17/17 172/13/13 171/12/12\nf 49/14/14 177/6/6 50/18/18\nf 27/17/17 28/19/19 172/13/13\nf 50/18/18 177/6/6 108/20/20\nf 50/18/18 108/20/20 18/21/21\nf 177/6/6 109/7/7 108/20/20\nf 28/19/19 173/22/22 172/13/13\nf 172/13/13 173/22/22 176/10/10\nf 28/19/19 29/23/23 173/22/22\nf 200/24/24 18/21/21 108/20/20\nf 200/24/24 47/25/25 18/21/21\nf 203/26/26 108/20/20 109/7/7\nf 203/26/26 200/24/24 108/20/20\nf 206/9/9 203/26/26 109/7/7\nf 173/22/22 100/27/27 176/10/10\nf 176/10/10 100/27/27 101/11/11\nf 29/23/23 99/28/28 173/22/22\nf 173/22/22 99/28/28 100/27/27\nf 29/23/23 15/29/29 99/28/28\nf 199/30/30 47/25/25 200/24/24\nf 199/30/30 46/31/31 47/25/25\nf 202/32/32 200/24/24 203/26/26\nf 202/32/32 199/30/30 200/24/24\nf 15/29/29 186/33/33 99/28/28\nf 15/29/29 32/34/34 186/33/33\nf 99/28/28 187/35/35 100/27/27\nf 99/28/28 186/33/33 187/35/35\nf 100/27/27 188/36/36 101/11/11\nf 100/27/27 187/35/35 188/36/36\nf 198/37/37 46/31/31 199/30/30\nf 198/37/37 45/38/38 46/31/31\nf 32/34/34 183/39/39 186/33/33\nf 32/34/34 31/40/40 183/39/39\nf 101/11/11 188/36/36 104/41/41\nf 101/11/11 104/41/41 9/42/8\nf 188/36/36 103/43/42 104/41/41\nf 188/36/36 185/44/43 103/43/42\nf 187/35/35 185/44/43 188/36/36\nf 185/44/43 102/45/44 103/43/42\nf 186/33/33 184/46/45 187/35/35\nf 187/35/35 184/46/45 185/44/43\nf 186/33/33 183/39/39 184/46/45\nf 185/44/43 182/47/46 102/45/44\nf 184/46/45 182/47/46 185/44/43\nf 182/47/46 16/48/47 102/45/44\nf 182/47/46 35/49/48 16/48/47\nf 184/46/45 181/50/49 182/47/46\nf 181/50/49 35/49/48 182/47/46\nf 183/39/39 181/50/49 184/46/45\nf 181/50/49 34/51/50 35/49/48\nf 183/39/39 180/52/51 181/50/49\nf 180/52/51 34/51/50 181/50/49\nf 31/40/40 180/52/51 183/39/39\nf 180/52/51 33/53/52 34/51/50\nf 31/40/40 30/54/53 180/52/51\nf 30/54/53 33/53/52 180/52/51\nf 30/54/53 3/55/54 33/53/52\nf 107/56/55 110/3/3 9/57/8\nf 197/58/56 107/56/55 9/59/8\nf 194/60/57 107/56/55 197/58/56\nf 194/60/57 106/61/58 107/56/55\nf 106/61/58 206/9/9 107/56/55\nf 107/56/55 206/9/9 110/3/3\nf 106/61/58 205/62/59 206/9/9\nf 205/62/59 203/26/26 206/9/9\nf 205/62/59 202/32/32 203/26/26\nf 105/63/60 205/62/59 106/61/58\nf 204/64/61 202/32/32 205/62/59\nf 105/63/60 204/64/61 205/62/59\nf 191/65/62 106/61/58 194/60/57\nf 191/65/62 105/63/60 106/61/58\nf 204/64/61 201/66/63 202/32/32\nf 201/66/63 199/30/30 202/32/32\nf 201/66/63 198/37/37 199/30/30\nf 17/67/64 204/64/61 105/63/60\nf 41/68/65 17/67/64 105/63/60\nf 41/68/65 105/63/60 191/65/62\nf 17/67/64 44/69/66 204/64/61\nf 44/69/66 201/66/63 204/64/61\nf 44/69/66 43/70/67 201/66/63\nf 43/70/67 198/37/37 201/66/63\nf 43/70/67 42/71/68 198/37/37\nf 42/71/68 45/38/38 198/37/37\nf 42/71/68 2/72/69 45/38/38\nf 40/73/70 41/68/65 191/65/62\nf 40/73/70 191/65/62 190/74/71\nf 39/75/72 40/73/70 190/74/71\nf 190/74/71 191/65/62 194/60/57\nf 39/75/72 190/74/71 189/76/73\nf 4/77/74 39/75/72 189/76/73\nf 4/77/74 189/76/73 36/78/75\nf 190/74/71 194/60/57 193/79/76\nf 189/76/73 190/74/71 193/79/76\nf 193/79/76 194/60/57 197/58/56\nf 36/78/75 189/76/73 192/80/77\nf 189/76/73 193/79/76 192/80/77\nf 36/78/75 192/80/77 37/81/78\nf 193/79/76 197/58/56 196/82/79\nf 192/80/77 193/79/76 196/82/79\nf 37/81/78 192/80/77 195/83/80\nf 192/80/77 196/82/79 195/83/80\nf 37/81/78 195/83/80 38/84/81\nf 196/82/79 197/58/56 104/85/41\nf 197/58/56 9/59/8 104/85/41\nf 196/82/79 104/85/41 103/86/42\nf 195/83/80 196/82/79 103/86/42\nf 195/83/80 103/86/42 102/87/44\nf 38/84/81 195/83/80 102/87/44\nf 38/84/81 102/87/44 16/88/47\nf 215/89/82 10/90/83 122/91/84\nf 214/92/85 215/89/82 122/91/84\nf 211/93/86 215/89/82 214/92/85\nf 211/93/86 212/94/87 215/89/82\nf 212/94/87 113/95/88 215/89/82\nf 215/89/82 113/95/88 10/90/83\nf 214/92/85 122/91/84 121/96/89\nf 210/97/90 211/93/86 214/92/85\nf 213/98/91 214/92/85 121/96/89\nf 210/97/90 214/92/85 213/98/91\nf 208/99/92 212/94/87 211/93/86\nf 207/100/93 211/93/86 210/97/90\nf 207/100/93 208/99/92 211/93/86\nf 73/101/94 210/97/90 213/98/91\nf 72/102/95 210/97/90 73/101/94\nf 72/102/95 207/100/93 210/97/90\nf 5/103/96 207/100/93 72/102/95\nf 5/103/96 51/104/97 207/100/93\nf 51/104/97 208/99/92 207/100/93\nf 73/101/94 213/98/91 74/105/98\nf 51/104/97 52/106/99 208/99/92\nf 74/105/98 213/98/91 120/107/100\nf 213/98/91 121/96/89 120/107/100\nf 74/105/98 120/107/100 22/108/101\nf 52/106/99 209/109/102 208/99/92\nf 208/99/92 209/109/102 212/94/87\nf 52/106/99 53/110/103 209/109/102\nf 236/111/104 22/108/101 120/107/100\nf 236/111/104 71/112/105 22/108/101\nf 239/113/106 120/107/100 121/96/89\nf 239/113/106 236/111/104 120/107/100\nf 209/109/102 112/114/107 212/94/87\nf 212/94/87 112/114/107 113/95/88\nf 53/110/103 111/115/108 209/109/102\nf 209/109/102 111/115/108 112/114/107\nf 53/110/103 19/116/109 111/115/108\nf 235/117/110 71/112/105 236/111/104\nf 235/117/110 70/118/111 71/112/105\nf 19/116/109 222/119/112 111/115/108\nf 19/116/109 56/120/113 222/119/112\nf 111/115/108 223/121/114 112/114/107\nf 111/115/108 222/119/112 223/121/114\nf 112/114/107 224/122/115 113/95/88\nf 112/114/107 223/121/114 224/122/115\nf 56/120/113 219/123/116 222/119/112\nf 56/120/113 55/124/117 219/123/116\nf 222/119/112 220/125/118 223/121/114\nf 222/119/112 219/123/116 220/125/118\nf 55/124/117 216/126/119 219/123/116\nf 55/124/117 54/127/120 216/126/119\nf 54/127/120 57/128/121 216/126/119\nf 54/127/120 6/129/122 57/128/121\nf 216/126/119 57/128/121 58/130/123\nf 219/123/116 216/126/119 217/131/124\nf 216/126/119 58/130/123 217/131/124\nf 219/123/116 217/131/124 220/125/118\nf 217/131/124 58/130/123 59/132/125\nf 217/131/124 59/132/125 218/133/126\nf 220/125/118 217/131/124 218/133/126\nf 218/133/126 59/132/125 20/134/127\nf 223/121/114 220/125/118 221/135/128\nf 220/125/118 218/133/126 221/135/128\nf 223/121/114 221/135/128 224/122/115\nf 218/133/126 20/134/127 114/136/129\nf 221/135/128 218/133/126 114/136/129\nf 62/137/130 114/136/129 20/134/127\nf 224/122/115 221/135/128 115/138/131\nf 221/135/128 114/136/129 115/138/131\nf 62/137/130 231/139/132 114/136/129\nf 231/139/132 115/138/131 114/136/129\nf 61/140/133 231/139/132 62/137/130\nf 224/122/115 115/138/131 116/141/134\nf 113/95/88 224/122/115 116/141/134\nf 113/95/88 116/141/134 10/142/83\nf 61/140/133 228/143/135 231/139/132\nf 60/144/136 228/143/135 61/140/133\nf 231/139/132 232/145/137 115/138/131\nf 232/145/137 116/141/134 115/138/131\nf 228/143/135 232/145/137 231/139/132\nf 60/144/136 225/146/138 228/143/135\nf 8/147/139 225/146/138 60/144/136\nf 8/147/139 63/148/140 225/146/138\nf 232/145/137 233/149/141 116/141/134\nf 233/149/141 10/150/83 116/141/134\nf 228/143/135 229/151/142 232/145/137\nf 225/146/138 229/151/142 228/143/135\nf 229/151/142 233/149/141 232/145/137\nf 63/148/140 226/152/143 225/146/138\nf 225/146/138 226/152/143 229/151/142\nf 63/148/140 64/153/144 226/152/143\nf 229/151/142 230/154/145 233/149/141\nf 226/152/143 230/154/145 229/151/142\nf 64/153/144 227/155/146 226/152/143\nf 226/152/143 227/155/146 230/154/145\nf 64/153/144 65/156/147 227/155/146\nf 230/154/145 119/157/148 233/149/141\nf 233/149/141 119/157/148 10/150/83\nf 230/154/145 118/158/149 119/157/148\nf 227/155/146 118/158/149 230/154/145\nf 227/155/146 117/159/150 118/158/149\nf 65/156/147 117/159/150 227/155/146\nf 65/156/147 21/160/151 117/159/150\nf 66/161/152 7/162/153 69/163/154\nf 66/161/152 69/163/154 234/164/155\nf 234/164/155 69/163/154 70/118/111\nf 67/165/156 66/161/152 234/164/155\nf 234/164/155 70/118/111 235/117/110\nf 67/165/156 234/164/155 237/166/157\nf 237/166/157 234/164/155 235/117/110\nf 68/167/158 67/165/156 237/166/157\nf 237/166/157 235/117/110 238/168/159\nf 238/168/159 235/117/110 236/111/104\nf 238/168/159 236/111/104 239/113/106\nf 68/167/158 237/166/157 240/169/160\nf 240/169/160 237/166/157 238/168/159\nf 21/170/151 68/167/158 240/169/160\nf 21/170/151 240/169/160 117/171/150\nf 240/169/160 238/168/159 241/172/161\nf 117/171/150 240/169/160 241/172/161\nf 241/172/161 238/168/159 239/113/106\nf 117/171/150 241/172/161 118/173/149\nf 241/172/161 239/113/106 242/174/162\nf 118/173/149 241/172/161 242/174/162\nf 242/174/162 239/113/106 121/96/89\nf 118/173/149 242/174/162 119/175/148\nf 242/174/162 121/96/89 122/91/84\nf 119/175/148 242/174/162 122/91/84\nf 119/175/148 122/91/84 10/176/83\nf 45/38/38 2/72/69 75/177/163\nf 45/38/38 75/177/163 252/178/164\nf 46/31/31 45/38/38 252/178/164\nf 252/178/164 75/177/163 76/179/165\nf 46/31/31 252/178/164 255/180/166\nf 47/25/25 46/31/31 255/180/166\nf 252/178/164 76/179/165 253/181/167\nf 255/180/166 252/178/164 253/181/167\nf 253/181/167 76/179/165 77/182/168\nf 47/25/25 255/180/166 258/183/169\nf 18/21/21 47/25/25 258/183/169\nf 253/181/167 77/182/168 254/184/170\nf 254/184/170 77/182/168 23/185/171\nf 255/180/166 253/181/167 256/186/172\nf 258/183/169 255/180/166 256/186/172\nf 256/186/172 253/181/167 254/184/170\nf 18/21/21 258/183/169 123/187/173\nf 50/18/18 18/21/21 123/187/173\nf 254/184/170 23/185/171 126/188/174\nf 80/189/175 126/188/174 23/185/171\nf 256/186/172 254/184/170 257/190/176\nf 257/190/176 254/184/170 126/188/174\nf 258/183/169 256/186/172 259/191/177\nf 123/187/173 258/183/169 259/191/177\nf 259/191/177 256/186/172 257/190/176\nf 50/18/18 123/187/173 245/192/178\nf 49/14/14 50/18/18 245/192/178\nf 80/189/175 267/193/179 126/188/174\nf 79/194/180 267/193/179 80/189/175\nf 123/187/173 259/191/177 124/195/181\nf 245/192/178 123/187/173 124/195/181\nf 257/190/176 126/188/174 127/196/182\nf 267/193/179 127/196/182 126/188/174\nf 259/191/177 257/190/176 260/197/183\nf 124/195/181 259/191/177 260/197/183\nf 260/197/183 257/190/176 127/196/182\nf 49/14/14 245/192/178 244/198/184\nf 48/15/15 49/14/14 244/198/184\nf 79/194/180 264/199/185 267/193/179\nf 78/200/186 264/199/185 79/194/180\nf 245/192/178 124/195/181 248/201/187\nf 244/198/184 245/192/178 248/201/187\nf 124/195/181 260/197/183 125/202/188\nf 248/201/187 124/195/181 125/202/188\nf 260/197/183 127/196/182 128/203/189\nf 125/202/188 260/197/183 128/203/189\nf 267/193/179 268/204/190 127/196/182\nf 264/199/185 268/204/190 267/193/179\nf 268/204/190 128/203/189 127/196/182\nf 48/15/15 244/198/184 243/205/191\nf 1/16/16 48/15/15 243/205/191\nf 1/16/16 243/205/191 84/206/192\nf 244/198/184 248/201/187 247/207/193\nf 243/205/191 244/198/184 247/207/193\nf 84/206/192 243/205/191 246/208/194\nf 243/205/191 247/207/193 246/208/194\nf 84/206/192 246/208/194 85/209/195\nf 248/201/187 125/202/188 251/210/196\nf 247/207/193 248/201/187 251/210/196\nf 125/202/188 128/203/189 11/211/197\nf 251/210/196 125/202/188 11/211/197\nf 268/204/190 269/212/198 128/203/189\nf 269/212/198 11/211/197 128/203/189\nf 85/209/195 246/208/194 249/213/199\nf 85/209/195 249/213/199 86/214/200\nf 246/208/194 247/207/193 250/215/201\nf 247/207/193 251/210/196 250/215/201\nf 246/208/194 250/215/201 249/213/199\nf 251/210/196 11/211/197 134/216/202\nf 250/215/201 251/210/196 134/216/202\nf 86/214/200 249/213/199 132/217/203\nf 86/214/200 132/217/203 24/218/204\nf 249/213/199 250/215/201 133/219/205\nf 250/215/201 134/216/202 133/219/205\nf 249/213/199 133/219/205 132/217/203\nf 131/220/206 134/216/202 11/211/197\nf 269/212/198 131/220/206 11/211/197\nf 265/221/207 269/212/198 268/204/190\nf 264/199/185 265/221/207 268/204/190\nf 272/222/208 24/218/204 132/217/203\nf 272/222/208 83/223/209 24/218/204\nf 275/224/210 132/217/203 133/219/205\nf 275/224/210 272/222/208 132/217/203\nf 278/225/211 133/219/205 134/216/202\nf 131/220/206 278/225/211 134/216/202\nf 278/225/211 275/224/210 133/219/205\nf 266/226/212 131/220/206 269/212/198\nf 265/221/207 266/226/212 269/212/198\nf 261/227/213 265/221/207 264/199/185\nf 78/200/186 261/227/213 264/199/185\nf 6/129/122 261/227/213 78/200/186\nf 6/129/122 54/127/120 261/227/213\nf 261/227/213 262/228/214 265/221/207\nf 54/127/120 262/228/214 261/227/213\nf 262/228/214 266/226/212 265/221/207\nf 54/127/120 55/124/117 262/228/214\nf 55/124/117 263/229/215 262/228/214\nf 262/228/214 263/229/215 266/226/212\nf 55/124/117 56/120/113 263/229/215\nf 266/226/212 130/230/216 131/220/206\nf 263/229/215 130/230/216 266/226/212\nf 130/230/216 278/225/211 131/220/206\nf 56/120/113 129/231/217 263/229/215\nf 263/229/215 129/231/217 130/230/216\nf 56/120/113 19/116/109 129/231/217\nf 130/230/216 277/232/218 278/225/211\nf 129/231/217 277/232/218 130/230/216\nf 277/232/218 275/224/210 278/225/211\nf 19/116/109 276/233/219 129/231/217\nf 129/231/217 276/233/219 277/232/218\nf 19/116/109 53/110/103 276/233/219\nf 277/232/218 274/234/220 275/224/210\nf 276/233/219 274/234/220 277/232/218\nf 274/234/220 272/222/208 275/224/210\nf 53/110/103 273/235/221 276/233/219\nf 276/233/219 273/235/221 274/234/220\nf 53/110/103 52/106/99 273/235/221\nf 274/234/220 271/236/222 272/222/208\nf 273/235/221 271/236/222 274/234/220\nf 271/236/222 83/223/209 272/222/208\nf 271/236/222 82/237/223 83/223/209\nf 52/106/99 270/238/224 273/235/221\nf 273/235/221 270/238/224 271/236/222\nf 270/238/224 82/237/223 271/236/222\nf 52/106/99 51/104/97 270/238/224\nf 270/238/224 81/239/225 82/237/223\nf 51/104/97 81/239/225 270/238/224\nf 51/104/97 5/103/96 81/239/225\nf 39/75/72 4/77/74 87/240/226\nf 39/75/72 87/240/226 288/241/227\nf 40/73/70 39/75/72 288/241/227\nf 288/241/227 87/240/226 88/242/228\nf 40/73/70 288/241/227 291/243/229\nf 41/68/65 40/73/70 291/243/229\nf 288/241/227 88/242/228 289/244/230\nf 291/243/229 288/241/227 289/244/230\nf 289/244/230 88/242/228 89/245/231\nf 41/68/65 291/243/229 294/246/232\nf 17/67/64 41/68/65 294/246/232\nf 289/244/230 89/245/231 290/247/233\nf 290/247/233 89/245/231 25/248/234\nf 291/243/229 289/244/230 292/249/235\nf 294/246/232 291/243/229 292/249/235\nf 292/249/235 289/244/230 290/247/233\nf 17/67/64 294/246/232 135/250/236\nf 44/69/66 17/67/64 135/250/236\nf 290/247/233 25/248/234 138/251/237\nf 92/252/238 138/251/237 25/248/234\nf 292/249/235 290/247/233 293/253/239\nf 293/253/239 290/247/233 138/251/237\nf 294/246/232 292/249/235 295/254/240\nf 135/250/236 294/246/232 295/254/240\nf 295/254/240 292/249/235 293/253/239\nf 44/69/66 135/250/236 281/255/241\nf 43/70/67 44/69/66 281/255/241\nf 92/252/238 303/256/242 138/251/237\nf 91/257/243 303/256/242 92/252/238\nf 135/250/236 295/254/240 136/258/244\nf 281/255/241 135/250/236 136/258/244\nf 293/253/239 138/251/237 139/259/245\nf 303/256/242 139/259/245 138/251/237\nf 295/254/240 293/253/239 296/260/246\nf 136/258/244 295/254/240 296/260/246\nf 296/260/246 293/253/239 139/259/245\nf 43/70/67 281/255/241 280/261/247\nf 42/71/68 43/70/67 280/261/247\nf 91/257/243 300/262/248 303/256/242\nf 90/263/249 300/262/248 91/257/243\nf 281/255/241 136/258/244 284/264/250\nf 280/261/247 281/255/241 284/264/250\nf 136/258/244 296/260/246 137/265/251\nf 284/264/250 136/258/244 137/265/251\nf 296/260/246 139/259/245 140/266/252\nf 137/265/251 296/260/246 140/266/252\nf 303/256/242 304/267/253 139/259/245\nf 300/262/248 304/267/253 303/256/242\nf 304/267/253 140/266/252 139/259/245\nf 42/71/68 280/261/247 279/268/254\nf 2/72/69 42/71/68 279/268/254\nf 2/72/69 279/268/254 75/177/163\nf 280/261/247 284/264/250 283/269/255\nf 279/268/254 280/261/247 283/269/255\nf 75/177/163 279/268/254 282/270/256\nf 279/268/254 283/269/255 282/270/256\nf 75/177/163 282/270/256 76/179/165\nf 284/264/250 137/265/251 287/271/257\nf 283/269/255 284/264/250 287/271/257\nf 137/265/251 140/266/252 12/272/258\nf 287/271/257 137/265/251 12/272/258\nf 304/267/253 305/273/259 140/266/252\nf 305/273/259 12/272/258 140/266/252\nf 76/179/165 282/270/256 285/274/260\nf 76/179/165 285/274/260 77/182/168\nf 282/270/256 283/269/255 286/275/261\nf 283/269/255 287/271/257 286/275/261\nf 282/270/256 286/275/261 285/274/260\nf 287/271/257 12/272/258 146/276/262\nf 286/275/261 287/271/257 146/276/262\nf 77/182/168 285/274/260 144/277/263\nf 77/182/168 144/277/263 23/185/171\nf 285/274/260 286/275/261 145/278/264\nf 286/275/261 146/276/262 145/278/264\nf 285/274/260 145/278/264 144/277/263\nf 143/279/265 146/276/262 12/272/258\nf 305/273/259 143/279/265 12/272/258\nf 301/280/266 305/273/259 304/267/253\nf 300/262/248 301/280/266 304/267/253\nf 308/281/267 23/185/171 144/277/263\nf 308/281/267 80/189/175 23/185/171\nf 311/282/268 144/277/263 145/278/264\nf 311/282/268 308/281/267 144/277/263\nf 314/283/269 145/278/264 146/276/262\nf 143/279/265 314/283/269 146/276/262\nf 314/283/269 311/282/268 145/278/264\nf 302/284/270 143/279/265 305/273/259\nf 301/280/266 302/284/270 305/273/259\nf 297/285/271 301/280/266 300/262/248\nf 90/263/249 297/285/271 300/262/248\nf 8/147/139 297/285/271 90/263/249\nf 8/147/139 60/144/136 297/285/271\nf 297/285/271 298/286/272 301/280/266\nf 60/144/136 298/286/272 297/285/271\nf 298/286/272 302/284/270 301/280/266\nf 60/144/136 61/140/133 298/286/272\nf 61/140/133 299/287/273 298/286/272\nf 298/286/272 299/287/273 302/284/270\nf 61/140/133 62/137/130 299/287/273\nf 302/284/270 142/288/274 143/279/265\nf 299/287/273 142/288/274 302/284/270\nf 142/288/274 314/283/269 143/279/265\nf 62/137/130 141/289/275 299/287/273\nf 299/287/273 141/289/275 142/288/274\nf 62/137/130 20/134/127 141/289/275\nf 142/288/274 313/290/276 314/283/269\nf 141/289/275 313/290/276 142/288/274\nf 313/290/276 311/282/268 314/283/269\nf 20/134/127 312/291/277 141/289/275\nf 141/289/275 312/291/277 313/290/276\nf 20/134/127 59/132/125 312/291/277\nf 313/290/276 310/292/278 311/282/268\nf 312/291/277 310/292/278 313/290/276\nf 310/292/278 308/281/267 311/282/268\nf 59/132/125 309/293/279 312/291/277\nf 312/291/277 309/293/279 310/292/278\nf 59/132/125 58/130/123 309/293/279\nf 310/292/278 307/294/280 308/281/267\nf 309/293/279 307/294/280 310/292/278\nf 307/294/280 80/189/175 308/281/267\nf 307/294/280 79/194/180 80/189/175\nf 58/130/123 306/295/281 309/293/279\nf 309/293/279 306/295/281 307/294/280\nf 306/295/281 79/194/180 307/294/280\nf 58/130/123 57/128/121 306/295/281\nf 306/295/281 78/200/186 79/194/180\nf 57/128/121 78/200/186 306/295/281\nf 57/128/121 6/129/122 78/200/186\nf 38/84/81 16/88/47 147/296/282\nf 38/84/81 147/296/282 317/297/283\nf 37/81/78 38/84/81 317/297/283\nf 317/297/283 147/296/282 148/298/284\nf 37/81/78 317/297/283 316/299/285\nf 36/78/75 37/81/78 316/299/285\nf 317/297/283 148/298/284 320/300/286\nf 316/299/285 317/297/283 320/300/286\nf 320/300/286 148/298/284 149/301/287\nf 36/78/75 316/299/285 315/302/288\nf 4/77/74 36/78/75 315/302/288\nf 4/77/74 315/302/288 87/240/226\nf 316/299/285 320/300/286 319/303/289\nf 315/302/288 316/299/285 319/303/289\nf 320/300/286 149/301/287 323/304/290\nf 319/303/289 320/300/286 323/304/290\nf 323/304/290 149/301/287 13/305/291\nf 87/240/226 315/302/288 318/306/292\nf 315/302/288 319/303/289 318/306/292\nf 87/240/226 318/306/292 88/242/228\nf 319/303/289 323/304/290 322/307/293\nf 318/306/292 319/303/289 322/307/293\nf 323/304/290 13/305/291 158/308/294\nf 322/307/293 323/304/290 158/308/294\nf 155/309/295 158/308/294 13/305/291\nf 88/242/228 318/306/292 321/310/296\nf 318/306/292 322/307/293 321/310/296\nf 88/242/228 321/310/296 89/245/231\nf 155/309/295 350/311/297 158/308/294\nf 154/312/298 350/311/297 155/309/295\nf 322/307/293 158/308/294 157/313/299\nf 321/310/296 322/307/293 157/313/299\nf 350/311/297 157/313/299 158/308/294\nf 89/245/231 321/310/296 156/314/300\nf 321/310/296 157/313/299 156/314/300\nf 89/245/231 156/314/300 25/248/234\nf 154/312/298 349/315/301 350/311/297\nf 153/316/302 349/315/301 154/312/298\nf 350/311/297 347/317/303 157/313/299\nf 347/317/303 156/314/300 157/313/299\nf 349/315/301 347/317/303 350/311/297\nf 344/318/304 25/248/234 156/314/300\nf 347/317/303 344/318/304 156/314/300\nf 344/318/304 92/252/238 25/248/234\nf 153/316/302 348/319/305 349/315/301\nf 21/160/151 348/319/305 153/316/302\nf 21/160/151 65/156/147 348/319/305\nf 349/315/301 346/320/306 347/317/303\nf 346/320/306 344/318/304 347/317/303\nf 348/319/305 346/320/306 349/315/301\nf 343/321/307 92/252/238 344/318/304\nf 346/320/306 343/321/307 344/318/304\nf 343/321/307 91/257/243 92/252/238\nf 65/156/147 345/322/308 348/319/305\nf 348/319/305 345/322/308 346/320/306\nf 345/322/308 343/321/307 346/320/306\nf 65/156/147 64/153/144 345/322/308\nf 342/323/309 91/257/243 343/321/307\nf 345/322/308 342/323/309 343/321/307\nf 64/153/144 342/323/309 345/322/308\nf 342/323/309 90/263/249 91/257/243\nf 64/153/144 63/148/140 342/323/309\nf 63/148/140 90/263/249 342/323/309\nf 63/148/140 8/147/139 90/263/249\nf 33/53/52 3/55/54 93/324/310\nf 33/53/52 93/324/310 324/325/311\nf 34/51/50 33/53/52 324/325/311\nf 324/325/311 93/324/310 94/326/312\nf 34/51/50 324/325/311 327/327/313\nf 35/49/48 34/51/50 327/327/313\nf 324/325/311 94/326/312 325/328/314\nf 327/327/313 324/325/311 325/328/314\nf 325/328/314 94/326/312 95/329/315\nf 35/49/48 327/327/313 330/330/316\nf 16/48/47 35/49/48 330/330/316\nf 16/48/47 330/330/316 147/331/282\nf 325/328/314 95/329/315 326/332/317\nf 326/332/317 95/329/315 26/333/318\nf 327/327/313 325/328/314 328/334/319\nf 330/330/316 327/327/313 328/334/319\nf 328/334/319 325/328/314 326/332/317\nf 147/331/282 330/330/316 331/335/320\nf 330/330/316 328/334/319 331/335/320\nf 147/331/282 331/335/320 148/336/284\nf 326/332/317 26/333/318 150/337/321\nf 98/338/322 150/337/321 26/333/318\nf 328/334/319 326/332/317 329/339/323\nf 331/335/320 328/334/319 329/339/323\nf 329/339/323 326/332/317 150/337/321\nf 148/336/284 331/335/320 332/340/324\nf 331/335/320 329/339/323 332/340/324\nf 148/336/284 332/340/324 149/341/287\nf 98/338/322 339/342/325 150/337/321\nf 97/343/326 339/342/325 98/338/322\nf 329/339/323 150/337/321 151/344/327\nf 332/340/324 329/339/323 151/344/327\nf 339/342/325 151/344/327 150/337/321\nf 149/341/287 332/340/324 152/345/328\nf 332/340/324 151/344/327 152/345/328\nf 149/341/287 152/345/328 13/346/291\nf 97/343/326 336/347/329 339/342/325\nf 96/348/330 336/347/329 97/343/326\nf 339/342/325 340/349/331 151/344/327\nf 340/349/331 152/345/328 151/344/327\nf 336/347/329 340/349/331 339/342/325\nf 341/350/332 13/346/291 152/345/328\nf 340/349/331 341/350/332 152/345/328\nf 341/350/332 155/351/295 13/346/291\nf 96/348/330 333/352/333 336/347/329\nf 7/162/153 333/352/333 96/348/330\nf 7/162/153 66/161/152 333/352/333\nf 336/347/329 337/353/334 340/349/331\nf 337/353/334 341/350/332 340/349/331\nf 333/352/333 337/353/334 336/347/329\nf 338/354/335 155/351/295 341/350/332\nf 337/353/334 338/354/335 341/350/332\nf 338/354/335 154/355/298 155/351/295\nf 66/161/152 334/356/336 333/352/333\nf 333/352/333 334/356/336 337/353/334\nf 334/356/336 338/354/335 337/353/334\nf 66/161/152 67/165/156 334/356/336\nf 335/357/337 154/355/298 338/354/335\nf 334/356/336 335/357/337 338/354/335\nf 67/165/156 335/357/337 334/356/336\nf 335/357/337 153/358/302 154/355/298\nf 67/165/156 68/167/158 335/357/337\nf 68/167/158 153/358/302 335/357/337\nf 68/167/158 21/170/151 153/358/302\nf 27/17/17 1/16/16 84/206/192\nf 27/17/17 84/206/192 360/359/338\nf 28/19/19 27/17/17 360/359/338\nf 360/359/338 84/206/192 85/209/195\nf 28/19/19 360/359/338 363/360/339\nf 29/23/23 28/19/19 363/360/339\nf 360/359/338 85/209/195 361/361/340\nf 363/360/339 360/359/338 361/361/340\nf 361/361/340 85/209/195 86/214/200\nf 29/23/23 363/360/339 366/362/341\nf 15/29/29 29/23/23 366/362/341\nf 361/361/340 86/214/200 362/363/342\nf 362/363/342 86/214/200 24/218/204\nf 363/360/339 361/361/340 364/364/343\nf 366/362/341 363/360/339 364/364/343\nf 364/364/343 361/361/340 362/363/342\nf 15/29/29 366/362/341 159/365/344\nf 32/34/34 15/29/29 159/365/344\nf 362/363/342 24/218/204 162/366/345\nf 83/223/209 162/366/345 24/218/204\nf 364/364/343 362/363/342 365/367/346\nf 365/367/346 362/363/342 162/366/345\nf 366/362/341 364/364/343 367/368/347\nf 159/365/344 366/362/341 367/368/347\nf 367/368/347 364/364/343 365/367/346\nf 32/34/34 159/365/344 353/369/348\nf 31/40/40 32/34/34 353/369/348\nf 83/223/209 375/370/349 162/366/345\nf 82/237/223 375/370/349 83/223/209\nf 159/365/344 367/368/347 160/371/350\nf 353/369/348 159/365/344 160/371/350\nf 365/367/346 162/366/345 163/372/351\nf 375/370/349 163/372/351 162/366/345\nf 367/368/347 365/367/346 368/373/352\nf 160/371/350 367/368/347 368/373/352\nf 368/373/352 365/367/346 163/372/351\nf 31/40/40 353/369/348 352/374/353\nf 30/54/53 31/40/40 352/374/353\nf 82/237/223 372/375/354 375/370/349\nf 81/239/225 372/375/354 82/237/223\nf 353/369/348 160/371/350 356/376/355\nf 352/374/353 353/369/348 356/376/355\nf 160/371/350 368/373/352 161/377/356\nf 356/376/355 160/371/350 161/377/356\nf 368/373/352 163/372/351 164/378/357\nf 161/377/356 368/373/352 164/378/357\nf 375/370/349 376/379/358 163/372/351\nf 372/375/354 376/379/358 375/370/349\nf 376/379/358 164/378/357 163/372/351\nf 30/54/53 352/374/353 351/380/359\nf 3/55/54 30/54/53 351/380/359\nf 3/55/54 351/380/359 93/324/310\nf 352/374/353 356/376/355 355/381/360\nf 351/380/359 352/374/353 355/381/360\nf 93/324/310 351/380/359 354/382/361\nf 351/380/359 355/381/360 354/382/361\nf 93/324/310 354/382/361 94/326/312\nf 356/376/355 161/377/356 359/383/362\nf 355/381/360 356/376/355 359/383/362\nf 161/377/356 164/378/357 14/384/363\nf 359/383/362 161/377/356 14/384/363\nf 376/379/358 377/385/364 164/378/357\nf 377/385/364 14/384/363 164/378/357\nf 94/326/312 354/382/361 357/386/365\nf 94/326/312 357/386/365 95/329/315\nf 354/382/361 355/381/360 358/387/366\nf 355/381/360 359/383/362 358/387/366\nf 354/382/361 358/387/366 357/386/365\nf 359/383/362 14/384/363 170/388/367\nf 358/387/366 359/383/362 170/388/367\nf 95/329/315 357/386/365 168/389/368\nf 95/329/315 168/389/368 26/333/318\nf 357/386/365 358/387/366 169/390/369\nf 358/387/366 170/388/367 169/390/369\nf 357/386/365 169/390/369 168/389/368\nf 167/391/370 170/388/367 14/384/363\nf 377/385/364 167/391/370 14/384/363\nf 373/392/371 377/385/364 376/379/358\nf 372/375/354 373/392/371 376/379/358\nf 380/393/372 26/333/318 168/389/368\nf 380/393/372 98/338/322 26/333/318\nf 383/394/373 168/389/368 169/390/369\nf 383/394/373 380/393/372 168/389/368\nf 386/395/374 169/390/369 170/388/367\nf 167/391/370 386/395/374 170/388/367\nf 386/395/374 383/394/373 169/390/369\nf 374/396/375 167/391/370 377/385/364\nf 373/392/371 374/396/375 377/385/364\nf 369/397/376 373/392/371 372/375/354\nf 81/239/225 369/397/376 372/375/354\nf 5/103/96 369/397/376 81/239/225\nf 5/103/96 72/102/95 369/397/376\nf 369/397/376 370/398/377 373/392/371\nf 72/102/95 370/398/377 369/397/376\nf 370/398/377 374/396/375 373/392/371\nf 72/102/95 73/101/94 370/398/377\nf 73/101/94 371/399/378 370/398/377\nf 370/398/377 371/399/378 374/396/375\nf 73/101/94 74/105/98 371/399/378\nf 374/396/375 166/400/379 167/391/370\nf 371/399/378 166/400/379 374/396/375\nf 166/400/379 386/395/374 167/391/370\nf 74/105/98 165/401/380 371/399/378\nf 371/399/378 165/401/380 166/400/379\nf 74/105/98 22/108/101 165/401/380\nf 166/400/379 385/402/381 386/395/374\nf 165/401/380 385/402/381 166/400/379\nf 385/402/381 383/394/373 386/395/374\nf 22/108/101 384/403/382 165/401/380\nf 165/401/380 384/403/382 385/402/381\nf 22/108/101 71/112/105 384/403/382\nf 385/402/381 382/404/383 383/394/373\nf 384/403/382 382/404/383 385/402/381\nf 382/404/383 380/393/372 383/394/373\nf 71/112/105 381/405/384 384/403/382\nf 384/403/382 381/405/384 382/404/383\nf 71/112/105 70/118/111 381/405/384\nf 382/404/383 379/406/385 380/393/372\nf 381/405/384 379/406/385 382/404/383\nf 379/406/385 98/338/322 380/393/372\nf 379/406/385 97/343/326 98/338/322\nf 70/118/111 378/407/386 381/405/384\nf 381/405/384 378/407/386 379/406/385\nf 378/407/386 97/343/326 379/406/385\nf 70/118/111 69/163/154 378/407/386\nf 378/407/386 96/348/330 97/343/326\nf 69/163/154 96/348/330 378/407/386\nf 69/163/154 7/162/153 96/348/330\n";
		
	}
}

//utilidades de genetica
//ignora el nombre largo del namespace
namespace RandomGeneStuffThatIsSupossedToExistsForASimsLikeGame
{
	[Serializable]
	public struct EpiGeneticInstance
	{
		public string Key;
		public bool Value;

		public EpiGeneticInstance(string key, bool value)
		{
			Key = key;
			Value = value;
		}

		public override bool Equals(object obj)
		{
			return obj is EpiGeneticInstance instance &&
				   Key == instance.Key &&
				   Value == instance.Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Key, Value);
		}
		public static bool operator ==(EpiGeneticInstance a, EpiGeneticInstance b)
		{
			return a.Equals(b);
		}
		public static bool operator !=(EpiGeneticInstance a, EpiGeneticInstance b)
		{ return !a.Equals(b); }
	}
	[Serializable]
	public class Gene
	{
		public string Name;
		public string Description;// ni me acuerdo para que es esto pero lo dejo por si acaso
		public long value; //simplificacion por que para guardar muchas cosas dde genetica de sim cabe en long como color de cabello boca y demas
		public bool Dominance; //False recesisvo true Dominante si hay 2 genes  dominates: CODOMINANCIA pasaria pero actuamente e codigo explota  pero adenas esto representa un solo alelo cariotipo es el que maneja los genes de los 2 padres 
		public List<EpiGeneticInstance> EpigeneitcFactors;

		public Gene()
		{
			Name = "";
			Description = "";
			value = 0;
			Dominance = false;
			EpigeneitcFactors = new();
		}

		public Gene(string name, string description, long value, bool dominance, Dictionary<string, bool> epigeneitcFactors)
		{
			Name = name;
			Description = description;
			this.value = value;
			Dominance = dominance;
			List<EpiGeneticInstance> NEW_FACTORS = new();
			foreach (var i in epigeneitcFactors)
			{
				NEW_FACTORS.Add(new(i.Key, i.Value));
			}

			EpigeneitcFactors = NEW_FACTORS;
		}

		public override bool Equals(object obj)
		{
			if (obj is Gene gene)
				return gene == this;
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Description, value, Dominance);
		}
		public int GetHashCodeWithEpi()
		{
			return HashCode.Combine(Name, Description, value, Dominance, EpigeneitcFactors);
		}

		public static Boolean operator ==(Gene a, Gene b)
		{
			if (a is null && b is null) return true;
			if (b is not null && a is null) return false;
			if (a is not null && b is null) return false;


			bool NAM = (a.Name == b.Name);
			bool DES = (a.Description == b.Description);
			bool VAL = (a.value == b.value);
			bool DOM = (a.Dominance == b.Dominance);
			return (NAM && DES && VAL && DOM)
			;
		}
		public static Boolean operator !=(Gene a, Gene b)
		{
			return !(a == b);
		}
		public static bool FactorsMatch(Gene a, Gene b)
		{
			if (a is null && b is null) return true;
			if (b is not null && a is null) return false;
			if (a is not null && b is null) return false;
			if (a.EpigeneitcFactors is null && b.EpigeneitcFactors is null) return true;
			if (b.EpigeneitcFactors is not null && a.EpigeneitcFactors is null) return false;
			if (a.EpigeneitcFactors is not null && b.EpigeneitcFactors is null) return false;
			bool MATCHLEngt = a.EpigeneitcFactors.Count == b.EpigeneitcFactors.Count;


			if (MATCHLEngt)
			{
				return (StdUtils.Comparisons.ListsAreEqual(a.EpigeneitcFactors, b.EpigeneitcFactors))
				   ;
				//si al parecer ya tenia definido ese metodo y apenas me acuerod que existe 
			}
			return false;
		}

	}
	/// <summary>
	/// cromatida  
	/// si no sabes que es una cromatida es que no sabes nada de biologia asi que te lo explico rapido: es una cadena de ADN con sus genes ordenados, cada cromatida representa la contribucion genetica de cada padre, el orden de los genes en cada cromatida es importante para determinar la especie y demas cosas, ademas el cromosoma es el que maneja la interaccion entre los genes de las 2 cromatidas y sus factores epigeneticos para determinar la expresion genetica final del organismo
	/// </summary>
	[Serializable]

	public class Chromatid
	{
		public string Name; public string Description;
		public List<Gene> Genes;

		public Chromatid()
		{
			Genes = new();
			Name = "";
			Description = "";
		}

		public Chromatid(string name, string description, List<Gene> genes)
		{
			Name = name;
			Description = description;
			Genes = genes;
		}

		public static bool operator ==(Chromatid a, Chromatid b)
		{
			if (a is null && b is null) return true;
			if (b is not null && a is null) return false;
			if (a is not null && b is null) return false;
			//solo nos fijamos en genes y que sea el mismo orden por que  otro orden == otra especie
			int i = 0;
			foreach (Gene e in a.Genes)
			{
				if (e != b.Genes[i])
					return false;
				i++;
			}
			return true;
		}
		public static Boolean operator !=(Chromatid a, Chromatid b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{
			if (obj is Chromatid a)
			{
				return a == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Genes.Count);
		}
	}
	/// <summary>
	/// 2 cromatidas forman un cromosoma, cada cromatida representa la contribucion genetica de cada padre, el orden de los genes en cada cromatida es importante para determinar la especie y demas cosas, ademas el cromosoma es el que maneja la interaccion entre los genes de las 2 cromatidas y sus factores epigeneticos para determinar la expresion genetica final del organismo
	/// </summary>
	[Serializable]

	public class Chromosome
	{
		public Chromatid ChromatidA;
		public Chromatid ChromatidB;

		public Chromosome()
		{
		}

		public Chromosome(Chromatid chromatidA, Chromatid chromatidB)
		{
			ChromatidA = chromatidA;
			ChromatidB = chromatidB;
		}
	}
	[Serializable]

	public class Kariotype
	{
		public string Name;
		public string Description;
		public List<Chromosome> Chromosomes;
	}
}
