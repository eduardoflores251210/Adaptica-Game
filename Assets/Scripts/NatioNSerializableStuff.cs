using System;
using System.Collections.Generic;
// PLACE HOLDER ESTO NO ES FINAL EN EL FUTURO SE GUARDARA DE MEJOR MANERA PERO POR AHORA ES PARA PROBAR COSAS Y VER COMO es
namespace SerializableTypes.Game.Nation
{
	public enum GovermentType
	{
		Tribal = 0,
		Feudal = 1,
		DirectDemocracy = 2,
		RepresentativeDemocracy = 3,
		Dictatorship = 4, //cof cof Kim Jong Un cof cof-
		Monarchies_Absolute = 5,
		Monarchies_Constitutional = 6,
		Monarchies_Parliamentary = 7, // god Save the Queen ... OH ups King XD aun no he superado lo de Elizabeth II. (1936-2022) RIP
		Monarchies_Unspecified = 8, // para los casos en los que no se sabe si es absoluta o constitucional, o simplemente no se quiere especificar, como por ejemplo el reino de España, que es una monarquia constitucional pero el rey tiene muy poco poder real, aunque oficialmente es el jefe del estado y el comandante en jefe de las fuerzas armadas... (si es que eso tiene algo de poder XD)
		Communism = 9,       // Soyuz Sovetskikh Sotsialisticheskikh Respublik, o Union de Repúblicas Socialistas Soviéticas, o simplemente URSS, o simplemente Rusia... (si es que se puede llamar Rusia a eso XD)
		Socialism = 10,
		Anarchy = 11, 
		Oligarchy = 12,      //ugh, basicamente las "democracias" de hoy en dia, donde el "pueblo" tiene el poder pero en realidad el poder lo tienen los ricos y las corporaciones... (si es que se puede llamar poder a eso XD)
		Theocracy = 13,     // oh señor del universo por favor no me hagas esto... [inserta religion] es la verdadera religion y el que no lo crea se va al infierno... (si es que existe XD)
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
		TieneMaximoPoderConservador = 1 << 3, //si inspirado en Mexico  pero en el tiempo de republivas centralistas XD
	}
	//PRUEBA 
	[Serializable]
	public struct NationSerializable
	{
		public string Name;
		public GovermentType GovermentType;
		public GovermentPowerSeparation GovermentPowerSeparation;
		public int Population;
		public float GDP;
		public float MilitaryStrength;
		public float HappinessIndex;
		public float CorruptionLevel;
		public float EducationLevel;
		public float HealthcareQuality;
		public float EnvironmentalSustainability;
		public string id; // id unica de la nacion, para poder referenciarla desde otras estructuras, como por ejemplo las organizaciones internacionales
		public GovermentInstitution[] GovermentInstitutions; // Lista de instituciones gubernamentales, como por ejemplo el congreso, la corte suprema, etc
	}
	public enum INOrganizationType
	{
		Alliance = 0,
		TradeUnion = 1,
		EULike = 2,
		UNLike = 3,
		TransportPact = 4,
	}
	public enum INOrganizationMembershipStatus
	{
		Member = 0,
		Observer = 1,
		NonMember = 2,
	}
	/// <summary>
	/// Estructura que representa una organización internacional.
	/// </summary>
	[Serializable]
	public struct InterNationalOrg
	{
		public string Name;
		public INOrganizationType Type;
		public List<string> MemberNations; // Lista de ids de las naciones miembros
		public List<string> ObserverNations; // Lista de ids de las naciones observadoras
		public SubjectOfPower[] Subjects; // Lista de temas o áreas de poder que maneja la organización, como por ejemplo comercio internacional, seguridad internacional, derechos humanos, etc
	}
	public enum WarType
	{
		Conventional = 0,
		Civil = 1,
		Proxy = 2,
		Nuclear = 3,
		Cyber = 4,
		Economic = 5,
	}
	public enum GovermentInstitutionType
	{
		DIRECTPOWER = 0,
		IndependentAgency = 1,
		MinisterioOSecretariaODepartamento = 2, // a menudo son casi Sinonimos asi que flojera los Caso
		Other = 3,
	}
	public enum SubjectOfPower
	{
		Economy = 0,
		Military = 1,
		ForeignPolicy = 2,
		DomesticPolicy = 3,
		Culture = 4,
		Education = 5,
		Healthcare = 6,
		Environment = 7,
		Transportation = 8,
		Energy = 9,
		ScienceAndTechnology = 10,
		ComputerSecurity = 11,
		Intelligence = 12,
		CorruptionPrevention = 13,
		inai = 14, //easter egg, inai rip 2002-2025 un minuto de silencio por el instituto nacional de acceso a la informacion, que era una institucion mexicana encargada de garantizar el derecho de acceso a la informacion publica, pero que fue eliminado por el gobierno actual... (si es que se puede llamar gobierno a eso XD)
		TV_Regulation = 15,
		Radio_Regulation = 16,
		Film_Regulation = 17,
		legislation = 18,
		elections = 19,
		executivepower = 20,
		SupremeCourt = 21,
		transparencia = 22,
		accesoainformacion = 23,
		policia = 24,
		proteccioncivil = 25,
		proteccionALaPrivacidad = 26,
		Copyright = 27,
		ambiente = 28,
		registrocivil = 29,
		proteccionALosAnimales = 30,
		derechosHumanos = 31,
		derechosLaborales = 32,
		Consumidor = 33,
		SeguroSocial = 34,
		gastronomia = 35,
		SANIDAD = 36,
		AI_Regulation = 37,
		internet_Regulation = 38,
		spaceExploration = 39, //NASA, SpaceX, etc
		farms = 40, // agricultura, ganaderia, pesca, etc
		fabrics = 41, // industria, manufactura, etc
		LaRespuestaALaPreguntaDeLaVidaElUniversoYTodoLoDemas = 42, // eureka, el sentido de la vida, el universo y todo lo demas es 42... (si es que eso tiene algo de poder XD)
		finanzas = 43, // bancos, bolsa de valores, etc
		ciencia = 44, // investigacion cientifica, universidades, etc
		tecnologia = 45, // desarrollo tecnologico, innovacion, etc
						 //... etc, hay muchos temas de poder que se pueden agregar aqui, como por ejemplo el deporte, la cultura, la religion, etc, pero por ahora con estos creo que es suficiente para probar cosas y ver como funciona el sistema de instituciones gubernamentales y su relacion con el tipo de gobierno y la separacion de poderes.
		SPAMTOM = 99999, // easter egg XD... hello do you wanto buy [SUBJECT OF POWER] for only 1 easy payment of 9.99 kromers?
		Ralsei = 100000, //kris why did you do this to me? I thought we were friends, Please dont turn me into a subject of power.... NOOOOOOOO.
		sans = 100001, // papyrus why did you do this to me? I thought we were brothers, Please dont turn me into a subject of power.... NOOOOOOOO.
		GASTER = 100002, //☟︎☜︎☹︎☹︎⚐︎ ❄︎☟︎✋︎💧︎ ✋︎💧︎ 🕈︎👎︎📬︎ ☝︎✌︎💧︎❄︎☜︎☼︎      (HELLO THIS IS WD. GASTER)
	}//oh no pobre ralsei.
	[Serializable]
	public struct GovermentInstitution
	{
		public string Name;
		public GovermentInstitutionType Type;
		public string Description;
		public GovermentPowerSeparation Powers; // Lista de poderes o responsabilidades de la institución
		public SubjectOfPower[] Subjects; // Lista de temas o áreas de poder que maneja la institución
	}



}