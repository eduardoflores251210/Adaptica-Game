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
using UnityEngine;
using UnityEngine.SceneManagement;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; //ignorar este remanete 
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using UnityEngine.AddressableAssets;


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

		public CellGameData(float dNA_Amount, float maxDNA_Got, float progress, float playerHealth, GéneroBiológico gender)
		{
			DNA_Amount = dNA_Amount;
			MaxDNA_Got = maxDNA_Got;
			Progress = progress;
			PlayerHealth = playerHealth;
			Gender = gender;
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
			return $"DNA {DNA_Amount}, progresss {Progress}, HP {PlayerHealth}, Gender {Gender}";
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
		InteractWithCreature, // hace amigos, pelea o ignora según el humor del alien
		InteractWithSpecies,  // alianzas o exterminios interestelares, todo en uno
		UseSuperThing,      //usar una super habilidad como Frenesi de compras
		DETERMINATION,      // Undertale mode ON
		FindChara,          // Si estaba EN una hiperfijación de UNDERTALE cuando hice el Enum
		Hope,
		Dream,
		HopeAndDream,       // combo Asriel que te da DETERMINACIÓN
		AdvanceStage,
		BuildBuilding,
		Gift,
		FindBean,           // [bean es la criatura mas adorable del spore de Maxis]
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
		DELTARUNE,						//si también estaba en una hiperfijación de DELTARUNE cuando hice el Enum
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
		public static void EditMicrobe()
		{
			if (!Saver.HasLoadedAnySave())
			{
				CrossScenePackageSender Mailman = CrossScenePackageSender.Instance;//No  puedo cambiar esos nombres de destinatario de MC yMain Camera CS por que el cartero No tiene codigo postal solo nombre de destinatario :(
				Mailman.SendTypedPackage("EnterEdit", "CellSaver", false, new string[2] { nameof(Boolean), "LodStg" }); //avisarle a cellsaver QUE AL GUARDAR NO ENTRAREMOS AL ESTADIO CELULA DIGO MICROBIO
				LoadWithLoadingScreen.LoadScene(1, Stages.Microbe); // Microbe Editor
				return;
			}
			if (Saver.TryToLoadLastMicrobeRevision(Saver.CurrentGame.CreatureName, out var data))
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
			var handle = Addressables.LoadAssetAsync<ConfigLoadScreen>("Assets/GLSS"); // "GLSS" es el Address que le pusiste
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
	/// XD
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
