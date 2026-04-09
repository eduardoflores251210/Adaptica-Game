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
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; //ignorar este remanete 
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

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
			Actions = actions ?? throw new ArgumentNullException(nameof(actions) + "ES NULL!!!!!!!!!");
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
			return DNA_Amount == other.DNA_Amount && Progress == other.Progress && PlayerHealth == other.PlayerHealth && Gender == other.Gender && MaxDNA_Got == other.MaxDNA_Got;
		}
		public override bool Equals(object o)
		{
			if (ReferenceEquals(this, o)) return true;
			if (o is null) return false;

			if (o is CellGameData Cell)
			{
				return Equals((CellGameData)o);
			}
			else return false;
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
		Transport_Taxi_Stop = 39,
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
		Friendly = 1,
		Neutral = 0,
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
		PlayMusic,          //una de las fromas de imcrementar lreación en tribu
		MakeAthemn,
		SPORE,                                              //esto deveria ser un logro no una acción
		CrashGAME,                      //COMO LO LOGRASTE???      [sarcasmo]
		Respuesta,
		Suerte,
		DessignClothesForCreature       //diseñar una nueva ropa
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
					LoadWithLoadingScreen.LoadScene(0, Stages.MainMenu);
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
				if (microbe == null)
				{
					Debug.LogWarning("SavedGame no tiene criatura válida, cargando microbio vacío");
					LoadEmptyMicrobe(mailMan);
					return;
				}
				microbe.CenterMicrobe();
				microbe.RotateMicrobeEuler(new(0, 90, 0));

				mailMan.SendTypedPackage("StageLoader", "Player", microbe, new string[] { nameof(MicrobeData) });
				LoadWithLoadingScreen.LoadScene(4, Stages.Microbe); // Microbe stage
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
			if (Saver.CurrentSaveName == null || Saver.CurrentGame == null)
			{
				Debug.LogError("No hay partida cargada en Saver");
				return;
			}
			switch (Saver.CurrentGame.CurentStage)
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

		[ConsoleCommand(Name = "!editarmic")]
		public static void EditMicrobeCommand()
		{
			//asume que estas en el estadio celula 
			if (Saver.HasLoadedAnySave())
			{
				Debug.Log("ENTRANDO AL EDITOR, ADVERTENCIA ESTO NO ESTA PROVADO ASI QUE PODRIA CORROMPER TU HERMOSA CREACIÓN");
				EditMicrobe();
			}
			else
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
				LoadWithLoadingScreen.LoadScene(1, Stages.Microbe); // Microbe Editor
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
		public static bool TryToLoadMicrobe(string name, out MicrobeData data)
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
			else
			{
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
			else
			{
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

				}
				catch (Exception ex) { Debug.LogError(ex); }
			}
			else
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
				}
				catch (Exception ex) { Debug.LogError(ex); return false; }
			}
			else
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
			}
			catch
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
