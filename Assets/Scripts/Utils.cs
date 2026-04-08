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
	/// Instancia un sistema solar... ¡AHORA CON POSICIONES BASADAS EN ÍNDICE! 
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
		public static Mesh SphereMesh;
		private static bool Inited;
		private static UnityEngine.Mesh CacheSphere;

		public static void InitStuf()
		{
			foreach (StarTypes st in Enum.GetValues(typeof(StarTypes)))
			{
				Material material = new Material(BaseMat);
				material.SetFloat("_Temp_K", StarData.Temperatures[st]);
				Mats[st] = material;
				if (st == StarTypes.X) Mats[st] = BholMat;
				else if (st == StarTypes.EN) Mats[st] = OpaceMat;
			}
            SphereMesh = JsonUtility.FromJson<Mesh>(JSONMESHES.Sphere);
			Inited = true;
		}

		static void GiveSphere(GameObject @object, Material mat)
		{
			if (CacheSphere == null) CacheSphere = (UnityEngine.Mesh)SphereMesh;
			@object.AddComponent<MeshFilter>().mesh = CacheSphere;
			@object.AddComponent<MeshRenderer>().material = mat;
			@object.AddComponent<MeshCollider>();
		}

		public static GameObject InstantiateStar(StarData starData)
		{
			GameObject starGO = new GameObject(starData.Name);
			GiveSphere(starGO, Mats[starData.type]);
			starGO.transform.localScale = Vector3.one * 5f; // Placeholder de radio estelar

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
			return gol;
		}

		public static GameObject InstantiatePlanet(PlanetData planetData)
		{
			GameObject planetGO = new GameObject(planetData.Name);
			Material Mat = OpaceMat;

			if (planetData.type == PlanetTypes.BasicGas || planetData.type == PlanetTypes.IceGas)
			{
				if (planetData.GasColors != null && planetData.GasColors.Count >= 5 && BaseGasMaterial != null)
				{
					Mat = new Material(BaseGasMaterial);
					Mat.SetColor("_PoloNorte", planetData.GasColors[0]);
					Mat.SetColor("_Arriba", planetData.GasColors[1]);
					Mat.SetColor("_Ecuador", planetData.GasColors[2]);
					Mat.SetColor("_Abajo", planetData.GasColors[3]);
					Mat.SetColor("_PoloSur", planetData.GasColors[4]);
				}
			}

			GiveSphere(planetGO, Mat);
			planetGO.transform.localScale = Vector3.one * planetData.radius;

			systemData.IDS.planets.Add(BodyID.FromString(planetData.id));
			systemData.Datas.planets.Add(planetData);
			systemData.ObjAndIDS.Add(planetGO, planetData.id);
			return planetGO;
		}

		public static void InstantiateBody(string StartID, UnityEngine.Transform parent, int index = 0)
		{
			if (StartID[0] == 'S') throw new ArgumentException("NO SECTORES");

			if (galaxyData == null && !GalaxyData.TryToLoadGalaxy(out galaxyData))
				throw new Exception("ERROR AL CARGAR GALAXIA");

			BodyID bodyID = BodyID.FromString(StartID);
			if (!TryToLoadABody(bodyID, out var body, out var celestialBodyType))
				throw new Exception("ERROR CARGANDO");

			GameObject gameObject = celestialBodyType switch
			{
				CelestialBodyType.Planet => InstantiatePlanet((PlanetData)body),
				CelestialBodyType.Star => InstantiateStar((StarData)body),
				CelestialBodyType.Baricenter => InstantiateBaricenter((BaricenterData)body),
				CelestialBodyType.Nova => throw new NotImplementedException(),
				CelestialBodyType.Nebula => throw new NotImplementedException(),
				_ => null
			};

			if (gameObject != null)
			{
				gameObject.transform.SetParent(parent);
				// POSICIONAMIENTO POR ÍNDICE: Separación de 20 unidades por nivel
				float dist = (index + 1) * 20f;
				gameObject.transform.localPosition = new Vector3(dist, 0, 0);
			}

			if (body.Children != null && body.Children.Count > 0)
			{
				for (int i = 0; i < body.Children.Count; i++)
				{
					InstantiateBody(body.Children[i], gameObject.transform, i);
				}
			}
		}

		public static void InstantiateSystem(string ParentId)
		{
			if (!Inited) InitStuf();

			systemData = new InstantiatedSystemData
			{
				Datas = new GalObjCollection(),
				IDS = new GalObjCollectionID(),
				ObjAndIDS = new Dictionary<GameObject, string>()
			};

			InstantiateBody(ParentId, null);
		}

		static bool TryToLoadABody(BodyID bodyID, out CelestialBody body, out CelestialBodyType d)
		{
			try
			{
				d = bodyID.GetCelestialBodyType();
				body = d switch
				{
					CelestialBodyType.Planet => galaxyData.LoadPlanet(bodyID.GetID()),
					CelestialBodyType.Star => galaxyData.LookForStar(bodyID.GetID()),
					CelestialBodyType.Baricenter => galaxyData.LookForBaricenter(bodyID.GetID()),
					CelestialBodyType.Nova => galaxyData.LookForNova(bodyID.GetID()),
					CelestialBodyType.Nebula => galaxyData.LookForNebula(bodyID.GetID()),
					_ => throw new Exception("TIPO DESCONOCIDO"),
				};
				return true;
			}
			catch { body = null; d = CelestialBodyType.None; return false; }
		}

		[Serializable]
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
        public static readonly string Sphere = @"";



			
	}
	public static class JSONMESHES
	{
        public static readonly string Sphere = @"{
    ""Vertices"": [
      {
        ""x"": -28.782712936401367,
        ""y"": -28.782712936401367,
        ""z"": 28.782712936401367
      },
      {
        ""x"": -28.782712936401367,
        ""y"": 28.782712936401367,
        ""z"": 28.782712936401367
      },
      {
        ""x"": 28.782712936401367,
        ""y"": -28.782712936401367,
        ""z"": 28.782712936401367
      },
      {
        ""x"": 28.782712936401367,
        ""y"": 28.782712936401367,
        ""z"": 28.782712936401367
      },
      {
        ""x"": -28.883543014526367,
        ""y"": -28.883543014526367,
        ""z"": -28.7900390625
      },
      {
        ""x"": -28.883543014526367,
        ""y"": 28.883543014526367,
        ""z"": -28.7900390625
      },
      {
        ""x"": 28.883543014526367,
        ""y"": -28.883543014526367,
        ""z"": -28.7900390625
      },
      {
        ""x"": 28.883543014526367,
        ""y"": 28.883543014526367,
        ""z"": -28.7900390625
      },
      {
        ""x"": -0.0,
        ""y"": 0.0,
        ""z"": 50.0
      },
      {
        ""x"": -4.7642970457673073e-07,
        ""y"": -4.76837158203125e-07,
        ""z"": -50.0
      },
      {
        ""x"": -50.0,
        ""y"": -1.6391277313232422e-05,
        ""z"": 4.7642970457673073e-07
      },
      {
        ""x"": -8.195580448955297e-06,
        ""y"": 50.0,
        ""z"": 9.537070582155138e-07
      },
      {
        ""x"": 50.0,
        ""y"": 0.0,
        ""z"": -4.7642970457673073e-07
      },
      {
        ""x"": 2.2351741790771484e-05,
        ""y"": -50.0,
        ""z"": -3.2741809263825417e-11
      },
      {
        ""x"": 5.340552888810635e-06,
        ""y"": -35.25146484375,
        ""z"": 35.25146484375
      },
      {
        ""x"": 35.25146484375,
        ""y"": 0.0,
        ""z"": 35.25146484375
      },
      {
        ""x"": -5.7220458984375e-06,
        ""y"": 35.25146484375,
        ""z"": 35.25146484375
      },
      {
        ""x"": -35.25146484375,
        ""y"": 3.814697265625e-06,
        ""z"": 35.25146484375
      },
      {
        ""x"": -35.369140625,
        ""y"": -1.1444091796875e-05,
        ""z"": -35.27880859375
      },
      {
        ""x"": -5.7220458984375e-06,
        ""y"": 35.369140625,
        ""z"": -35.27880859375
      },
      {
        ""x"": 35.369140625,
        ""y"": 0.0,
        ""z"": -35.27880859375
      },
      {
        ""x"": 1.5735626220703125e-05,
        ""y"": -35.369140625,
        ""z"": -35.27880859375
      },
      {
        ""x"": -35.355224609375,
        ""y"": 35.355224609375,
        ""z"": 3.1286615520542682e-09
      },
      {
        ""x"": -35.369140625,
        ""y"": -35.369140625,
        ""z"": 0.08836466073989868
      },
      {
        ""x"": 35.355224609375,
        ""y"": 35.355224609375,
        ""z"": -3.1286615520542682e-09
      },
      {
        ""x"": 35.369140625,
        ""y"": -35.369140625,
        ""z"": 0.08836115896701813
      },
      {
        ""x"": -24.019285202026367,
        ""y"": -30.89013671875,
        ""z"": 30.89013671875
      },
      {
        ""x"": -17.01416015625,
        ""y"": -33.134765625,
        ""z"": 33.134765625
      },
      {
        ""x"": -8.806029319763184,
        ""y"": -34.697021484375,
        ""z"": 34.697021484375
      },
      {
        ""x"": 24.019285202026367,
        ""y"": -30.89013671875,
        ""z"": 30.89013671875
      },
      {
        ""x"": 17.01416015625,
        ""y"": -33.134765625,
        ""z"": 33.134765625
      },
      {
        ""x"": 8.806029319763184,
        ""y"": -34.697021484375,
        ""z"": 34.697021484375
      },
      {
        ""x"": 30.89013671875,
        ""y"": -24.019285202026367,
        ""z"": 30.89013671875
      },
      {
        ""x"": 33.134765625,
        ""y"": -17.01416015625,
        ""z"": 33.134765625
      },
      {
        ""x"": 34.697021484375,
        ""y"": -8.806029319763184,
        ""z"": 34.697021484375
      },
      {
        ""x"": 30.89013671875,
        ""y"": 24.019285202026367,
        ""z"": 30.89013671875
      },
      {
        ""x"": 33.134765625,
        ""y"": 17.01416015625,
        ""z"": 33.134765625
      },
      {
        ""x"": 34.697021484375,
        ""y"": 8.806029319763184,
        ""z"": 34.697021484375
      },
      {
        ""x"": 24.019285202026367,
        ""y"": 30.89013671875,
        ""z"": 30.89013671875
      },
      {
        ""x"": 17.01416015625,
        ""y"": 33.134765625,
        ""z"": 33.134765625
      },
      {
        ""x"": 8.806029319763184,
        ""y"": 34.697021484375,
        ""z"": 34.697021484375
      },
      {
        ""x"": -24.019285202026367,
        ""y"": 30.89013671875,
        ""z"": 30.89013671875
      },
      {
        ""x"": -17.01416015625,
        ""y"": 33.134765625,
        ""z"": 33.134765625
      },
      {
        ""x"": -8.806029319763184,
        ""y"": 34.697021484375,
        ""z"": 34.697021484375
      },
      {
        ""x"": -30.89013671875,
        ""y"": 24.019285202026367,
        ""z"": 30.89013671875
      },
      {
        ""x"": -33.134765625,
        ""y"": 17.01416015625,
        ""z"": 33.134765625
      },
      {
        ""x"": -34.697021484375,
        ""y"": 8.806029319763184,
        ""z"": 34.697021484375
      },
      {
        ""x"": -30.89013671875,
        ""y"": -24.019285202026367,
        ""z"": 30.89013671875
      },
      {
        ""x"": -33.134765625,
        ""y"": -17.01416015625,
        ""z"": 33.134765625
      },
      {
        ""x"": -34.697021484375,
        ""y"": -8.806029319763184,
        ""z"": 34.697021484375
      },
      {
        ""x"": -30.995115280151367,
        ""y"": -24.090576171875,
        ""z"": -30.908935546875
      },
      {
        ""x"": -33.247314453125,
        ""y"": -17.0712890625,
        ""z"": -33.156982421875
      },
      {
        ""x"": -34.812744140625,
        ""y"": -8.83544921875,
        ""z"": -34.72265625
      },
      {
        ""x"": -30.995115280151367,
        ""y"": 24.090576171875,
        ""z"": -30.908935546875
      },
      {
        ""x"": -33.247314453125,
        ""y"": 17.0712890625,
        ""z"": -33.156982421875
      },
      {
        ""x"": -34.812744140625,
        ""y"": 8.83544921875,
        ""z"": -34.72265625
      },
      {
        ""x"": -24.090576171875,
        ""y"": 30.995115280151367,
        ""z"": -30.908935546875
      },
      {
        ""x"": -17.0712890625,
        ""y"": 33.247314453125,
        ""z"": -33.156982421875
      },
      {
        ""x"": -8.83544921875,
        ""y"": 34.812744140625,
        ""z"": -34.72265625
      },
      {
        ""x"": 24.090576171875,
        ""y"": 30.995115280151367,
        ""z"": -30.908935546875
      },
      {
        ""x"": 17.0712890625,
        ""y"": 33.247314453125,
        ""z"": -33.156982421875
      },
      {
        ""x"": 8.83544921875,
        ""y"": 34.812744140625,
        ""z"": -34.72265625
      },
      {
        ""x"": 30.995115280151367,
        ""y"": 24.090576171875,
        ""z"": -30.908935546875
      },
      {
        ""x"": 33.247314453125,
        ""y"": 17.0712890625,
        ""z"": -33.156982421875
      },
      {
        ""x"": 34.812744140625,
        ""y"": 8.83544921875,
        ""z"": -34.72265625
      },
      {
        ""x"": 30.995115280151367,
        ""y"": -24.090576171875,
        ""z"": -30.908935546875
      },
      {
        ""x"": 33.247314453125,
        ""y"": -17.0712890625,
        ""z"": -33.156982421875
      },
      {
        ""x"": 34.812744140625,
        ""y"": -8.83544921875,
        ""z"": -34.72265625
      },
      {
        ""x"": 24.090576171875,
        ""y"": -30.995115280151367,
        ""z"": -30.908935546875
      },
      {
        ""x"": 17.0712890625,
        ""y"": -33.247314453125,
        ""z"": -33.156982421875
      },
      {
        ""x"": 8.83544921875,
        ""y"": -34.812744140625,
        ""z"": -34.72265625
      },
      {
        ""x"": -24.090576171875,
        ""y"": -30.995115280151367,
        ""z"": -30.908935546875
      },
      {
        ""x"": -17.0712890625,
        ""y"": -33.247314453125,
        ""z"": -33.156982421875
      },
      {
        ""x"": -8.83544921875,
        ""y"": -34.812744140625,
        ""z"": -34.72265625
      },
      {
        ""x"": -30.89013671875,
        ""y"": 30.89013671875,
        ""z"": 24.019285202026367
      },
      {
        ""x"": -33.134765625,
        ""y"": 33.134765625,
        ""z"": 17.01416015625
      },
      {
        ""x"": -34.697021484375,
        ""y"": 34.697021484375,
        ""z"": 8.806029319763184
      },
      {
        ""x"": -30.995115280151367,
        ""y"": 30.995115280151367,
        ""z"": -24.017333984375
      },
      {
        ""x"": -33.247314453125,
        ""y"": 33.247314453125,
        ""z"": -16.9892578125
      },
      {
        ""x"": -34.812744140625,
        ""y"": 34.812744140625,
        ""z"": -8.75067138671875
      },
      {
        ""x"": -30.995115280151367,
        ""y"": -30.995115280151367,
        ""z"": -24.017333984375
      },
      {
        ""x"": -33.247314453125,
        ""y"": -33.247314453125,
        ""z"": -16.9892578125
      },
      {
        ""x"": -34.812744140625,
        ""y"": -34.812744140625,
        ""z"": -8.75067138671875
      },
      {
        ""x"": -30.89013671875,
        ""y"": -30.89013671875,
        ""z"": 24.019285202026367
      },
      {
        ""x"": -33.134765625,
        ""y"": -33.134765625,
        ""z"": 17.01416015625
      },
      {
        ""x"": -34.697021484375,
        ""y"": -34.697021484375,
        ""z"": 8.806029319763184
      },
      {
        ""x"": 30.89013671875,
        ""y"": 30.89013671875,
        ""z"": 24.019285202026367
      },
      {
        ""x"": 33.134765625,
        ""y"": 33.134765625,
        ""z"": 17.01416015625
      },
      {
        ""x"": 34.697021484375,
        ""y"": 34.697021484375,
        ""z"": 8.806029319763184
      },
      {
        ""x"": 30.995115280151367,
        ""y"": 30.995115280151367,
        ""z"": -24.017333984375
      },
      {
        ""x"": 33.247314453125,
        ""y"": 33.247314453125,
        ""z"": -16.9892578125
      },
      {
        ""x"": 34.812744140625,
        ""y"": 34.812744140625,
        ""z"": -8.75067138671875
      },
      {
        ""x"": 30.89013671875,
        ""y"": -30.89013671875,
        ""z"": 24.019285202026367
      },
      {
        ""x"": 33.134765625,
        ""y"": -33.134765625,
        ""z"": 17.01416015625
      },
      {
        ""x"": 34.697021484375,
        ""y"": -34.697021484375,
        ""z"": 8.806029319763184
      },
      {
        ""x"": 30.995115280151367,
        ""y"": -30.995115280151367,
        ""z"": -24.017333984375
      },
      {
        ""x"": 33.247314453125,
        ""y"": -33.247314453125,
        ""z"": -16.9892578125
      },
      {
        ""x"": 34.812744140625,
        ""y"": -34.812744140625,
        ""z"": -8.75067138671875
      },
      {
        ""x"": 1.811975266718946e-06,
        ""y"": -28.015869140625,
        ""z"": 41.236324310302734
      },
      {
        ""x"": 3.910070518031716e-06,
        ""y"": -19.560546875,
        ""z"": 45.85546875
      },
      {
        ""x"": 1.1825540013887803e-06,
        ""y"": -10.0794677734375,
        ""z"": 48.8232421875
      },
      {
        ""x"": 28.015869140625,
        ""y"": 1.9073486328125e-06,
        ""z"": 41.236324310302734
      },
      {
        ""x"": 19.560546875,
        ""y"": -3.814693627646193e-08,
        ""z"": 45.85546875
      },
      {
        ""x"": 10.07952880859375,
        ""y"": 0.0,
        ""z"": 48.8232421875
      },
      {
        ""x"": -4.768371127283899e-06,
        ""y"": 28.015869140625,
        ""z"": 41.236324310302734
      },
      {
        ""x"": 2.0027218852192163e-06,
        ""y"": 19.560546875,
        ""z"": 45.85546875
      },
      {
        ""x"": 2.1362357074394822e-06,
        ""y"": 10.0794677734375,
        ""z"": 48.8232421875
      },
      {
        ""x"": -28.015869140625,
        ""y"": 3.814697265625e-06,
        ""z"": 41.236324310302734
      },
      {
        ""x"": -19.560546875,
        ""y"": 1.9454892026260495e-06,
        ""z"": 45.85546875
      },
      {
        ""x"": -10.0794677734375,
        ""y"": -9.5367431640625e-07,
        ""z"": 48.8232421875
      },
      {
        ""x"": -28.114990234375,
        ""y"": -9.059906005859375e-06,
        ""z"": -41.2861328125
      },
      {
        ""x"": -19.6275634765625,
        ""y"": -6.198883056640625e-06,
        ""z"": -45.91015625
      },
      {
        ""x"": -10.1136474609375,
        ""y"": -3.2186505904974183e-06,
        ""z"": -48.8857421875
      },
      {
        ""x"": -4.768371127283899e-06,
        ""y"": 28.114990234375,
        ""z"": -41.2861328125
      },
      {
        ""x"": -3.0994415283203125e-06,
        ""y"": 19.6275634765625,
        ""z"": -45.91015625
      },
      {
        ""x"": -1.5497207641601562e-06,
        ""y"": 10.11358642578125,
        ""z"": -48.8857421875
      },
      {
        ""x"": 28.114990234375,
        ""y"": 0.0,
        ""z"": -41.2861328125
      },
      {
        ""x"": 19.6275634765625,
        ""y"": 0.0,
        ""z"": -45.91015625
      },
      {
        ""x"": 10.11358642578125,
        ""y"": 0.0,
        ""z"": -48.8857421875
      },
      {
        ""x"": 1.239776611328125e-05,
        ""y"": -28.114990234375,
        ""z"": -41.2861328125
      },
      {
        ""x"": 8.58306884765625e-06,
        ""y"": -19.6275634765625,
        ""z"": -45.91015625
      },
      {
        ""x"": 4.410743713378906e-06,
        ""y"": -10.11358642578125,
        ""z"": -48.8857421875
      },
      {
        ""x"": -41.236324310302734,
        ""y"": -3.814697265625e-06,
        ""z"": 28.015869140625
      },
      {
        ""x"": -45.85546875,
        ""y"": -7.667520549148321e-06,
        ""z"": 19.560546875
      },
      {
        ""x"": -48.8232421875,
        ""y"": -3.814697265625e-06,
        ""z"": 10.07952880859375
      },
      {
        ""x"": -41.573486328125,
        ""y"": 27.778562545776367,
        ""z"": -2.270098775625229e-09
      },
      {
        ""x"": -46.19384765625,
        ""y"": 19.1341552734375,
        ""z"": -3.4924594327634395e-09
      },
      {
        ""x"": -49.0390625,
        ""y"": 9.7545166015625,
        ""z"": -7.275957614183426e-10
      },
      {
        ""x"": -41.374755859375,
        ""y"": -1.33514404296875e-05,
        ""z"": -28.022214889526367
      },
      {
        ""x"": -45.984371185302734,
        ""y"": -1.52587890625e-05,
        ""z"": -19.5458984375
      },
      {
        ""x"": -48.9658203125,
        ""y"": -1.621246337890625e-05,
        ""z"": -10.0296630859375
      },
      {
        ""x"": -41.374755859375,
        ""y"": -28.114990234375,
        ""z"": 0.08836472034454346
      },
      {
        ""x"": -45.984371185302734,
        ""y"": -19.6275634765625,
        ""z"": 0.08836464583873749
      },
      {
        ""x"": -48.9658203125,
        ""y"": -10.11358642578125,
        ""z"": 0.0883650854229927
      },
      {
        ""x"": -6.67572021484375e-06,
        ""y"": 41.236324310302734,
        ""z"": 28.015869140625
      },
      {
        ""x"": -7.571768492198316e-06,
        ""y"": 45.85546875,
        ""z"": 19.560546875
      },
      {
        ""x"": -8.038161467993632e-06,
        ""y"": 48.8232421875,
        ""z"": 10.07952880859375
      },
      {
        ""x"": 27.778562545776367,
        ""y"": 41.573486328125,
        ""z"": -3.143213467282635e-09
      },
      {
        ""x"": 19.1341552734375,
        ""y"": 46.19384765625,
        ""z"": 8.076312951743603e-10
      },
      {
        ""x"": 9.7545166015625,
        ""y"": 49.0390625,
        ""z"": -3.588866093195975e-09
      },
      {
        ""x"": -6.67572021484375e-06,
        ""y"": 41.374755859375,
        ""z"": -28.022214889526367
      },
      {
        ""x"": -7.62939453125e-06,
        ""y"": 45.984371185302734,
        ""z"": -19.5458984375
      },
      {
        ""x"": -7.62939453125e-06,
        ""y"": 48.9658203125,
        ""z"": -10.02972412109375
      },
      {
        ""x"": -27.778562545776367,
        ""y"": 41.573486328125,
        ""z"": 3.143213467282635e-09
      },
      {
        ""x"": -19.1341552734375,
        ""y"": 46.19384765625,
        ""z"": -8.076312951743603e-10
      },
      {
        ""x"": -9.7545166015625,
        ""y"": 49.0390625,
        ""z"": 3.6052369978278875e-09
      },
      {
        ""x"": 41.236324310302734,
        ""y"": 3.814697265625e-06,
        ""z"": 28.015869140625
      },
      {
        ""x"": 45.85546875,
        ""y"": 3.852852387353778e-06,
        ""z"": 19.560546875
      },
      {
        ""x"": 48.8232421875,
        ""y"": 3.814697265625e-06,
        ""z"": 10.0794677734375
      },
      {
        ""x"": 41.374755859375,
        ""y"": -28.114990234375,
        ""z"": 0.0883609727025032
      },
      {
        ""x"": 45.984371185302734,
        ""y"": -19.6275634765625,
        ""z"": 0.08836057037115097
      },
      {
        ""x"": 48.9658203125,
        ""y"": -10.11358642578125,
        ""z"": 0.08836060017347336
      },
      {
        ""x"": 41.374755859375,
        ""y"": 0.0,
        ""z"": -28.022214889526367
      },
      {
        ""x"": 45.984371185302734,
        ""y"": 0.0,
        ""z"": -19.5458984375
      },
      {
        ""x"": 48.9658203125,
        ""y"": 0.0,
        ""z"": -10.02972412109375
      },
      {
        ""x"": 41.573486328125,
        ""y"": 27.778562545776367,
        ""z"": 2.3137542992657245e-09
      },
      {
        ""x"": 46.19384765625,
        ""y"": 19.1341552734375,
        ""z"": 3.4924594327634395e-09
      },
      {
        ""x"": 49.0390625,
        ""y"": 9.7545166015625,
        ""z"": 7.275957614183426e-10
      },
      {
        ""x"": -2.5939953047782183e-06,
        ""y"": -41.236324310302734,
        ""z"": 28.015869140625
      },
      {
        ""x"": 8.3923072224934e-07,
        ""y"": -45.85546875,
        ""z"": 19.560546875
      },
      {
        ""x"": -3.35692288899736e-06,
        ""y"": -48.8232421875,
        ""z"": 10.07952880859375
      },
      {
        ""x"": -28.114990234375,
        ""y"": -41.374755859375,
        ""z"": 0.08836342394351959
      },
      {
        ""x"": -19.6275634765625,
        ""y"": -45.984371185302734,
        ""z"": 0.08836378902196884
      },
      {
        ""x"": -10.11358642578125,
        ""y"": -48.9658203125,
        ""z"": 0.08836288750171661
      },
      {
        ""x"": 1.811981201171875e-05,
        ""y"": -41.374755859375,
        ""z"": -28.022214889526367
      },
      {
        ""x"": 2.09808349609375e-05,
        ""y"": -45.984371185302734,
        ""z"": -19.5458984375
      },
      {
        ""x"": 2.1934507458354346e-05,
        ""y"": -48.9658203125,
        ""z"": -10.02972412109375
      },
      {
        ""x"": 28.114990234375,
        ""y"": -41.374755859375,
        ""z"": 0.0883614718914032
      },
      {
        ""x"": 19.6275634765625,
        ""y"": -45.984371185302734,
        ""z"": 0.08836183696985245
      },
      {
        ""x"": 10.11358642578125,
        ""y"": -48.9658203125,
        ""z"": 0.08836251497268677
      },
      {
        ""x"": -24.850341796875,
        ""y"": -24.850341796875,
        ""z"": 35.358638763427734
      },
      {
        ""x"": -17.48779296875,
        ""y"": -26.370849609375,
        ""z"": 38.523681640625
      },
      {
        ""x"": -9.021239280700684,
        ""y"": -27.549558639526367,
        ""z"": 40.55810546875
      },
      {
        ""x"": -26.370849609375,
        ""y"": -17.48779296875,
        ""z"": 38.523681640625
      },
      {
        ""x"": -18.394287109375,
        ""y"": -18.394287109375,
        ""z"": 42.52783203125
      },
      {
        ""x"": -9.450743675231934,
        ""y"": -19.199951171875,
        ""z"": 45.026363372802734
      },
      {
        ""x"": -27.549558639526367,
        ""y"": -9.021239280700684,
        ""z"": 40.55810546875
      },
      {
        ""x"": -19.199951171875,
        ""y"": -9.450743675231934,
        ""z"": 45.026363372802734
      },
      {
        ""x"": -9.875244140625,
        ""y"": -9.875244140625,
        ""z"": 47.85693359375
      },
      {
        ""x"": 24.850341796875,
        ""y"": -24.850341796875,
        ""z"": 35.358638763427734
      },
      {
        ""x"": 26.370849609375,
        ""y"": -17.48779296875,
        ""z"": 38.523681640625
      },
      {
        ""x"": 27.549558639526367,
        ""y"": -9.021239280700684,
        ""z"": 40.55810546875
      },
      {
        ""x"": 17.48779296875,
        ""y"": -26.370849609375,
        ""z"": 38.523681640625
      },
      {
        ""x"": 18.394287109375,
        ""y"": -18.394287109375,
        ""z"": 42.52783203125
      },
      {
        ""x"": 19.199951171875,
        ""y"": -9.450743675231934,
        ""z"": 45.026363372802734
      },
      {
        ""x"": 9.021239280700684,
        ""y"": -27.549558639526367,
        ""z"": 40.55810546875
      },
      {
        ""x"": 9.450743675231934,
        ""y"": -19.199951171875,
        ""z"": 45.026363372802734
      },
      {
        ""x"": 9.875244140625,
        ""y"": -9.875244140625,
        ""z"": 47.85693359375
      },
      {
        ""x"": 24.850341796875,
        ""y"": 24.850341796875,
        ""z"": 35.358638763427734
      },
      {
        ""x"": 17.48779296875,
        ""y"": 26.370849609375,
        ""z"": 38.523681640625
      },
      {
        ""x"": 9.021239280700684,
        ""y"": 27.549558639526367,
        ""z"": 40.55810546875
      },
      {
        ""x"": 26.370849609375,
        ""y"": 17.48779296875,
        ""z"": 38.523681640625
      },
      {
        ""x"": 18.394287109375,
        ""y"": 18.394287109375,
        ""z"": 42.52783203125
      },
      {
        ""x"": 9.450743675231934,
        ""y"": 19.199951171875,
        ""z"": 45.026363372802734
      },
      {
        ""x"": 27.549558639526367,
        ""y"": 9.021239280700684,
        ""z"": 40.55810546875
      },
      {
        ""x"": 19.199951171875,
        ""y"": 9.450743675231934,
        ""z"": 45.026363372802734
      },
      {
        ""x"": 9.875244140625,
        ""y"": 9.875244140625,
        ""z"": 47.85693359375
      },
      {
        ""x"": -24.850341796875,
        ""y"": 24.850341796875,
        ""z"": 35.358638763427734
      },
      {
        ""x"": -26.370849609375,
        ""y"": 17.48779296875,
        ""z"": 38.523681640625
      },
      {
        ""x"": -27.549558639526367,
        ""y"": 9.021239280700684,
        ""z"": 40.55810546875
      },
      {
        ""x"": -17.48779296875,
        ""y"": 26.370849609375,
        ""z"": 38.523681640625
      },
      {
        ""x"": -18.394287109375,
        ""y"": 18.394287109375,
        ""z"": 42.52783203125
      },
      {
        ""x"": -19.199951171875,
        ""y"": 9.450743675231934,
        ""z"": 45.026363372802734
      },
      {
        ""x"": -9.021239280700684,
        ""y"": 27.549558639526367,
        ""z"": 40.55810546875
      },
      {
        ""x"": -9.450743675231934,
        ""y"": 19.199951171875,
        ""z"": 45.026363372802734
      },
      {
        ""x"": -9.875244140625,
        ""y"": 9.875244140625,
        ""z"": 47.85693359375
      },
      {
        ""x"": -24.923583984375,
        ""y"": -24.923583984375,
        ""z"": -35.385982513427734
      },
      {
        ""x"": -26.46435546875,
        ""y"": -17.545654296875,
        ""z"": -38.563720703125
      },
      {
        ""x"": -27.647459030151367,
        ""y"": -9.05194091796875,
        ""z"": -40.60791015625
      },
      {
        ""x"": -17.545654296875,
        ""y"": -26.46435546875,
        ""z"": -38.563720703125
      },
      {
        ""x"": -18.4566650390625,
        ""y"": -18.4566650390625,
        ""z"": -42.578125
      },
      {
        ""x"": -19.2635498046875,
        ""y"": -9.48211669921875,
        ""z"": -45.0810546875
      },
      {
        ""x"": -9.05194091796875,
        ""y"": -27.647459030151367,
        ""z"": -40.60791015625
      },
      {
        ""x"": -9.48211669921875,
        ""y"": -19.2635498046875,
        ""z"": -45.0810546875
      },
      {
        ""x"": -9.90875244140625,
        ""y"": -9.90875244140625,
        ""z"": -47.919921875
      },
      {
        ""x"": -24.923583984375,
        ""y"": 24.923583984375,
        ""z"": -35.385982513427734
      },
      {
        ""x"": -17.545654296875,
        ""y"": 26.46435546875,
        ""z"": -38.563720703125
      },
      {
        ""x"": -9.05194091796875,
        ""y"": 27.647459030151367,
        ""z"": -40.60791015625
      },
      {
        ""x"": -26.46435546875,
        ""y"": 17.545654296875,
        ""z"": -38.563720703125
      },
      {
        ""x"": -18.4566650390625,
        ""y"": 18.4566650390625,
        ""z"": -42.578125
      },
      {
        ""x"": -9.48211669921875,
        ""y"": 19.2635498046875,
        ""z"": -45.0810546875
      },
      {
        ""x"": -27.647459030151367,
        ""y"": 9.05194091796875,
        ""z"": -40.60791015625
      },
      {
        ""x"": -19.2635498046875,
        ""y"": 9.48211669921875,
        ""z"": -45.0810546875
      },
      {
        ""x"": -9.90875244140625,
        ""y"": 9.90875244140625,
        ""z"": -47.919921875
      },
      {
        ""x"": 24.923583984375,
        ""y"": 24.923583984375,
        ""z"": -35.385982513427734
      },
      {
        ""x"": 26.46435546875,
        ""y"": 17.545654296875,
        ""z"": -38.563720703125
      },
      {
        ""x"": 27.647459030151367,
        ""y"": 9.05194091796875,
        ""z"": -40.60791015625
      },
      {
        ""x"": 17.545654296875,
        ""y"": 26.46435546875,
        ""z"": -38.563720703125
      },
      {
        ""x"": 18.4566650390625,
        ""y"": 18.4566650390625,
        ""z"": -42.578125
      },
      {
        ""x"": 19.2635498046875,
        ""y"": 9.48211669921875,
        ""z"": -45.0810546875
      },
      {
        ""x"": 9.05194091796875,
        ""y"": 27.647459030151367,
        ""z"": -40.60791015625
      },
      {
        ""x"": 9.48211669921875,
        ""y"": 19.2635498046875,
        ""z"": -45.0810546875
      },
      {
        ""x"": 9.90875244140625,
        ""y"": 9.90875244140625,
        ""z"": -47.919921875
      },
      {
        ""x"": 24.923583984375,
        ""y"": -24.923583984375,
        ""z"": -35.385982513427734
      },
      {
        ""x"": 17.545654296875,
        ""y"": -26.46435546875,
        ""z"": -38.563720703125
      },
      {
        ""x"": 9.0518798828125,
        ""y"": -27.647459030151367,
        ""z"": -40.60791015625
      },
      {
        ""x"": 26.46435546875,
        ""y"": -17.545654296875,
        ""z"": -38.563720703125
      },
      {
        ""x"": 18.4566650390625,
        ""y"": -18.4566650390625,
        ""z"": -42.578125
      },
      {
        ""x"": 9.48211669921875,
        ""y"": -19.2635498046875,
        ""z"": -45.0810546875
      },
      {
        ""x"": 27.647459030151367,
        ""y"": -9.05194091796875,
        ""z"": -40.60791015625
      },
      {
        ""x"": 19.2635498046875,
        ""y"": -9.48211669921875,
        ""z"": -45.0810546875
      },
      {
        ""x"": 9.90875244140625,
        ""y"": -9.90875244140625,
        ""z"": -47.919921875
      },
      {
        ""x"": -35.358638763427734,
        ""y"": -24.850341796875,
        ""z"": 24.850341796875
      },
      {
        ""x"": -38.523681640625,
        ""y"": -17.48779296875,
        ""z"": 26.370849609375
      },
      {
        ""x"": -40.55810546875,
        ""y"": -9.021239280700684,
        ""z"": 27.549558639526367
      },
      {
        ""x"": -38.523681640625,
        ""y"": -26.370849609375,
        ""z"": 17.4879150390625
      },
      {
        ""x"": -42.52783203125,
        ""y"": -18.394287109375,
        ""z"": 18.394287109375
      },
      {
        ""x"": -45.026363372802734,
        ""y"": -9.450743675231934,
        ""z"": 19.199951171875
      },
      {
        ""x"": -40.55810546875,
        ""y"": -27.549558639526367,
        ""z"": 9.021239280700684
      },
      {
        ""x"": -45.026363372802734,
        ""y"": -19.199951171875,
        ""z"": 9.450743675231934
      },
      {
        ""x"": -47.85693359375,
        ""y"": -9.875244140625,
        ""z"": 9.875244140625
      },
      {
        ""x"": -35.358638763427734,
        ""y"": 24.850341796875,
        ""z"": 24.850341796875
      },
      {
        ""x"": -38.523681640625,
        ""y"": 26.370849609375,
        ""z"": 17.4879150390625
      },
      {
        ""x"": -40.55810546875,
        ""y"": 27.549558639526367,
        ""z"": 9.021239280700684
      },
      {
        ""x"": -38.523681640625,
        ""y"": 17.48779296875,
        ""z"": 26.370849609375
      },
      {
        ""x"": -42.52783203125,
        ""y"": 18.394287109375,
        ""z"": 18.394287109375
      },
      {
        ""x"": -45.026363372802734,
        ""y"": 19.199951171875,
        ""z"": 9.450743675231934
      },
      {
        ""x"": -40.55810546875,
        ""y"": 9.021239280700684,
        ""z"": 27.549558639526367
      },
      {
        ""x"": -45.026363372802734,
        ""y"": 9.450743675231934,
        ""z"": 19.199951171875
      },
      {
        ""x"": -47.85693359375,
        ""y"": 9.875244140625,
        ""z"": 9.875244140625
      },
      {
        ""x"": -35.47509765625,
        ""y"": 24.923583984375,
        ""z"": -24.850341796875
      },
      {
        ""x"": -38.65087890625,
        ""y"": 17.545654296875,
        ""z"": -26.374753952026367
      },
      {
        ""x"": -40.69677734375,
        ""y"": 9.05194091796875,
        ""z"": -27.55517578125
      },
      {
        ""x"": -38.65087890625,
        ""y"": 26.46435546875,
        ""z"": -17.4652099609375
      },
      {
        ""x"": -42.650390625,
        ""y"": 18.4566650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": -45.1552734375,
        ""y"": 9.48211669921875,
        ""z"": -19.1842041015625
      },
      {
        ""x"": -40.69677734375,
        ""y"": 27.647459030151367,
        ""z"": -8.96771240234375
      },
      {
        ""x"": -45.1552734375,
        ""y"": 19.2635498046875,
        ""z"": -9.39825439453125
      },
      {
        ""x"": -47.995601654052734,
        ""y"": 9.90875244140625,
        ""z"": -9.823790550231934
      },
      {
        ""x"": -35.47509765625,
        ""y"": -24.923583984375,
        ""z"": -24.850341796875
      },
      {
        ""x"": -38.65087890625,
        ""y"": -26.46435546875,
        ""z"": -17.4652099609375
      },
      {
        ""x"": -40.69677734375,
        ""y"": -27.647459030151367,
        ""z"": -8.96771240234375
      },
      {
        ""x"": -38.65087890625,
        ""y"": -17.545654296875,
        ""z"": -26.374753952026367
      },
      {
        ""x"": -42.650390625,
        ""y"": -18.4566650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": -45.1552734375,
        ""y"": -19.2635498046875,
        ""z"": -9.39825439453125
      },
      {
        ""x"": -40.69677734375,
        ""y"": -9.0518798828125,
        ""z"": -27.55517578125
      },
      {
        ""x"": -45.1552734375,
        ""y"": -9.48211669921875,
        ""z"": -19.1842041015625
      },
      {
        ""x"": -47.995601654052734,
        ""y"": -9.90875244140625,
        ""z"": -9.823790550231934
      },
      {
        ""x"": -24.850341796875,
        ""y"": 35.358638763427734,
        ""z"": 24.850341796875
      },
      {
        ""x"": -17.4879150390625,
        ""y"": 38.523681640625,
        ""z"": 26.370849609375
      },
      {
        ""x"": -9.021178245544434,
        ""y"": 40.55810546875,
        ""z"": 27.549558639526367
      },
      {
        ""x"": -26.370849609375,
        ""y"": 38.523681640625,
        ""z"": 17.4879150390625
      },
      {
        ""x"": -18.394287109375,
        ""y"": 42.52783203125,
        ""z"": 18.394287109375
      },
      {
        ""x"": -9.450743675231934,
        ""y"": 45.026363372802734,
        ""z"": 19.199951171875
      },
      {
        ""x"": -27.549558639526367,
        ""y"": 40.55810546875,
        ""z"": 9.021239280700684
      },
      {
        ""x"": -19.199951171875,
        ""y"": 45.026363372802734,
        ""z"": 9.450743675231934
      },
      {
        ""x"": -9.875244140625,
        ""y"": 47.85693359375,
        ""z"": 9.875244140625
      },
      {
        ""x"": 24.850341796875,
        ""y"": 35.358638763427734,
        ""z"": 24.850341796875
      },
      {
        ""x"": 26.370849609375,
        ""y"": 38.523681640625,
        ""z"": 17.48779296875
      },
      {
        ""x"": 27.549558639526367,
        ""y"": 40.55810546875,
        ""z"": 9.021239280700684
      },
      {
        ""x"": 17.48779296875,
        ""y"": 38.523681640625,
        ""z"": 26.370849609375
      },
      {
        ""x"": 18.394287109375,
        ""y"": 42.52783203125,
        ""z"": 18.394287109375
      },
      {
        ""x"": 19.199951171875,
        ""y"": 45.026363372802734,
        ""z"": 9.450743675231934
      },
      {
        ""x"": 9.021178245544434,
        ""y"": 40.55810546875,
        ""z"": 27.549558639526367
      },
      {
        ""x"": 9.450743675231934,
        ""y"": 45.026363372802734,
        ""z"": 19.199951171875
      },
      {
        ""x"": 9.875244140625,
        ""y"": 47.85693359375,
        ""z"": 9.875244140625
      },
      {
        ""x"": 24.923583984375,
        ""y"": 35.47509765625,
        ""z"": -24.850341796875
      },
      {
        ""x"": 17.545654296875,
        ""y"": 38.65087890625,
        ""z"": -26.374753952026367
      },
      {
        ""x"": 9.05194091796875,
        ""y"": 40.69677734375,
        ""z"": -27.55517578125
      },
      {
        ""x"": 26.46435546875,
        ""y"": 38.65087890625,
        ""z"": -17.46533203125
      },
      {
        ""x"": 18.4566650390625,
        ""y"": 42.650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": 9.48211669921875,
        ""y"": 45.1552734375,
        ""z"": -19.1842041015625
      },
      {
        ""x"": 27.647459030151367,
        ""y"": 40.69677734375,
        ""z"": -8.96771240234375
      },
      {
        ""x"": 19.2635498046875,
        ""y"": 45.1552734375,
        ""z"": -9.39825439453125
      },
      {
        ""x"": 9.90875244140625,
        ""y"": 47.995601654052734,
        ""z"": -9.823790550231934
      },
      {
        ""x"": -24.923583984375,
        ""y"": 35.47509765625,
        ""z"": -24.850341796875
      },
      {
        ""x"": -26.46435546875,
        ""y"": 38.65087890625,
        ""z"": -17.46533203125
      },
      {
        ""x"": -27.647459030151367,
        ""y"": 40.69677734375,
        ""z"": -8.96771240234375
      },
      {
        ""x"": -17.545654296875,
        ""y"": 38.65087890625,
        ""z"": -26.374753952026367
      },
      {
        ""x"": -18.4566650390625,
        ""y"": 42.650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": -19.2635498046875,
        ""y"": 45.1552734375,
        ""z"": -9.39825439453125
      },
      {
        ""x"": -9.05194091796875,
        ""y"": 40.69677734375,
        ""z"": -27.55517578125
      },
      {
        ""x"": -9.48211669921875,
        ""y"": 45.1552734375,
        ""z"": -19.1842041015625
      },
      {
        ""x"": -9.90875244140625,
        ""y"": 47.995601654052734,
        ""z"": -9.823790550231934
      },
      {
        ""x"": 35.358638763427734,
        ""y"": 24.850341796875,
        ""z"": 24.850341796875
      },
      {
        ""x"": 38.523681640625,
        ""y"": 17.4879150390625,
        ""z"": 26.370849609375
      },
      {
        ""x"": 40.55810546875,
        ""y"": 9.021239280700684,
        ""z"": 27.549558639526367
      },
      {
        ""x"": 38.523681640625,
        ""y"": 26.370849609375,
        ""z"": 17.48779296875
      },
      {
        ""x"": 42.52783203125,
        ""y"": 18.394287109375,
        ""z"": 18.394287109375
      },
      {
        ""x"": 45.026363372802734,
        ""y"": 9.450743675231934,
        ""z"": 19.199951171875
      },
      {
        ""x"": 40.55810546875,
        ""y"": 27.549558639526367,
        ""z"": 9.021239280700684
      },
      {
        ""x"": 45.026363372802734,
        ""y"": 19.199951171875,
        ""z"": 9.450743675231934
      },
      {
        ""x"": 47.85693359375,
        ""y"": 9.875244140625,
        ""z"": 9.875244140625
      },
      {
        ""x"": 35.358638763427734,
        ""y"": -24.850341796875,
        ""z"": 24.850341796875
      },
      {
        ""x"": 38.523681640625,
        ""y"": -26.370849609375,
        ""z"": 17.48779296875
      },
      {
        ""x"": 40.55810546875,
        ""y"": -27.549558639526367,
        ""z"": 9.021239280700684
      },
      {
        ""x"": 38.523681640625,
        ""y"": -17.48779296875,
        ""z"": 26.370849609375
      },
      {
        ""x"": 42.52783203125,
        ""y"": -18.394287109375,
        ""z"": 18.394287109375
      },
      {
        ""x"": 45.026363372802734,
        ""y"": -19.199951171875,
        ""z"": 9.450743675231934
      },
      {
        ""x"": 40.55810546875,
        ""y"": -9.021239280700684,
        ""z"": 27.549558639526367
      },
      {
        ""x"": 45.026363372802734,
        ""y"": -9.450743675231934,
        ""z"": 19.199951171875
      },
      {
        ""x"": 47.85693359375,
        ""y"": -9.875244140625,
        ""z"": 9.875244140625
      },
      {
        ""x"": 35.47509765625,
        ""y"": -24.923583984375,
        ""z"": -24.850341796875
      },
      {
        ""x"": 38.65087890625,
        ""y"": -17.545654296875,
        ""z"": -26.374753952026367
      },
      {
        ""x"": 40.69677734375,
        ""y"": -9.05194091796875,
        ""z"": -27.55517578125
      },
      {
        ""x"": 38.65087890625,
        ""y"": -26.46435546875,
        ""z"": -17.46533203125
      },
      {
        ""x"": 42.650390625,
        ""y"": -18.4566650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": 45.1552734375,
        ""y"": -9.48211669921875,
        ""z"": -19.1842041015625
      },
      {
        ""x"": 40.69677734375,
        ""y"": -27.647459030151367,
        ""z"": -8.96771240234375
      },
      {
        ""x"": 45.1552734375,
        ""y"": -19.2635498046875,
        ""z"": -9.39825439453125
      },
      {
        ""x"": 47.995601654052734,
        ""y"": -9.90875244140625,
        ""z"": -9.823790550231934
      },
      {
        ""x"": 35.47509765625,
        ""y"": 24.923583984375,
        ""z"": -24.850341796875
      },
      {
        ""x"": 38.65087890625,
        ""y"": 26.46435546875,
        ""z"": -17.46533203125
      },
      {
        ""x"": 40.69677734375,
        ""y"": 27.647459030151367,
        ""z"": -8.96771240234375
      },
      {
        ""x"": 38.65087890625,
        ""y"": 17.545654296875,
        ""z"": -26.374753952026367
      },
      {
        ""x"": 42.650390625,
        ""y"": 18.4566650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": 45.1552734375,
        ""y"": 19.2635498046875,
        ""z"": -9.39825439453125
      },
      {
        ""x"": 40.69677734375,
        ""y"": 9.05194091796875,
        ""z"": -27.55517578125
      },
      {
        ""x"": 45.1552734375,
        ""y"": 9.48211669921875,
        ""z"": -19.1842041015625
      },
      {
        ""x"": 47.995601654052734,
        ""y"": 9.90875244140625,
        ""z"": -9.823790550231934
      },
      {
        ""x"": 24.850341796875,
        ""y"": -35.358638763427734,
        ""z"": 24.850341796875
      },
      {
        ""x"": 17.4879150390625,
        ""y"": -38.523681640625,
        ""z"": 26.370849609375
      },
      {
        ""x"": 9.021239280700684,
        ""y"": -40.55810546875,
        ""z"": 27.549558639526367
      },
      {
        ""x"": 26.370849609375,
        ""y"": -38.523681640625,
        ""z"": 17.48779296875
      },
      {
        ""x"": 18.394287109375,
        ""y"": -42.52783203125,
        ""z"": 18.394287109375
      },
      {
        ""x"": 9.450743675231934,
        ""y"": -45.026363372802734,
        ""z"": 19.199951171875
      },
      {
        ""x"": 27.549558639526367,
        ""y"": -40.55810546875,
        ""z"": 9.021239280700684
      },
      {
        ""x"": 19.199951171875,
        ""y"": -45.026363372802734,
        ""z"": 9.450743675231934
      },
      {
        ""x"": 9.875244140625,
        ""y"": -47.85693359375,
        ""z"": 9.875244140625
      },
      {
        ""x"": -24.850341796875,
        ""y"": -35.358638763427734,
        ""z"": 24.850341796875
      },
      {
        ""x"": -26.370849609375,
        ""y"": -38.523681640625,
        ""z"": 17.4879150390625
      },
      {
        ""x"": -27.549558639526367,
        ""y"": -40.55810546875,
        ""z"": 9.021239280700684
      },
      {
        ""x"": -17.48779296875,
        ""y"": -38.523681640625,
        ""z"": 26.370849609375
      },
      {
        ""x"": -18.394287109375,
        ""y"": -42.52783203125,
        ""z"": 18.394287109375
      },
      {
        ""x"": -19.199951171875,
        ""y"": -45.026363372802734,
        ""z"": 9.450743675231934
      },
      {
        ""x"": -9.021239280700684,
        ""y"": -40.55810546875,
        ""z"": 27.549558639526367
      },
      {
        ""x"": -9.450743675231934,
        ""y"": -45.026363372802734,
        ""z"": 19.199951171875
      },
      {
        ""x"": -9.875244140625,
        ""y"": -47.85693359375,
        ""z"": 9.875244140625
      },
      {
        ""x"": -24.923583984375,
        ""y"": -35.47509765625,
        ""z"": -24.850341796875
      },
      {
        ""x"": -17.545654296875,
        ""y"": -38.65087890625,
        ""z"": -26.374753952026367
      },
      {
        ""x"": -9.0518798828125,
        ""y"": -40.69677734375,
        ""z"": -27.55517578125
      },
      {
        ""x"": -26.46435546875,
        ""y"": -38.65087890625,
        ""z"": -17.46533203125
      },
      {
        ""x"": -18.4566650390625,
        ""y"": -42.650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": -9.48211669921875,
        ""y"": -45.1552734375,
        ""z"": -19.1842041015625
      },
      {
        ""x"": -27.647459030151367,
        ""y"": -40.69677734375,
        ""z"": -8.96771240234375
      },
      {
        ""x"": -19.2635498046875,
        ""y"": -45.1552734375,
        ""z"": -9.39825439453125
      },
      {
        ""x"": -9.90875244140625,
        ""y"": -47.995601654052734,
        ""z"": -9.823790550231934
      },
      {
        ""x"": 24.923583984375,
        ""y"": -35.47509765625,
        ""z"": -24.850341796875
      },
      {
        ""x"": 26.46435546875,
        ""y"": -38.65087890625,
        ""z"": -17.46533203125
      },
      {
        ""x"": 27.647459030151367,
        ""y"": -40.69677734375,
        ""z"": -8.96771240234375
      },
      {
        ""x"": 17.545654296875,
        ""y"": -38.65087890625,
        ""z"": -26.374753952026367
      },
      {
        ""x"": 18.4566650390625,
        ""y"": -42.650390625,
        ""z"": -18.374267578125
      },
      {
        ""x"": 19.2635498046875,
        ""y"": -45.1552734375,
        ""z"": -9.39825439453125
      },
      {
        ""x"": 9.0518798828125,
        ""y"": -40.69677734375,
        ""z"": -27.55517578125
      },
      {
        ""x"": 9.48211669921875,
        ""y"": -45.1552734375,
        ""z"": -19.1842041015625
      },
      {
        ""x"": 9.90875244140625,
        ""y"": -47.995601654052734,
        ""z"": -9.823790550231934
      }
    ],
    ""Triangles"": [
      {
        ""p"": [
          177,
          178,
          109
        ]
      },
      {
        ""p"": [
          174,
          178,
          177
        ]
      },
      {
        ""p"": [
          173,
          174,
          177
        ]
      },
      {
        ""p"": [
          173,
          177,
          176
        ]
      },
      {
        ""p"": [
          176,
          177,
          108
        ]
      },
      {
        ""p"": [
          177,
          109,
          108
        ]
      },
      {
        ""p"": [
          178,
          8,
          109
        ]
      },
      {
        ""p"": [
          205,
          108,
          109
        ]
      },
      {
        ""p"": [
          174,
          175,
          178
        ]
      },
      {
        ""p"": [
          175,
          100,
          178
        ]
      },
      {
        ""p"": [
          178,
          100,
          8
        ]
      },
      {
        ""p"": [
          170,
          174,
          173
        ]
      },
      {
        ""p"": [
          171,
          175,
          174
        ]
      },
      {
        ""p"": [
          170,
          171,
          174
        ]
      },
      {
        ""p"": [
          48,
          173,
          176
        ]
      },
      {
        ""p"": [
          47,
          173,
          48
        ]
      },
      {
        ""p"": [
          47,
          170,
          173
        ]
      },
      {
        ""p"": [
          0,
          170,
          47
        ]
      },
      {
        ""p"": [
          0,
          26,
          170
        ]
      },
      {
        ""p"": [
          26,
          171,
          170
        ]
      },
      {
        ""p"": [
          48,
          176,
          49
        ]
      },
      {
        ""p"": [
          26,
          27,
          171
        ]
      },
      {
        ""p"": [
          49,
          176,
          107
        ]
      },
      {
        ""p"": [
          49,
          107,
          17
        ]
      },
      {
        ""p"": [
          176,
          108,
          107
        ]
      },
      {
        ""p"": [
          27,
          172,
          171
        ]
      },
      {
        ""p"": [
          171,
          172,
          175
        ]
      },
      {
        ""p"": [
          27,
          28,
          172
        ]
      },
      {
        ""p"": [
          199,
          17,
          107
        ]
      },
      {
        ""p"": [
          199,
          46,
          17
        ]
      },
      {
        ""p"": [
          202,
          107,
          108
        ]
      },
      {
        ""p"": [
          202,
          199,
          107
        ]
      },
      {
        ""p"": [
          205,
          202,
          108
        ]
      },
      {
        ""p"": [
          172,
          99,
          175
        ]
      },
      {
        ""p"": [
          175,
          99,
          100
        ]
      },
      {
        ""p"": [
          28,
          98,
          172
        ]
      },
      {
        ""p"": [
          172,
          98,
          99
        ]
      },
      {
        ""p"": [
          28,
          14,
          98
        ]
      },
      {
        ""p"": [
          198,
          46,
          199
        ]
      },
      {
        ""p"": [
          198,
          45,
          46
        ]
      },
      {
        ""p"": [
          201,
          199,
          202
        ]
      },
      {
        ""p"": [
          201,
          198,
          199
        ]
      },
      {
        ""p"": [
          14,
          185,
          98
        ]
      },
      {
        ""p"": [
          14,
          31,
          185
        ]
      },
      {
        ""p"": [
          98,
          186,
          99
        ]
      },
      {
        ""p"": [
          98,
          185,
          186
        ]
      },
      {
        ""p"": [
          99,
          187,
          100
        ]
      },
      {
        ""p"": [
          99,
          186,
          187
        ]
      },
      {
        ""p"": [
          197,
          45,
          198
        ]
      },
      {
        ""p"": [
          197,
          44,
          45
        ]
      },
      {
        ""p"": [
          31,
          182,
          185
        ]
      },
      {
        ""p"": [
          31,
          30,
          182
        ]
      },
      {
        ""p"": [
          100,
          187,
          103
        ]
      },
      {
        ""p"": [
          100,
          103,
          8
        ]
      },
      {
        ""p"": [
          187,
          102,
          103
        ]
      },
      {
        ""p"": [
          187,
          184,
          102
        ]
      },
      {
        ""p"": [
          186,
          184,
          187
        ]
      },
      {
        ""p"": [
          184,
          101,
          102
        ]
      },
      {
        ""p"": [
          185,
          183,
          186
        ]
      },
      {
        ""p"": [
          186,
          183,
          184
        ]
      },
      {
        ""p"": [
          185,
          182,
          183
        ]
      },
      {
        ""p"": [
          184,
          181,
          101
        ]
      },
      {
        ""p"": [
          183,
          181,
          184
        ]
      },
      {
        ""p"": [
          181,
          15,
          101
        ]
      },
      {
        ""p"": [
          181,
          34,
          15
        ]
      },
      {
        ""p"": [
          183,
          180,
          181
        ]
      },
      {
        ""p"": [
          180,
          34,
          181
        ]
      },
      {
        ""p"": [
          182,
          180,
          183
        ]
      },
      {
        ""p"": [
          180,
          33,
          34
        ]
      },
      {
        ""p"": [
          182,
          179,
          180
        ]
      },
      {
        ""p"": [
          179,
          33,
          180
        ]
      },
      {
        ""p"": [
          30,
          179,
          182
        ]
      },
      {
        ""p"": [
          179,
          32,
          33
        ]
      },
      {
        ""p"": [
          30,
          29,
          179
        ]
      },
      {
        ""p"": [
          29,
          32,
          179
        ]
      },
      {
        ""p"": [
          29,
          2,
          32
        ]
      },
      {
        ""p"": [
          106,
          109,
          8
        ]
      },
      {
        ""p"": [
          196,
          106,
          8
        ]
      },
      {
        ""p"": [
          193,
          106,
          196
        ]
      },
      {
        ""p"": [
          193,
          105,
          106
        ]
      },
      {
        ""p"": [
          105,
          205,
          106
        ]
      },
      {
        ""p"": [
          106,
          205,
          109
        ]
      },
      {
        ""p"": [
          105,
          204,
          205
        ]
      },
      {
        ""p"": [
          204,
          202,
          205
        ]
      },
      {
        ""p"": [
          204,
          201,
          202
        ]
      },
      {
        ""p"": [
          104,
          204,
          105
        ]
      },
      {
        ""p"": [
          203,
          201,
          204
        ]
      },
      {
        ""p"": [
          104,
          203,
          204
        ]
      },
      {
        ""p"": [
          190,
          105,
          193
        ]
      },
      {
        ""p"": [
          190,
          104,
          105
        ]
      },
      {
        ""p"": [
          203,
          200,
          201
        ]
      },
      {
        ""p"": [
          200,
          198,
          201
        ]
      },
      {
        ""p"": [
          200,
          197,
          198
        ]
      },
      {
        ""p"": [
          16,
          203,
          104
        ]
      },
      {
        ""p"": [
          40,
          16,
          104
        ]
      },
      {
        ""p"": [
          40,
          104,
          190
        ]
      },
      {
        ""p"": [
          16,
          43,
          203
        ]
      },
      {
        ""p"": [
          43,
          200,
          203
        ]
      },
      {
        ""p"": [
          43,
          42,
          200
        ]
      },
      {
        ""p"": [
          42,
          197,
          200
        ]
      },
      {
        ""p"": [
          42,
          41,
          197
        ]
      },
      {
        ""p"": [
          41,
          44,
          197
        ]
      },
      {
        ""p"": [
          41,
          1,
          44
        ]
      },
      {
        ""p"": [
          39,
          40,
          190
        ]
      },
      {
        ""p"": [
          39,
          190,
          189
        ]
      },
      {
        ""p"": [
          38,
          39,
          189
        ]
      },
      {
        ""p"": [
          189,
          190,
          193
        ]
      },
      {
        ""p"": [
          38,
          189,
          188
        ]
      },
      {
        ""p"": [
          3,
          38,
          188
        ]
      },
      {
        ""p"": [
          3,
          188,
          35
        ]
      },
      {
        ""p"": [
          189,
          193,
          192
        ]
      },
      {
        ""p"": [
          188,
          189,
          192
        ]
      },
      {
        ""p"": [
          192,
          193,
          196
        ]
      },
      {
        ""p"": [
          35,
          188,
          191
        ]
      },
      {
        ""p"": [
          188,
          192,
          191
        ]
      },
      {
        ""p"": [
          35,
          191,
          36
        ]
      },
      {
        ""p"": [
          192,
          196,
          195
        ]
      },
      {
        ""p"": [
          191,
          192,
          195
        ]
      },
      {
        ""p"": [
          36,
          191,
          194
        ]
      },
      {
        ""p"": [
          191,
          195,
          194
        ]
      },
      {
        ""p"": [
          36,
          194,
          37
        ]
      },
      {
        ""p"": [
          195,
          196,
          103
        ]
      },
      {
        ""p"": [
          196,
          8,
          103
        ]
      },
      {
        ""p"": [
          195,
          103,
          102
        ]
      },
      {
        ""p"": [
          194,
          195,
          102
        ]
      },
      {
        ""p"": [
          194,
          102,
          101
        ]
      },
      {
        ""p"": [
          37,
          194,
          101
        ]
      },
      {
        ""p"": [
          37,
          101,
          15
        ]
      },
      {
        ""p"": [
          214,
          9,
          121
        ]
      },
      {
        ""p"": [
          213,
          214,
          121
        ]
      },
      {
        ""p"": [
          210,
          214,
          213
        ]
      },
      {
        ""p"": [
          210,
          211,
          214
        ]
      },
      {
        ""p"": [
          211,
          112,
          214
        ]
      },
      {
        ""p"": [
          214,
          112,
          9
        ]
      },
      {
        ""p"": [
          213,
          121,
          120
        ]
      },
      {
        ""p"": [
          209,
          210,
          213
        ]
      },
      {
        ""p"": [
          212,
          213,
          120
        ]
      },
      {
        ""p"": [
          209,
          213,
          212
        ]
      },
      {
        ""p"": [
          207,
          211,
          210
        ]
      },
      {
        ""p"": [
          206,
          210,
          209
        ]
      },
      {
        ""p"": [
          206,
          207,
          210
        ]
      },
      {
        ""p"": [
          72,
          209,
          212
        ]
      },
      {
        ""p"": [
          71,
          209,
          72
        ]
      },
      {
        ""p"": [
          71,
          206,
          209
        ]
      },
      {
        ""p"": [
          4,
          206,
          71
        ]
      },
      {
        ""p"": [
          4,
          50,
          206
        ]
      },
      {
        ""p"": [
          50,
          207,
          206
        ]
      },
      {
        ""p"": [
          72,
          212,
          73
        ]
      },
      {
        ""p"": [
          50,
          51,
          207
        ]
      },
      {
        ""p"": [
          73,
          212,
          119
        ]
      },
      {
        ""p"": [
          212,
          120,
          119
        ]
      },
      {
        ""p"": [
          73,
          119,
          21
        ]
      },
      {
        ""p"": [
          51,
          208,
          207
        ]
      },
      {
        ""p"": [
          207,
          208,
          211
        ]
      },
      {
        ""p"": [
          51,
          52,
          208
        ]
      },
      {
        ""p"": [
          235,
          21,
          119
        ]
      },
      {
        ""p"": [
          235,
          70,
          21
        ]
      },
      {
        ""p"": [
          238,
          119,
          120
        ]
      },
      {
        ""p"": [
          238,
          235,
          119
        ]
      },
      {
        ""p"": [
          208,
          111,
          211
        ]
      },
      {
        ""p"": [
          211,
          111,
          112
        ]
      },
      {
        ""p"": [
          52,
          110,
          208
        ]
      },
      {
        ""p"": [
          208,
          110,
          111
        ]
      },
      {
        ""p"": [
          52,
          18,
          110
        ]
      },
      {
        ""p"": [
          234,
          70,
          235
        ]
      },
      {
        ""p"": [
          234,
          69,
          70
        ]
      },
      {
        ""p"": [
          18,
          221,
          110
        ]
      },
      {
        ""p"": [
          18,
          55,
          221
        ]
      },
      {
        ""p"": [
          110,
          222,
          111
        ]
      },
      {
        ""p"": [
          110,
          221,
          222
        ]
      },
      {
        ""p"": [
          111,
          223,
          112
        ]
      },
      {
        ""p"": [
          111,
          222,
          223
        ]
      },
      {
        ""p"": [
          55,
          218,
          221
        ]
      },
      {
        ""p"": [
          55,
          54,
          218
        ]
      },
      {
        ""p"": [
          221,
          219,
          222
        ]
      },
      {
        ""p"": [
          221,
          218,
          219
        ]
      },
      {
        ""p"": [
          54,
          215,
          218
        ]
      },
      {
        ""p"": [
          54,
          53,
          215
        ]
      },
      {
        ""p"": [
          53,
          56,
          215
        ]
      },
      {
        ""p"": [
          53,
          5,
          56
        ]
      },
      {
        ""p"": [
          215,
          56,
          57
        ]
      },
      {
        ""p"": [
          218,
          215,
          216
        ]
      },
      {
        ""p"": [
          215,
          57,
          216
        ]
      },
      {
        ""p"": [
          218,
          216,
          219
        ]
      },
      {
        ""p"": [
          216,
          57,
          58
        ]
      },
      {
        ""p"": [
          216,
          58,
          217
        ]
      },
      {
        ""p"": [
          219,
          216,
          217
        ]
      },
      {
        ""p"": [
          217,
          58,
          19
        ]
      },
      {
        ""p"": [
          222,
          219,
          220
        ]
      },
      {
        ""p"": [
          219,
          217,
          220
        ]
      },
      {
        ""p"": [
          222,
          220,
          223
        ]
      },
      {
        ""p"": [
          217,
          19,
          113
        ]
      },
      {
        ""p"": [
          220,
          217,
          113
        ]
      },
      {
        ""p"": [
          61,
          113,
          19
        ]
      },
      {
        ""p"": [
          223,
          220,
          114
        ]
      },
      {
        ""p"": [
          220,
          113,
          114
        ]
      },
      {
        ""p"": [
          61,
          230,
          113
        ]
      },
      {
        ""p"": [
          230,
          114,
          113
        ]
      },
      {
        ""p"": [
          60,
          230,
          61
        ]
      },
      {
        ""p"": [
          223,
          114,
          115
        ]
      },
      {
        ""p"": [
          112,
          223,
          115
        ]
      },
      {
        ""p"": [
          112,
          115,
          9
        ]
      },
      {
        ""p"": [
          60,
          227,
          230
        ]
      },
      {
        ""p"": [
          59,
          227,
          60
        ]
      },
      {
        ""p"": [
          230,
          231,
          114
        ]
      },
      {
        ""p"": [
          231,
          115,
          114
        ]
      },
      {
        ""p"": [
          227,
          231,
          230
        ]
      },
      {
        ""p"": [
          59,
          224,
          227
        ]
      },
      {
        ""p"": [
          7,
          224,
          59
        ]
      },
      {
        ""p"": [
          7,
          62,
          224
        ]
      },
      {
        ""p"": [
          231,
          232,
          115
        ]
      },
      {
        ""p"": [
          232,
          9,
          115
        ]
      },
      {
        ""p"": [
          227,
          228,
          231
        ]
      },
      {
        ""p"": [
          224,
          228,
          227
        ]
      },
      {
        ""p"": [
          228,
          232,
          231
        ]
      },
      {
        ""p"": [
          62,
          225,
          224
        ]
      },
      {
        ""p"": [
          224,
          225,
          228
        ]
      },
      {
        ""p"": [
          62,
          63,
          225
        ]
      },
      {
        ""p"": [
          228,
          229,
          232
        ]
      },
      {
        ""p"": [
          225,
          229,
          228
        ]
      },
      {
        ""p"": [
          63,
          226,
          225
        ]
      },
      {
        ""p"": [
          225,
          226,
          229
        ]
      },
      {
        ""p"": [
          63,
          64,
          226
        ]
      },
      {
        ""p"": [
          229,
          118,
          232
        ]
      },
      {
        ""p"": [
          232,
          118,
          9
        ]
      },
      {
        ""p"": [
          229,
          117,
          118
        ]
      },
      {
        ""p"": [
          226,
          117,
          229
        ]
      },
      {
        ""p"": [
          226,
          116,
          117
        ]
      },
      {
        ""p"": [
          64,
          116,
          226
        ]
      },
      {
        ""p"": [
          64,
          20,
          116
        ]
      },
      {
        ""p"": [
          65,
          6,
          68
        ]
      },
      {
        ""p"": [
          65,
          68,
          233
        ]
      },
      {
        ""p"": [
          233,
          68,
          69
        ]
      },
      {
        ""p"": [
          66,
          65,
          233
        ]
      },
      {
        ""p"": [
          233,
          69,
          234
        ]
      },
      {
        ""p"": [
          66,
          233,
          236
        ]
      },
      {
        ""p"": [
          236,
          233,
          234
        ]
      },
      {
        ""p"": [
          67,
          66,
          236
        ]
      },
      {
        ""p"": [
          236,
          234,
          237
        ]
      },
      {
        ""p"": [
          237,
          234,
          235
        ]
      },
      {
        ""p"": [
          237,
          235,
          238
        ]
      },
      {
        ""p"": [
          67,
          236,
          239
        ]
      },
      {
        ""p"": [
          239,
          236,
          237
        ]
      },
      {
        ""p"": [
          20,
          67,
          239
        ]
      },
      {
        ""p"": [
          20,
          239,
          116
        ]
      },
      {
        ""p"": [
          239,
          237,
          240
        ]
      },
      {
        ""p"": [
          116,
          239,
          240
        ]
      },
      {
        ""p"": [
          240,
          237,
          238
        ]
      },
      {
        ""p"": [
          116,
          240,
          117
        ]
      },
      {
        ""p"": [
          240,
          238,
          241
        ]
      },
      {
        ""p"": [
          117,
          240,
          241
        ]
      },
      {
        ""p"": [
          241,
          238,
          120
        ]
      },
      {
        ""p"": [
          117,
          241,
          118
        ]
      },
      {
        ""p"": [
          241,
          120,
          121
        ]
      },
      {
        ""p"": [
          118,
          241,
          121
        ]
      },
      {
        ""p"": [
          118,
          121,
          9
        ]
      },
      {
        ""p"": [
          44,
          1,
          74
        ]
      },
      {
        ""p"": [
          44,
          74,
          251
        ]
      },
      {
        ""p"": [
          45,
          44,
          251
        ]
      },
      {
        ""p"": [
          251,
          74,
          75
        ]
      },
      {
        ""p"": [
          45,
          251,
          254
        ]
      },
      {
        ""p"": [
          46,
          45,
          254
        ]
      },
      {
        ""p"": [
          251,
          75,
          252
        ]
      },
      {
        ""p"": [
          254,
          251,
          252
        ]
      },
      {
        ""p"": [
          252,
          75,
          76
        ]
      },
      {
        ""p"": [
          46,
          254,
          257
        ]
      },
      {
        ""p"": [
          17,
          46,
          257
        ]
      },
      {
        ""p"": [
          252,
          76,
          253
        ]
      },
      {
        ""p"": [
          253,
          76,
          22
        ]
      },
      {
        ""p"": [
          254,
          252,
          255
        ]
      },
      {
        ""p"": [
          257,
          254,
          255
        ]
      },
      {
        ""p"": [
          255,
          252,
          253
        ]
      },
      {
        ""p"": [
          17,
          257,
          122
        ]
      },
      {
        ""p"": [
          49,
          17,
          122
        ]
      },
      {
        ""p"": [
          253,
          22,
          125
        ]
      },
      {
        ""p"": [
          79,
          125,
          22
        ]
      },
      {
        ""p"": [
          255,
          253,
          256
        ]
      },
      {
        ""p"": [
          256,
          253,
          125
        ]
      },
      {
        ""p"": [
          257,
          255,
          258
        ]
      },
      {
        ""p"": [
          122,
          257,
          258
        ]
      },
      {
        ""p"": [
          258,
          255,
          256
        ]
      },
      {
        ""p"": [
          49,
          122,
          244
        ]
      },
      {
        ""p"": [
          48,
          49,
          244
        ]
      },
      {
        ""p"": [
          79,
          266,
          125
        ]
      },
      {
        ""p"": [
          78,
          266,
          79
        ]
      },
      {
        ""p"": [
          122,
          258,
          123
        ]
      },
      {
        ""p"": [
          244,
          122,
          123
        ]
      },
      {
        ""p"": [
          256,
          125,
          126
        ]
      },
      {
        ""p"": [
          266,
          126,
          125
        ]
      },
      {
        ""p"": [
          258,
          256,
          259
        ]
      },
      {
        ""p"": [
          123,
          258,
          259
        ]
      },
      {
        ""p"": [
          259,
          256,
          126
        ]
      },
      {
        ""p"": [
          48,
          244,
          243
        ]
      },
      {
        ""p"": [
          47,
          48,
          243
        ]
      },
      {
        ""p"": [
          78,
          263,
          266
        ]
      },
      {
        ""p"": [
          77,
          263,
          78
        ]
      },
      {
        ""p"": [
          244,
          123,
          247
        ]
      },
      {
        ""p"": [
          243,
          244,
          247
        ]
      },
      {
        ""p"": [
          123,
          259,
          124
        ]
      },
      {
        ""p"": [
          247,
          123,
          124
        ]
      },
      {
        ""p"": [
          259,
          126,
          127
        ]
      },
      {
        ""p"": [
          124,
          259,
          127
        ]
      },
      {
        ""p"": [
          266,
          267,
          126
        ]
      },
      {
        ""p"": [
          263,
          267,
          266
        ]
      },
      {
        ""p"": [
          267,
          127,
          126
        ]
      },
      {
        ""p"": [
          47,
          243,
          242
        ]
      },
      {
        ""p"": [
          0,
          47,
          242
        ]
      },
      {
        ""p"": [
          0,
          242,
          83
        ]
      },
      {
        ""p"": [
          243,
          247,
          246
        ]
      },
      {
        ""p"": [
          242,
          243,
          246
        ]
      },
      {
        ""p"": [
          83,
          242,
          245
        ]
      },
      {
        ""p"": [
          242,
          246,
          245
        ]
      },
      {
        ""p"": [
          83,
          245,
          84
        ]
      },
      {
        ""p"": [
          247,
          124,
          250
        ]
      },
      {
        ""p"": [
          246,
          247,
          250
        ]
      },
      {
        ""p"": [
          124,
          127,
          10
        ]
      },
      {
        ""p"": [
          250,
          124,
          10
        ]
      },
      {
        ""p"": [
          267,
          268,
          127
        ]
      },
      {
        ""p"": [
          268,
          10,
          127
        ]
      },
      {
        ""p"": [
          84,
          245,
          248
        ]
      },
      {
        ""p"": [
          84,
          248,
          85
        ]
      },
      {
        ""p"": [
          245,
          246,
          249
        ]
      },
      {
        ""p"": [
          246,
          250,
          249
        ]
      },
      {
        ""p"": [
          245,
          249,
          248
        ]
      },
      {
        ""p"": [
          250,
          10,
          133
        ]
      },
      {
        ""p"": [
          249,
          250,
          133
        ]
      },
      {
        ""p"": [
          85,
          248,
          131
        ]
      },
      {
        ""p"": [
          85,
          131,
          23
        ]
      },
      {
        ""p"": [
          248,
          249,
          132
        ]
      },
      {
        ""p"": [
          249,
          133,
          132
        ]
      },
      {
        ""p"": [
          248,
          132,
          131
        ]
      },
      {
        ""p"": [
          130,
          133,
          10
        ]
      },
      {
        ""p"": [
          268,
          130,
          10
        ]
      },
      {
        ""p"": [
          264,
          268,
          267
        ]
      },
      {
        ""p"": [
          263,
          264,
          267
        ]
      },
      {
        ""p"": [
          271,
          23,
          131
        ]
      },
      {
        ""p"": [
          271,
          82,
          23
        ]
      },
      {
        ""p"": [
          274,
          131,
          132
        ]
      },
      {
        ""p"": [
          274,
          271,
          131
        ]
      },
      {
        ""p"": [
          277,
          132,
          133
        ]
      },
      {
        ""p"": [
          130,
          277,
          133
        ]
      },
      {
        ""p"": [
          277,
          274,
          132
        ]
      },
      {
        ""p"": [
          265,
          130,
          268
        ]
      },
      {
        ""p"": [
          264,
          265,
          268
        ]
      },
      {
        ""p"": [
          260,
          264,
          263
        ]
      },
      {
        ""p"": [
          77,
          260,
          263
        ]
      },
      {
        ""p"": [
          5,
          260,
          77
        ]
      },
      {
        ""p"": [
          5,
          53,
          260
        ]
      },
      {
        ""p"": [
          260,
          261,
          264
        ]
      },
      {
        ""p"": [
          53,
          261,
          260
        ]
      },
      {
        ""p"": [
          261,
          265,
          264
        ]
      },
      {
        ""p"": [
          53,
          54,
          261
        ]
      },
      {
        ""p"": [
          54,
          262,
          261
        ]
      },
      {
        ""p"": [
          261,
          262,
          265
        ]
      },
      {
        ""p"": [
          54,
          55,
          262
        ]
      },
      {
        ""p"": [
          265,
          129,
          130
        ]
      },
      {
        ""p"": [
          262,
          129,
          265
        ]
      },
      {
        ""p"": [
          129,
          277,
          130
        ]
      },
      {
        ""p"": [
          55,
          128,
          262
        ]
      },
      {
        ""p"": [
          262,
          128,
          129
        ]
      },
      {
        ""p"": [
          55,
          18,
          128
        ]
      },
      {
        ""p"": [
          129,
          276,
          277
        ]
      },
      {
        ""p"": [
          128,
          276,
          129
        ]
      },
      {
        ""p"": [
          276,
          274,
          277
        ]
      },
      {
        ""p"": [
          18,
          275,
          128
        ]
      },
      {
        ""p"": [
          128,
          275,
          276
        ]
      },
      {
        ""p"": [
          18,
          52,
          275
        ]
      },
      {
        ""p"": [
          276,
          273,
          274
        ]
      },
      {
        ""p"": [
          275,
          273,
          276
        ]
      },
      {
        ""p"": [
          273,
          271,
          274
        ]
      },
      {
        ""p"": [
          52,
          272,
          275
        ]
      },
      {
        ""p"": [
          275,
          272,
          273
        ]
      },
      {
        ""p"": [
          52,
          51,
          272
        ]
      },
      {
        ""p"": [
          273,
          270,
          271
        ]
      },
      {
        ""p"": [
          272,
          270,
          273
        ]
      },
      {
        ""p"": [
          270,
          82,
          271
        ]
      },
      {
        ""p"": [
          270,
          81,
          82
        ]
      },
      {
        ""p"": [
          51,
          269,
          272
        ]
      },
      {
        ""p"": [
          272,
          269,
          270
        ]
      },
      {
        ""p"": [
          269,
          81,
          270
        ]
      },
      {
        ""p"": [
          51,
          50,
          269
        ]
      },
      {
        ""p"": [
          269,
          80,
          81
        ]
      },
      {
        ""p"": [
          50,
          80,
          269
        ]
      },
      {
        ""p"": [
          50,
          4,
          80
        ]
      },
      {
        ""p"": [
          38,
          3,
          86
        ]
      },
      {
        ""p"": [
          38,
          86,
          287
        ]
      },
      {
        ""p"": [
          39,
          38,
          287
        ]
      },
      {
        ""p"": [
          287,
          86,
          87
        ]
      },
      {
        ""p"": [
          39,
          287,
          290
        ]
      },
      {
        ""p"": [
          40,
          39,
          290
        ]
      },
      {
        ""p"": [
          287,
          87,
          288
        ]
      },
      {
        ""p"": [
          290,
          287,
          288
        ]
      },
      {
        ""p"": [
          288,
          87,
          88
        ]
      },
      {
        ""p"": [
          40,
          290,
          293
        ]
      },
      {
        ""p"": [
          16,
          40,
          293
        ]
      },
      {
        ""p"": [
          288,
          88,
          289
        ]
      },
      {
        ""p"": [
          289,
          88,
          24
        ]
      },
      {
        ""p"": [
          290,
          288,
          291
        ]
      },
      {
        ""p"": [
          293,
          290,
          291
        ]
      },
      {
        ""p"": [
          291,
          288,
          289
        ]
      },
      {
        ""p"": [
          16,
          293,
          134
        ]
      },
      {
        ""p"": [
          43,
          16,
          134
        ]
      },
      {
        ""p"": [
          289,
          24,
          137
        ]
      },
      {
        ""p"": [
          91,
          137,
          24
        ]
      },
      {
        ""p"": [
          291,
          289,
          292
        ]
      },
      {
        ""p"": [
          292,
          289,
          137
        ]
      },
      {
        ""p"": [
          293,
          291,
          294
        ]
      },
      {
        ""p"": [
          134,
          293,
          294
        ]
      },
      {
        ""p"": [
          294,
          291,
          292
        ]
      },
      {
        ""p"": [
          43,
          134,
          280
        ]
      },
      {
        ""p"": [
          42,
          43,
          280
        ]
      },
      {
        ""p"": [
          91,
          302,
          137
        ]
      },
      {
        ""p"": [
          90,
          302,
          91
        ]
      },
      {
        ""p"": [
          134,
          294,
          135
        ]
      },
      {
        ""p"": [
          280,
          134,
          135
        ]
      },
      {
        ""p"": [
          292,
          137,
          138
        ]
      },
      {
        ""p"": [
          302,
          138,
          137
        ]
      },
      {
        ""p"": [
          294,
          292,
          295
        ]
      },
      {
        ""p"": [
          135,
          294,
          295
        ]
      },
      {
        ""p"": [
          295,
          292,
          138
        ]
      },
      {
        ""p"": [
          42,
          280,
          279
        ]
      },
      {
        ""p"": [
          41,
          42,
          279
        ]
      },
      {
        ""p"": [
          90,
          299,
          302
        ]
      },
      {
        ""p"": [
          89,
          299,
          90
        ]
      },
      {
        ""p"": [
          280,
          135,
          283
        ]
      },
      {
        ""p"": [
          279,
          280,
          283
        ]
      },
      {
        ""p"": [
          135,
          295,
          136
        ]
      },
      {
        ""p"": [
          283,
          135,
          136
        ]
      },
      {
        ""p"": [
          295,
          138,
          139
        ]
      },
      {
        ""p"": [
          136,
          295,
          139
        ]
      },
      {
        ""p"": [
          302,
          303,
          138
        ]
      },
      {
        ""p"": [
          299,
          303,
          302
        ]
      },
      {
        ""p"": [
          303,
          139,
          138
        ]
      },
      {
        ""p"": [
          41,
          279,
          278
        ]
      },
      {
        ""p"": [
          1,
          41,
          278
        ]
      },
      {
        ""p"": [
          1,
          278,
          74
        ]
      },
      {
        ""p"": [
          279,
          283,
          282
        ]
      },
      {
        ""p"": [
          278,
          279,
          282
        ]
      },
      {
        ""p"": [
          74,
          278,
          281
        ]
      },
      {
        ""p"": [
          278,
          282,
          281
        ]
      },
      {
        ""p"": [
          74,
          281,
          75
        ]
      },
      {
        ""p"": [
          283,
          136,
          286
        ]
      },
      {
        ""p"": [
          282,
          283,
          286
        ]
      },
      {
        ""p"": [
          136,
          139,
          11
        ]
      },
      {
        ""p"": [
          286,
          136,
          11
        ]
      },
      {
        ""p"": [
          303,
          304,
          139
        ]
      },
      {
        ""p"": [
          304,
          11,
          139
        ]
      },
      {
        ""p"": [
          75,
          281,
          284
        ]
      },
      {
        ""p"": [
          75,
          284,
          76
        ]
      },
      {
        ""p"": [
          281,
          282,
          285
        ]
      },
      {
        ""p"": [
          282,
          286,
          285
        ]
      },
      {
        ""p"": [
          281,
          285,
          284
        ]
      },
      {
        ""p"": [
          286,
          11,
          145
        ]
      },
      {
        ""p"": [
          285,
          286,
          145
        ]
      },
      {
        ""p"": [
          76,
          284,
          143
        ]
      },
      {
        ""p"": [
          76,
          143,
          22
        ]
      },
      {
        ""p"": [
          284,
          285,
          144
        ]
      },
      {
        ""p"": [
          285,
          145,
          144
        ]
      },
      {
        ""p"": [
          284,
          144,
          143
        ]
      },
      {
        ""p"": [
          142,
          145,
          11
        ]
      },
      {
        ""p"": [
          304,
          142,
          11
        ]
      },
      {
        ""p"": [
          300,
          304,
          303
        ]
      },
      {
        ""p"": [
          299,
          300,
          303
        ]
      },
      {
        ""p"": [
          307,
          22,
          143
        ]
      },
      {
        ""p"": [
          307,
          79,
          22
        ]
      },
      {
        ""p"": [
          310,
          143,
          144
        ]
      },
      {
        ""p"": [
          310,
          307,
          143
        ]
      },
      {
        ""p"": [
          313,
          144,
          145
        ]
      },
      {
        ""p"": [
          142,
          313,
          145
        ]
      },
      {
        ""p"": [
          313,
          310,
          144
        ]
      },
      {
        ""p"": [
          301,
          142,
          304
        ]
      },
      {
        ""p"": [
          300,
          301,
          304
        ]
      },
      {
        ""p"": [
          296,
          300,
          299
        ]
      },
      {
        ""p"": [
          89,
          296,
          299
        ]
      },
      {
        ""p"": [
          7,
          296,
          89
        ]
      },
      {
        ""p"": [
          7,
          59,
          296
        ]
      },
      {
        ""p"": [
          296,
          297,
          300
        ]
      },
      {
        ""p"": [
          59,
          297,
          296
        ]
      },
      {
        ""p"": [
          297,
          301,
          300
        ]
      },
      {
        ""p"": [
          59,
          60,
          297
        ]
      },
      {
        ""p"": [
          60,
          298,
          297
        ]
      },
      {
        ""p"": [
          297,
          298,
          301
        ]
      },
      {
        ""p"": [
          60,
          61,
          298
        ]
      },
      {
        ""p"": [
          301,
          141,
          142
        ]
      },
      {
        ""p"": [
          298,
          141,
          301
        ]
      },
      {
        ""p"": [
          141,
          313,
          142
        ]
      },
      {
        ""p"": [
          61,
          140,
          298
        ]
      },
      {
        ""p"": [
          298,
          140,
          141
        ]
      },
      {
        ""p"": [
          61,
          19,
          140
        ]
      },
      {
        ""p"": [
          141,
          312,
          313
        ]
      },
      {
        ""p"": [
          140,
          312,
          141
        ]
      },
      {
        ""p"": [
          312,
          310,
          313
        ]
      },
      {
        ""p"": [
          19,
          311,
          140
        ]
      },
      {
        ""p"": [
          140,
          311,
          312
        ]
      },
      {
        ""p"": [
          19,
          58,
          311
        ]
      },
      {
        ""p"": [
          312,
          309,
          310
        ]
      },
      {
        ""p"": [
          311,
          309,
          312
        ]
      },
      {
        ""p"": [
          309,
          307,
          310
        ]
      },
      {
        ""p"": [
          58,
          308,
          311
        ]
      },
      {
        ""p"": [
          311,
          308,
          309
        ]
      },
      {
        ""p"": [
          58,
          57,
          308
        ]
      },
      {
        ""p"": [
          309,
          306,
          307
        ]
      },
      {
        ""p"": [
          308,
          306,
          309
        ]
      },
      {
        ""p"": [
          306,
          79,
          307
        ]
      },
      {
        ""p"": [
          306,
          78,
          79
        ]
      },
      {
        ""p"": [
          57,
          305,
          308
        ]
      },
      {
        ""p"": [
          308,
          305,
          306
        ]
      },
      {
        ""p"": [
          305,
          78,
          306
        ]
      },
      {
        ""p"": [
          57,
          56,
          305
        ]
      },
      {
        ""p"": [
          305,
          77,
          78
        ]
      },
      {
        ""p"": [
          56,
          77,
          305
        ]
      },
      {
        ""p"": [
          56,
          5,
          77
        ]
      },
      {
        ""p"": [
          37,
          15,
          146
        ]
      },
      {
        ""p"": [
          37,
          146,
          316
        ]
      },
      {
        ""p"": [
          36,
          37,
          316
        ]
      },
      {
        ""p"": [
          316,
          146,
          147
        ]
      },
      {
        ""p"": [
          36,
          316,
          315
        ]
      },
      {
        ""p"": [
          35,
          36,
          315
        ]
      },
      {
        ""p"": [
          316,
          147,
          319
        ]
      },
      {
        ""p"": [
          315,
          316,
          319
        ]
      },
      {
        ""p"": [
          319,
          147,
          148
        ]
      },
      {
        ""p"": [
          35,
          315,
          314
        ]
      },
      {
        ""p"": [
          3,
          35,
          314
        ]
      },
      {
        ""p"": [
          3,
          314,
          86
        ]
      },
      {
        ""p"": [
          315,
          319,
          318
        ]
      },
      {
        ""p"": [
          314,
          315,
          318
        ]
      },
      {
        ""p"": [
          319,
          148,
          322
        ]
      },
      {
        ""p"": [
          318,
          319,
          322
        ]
      },
      {
        ""p"": [
          322,
          148,
          12
        ]
      },
      {
        ""p"": [
          86,
          314,
          317
        ]
      },
      {
        ""p"": [
          314,
          318,
          317
        ]
      },
      {
        ""p"": [
          86,
          317,
          87
        ]
      },
      {
        ""p"": [
          318,
          322,
          321
        ]
      },
      {
        ""p"": [
          317,
          318,
          321
        ]
      },
      {
        ""p"": [
          322,
          12,
          157
        ]
      },
      {
        ""p"": [
          321,
          322,
          157
        ]
      },
      {
        ""p"": [
          154,
          157,
          12
        ]
      },
      {
        ""p"": [
          87,
          317,
          320
        ]
      },
      {
        ""p"": [
          317,
          321,
          320
        ]
      },
      {
        ""p"": [
          87,
          320,
          88
        ]
      },
      {
        ""p"": [
          154,
          349,
          157
        ]
      },
      {
        ""p"": [
          153,
          349,
          154
        ]
      },
      {
        ""p"": [
          321,
          157,
          156
        ]
      },
      {
        ""p"": [
          320,
          321,
          156
        ]
      },
      {
        ""p"": [
          349,
          156,
          157
        ]
      },
      {
        ""p"": [
          88,
          320,
          155
        ]
      },
      {
        ""p"": [
          320,
          156,
          155
        ]
      },
      {
        ""p"": [
          88,
          155,
          24
        ]
      },
      {
        ""p"": [
          153,
          348,
          349
        ]
      },
      {
        ""p"": [
          152,
          348,
          153
        ]
      },
      {
        ""p"": [
          349,
          346,
          156
        ]
      },
      {
        ""p"": [
          346,
          155,
          156
        ]
      },
      {
        ""p"": [
          348,
          346,
          349
        ]
      },
      {
        ""p"": [
          343,
          24,
          155
        ]
      },
      {
        ""p"": [
          346,
          343,
          155
        ]
      },
      {
        ""p"": [
          343,
          91,
          24
        ]
      },
      {
        ""p"": [
          152,
          347,
          348
        ]
      },
      {
        ""p"": [
          20,
          347,
          152
        ]
      },
      {
        ""p"": [
          20,
          64,
          347
        ]
      },
      {
        ""p"": [
          348,
          345,
          346
        ]
      },
      {
        ""p"": [
          345,
          343,
          346
        ]
      },
      {
        ""p"": [
          347,
          345,
          348
        ]
      },
      {
        ""p"": [
          342,
          91,
          343
        ]
      },
      {
        ""p"": [
          345,
          342,
          343
        ]
      },
      {
        ""p"": [
          342,
          90,
          91
        ]
      },
      {
        ""p"": [
          64,
          344,
          347
        ]
      },
      {
        ""p"": [
          347,
          344,
          345
        ]
      },
      {
        ""p"": [
          344,
          342,
          345
        ]
      },
      {
        ""p"": [
          64,
          63,
          344
        ]
      },
      {
        ""p"": [
          341,
          90,
          342
        ]
      },
      {
        ""p"": [
          344,
          341,
          342
        ]
      },
      {
        ""p"": [
          63,
          341,
          344
        ]
      },
      {
        ""p"": [
          341,
          89,
          90
        ]
      },
      {
        ""p"": [
          63,
          62,
          341
        ]
      },
      {
        ""p"": [
          62,
          89,
          341
        ]
      },
      {
        ""p"": [
          62,
          7,
          89
        ]
      },
      {
        ""p"": [
          32,
          2,
          92
        ]
      },
      {
        ""p"": [
          32,
          92,
          323
        ]
      },
      {
        ""p"": [
          33,
          32,
          323
        ]
      },
      {
        ""p"": [
          323,
          92,
          93
        ]
      },
      {
        ""p"": [
          33,
          323,
          326
        ]
      },
      {
        ""p"": [
          34,
          33,
          326
        ]
      },
      {
        ""p"": [
          323,
          93,
          324
        ]
      },
      {
        ""p"": [
          326,
          323,
          324
        ]
      },
      {
        ""p"": [
          324,
          93,
          94
        ]
      },
      {
        ""p"": [
          34,
          326,
          329
        ]
      },
      {
        ""p"": [
          15,
          34,
          329
        ]
      },
      {
        ""p"": [
          15,
          329,
          146
        ]
      },
      {
        ""p"": [
          324,
          94,
          325
        ]
      },
      {
        ""p"": [
          325,
          94,
          25
        ]
      },
      {
        ""p"": [
          326,
          324,
          327
        ]
      },
      {
        ""p"": [
          329,
          326,
          327
        ]
      },
      {
        ""p"": [
          327,
          324,
          325
        ]
      },
      {
        ""p"": [
          146,
          329,
          330
        ]
      },
      {
        ""p"": [
          329,
          327,
          330
        ]
      },
      {
        ""p"": [
          146,
          330,
          147
        ]
      },
      {
        ""p"": [
          325,
          25,
          149
        ]
      },
      {
        ""p"": [
          97,
          149,
          25
        ]
      },
      {
        ""p"": [
          327,
          325,
          328
        ]
      },
      {
        ""p"": [
          330,
          327,
          328
        ]
      },
      {
        ""p"": [
          328,
          325,
          149
        ]
      },
      {
        ""p"": [
          147,
          330,
          331
        ]
      },
      {
        ""p"": [
          330,
          328,
          331
        ]
      },
      {
        ""p"": [
          147,
          331,
          148
        ]
      },
      {
        ""p"": [
          97,
          338,
          149
        ]
      },
      {
        ""p"": [
          96,
          338,
          97
        ]
      },
      {
        ""p"": [
          328,
          149,
          150
        ]
      },
      {
        ""p"": [
          331,
          328,
          150
        ]
      },
      {
        ""p"": [
          338,
          150,
          149
        ]
      },
      {
        ""p"": [
          148,
          331,
          151
        ]
      },
      {
        ""p"": [
          331,
          150,
          151
        ]
      },
      {
        ""p"": [
          148,
          151,
          12
        ]
      },
      {
        ""p"": [
          96,
          335,
          338
        ]
      },
      {
        ""p"": [
          95,
          335,
          96
        ]
      },
      {
        ""p"": [
          338,
          339,
          150
        ]
      },
      {
        ""p"": [
          339,
          151,
          150
        ]
      },
      {
        ""p"": [
          335,
          339,
          338
        ]
      },
      {
        ""p"": [
          340,
          12,
          151
        ]
      },
      {
        ""p"": [
          339,
          340,
          151
        ]
      },
      {
        ""p"": [
          340,
          154,
          12
        ]
      },
      {
        ""p"": [
          95,
          332,
          335
        ]
      },
      {
        ""p"": [
          6,
          332,
          95
        ]
      },
      {
        ""p"": [
          6,
          65,
          332
        ]
      },
      {
        ""p"": [
          335,
          336,
          339
        ]
      },
      {
        ""p"": [
          336,
          340,
          339
        ]
      },
      {
        ""p"": [
          332,
          336,
          335
        ]
      },
      {
        ""p"": [
          337,
          154,
          340
        ]
      },
      {
        ""p"": [
          336,
          337,
          340
        ]
      },
      {
        ""p"": [
          337,
          153,
          154
        ]
      },
      {
        ""p"": [
          65,
          333,
          332
        ]
      },
      {
        ""p"": [
          332,
          333,
          336
        ]
      },
      {
        ""p"": [
          333,
          337,
          336
        ]
      },
      {
        ""p"": [
          65,
          66,
          333
        ]
      },
      {
        ""p"": [
          334,
          153,
          337
        ]
      },
      {
        ""p"": [
          333,
          334,
          337
        ]
      },
      {
        ""p"": [
          66,
          334,
          333
        ]
      },
      {
        ""p"": [
          334,
          152,
          153
        ]
      },
      {
        ""p"": [
          66,
          67,
          334
        ]
      },
      {
        ""p"": [
          67,
          152,
          334
        ]
      },
      {
        ""p"": [
          67,
          20,
          152
        ]
      },
      {
        ""p"": [
          26,
          0,
          83
        ]
      },
      {
        ""p"": [
          26,
          83,
          359
        ]
      },
      {
        ""p"": [
          27,
          26,
          359
        ]
      },
      {
        ""p"": [
          359,
          83,
          84
        ]
      },
      {
        ""p"": [
          27,
          359,
          362
        ]
      },
      {
        ""p"": [
          28,
          27,
          362
        ]
      },
      {
        ""p"": [
          359,
          84,
          360
        ]
      },
      {
        ""p"": [
          362,
          359,
          360
        ]
      },
      {
        ""p"": [
          360,
          84,
          85
        ]
      },
      {
        ""p"": [
          28,
          362,
          365
        ]
      },
      {
        ""p"": [
          14,
          28,
          365
        ]
      },
      {
        ""p"": [
          360,
          85,
          361
        ]
      },
      {
        ""p"": [
          361,
          85,
          23
        ]
      },
      {
        ""p"": [
          362,
          360,
          363
        ]
      },
      {
        ""p"": [
          365,
          362,
          363
        ]
      },
      {
        ""p"": [
          363,
          360,
          361
        ]
      },
      {
        ""p"": [
          14,
          365,
          158
        ]
      },
      {
        ""p"": [
          31,
          14,
          158
        ]
      },
      {
        ""p"": [
          361,
          23,
          161
        ]
      },
      {
        ""p"": [
          82,
          161,
          23
        ]
      },
      {
        ""p"": [
          363,
          361,
          364
        ]
      },
      {
        ""p"": [
          364,
          361,
          161
        ]
      },
      {
        ""p"": [
          365,
          363,
          366
        ]
      },
      {
        ""p"": [
          158,
          365,
          366
        ]
      },
      {
        ""p"": [
          366,
          363,
          364
        ]
      },
      {
        ""p"": [
          31,
          158,
          352
        ]
      },
      {
        ""p"": [
          30,
          31,
          352
        ]
      },
      {
        ""p"": [
          82,
          374,
          161
        ]
      },
      {
        ""p"": [
          81,
          374,
          82
        ]
      },
      {
        ""p"": [
          158,
          366,
          159
        ]
      },
      {
        ""p"": [
          352,
          158,
          159
        ]
      },
      {
        ""p"": [
          364,
          161,
          162
        ]
      },
      {
        ""p"": [
          374,
          162,
          161
        ]
      },
      {
        ""p"": [
          366,
          364,
          367
        ]
      },
      {
        ""p"": [
          159,
          366,
          367
        ]
      },
      {
        ""p"": [
          367,
          364,
          162
        ]
      },
      {
        ""p"": [
          30,
          352,
          351
        ]
      },
      {
        ""p"": [
          29,
          30,
          351
        ]
      },
      {
        ""p"": [
          81,
          371,
          374
        ]
      },
      {
        ""p"": [
          80,
          371,
          81
        ]
      },
      {
        ""p"": [
          352,
          159,
          355
        ]
      },
      {
        ""p"": [
          351,
          352,
          355
        ]
      },
      {
        ""p"": [
          159,
          367,
          160
        ]
      },
      {
        ""p"": [
          355,
          159,
          160
        ]
      },
      {
        ""p"": [
          367,
          162,
          163
        ]
      },
      {
        ""p"": [
          160,
          367,
          163
        ]
      },
      {
        ""p"": [
          374,
          375,
          162
        ]
      },
      {
        ""p"": [
          371,
          375,
          374
        ]
      },
      {
        ""p"": [
          375,
          163,
          162
        ]
      },
      {
        ""p"": [
          29,
          351,
          350
        ]
      },
      {
        ""p"": [
          2,
          29,
          350
        ]
      },
      {
        ""p"": [
          2,
          350,
          92
        ]
      },
      {
        ""p"": [
          351,
          355,
          354
        ]
      },
      {
        ""p"": [
          350,
          351,
          354
        ]
      },
      {
        ""p"": [
          92,
          350,
          353
        ]
      },
      {
        ""p"": [
          350,
          354,
          353
        ]
      },
      {
        ""p"": [
          92,
          353,
          93
        ]
      },
      {
        ""p"": [
          355,
          160,
          358
        ]
      },
      {
        ""p"": [
          354,
          355,
          358
        ]
      },
      {
        ""p"": [
          160,
          163,
          13
        ]
      },
      {
        ""p"": [
          358,
          160,
          13
        ]
      },
      {
        ""p"": [
          375,
          376,
          163
        ]
      },
      {
        ""p"": [
          376,
          13,
          163
        ]
      },
      {
        ""p"": [
          93,
          353,
          356
        ]
      },
      {
        ""p"": [
          93,
          356,
          94
        ]
      },
      {
        ""p"": [
          353,
          354,
          357
        ]
      },
      {
        ""p"": [
          354,
          358,
          357
        ]
      },
      {
        ""p"": [
          353,
          357,
          356
        ]
      },
      {
        ""p"": [
          358,
          13,
          169
        ]
      },
      {
        ""p"": [
          357,
          358,
          169
        ]
      },
      {
        ""p"": [
          94,
          356,
          167
        ]
      },
      {
        ""p"": [
          94,
          167,
          25
        ]
      },
      {
        ""p"": [
          356,
          357,
          168
        ]
      },
      {
        ""p"": [
          357,
          169,
          168
        ]
      },
      {
        ""p"": [
          356,
          168,
          167
        ]
      },
      {
        ""p"": [
          166,
          169,
          13
        ]
      },
      {
        ""p"": [
          376,
          166,
          13
        ]
      },
      {
        ""p"": [
          372,
          376,
          375
        ]
      },
      {
        ""p"": [
          371,
          372,
          375
        ]
      },
      {
        ""p"": [
          379,
          25,
          167
        ]
      },
      {
        ""p"": [
          379,
          97,
          25
        ]
      },
      {
        ""p"": [
          382,
          167,
          168
        ]
      },
      {
        ""p"": [
          382,
          379,
          167
        ]
      },
      {
        ""p"": [
          385,
          168,
          169
        ]
      },
      {
        ""p"": [
          166,
          385,
          169
        ]
      },
      {
        ""p"": [
          385,
          382,
          168
        ]
      },
      {
        ""p"": [
          373,
          166,
          376
        ]
      },
      {
        ""p"": [
          372,
          373,
          376
        ]
      },
      {
        ""p"": [
          368,
          372,
          371
        ]
      },
      {
        ""p"": [
          80,
          368,
          371
        ]
      },
      {
        ""p"": [
          4,
          368,
          80
        ]
      },
      {
        ""p"": [
          4,
          71,
          368
        ]
      },
      {
        ""p"": [
          368,
          369,
          372
        ]
      },
      {
        ""p"": [
          71,
          369,
          368
        ]
      },
      {
        ""p"": [
          369,
          373,
          372
        ]
      },
      {
        ""p"": [
          71,
          72,
          369
        ]
      },
      {
        ""p"": [
          72,
          370,
          369
        ]
      },
      {
        ""p"": [
          369,
          370,
          373
        ]
      },
      {
        ""p"": [
          72,
          73,
          370
        ]
      },
      {
        ""p"": [
          373,
          165,
          166
        ]
      },
      {
        ""p"": [
          370,
          165,
          373
        ]
      },
      {
        ""p"": [
          165,
          385,
          166
        ]
      },
      {
        ""p"": [
          73,
          164,
          370
        ]
      },
      {
        ""p"": [
          370,
          164,
          165
        ]
      },
      {
        ""p"": [
          73,
          21,
          164
        ]
      },
      {
        ""p"": [
          165,
          384,
          385
        ]
      },
      {
        ""p"": [
          164,
          384,
          165
        ]
      },
      {
        ""p"": [
          384,
          382,
          385
        ]
      },
      {
        ""p"": [
          21,
          383,
          164
        ]
      },
      {
        ""p"": [
          164,
          383,
          384
        ]
      },
      {
        ""p"": [
          21,
          70,
          383
        ]
      },
      {
        ""p"": [
          384,
          381,
          382
        ]
      },
      {
        ""p"": [
          383,
          381,
          384
        ]
      },
      {
        ""p"": [
          381,
          379,
          382
        ]
      },
      {
        ""p"": [
          70,
          380,
          383
        ]
      },
      {
        ""p"": [
          383,
          380,
          381
        ]
      },
      {
        ""p"": [
          70,
          69,
          380
        ]
      },
      {
        ""p"": [
          381,
          378,
          379
        ]
      },
      {
        ""p"": [
          380,
          378,
          381
        ]
      },
      {
        ""p"": [
          378,
          97,
          379
        ]
      },
      {
        ""p"": [
          378,
          96,
          97
        ]
      },
      {
        ""p"": [
          69,
          377,
          380
        ]
      },
      {
        ""p"": [
          380,
          377,
          378
        ]
      },
      {
        ""p"": [
          377,
          96,
          378
        ]
      },
      {
        ""p"": [
          69,
          68,
          377
        ]
      },
      {
        ""p"": [
          377,
          95,
          96
        ]
      },
      {
        ""p"": [
          68,
          95,
          377
        ]
      },
      {
        ""p"": [
          68,
          6,
          95
        ]
      }
    ]
  }
";



			
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
