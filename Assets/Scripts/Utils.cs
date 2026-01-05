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
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; //ignorar este remanenre 
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
		public static string GenerateGalaxyName()
		{
			string[] Catalogues = { "GCA", "EGC", "SGC", "GCGC", "VGE" };
			int number1 = Random.Range(100, 9999);
			int number2 = Random.Range(10, 999);
			return $"{Catalogues[Random.Range(0, Catalogues.Length)]} {number1}-{number2}";
		}

		public static string GenerateStarName_NASAStyle()
		{
			int catalogNumber = Random.Range(10000, 999999);
			string catalogPrefix = Random.value > 0.5f ? "SC" : "SL";
			return $"{catalogPrefix} {catalogNumber}";
		}

		public static string GeneratePlanetName_NASAStyle(string systemName, int idx)
		{
			//Debug.Log(idx.ToString());
			char suffix = (char)('b' + idx); // b, c, d, etc.
			return $"{systemName}{suffix}";
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
	static void a(bool f)
	{
		if (f) 
		a(Random.Range(0,2)==0);
	}
}
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
namespace SerializableTypes
{
	/// <summary>
	/// El enum de los estadios del juego
	/// Microbio (cof cof Celúla de spore)
	/// criatura (tiene el mismo nombre que en spore)
	/// tribal (no ha empezado desarollo pero tiene el mismo nombre que en spore)
	/// city  (no ha empezado el desarollo pero tiene el mismo nombre que su equivalente descartado en spore)
	/// Civilization (no ha empezado desarollo pero tiene el mismo nombre que en spore)
	/// Space (estructuras de datos en cosntrucción aunque ya puedes visitar sistemas pero no planetas) tiene el mismo nombre que en spore
	/// </summary>
	public enum Stages
	{
		Microbe = 0, //en construccion aunque ya es Jugable
		Creature, //en construccion, NO, ni ha empezado desarollo
		tribal, //No ha empesado desarollos 
		City, //No ha empesado desarollos       ademas se le llama acvtualmente feudal
		Civilization, //No ha empesado desarollo		ademas se le llama actualmente NACION
		Space //estructuras de datos en cosntrucción  aunque ya puedes visitar sistemas pero no planetas
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
	public class HistoryActions
	{
		public HistoryPaths Path;
		public ActionType Tipo;
		public string[] Tags;
		public StatList propieties; //para propiedades Key Value
	}
	public enum HistoryPaths
	{
		Friendly,
		Neutral,
		Agressive
	}

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
		FindChara,
		Hope,
		Dream,
		HopeAndDream,       // combo Asriel que te da DETERMINACIÓN
		AdvanceStage,
		BuildBuilding,
		Gift,
		FindBean,           // bean spotted!
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
		DELTARUNE,
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
		SPORE,
		CrashGAME,
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
			var Filepath = /*Path.Join(*/PAth/*)*/;
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
					SceneManager.LoadScene(6);
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
			SceneManager.LoadScene(4); // Microbe stage
		}

		public static void LoadMicrobeStage(CrossScenePackageSender mailMan, string creatureName)
		{
			if (mailMan == null)
			{
				Debug.LogError("No se pudo obtener CrossScenePackageSender");
				return;
			}

			string filePath = Path.Combine(Paths.Cells, $"{creatureName}.json");

			if (!File.Exists(filePath))
			{
				Debug.LogWarning($"Archivo no encontrado: {filePath}, cargando microbio vacío");
				LoadEmptyMicrobe(mailMan);
				return;
			}

			try
			{
				string json = File.ReadAllText(filePath);
				MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(json);

				if (microbe == null )
				{
					Debug.LogWarning("SavedGame no tiene criatura válida, cargando microbio vacío");
					LoadEmptyMicrobe(mailMan);
					return;
				}
				microbe.CenterMicrobe();
				microbe.RotateMicrobeEuler(new(0, 90, 0)); 

				mailMan.SendTypedPackage("StageLoader", "Player", microbe, new string[] { nameof(MicrobeData) });
				SceneManager.LoadScene(4); // Microbe stage
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error al cargar microbio: {ex.Message}, cargando microbio vacío");
				LoadEmptyMicrobe(mailMan);
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
		//simbologia:
		//MC					: Marching Cubes
		//Main Camera CS		: Basurero con componentes distintos que tambien renderiza la escena y guarda el microbio
		//"EnterEdit"			: El Sender que le avisa a MicrobeSaver QUE YA HAY UNA PARTIDA Y NO TIENE QUE CREAR OTRA
		//"CellSaver"			: Se añadio la capacidad que el Mailman te de paquetes basándote en un string en vez de game objects asi que ya no tengo que escribir el nombre de la cámara solo "CellSaver"
		//"MC.SegmentManager"	: se especifica que es el segment manager de MC y no el componente de marching cubes

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

}

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
		public static SavedGame CreateSavefile(string CreatureName, ulong PlanetID)
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
			string fil = J(pt.SaveFiles, SHA);
			Directory.CreateDirectory(fil);
			string CC = J(fil, "CreationPrivate");
			Directory.CreateDirectory(CC);
			Directory.CreateDirectory(J(CC, "Microbe"));
			Directory.CreateDirectory(J(CC, "Creatures"));
			Directory.CreateDirectory(J(CC, "TribalClothes"));
			Directory.CreateDirectory(J(CC, "FeudalClothes"));
			Directory.CreateDirectory(J(CC, "NationClothes"));
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
			string SAV = J(fil, "Save.Json");
			string JAV = JsonUtility.ToJson(game, true);
			File.WriteAllText(SAV, JAV);

			return game;
		}
		public static string J(string a, string b) => Path.Combine(a, b); //si me da peresa escribir Path.Join
	}

}