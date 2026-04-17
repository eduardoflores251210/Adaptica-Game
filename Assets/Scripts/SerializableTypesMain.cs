//advertencia hay MUCHO comentario XD y NO hay que tocar mucho de este archivo por que es el corazon del juego
//un momento de silencio por el pobre SHA512 que se queda sin su gloria de nombrar galaxias y ahora solo nombra guardados XD
using SerializableTypes.Biology;
using SerializableTypes.Game;
using SerializableTypes.Game.Microbe;
using SerializableTypes.Space;
using System;
using System.Collections.Generic;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; //ignorar este remanete 
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale

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
		Vehicles_Plane_Religous, //este junto a economic y economuic y religious SI EXISten en spore 
		Vehicles_Boat_Civilian,//ok estos barcos a lo mejor si se usan
		Vehicles_Boat_Civilian_Ferry,
		Vehicles_Boat_Civilian_Cruise,
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
		AdvanceStage,        // ASI ES  EL QUE MAS FELIZ TE HACE 
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






}
