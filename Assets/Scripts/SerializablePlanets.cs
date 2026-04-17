using aaa;
using FixedMath; //ignorar eso era una prueba de un FixedPoint de 128 para un proyecto distinto con depuracion fa tal asi que tuve que usar unity por tener mejor depuracion 
using StandartUtilities; //Ni me acuerdo que metodos uso de mi libreria estandar de proyectos de Unity pero bueno...
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography; //remanente de cuando SerializableTypes, SerializablePlanets y SerializableBiology estaban en el mismo archivo y MicrobeData usaba su metodo para generar IDS de entidad
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; //remanente de cuando Los Planetas tenian Mallas 3D pero ahora usan Heightmaps
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;// using inecesario pero bueno... tal vez en el futuro lo use


namespace SerializableTypes.Space
{
	[Serializable]
	public class CelestialBody
	{
		public string Name;
		public string Description;
		public Transform transform; //transformacion en el espacio... si tiene padre se ignora poscicion  
		public List<string> Children;
		public string id; //string es mas conveniente que BodyID para Json
		public string ParentID;
		public StatList ExtraData; //
	}
	[Serializable]
	public class PlanetData : CelestialBody
	{
		public string HMapID;
		public bool IsSaveFile;
		public PlanetTypes type;
		public float radius;
		public List<Color> GasColors; // Si no es Gas  el color de terreno es [0] y agua es [1]
		public int Seed = 0x00; // Semilla para generación procedural del terreno

		public override string ToString()
		{
			return $"p: {Name}, r{radius}, s{Seed},    ty {type.ToString()}";
		}
	}
	[Serializable]
	/// <summary>
	/// El Star Data de EffiGalaxy
	/// </summary>
	public class StarData : CelestialBody
	{

		/// <summary>
		/// Tipo de estrella
		/// </summary>
		public StarTypes type;

		public static readonly Dictionary<StarTypes, float> Temperatures = new Dictionary<StarTypes, float>
		{
		{ StarTypes.X, 0f },          // Agujero negro, no aplica temperatura para la superficie pero si para el disco de acrección
		{ StarTypes.O, 40000f },
		{ StarTypes.B, 20000f },
		{ StarTypes.A, 8750f },
		{ StarTypes.F, 6750f },
		{ StarTypes.G, 5600f },
		{ StarTypes.K, 4450f },
		{ StarTypes.M, 3050f },
		{ StarTypes.L, 1850f },
		{ StarTypes.T, 1000f },
		{ StarTypes.EB, 24000f },
		{ StarTypes.NS, 800000f },
		{ StarTypes.EN, 3f }          // Enana negra, muy fría prácticamente es un planeta
		};


		public override string ToString()
		{
			return $"estrella {Name}: desc:{Description} tipo:{type} n hijos {Children.Count}";
		}
		public bool IsNull()
		{
			if (this is null)
				return true ;
			return (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(id) && string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(ParentID) && Children.Count == 0 && (transform.Pos == Vector3.zero && transform.Rot == Vector3.zero && transform.Scale == Vector3.zero));
		}

	}
	[Serializable]
	public class BaricenterData : CelestialBody
	{
		public override string ToString()
		{
			return $"Baricentro {Name}: desc:{Description} padre {ParentID} n Hijos {Children.Count}";
		}
	}

	/// <summary>
	/// Guarda la informacion de una Nebulosa en EffiGalaxy
	/// NOTA: es redundante decir EffiGalaxy pues no quedan rastros de su
	/// predecesor BloatyGalaxy
	/// </summary>
	[Serializable]
	public class NebulaData : CelestialBody
	{
		/// <summary>
		/// Color de la Nebulosa
		/// EN el futuro se cambiara a una lista de colores para hacer degradados
		/// </summary>
		public Color Color;
		/// <summary>
		/// Forma de la Nebulosa
		/// </summary>
		public GalacticCloudShape Shape;
		public override string ToString()
		{
			return $"Nebuola {Name}: desc:{Description} padre {ParentID} n Hijos {Children.Count} Color {Color}";
		}
	}
	[Serializable]
	public class NovaData : CelestialBody
	{
		/// <summary>
		/// Color de la Supernova o Nova o HiperNova o KiloNova  
		/// al igual que con la Nebulosa en el futuro sera una lista de colores
		/// </summary>
		public Color Color;
		/// <summary>
		/// tipo de Nova ya sea Nova, Supernova, HiperNova o KiloNova
		/// </summary>
		public NovaType Type;
		/// <summary>
		/// clase de la Nova SOLO APLICA PARA SUPERNOVAS e HIPERNOVAS
		/// </summary>
		public NovaClass Class;
		/// <summary>
		/// Forma de la nube de la Nova
		/// comparte el mismo enum que la Nebulosa 
		/// </summary>
		public GalacticCloudShape Shape;
		public override string ToString()
		{
			return $"{Type} {Name}: desc:{Description} padre {ParentID} n Hijos {Children.Count} Color {Color}";
		}
	}

	[Serializable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Convertir el miembro en 'readonly'", Justification = "<pendiente>")]
	/// <summary>
	/// Esto es una clase para mejorar la lectura y escritura de IDs de cuerpos celestes pues a menudo hay un string 
	/// que no sabes si es una estrella, planeta, baricentro, sector, nebulosa o supernova
	/// </summary>
	public class BodyID : IComparable<BodyID>
	{
		/// <summary>
		/// Tipo de cuerpo celeste (E: estrella, P: planeta, B: baricentro, S: sector, N: nebulosa, M: supernova, A: otro)
		/// </summary>
		private char Type;
		/// <summary>
		/// ID numerico del cuerpo celeste
		/// </summary>
		private ulong ID;
		public override string ToString()
		{
			return String.Concat(Type, ID);
		}
		public BodyID(char type, ulong id)
		{
			if (!(ValidChars.Contains(type)))
			{
				throw new ArgumentException("TIPO INVALIDO");
			}
			this.Type = type;
			this.ID = id;
		}
		public BodyID(CelestialBodyType type, ulong id)
		{
			if (type == CelestialBodyType.None)
				throw new ArgumentException("type no puede ser none");
			Type = type switch
			{
				CelestialBodyType.Planet => 'P',
				CelestialBodyType.Star => 'E',
				CelestialBodyType.Baricenter => 'B',
				CelestialBodyType.Sector => 'S',
				CelestialBodyType.Nebula => 'N',
				CelestialBodyType.Nova => 'M',
				_ => 'A',
			};
			this.ID = id;
		}
		public static BodyID FromString(string a)
		{
			if (a == null)
				throw new ArgumentException("NULL");
			if (a.Length < 2)
			{
				throw new ArgumentException("Cantidad de caracteres invalida o null,");
			}
			//153
			char Type = a[0];//154
			ulong ID = ulong.Parse(a.Substring(1));//155
			return new BodyID(Type, ID); //156
		}//157
		public CelestialBodyType GetCelestialBodyType()
		{
			return Type switch
			{
				'E' => CelestialBodyType.Star,
				'P' => CelestialBodyType.Planet,
				'B' => CelestialBodyType.Baricenter,
				'S' => CelestialBodyType.Sector,
				'N' => CelestialBodyType.Nebula,
				'M' => CelestialBodyType.Nova,
				_ => CelestialBodyType.None,
			};
		}
		public ulong GetID()
		{ return this.ID; }
		public char GetLiteralType()
		{
			return Type;
		}
		public BigInteger GetBigID()
		{
			uint TypeAbs = (uint)Math.Abs((int)GetCelestialBodyType() + 1);

			string IDDD = (TypeAbs.ToString() + ID.ToString());
			return BigInteger.Parse(IDDD);
		}
		public int CompareTo(BodyID other)
		{
			return BigInteger.Compare(this.GetBigID(), other.GetBigID());
		}

		public static List<char> ValidChars = new()
		{
			'A', // ADIVINA QUE [Groseria censurada] ES ESTO
			'B', // Baricentro
			'E', // Estrella 
			'P', // Planeta 
			'S', // Sector
			'N', // Nebulosa
			'M'  // Maldita  Nova O SuperNova O HiperNova O KiloNova
		};
	}


	[Serializable]
	public class PlanetSector
	{
		public PlanetData[] planets;
	}
	[Serializable]
	public class BaricenterSector
	{
		public BaricenterData[] Baricenters;
	}
	[Serializable]
	public class MiscSector
	{
		public NebulaData[] Nebulae;
		public NovaData[] Supernovae;
	}
	[Serializable]
	public class GalaxySector
	{
		public Vector2Int Position; // coordenada de sector
		public Vector2 Size;            // tamaño del sector
		public StarData[] Stars;        // tamaño fijo para optimizar memoria

		public GalaxySector(Vector2Int gridPos, Vector2 size, int starCapacity)
		{
			Position = gridPos;
			Size = size;
			Stars = new StarData[starCapacity];

		}
	}


	/// <summary>
	/// Tipo de galaxia
	/// </summary>
	public enum GalaxyTypes
	{
		/// <summary>
		/// Una galaxia espiral comun y corriente como la via lactea
		/// </summary>
		Spiral,
		/// <summary>
		/// La prima de espiral, pero sin brazos definidos
		/// originalmente llame a este tipo Eliptica por que no sabia la diferencia ahora que lo se son 2 distitnos
		/// </summary>
		Lenticular,
		/// <summary>
		/// Eliptica la galaxia esferoidal sin una forma definida
		/// QUE CAUSA BAJONES DE FPS MASIVOS Esspoiler ya no era por mala optimización
		/// </summary>
		Eliptica,
		/// <summary>
		/// Iregular la galaxia en forma de nube
		/// </summary>
		Irregular,

		//antes  de alphja 3.1.0 solo habia 3 Espiral aka PLANO Eliptica aka PLANO 2 e irregular aka Cubo 
	}
	[Serializable]
	/// <summary>
	/// Collecion de objetos galacticos
	/// </summary>
	public class GalObjCollection
	{
		public List<StarData> Stars;
		public List<PlanetData> planets;
		public List<BaricenterData> baricenters;
		public List<NovaData> novas;
		public List<NebulaData> nebulas;
	}
	[Serializable]

	/// <summary>
	/// Collecion de IDs objetos galacticos
	/// </summary>
	public class GalObjCollectionID
	{
		public List<BodyID> Stars;
		public List<BodyID> planets;
		public List<BodyID> baricenters;
		public List<BodyID> novas;
		public List<BodyID> nebulas;
	}


	/// <summary>
	/// Guarda las criaturas que hay en el planeta 
	/// se suponia que decia Polination pero typo
	/// asi que accidentalmente la vida ahora es Contaminación
	/// </summary>
	[Serializable]
	public struct PollutionPlanet
	{
		public Dictionary<PollutionEntry, int> CellStageF1Creatures;
		public Dictionary<PollutionEntry, int> CellStageF2Creatures;
		public Dictionary<PollutionEntry, int> CellStageF3Creatures;
		public Dictionary<PollutionEntry, int> CellStageF4Creatures;
		public Dictionary<PollutionEntry, int> CreatureStageOnwardCreatures;
		public Dictionary<PollutionEntry, int> SpaceStageSimplifiedFauna;
		public Dictionary<PollutionEntry, int> Flora;
		public int AsociatedPlanetID;
		/// <summary>
		/// Inicializa un Polution file vacio
		/// </summary>
		/// <param name="f">ignora este parametro es solo para que C# no haga berinche</param>
		public PollutionPlanet(bool f = false)
		{
			AsociatedPlanetID = 0;
			CellStageF1Creatures = new Dictionary<PollutionEntry, int>();
			CellStageF2Creatures = new Dictionary<PollutionEntry, int>();
			CellStageF3Creatures = new Dictionary<PollutionEntry, int>();
			CellStageF4Creatures = new Dictionary<PollutionEntry, int>();
			CreatureStageOnwardCreatures = new Dictionary<PollutionEntry, int>();
			SpaceStageSimplifiedFauna = new Dictionary<PollutionEntry, int>();
			Flora = new Dictionary<PollutionEntry, int>();
		}
		/// <summary>
		///  Genera un Polution file con todos las cosas especiicadas que quieras
		/// </summary>
		/// <param name="cellStageF1Creatures">celulas de la fase 1 de la etapa celula digo microbios</param>
		/// <param name="cellStageF2Creatures">microbios de la ase 2 de la etapa</param>
		/// <param name="cellStageF3Creatures">microbios de la fase 3</param>
		/// <param name="cellStageF4Creatures">micobios de la fase 4</param>
		/// <param name="creatureStageOnwardCreatures">Criaturas de la etapa ciratua en adelante</param>
		/// <param name="spaceStageSimplifiedFauna">fauna simplificada usada en el estadio espacial</param>
		/// <param name="asociatedPlanetID">OBLIGATORIO, id del planeta</param>
		public PollutionPlanet(Dictionary<PollutionEntry, int> cellStageF1Creatures, Dictionary<PollutionEntry, int> cellStageF2Creatures, Dictionary<PollutionEntry, int> cellStageF3Creatures, Dictionary<PollutionEntry, int> cellStageF4Creatures, Dictionary<PollutionEntry, int> creatureStageOnwardCreatures, Dictionary<PollutionEntry, int> spaceStageSimplifiedFauna, Dictionary<PollutionEntry, int> flora, int asociatedPlanetID)
		{
			CellStageF1Creatures = cellStageF1Creatures ?? new();
			CellStageF2Creatures = cellStageF2Creatures ?? new();
			CellStageF3Creatures = cellStageF3Creatures ?? new();
			CellStageF4Creatures = cellStageF4Creatures ?? new();
			CreatureStageOnwardCreatures = creatureStageOnwardCreatures ?? new();
			SpaceStageSimplifiedFauna = spaceStageSimplifiedFauna ?? new();
			Flora = flora ?? new();
			AsociatedPlanetID = asociatedPlanetID;
		}

		public static bool operator ==(PollutionPlanet a, PollutionPlanet b)
		{
			return
				(StdUtils.Comparisons.DictionariesAreEqual(a.CellStageF1Creatures, b.CellStageF1Creatures)) &&
				(StdUtils.Comparisons.DictionariesAreEqual(a.CellStageF2Creatures, b.CellStageF2Creatures)) &&
				(StdUtils.Comparisons.DictionariesAreEqual(a.CellStageF3Creatures, b.CellStageF3Creatures)) &&
				(StdUtils.Comparisons.DictionariesAreEqual(a.CellStageF4Creatures, b.CellStageF4Creatures)) &&
				(StdUtils.Comparisons.DictionariesAreEqual(a.CreatureStageOnwardCreatures, b.CreatureStageOnwardCreatures)) &&
				(StdUtils.Comparisons.DictionariesAreEqual(a.SpaceStageSimplifiedFauna, b.SpaceStageSimplifiedFauna)) &&
				(a.AsociatedPlanetID == b.AsociatedPlanetID);
		}
		public static bool operator !=(PollutionPlanet a, PollutionPlanet b)
		{
			return !(a == b);
		}


		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (obj is PollutionPlanet PPTX)
			{
				return this == PPTX;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return (base.GetHashCode() * (AsociatedPlanetID.GetHashCode() ^ CellStageF1Creatures.GetHashCode() ^ CellStageF2Creatures.GetHashCode() ^ CellStageF3Creatures.GetHashCode() ^ CellStageF4Creatures.GetHashCode() ^ CreatureStageOnwardCreatures.GetHashCode() ^ SpaceStageSimplifiedFauna.GetHashCode()));
		}

		public override string ToString()
		{
			return $"pppppp {AsociatedPlanetID}";
		}
	}
	[Serializable]
	///
	///  <summary>
	///  entrada de Pollution file
	///  </summary>
	///
	public struct PollutionEntry
	{
		public string CreatureName;
		public EntryTiers Tier;
	}
	/// <summary>
	/// Representa los Tiers de las entradas:
	/// 
	/// Tier0, Celula: nivel 0 Criatura: Normal Tribal/Ciudad/Civilización/Espacio: Animal         Planta: hierba
	/// Tier1, Celula: nivel 1 Criatura: Picaro Tribal/Ciudad/Civilización/Espacio: Animal Picaro  Planta: Helecho enano
	/// Tier2, Celula: nivel 2 Criatura: Epico  Tribal/Ciudad/Civilización/Espacio: Epico          Planta: Helecho
	/// Tier3, Celula: nivel 3 Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Ganado Normal  Planta: Arbusto
	/// Tier4, Celula: nivel 4 Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Mascota Normal Planta: Árbol Enano
	/// Tier5, Celula: N/A     Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Ganado Picaro  Planta: Árbol
	/// Tier6, Celula: N/A     Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Mascota Picaro Planta: Árbol Enorme
	/// </summary>
	public enum EntryTiers
	{
		None = -1,
		Tier0, //Celula: nivel 0 Criatura: Normal Tribal/Ciudad/Civilización/Espacio: Animal         Planta: hierba
		Tier1, //Celula: nivel 1 Criatura: Picaro Tribal/Ciudad/Civilización/Espacio: Animal Picaro  Planta: Helecho enano
		Tier2, //Celula: nivel 2 Criatura: Epico  Tribal/Ciudad/Civilización/Espacio: Epico          Planta: Helecho
		Tier3, //Celula: nivel 3 Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Ganado Normal  Planta: Arbusto
		Tier4, //Celula: nivel 4 Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Mascota Normal Planta: Árbol Enano
		Tier5, //Celula: N/A     Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Ganado Picaro  Planta: Árbol
		Tier6, //Celula: N/A     Criatura: N/A    Tribal/Ciudad/Civilización/Espacio: Mascota Picaro Planta: Árbol Enorme
	}
	#region Enum

	public enum CelestialBodyType
	{
		None = -1,
		Planet = 0,
		Star = 1,
		Baricenter = 2,
		Nova = 3,
		Nebula = 4,
		Sector = 5,
	}

	public enum GalacticCloudShape
	{
		Sphere,
		Iregular,
		Mitosis //2 esferas                       (nebulosa sdel humnculo)
	}
	public enum NovaType
	{
		Nova,
		Supernova,
		Kionova,
		Hipernova
	}
	/// <summary>
	/// Clases de supernovas
	/// </summary>
	public enum NovaClass
	{

		None,
		I,
		Ia,
		Ib,
		Ic,
		II,
		II_P,
		II_L,
		r,

	}
	/// <summary>
	/// Tipos de estrella
	/// </summary>
	public enum StarTypes
	{
		/// <summary>
		///agujero negro
		/// </summary>
		X = -1, //AH claro un agujero negro es un tipo de estrella por que siempre me despierto y el sol es Sagitario A*      (Sarcasmo)
		O,
		B,
		A,
		F,
		G,
		K,
		M,
		L,
		T,
		/// <summary>
		///enana blanca
		/// </summary>
		EB,
		/// <summary>
		/// estrella de neutrones o pulsar
		/// </summary>
		NS, //No se....... Es broma, es estrella de neutrones
		/// <summary>
		/// enana Negra aunque ni tiene sentido que existan junto a las otras pues estas tardarían MUCHO tiempo para existir al ser enanas blancas MUY frias	
		/// </summary>
		EN,
	}
	/// <summary>
	/// version sin easter eggs
	/// </summary>
	public enum PlanetTypesNoEgg
	{
		/// <summary>
		/// Sin tipo esto es posiblemente por una corupcion
		/// </summary>
		None = -1,
		/// <summary>
		/// Terra como la tierra real
		/// </summary>
		Terra,
		Barren,
		ExTerra,
		BasicGas,
		IceGas,
		IceRock,
		MarsLike,
		VenusLike,
		Toxic,
		WaterWorld,
		Jungle,
		Deserted,
		/// <summary>
		/// Planeta similar a una luna o a mercurio,
		/// Tiene muchos cráteres y poca o ninguna atmósfera
		/// </summary>
		MoonLike,


	}
	/// <summary>
	/// Tipos de planeta
	/// </summary>
	public enum PlanetTypes
	{
		/// <summary>
		/// Sin tipo esto es posiblemente por una corupcion
		/// </summary>
		None = -1,
		/// <summary>
		/// Terra como la tierra real
		/// </summary>
		Terra,
		Barren,
		ExTerra,
		BasicGas,
		IceGas,
		IceRock,
		MarsLike,
		VenusLike,
		Toxic,
		WaterWorld,
		Jungle,
		Deserted,
		/// <summary>
		/// Planeta similar a una luna o a mercurio,
		/// Tiene muchos cráteres y poca o ninguna atmósfera
		/// </summary>
		MoonLike,


		SPAMTON = 9999, //asi es NO ESCAPAS DE LOS EASTER EGGS DE SPAMTON
	}
	#endregion


	// ==========================================
	// ========== COSAS MALDITAS ABAJO =========
	// ==========================================
	//aka galaxydata
	[Serializable]
	///<summary>
	///almacena la informacion basica de una galaxia EffiGalaxy
	///y tiene la API para cargar sectores y buscar cuerpos celestes
	///</summary>
	public class GalaxyData
	{
		public string Name;
		public string Description;
		public Color NebulaColor;
		public Color NucleusColor;
		public GalaxyTypes Type;
		public string Format = "EffiGalaxy";
		public List<Vector2Int> SectorPositions; // Solo posiciones
		public Int32 Seed; //Semilla 

		// --- Cachés en memoria (uno por tipo de sector) ---
		private static Dictionary<Vector2Int, GalaxySector> sectorCache = new Dictionary<Vector2Int, GalaxySector>();
		private static Dictionary<Vector2Int, BaricenterSector> baricenterCache = new Dictionary<Vector2Int, BaricenterSector>();
		private static Dictionary<Vector2Int, MiscSector> miscCache = new Dictionary<Vector2Int, MiscSector>();

		// ---------- MÉTODOS DE CARGA (compatibles con la API anterior) ----------

		// carga sector de estrellas (igual que antes, pero soporta .bin y .json)
		public static void LoadSector(Vector2Int sectorPosition, out GalaxySector FA)
		{
			FA = TryLoadGalaxySectorFiles(sectorPosition, out var loaded) ? loaded : null;
		}

		// carga sector de estrellas con cache (igual que antes)
		public static GalaxySector LoadSectorCached(Vector2Int sectorPosition)
		{
			if (sectorCache.TryGetValue(sectorPosition, out var cached))
				return cached;

			if (TryLoadGalaxySectorFiles(sectorPosition, out var loaded))
			{
				sectorCache[sectorPosition] = loaded;
				return loaded;
			}

			return null;
		}

		// ----------------- NUEVO: Baricenter sector -----------------
		public static BaricenterSector LoadBaricenterSectorCached(Vector2Int sectorPosition)
		{
			if (baricenterCache.TryGetValue(sectorPosition, out var cached))
				return cached;

			if (TryLoadBaricenterSectorFiles(sectorPosition, out var loaded))
			{
				baricenterCache[sectorPosition] = loaded;
				return loaded;
			}
			return null;
		}

		// ----------------- NUEVO: Misc sector (Nebula / Nova) -----------------
		public static MiscSector LoadMiscSectorCached(Vector2Int sectorPosition)
		{
			if (miscCache.TryGetValue(sectorPosition, out var cached))
				return cached;

			if (TryLoadMiscSectorFiles(sectorPosition, out var loaded))
			{
				miscCache[sectorPosition] = loaded;
				return loaded;
			}
			return null;
		}

		// Vaciar cachés (si quieres liberar memoria)
		public static void ClearSectorCache()
		{
			sectorCache.Clear();
			baricenterCache.Clear();
			miscCache.Clear();
		}

		// ---------- BÚSQUEDAS (mantengo nombres parecidos y agrego versiones para nuevos tipos) ----------

		// Buscar estrella por ID (sigue buscando en GalaxySector como antes)
		public StarData LookForStar(ulong id)
		{
			foreach (var pos in SectorPositions)
			{
				var sc = LoadSectorCached(pos);
				if (sc != null)
				{
					foreach (var star in sc.Stars)
					{
						if (star == null) continue;
						if (star.IsNull()) continue;
						if (BodyID.FromString(star.id).GetID() == id)
							return star;
					}
				}
			}
			return null;
		}

		public bool TryToLookForStar(ulong id, out StarData star)
		{
			try
			{
				star = LookForStar(id);
			}
			catch
			{
				star = null;
			}
			return star != null;
		}

		// Buscar baricentro por ID (nuevo)
		public BaricenterData LookForBaricenter(ulong id)
		{
			foreach (var pos in SectorPositions)
			{
				var bs = LoadBaricenterSectorCached(pos);
				if (bs != null && bs.Baricenters != null)
				{
					foreach (var b in bs.Baricenters)
					{
						if (b != null && BodyID.FromString(b.id).GetID() == id)
							return b;
					}
				}
			}
			return null;
		}

		public bool TryToLookForBaricenter(ulong id, out BaricenterData b)
		{
			b = LookForBaricenter(id);
			return b != null;
		}

		// Buscar Nebula/Nova en sectores Misc (nuevo)
		public NebulaData LookForNebula(ulong id)
		{
			foreach (var pos in SectorPositions)
			{
				var ms = LoadMiscSectorCached(pos);
				if (ms != null && ms.Nebulae != null)
				{
					foreach (var n in ms.Nebulae)
					{
						if (n != null && BodyID.FromString(n.id).GetID() == id)
							return n;
					}
				}
			}
			return null;
		}
		public bool TryToLookForNebula(ulong id, out NebulaData neb)
		{
			neb = LookForNebula(id);
			return neb != null;
		}

		public NovaData LookForNova(ulong id)
		{
			foreach (var pos in SectorPositions)
			{
				var ms = LoadMiscSectorCached(pos);
				if (ms != null && ms.Supernovae != null)
				{
					foreach (var nv in ms.Supernovae)
					{
						if (nv != null && BodyID.FromString(nv.id).GetID() == id)
							return nv;
					}
				}
			}
			return null;
		}
		public bool TryToLookForNova(ulong id, out NovaData nova)
		{
			nova = LookForNova(id);
			return nova != null;
		}

		// Planetas: mantengo tu estrategia basada en archivos PlanetSector (ahora soporta .bin/.json)
		public PlanetData LookForPlanet(ulong id)
		{
			return LoadPlanet(id);
		}

		public PlanetData LoadPlanet(ulong id, Vector2Int SectorPos)
		{
			var Sid = (SectorPos);
			string baseName = $"SectorPlanet{Sid}";
			string binPath = Path.Combine(Paths.Planets, baseName + ".bin");
			string jsonPath = Path.Combine(Paths.Planets, baseName + ".json");

			if (File.Exists(binPath))
			{
				using var fs = File.OpenRead(binPath);
				var Plst = BinaryGalaxySerializer.DeserializePlanetSector(fs);
				if (Plst?.planets != null)
				{
					foreach (var p in Plst.planets)
						if (p != null && BodyID.FromString(p.id).GetID() == id) return p;
				}
			}
			else if (File.Exists(jsonPath))
			{
				var Plst = JsonUtility.FromJson<PlanetSector>(File.ReadAllText(jsonPath));
				if (Plst?.planets != null)
				{
					foreach (var p in Plst.planets)
						if (p != null && BodyID.FromString(p.id).GetID() == id) return p;
				}
			}

			return null;
		}

		public PlanetData LoadPlanet(ulong id)
		{
			var files = Directory.Exists(Paths.Planets) ? Directory.GetFiles(Paths.Planets) : Array.Empty<string>();

			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				PlanetSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<PlanetSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else // .bin
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializePlanetSector(fs);
					}
					catch { Plst = null; }
				}

				if (Plst?.planets == null) continue;
				foreach (var p in Plst.planets)
				{
					if (p != null && BodyID.FromString(p.id).GetID() == id)
						return p;
				}
			}
			return null;
		}
		public PlanetData LoadPlanet(ulong id, out Vector2Int SectorPos)
		{
			SectorPos = new Vector2Int(9999999,999999);
			var files = Directory.Exists(Paths.Planets) ? Directory.GetFiles(Paths.Planets) : Array.Empty<string>();

			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				PlanetSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<PlanetSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else // .bin
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializePlanetSector(fs);
					}
					catch { Plst = null; }
				}

				if (Plst?.planets == null) continue;
				foreach (var p in Plst.planets)
				{
					SectorPos = (Path.GetFileNameWithoutExtension(file).Replace($"SectorPlanet", "")).ParseVector2Int();
					if (p != null && BodyID.FromString(p.id).GetID() == id)
						return p;
				}
			}
			return null;
		}

		public bool TryToLookForPlanet(ulong id, out PlanetData planet, out Vector2Int FoundPos)
		{
			planet = LoadPlanet(id, out FoundPos);
			return planet != null;
		}
		public bool TryToLookForPlanet(ulong id, out PlanetData planet)
		{
			planet = LookForPlanet(id);
			return planet != null;
		}
		public bool TryToLookForPlanet(ulong id, Vector2Int Pos, out PlanetData planet)
		{
			planet = LoadPlanet(id, Pos);
			return planet != null;
		}

		public List<BaricenterData> LookForRougeBaricenterInThisSector(Vector2Int SectorPos)
		{
			List<BaricenterData> list = new List<BaricenterData>();
			if (!Directory.Exists(Paths.Baricenters)) return list;
			var files = Directory.GetFiles(Paths.Baricenters);
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				BaricenterSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<BaricenterSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializeBaricenterSector(fs);
					}
					catch { Plst = null; }
				}
				if (Plst?.Baricenters == null) continue;
				foreach (var p in Plst.Baricenters)
				{
					if (p != null && p.ParentID == $"S{SectorPositions.IndexOf(SectorPos)}") list.Add(p);
				}
			}
			return list;
		}

		public List<NovaData> LookForRougeNovaInThisSector(Vector2Int SectorPos)
		{
			if (!Directory.Exists(Paths.MiscGalaxy)) return null;
			List<NovaData> list = new List<NovaData>();
			var files = Directory.GetFiles(Paths.MiscGalaxy);
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				MiscSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<MiscSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializeMiscSector(fs);
					}
					catch { Plst = null; }
				}
				if (Plst?.Supernovae == null) continue;
				foreach (var p in Plst.Supernovae)
				{
					if (p != null && p.ParentID == $"S{SectorPositions.IndexOf(SectorPos)}") list.Add(p);
				}
			}
			return list;
		}

		public List<NebulaData> LookForRougeNebulaInThisSector(Vector2Int SectorPos)
		{
			if (!Directory.Exists(Paths.MiscGalaxy)) return null;
			List<NebulaData> list = new List<NebulaData>();
			var files = Directory.GetFiles(Paths.MiscGalaxy);
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				MiscSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<MiscSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializeMiscSector(fs);
					}
					catch { Plst = null; }
				}
				if (Plst?.Nebulae == null) continue;
				foreach (var p in Plst.Nebulae)
				{
					if (p != null && p.ParentID == $"S{SectorPositions.IndexOf(SectorPos)}") list.Add(p);
				}
			}
			return list;
		}

		public List<PlanetData> LookForRougePlanetInThisSector(Vector2Int SectorPos)
		{
			List<PlanetData> list = new List<PlanetData>();
			var sectorIndex = (SectorPos);
			var id = SectorPositions.IndexOf(SectorPos);
			string baseName = $"SectorPlanet{sectorIndex}";
			string binPath = Path.Combine(Paths.Planets, baseName + ".bin");
			string jsonPath = Path.Combine(Paths.Planets, baseName + ".json");

			if (File.Exists(binPath))
			{
				using var fs = File.OpenRead(binPath);
				var Plst = BinaryGalaxySerializer.DeserializePlanetSector(fs);
				if (Plst?.planets != null)
				{
					string parentID = $"S{id}";
					foreach (var p in Plst.planets)
						if (p != null && (p.ParentID[0] == 's' || p.ParentID[0] == 'S')) list.Add(p);
				}
			}
			else if (File.Exists(jsonPath))
			{
				var Plst = JsonUtility.FromJson<PlanetSector>(File.ReadAllText(jsonPath));
				if (Plst?.planets != null)
				{
					string parentID = $"S{sectorIndex}";
					foreach (var p in Plst.planets)
						if (p != null && (p.ParentID[0] == 's' || p.ParentID[0] == 'S')) list.Add(p);
				}
			}

			return list;
		}

		// Buscar por tipo de planeta (igual que antes)
		public List<PlanetData> LookForTypePlanet(PlanetTypes type)
		{
			List<PlanetData> list = new List<PlanetData>();
			var files = Directory.Exists(Paths.Planets) ? Directory.GetFiles(Paths.Planets) : Array.Empty<string>();
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				PlanetSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<PlanetSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializePlanetSector(fs);
					}
					catch { Plst = null; }
				}

				if (Plst?.planets == null) continue;
				foreach (var p in Plst.planets)
					if (p != null && p.type == type) list.Add(p);
			}
			return list;
		}

		public bool TryToLookForTypePlanet(PlanetTypes type, out List<PlanetData> list)
		{
			list = LookForTypePlanet(type);
			return list.Count != 0;
		}

		// Actualizar planeta (igual que antes) — ahora respeta formato del archivo origen (.json/.bin)
		public bool UpdatePlanet(ulong planetId, Action<PlanetData> updateAction)
		{
			var files = Directory.Exists(Paths.Planets) ? Directory.GetFiles(Paths.Planets) : Array.Empty<string>();
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file).ToLower();
				if (ext != ".json" && ext != ".bin") continue;

				PlanetSector Plst = null;
				if (ext == ".json")
				{
					try { Plst = JsonUtility.FromJson<PlanetSector>(File.ReadAllText(file)); } catch { Plst = null; }
				}
				else
				{
					try
					{
						using var fs = File.OpenRead(file);
						Plst = BinaryGalaxySerializer.DeserializePlanetSector(fs);
					}
					catch { Plst = null; }
				}

				if (Plst?.planets == null) continue;

				for (int i = 0; i < Plst.planets.Length; i++)
				{
					var p = Plst.planets[i];
					if (p == null) continue;
					if (BodyID.FromString(p.id).GetID() != planetId) continue;

					updateAction(p); // aplicamos cambios

					// Guardar sector modificado respetando formato original
					try
					{
						if (ext == ".json")
						{
							File.WriteAllText(file, JsonUtility.ToJson(Plst, true));
						}
						else // .bin
						{
							using var fs = File.Open(file, FileMode.Create, FileAccess.Write);
							BinaryGalaxySerializer.SerializePlanetSector(fs, Plst);
						}
					}
					catch (Exception)
					{
						// si guardar falla, puedes loggear; aquí devolvemos false
						return false;
					}
					return true;
				}
			}
			return false;
		}

		// Reemplazar estrella (igual firma que antes, ahora respeta .bin/.json)
		public bool ReplaceStar(ulong StarID, StarData newdata)
		{
			foreach (var pos in SectorPositions)
			{
				var sc = LoadSectorCached(pos);
				if (sc != null)
				{
					for (int sIndex = 0; sIndex < sc.Stars.Length; sIndex++)
					{
						var star = sc.Stars[sIndex];

						if (star == null || BodyID.FromString(star.id).GetID() == StarID)
						{
							sc.Stars[sIndex] = newdata; // Reemplaza la estrella

							// Guardar sector modificado: respetar formato si existe (.bin preferido)
							string binPath = Path.Combine(Paths.GalaxySectors, $"Sector{pos}.bin");
							string jsonPath = Path.Combine(Paths.GalaxySectors, $"Sector{pos}.json");

							try
							{
								if (File.Exists(binPath))
								{
									using var fs = File.Open(binPath, FileMode.Create, FileAccess.Write);
									BinaryGalaxySerializer.SerializeGalaxySector(fs, sc);
								}
								else if (File.Exists(jsonPath))
								{
									File.WriteAllText(jsonPath, JsonUtility.ToJson(sc, true));
								}
								else
								{
									// si no existía, creamos .bin por defecto
									using var fs = File.Open(binPath, FileMode.Create, FileAccess.Write);
									BinaryGalaxySerializer.SerializeGalaxySector(fs, sc);
								}
							}
							catch (Exception)
							{
								return false;
							}

							// Actualizar caché
							sectorCache[pos] = sc;

							return true; // Encontrada y reemplazada
						}
					}
				}
			}
			return false; // No se encontró nada
		}

		// UpdateStar (misma firma)
		public bool UpdateStar(ulong StarID, Action<StarData> updateAction)
		{
			foreach (var pos in SectorPositions)
			{
				var sc = LoadSectorCached(pos);
				if (sc != null)
				{
					for (int sIndex = 0; sIndex < sc.Stars.Length; sIndex++)
					{
						var star = sc.Stars[sIndex];
						if (star == null) continue;
						if (star.IsNull()) continue;
						if (BodyID.FromString(star.id).GetID() == StarID)
						{
							updateAction(star);

							string binPath = Path.Combine(Paths.GalaxySectors, $"Sector{pos}.bin");
							string jsonPath = Path.Combine(Paths.GalaxySectors, $"Sector{pos}.json");

							try
							{
								if (File.Exists(binPath))
								{
									using var fs = File.Open(binPath, FileMode.Create, FileAccess.Write);
									BinaryGalaxySerializer.SerializeGalaxySector(fs, sc);
								}
								else if (File.Exists(jsonPath))
								{
									File.WriteAllText(jsonPath, JsonUtility.ToJson(sc, true));
								}
								else
								{
									// predeterminado: crear bin
									using var fs = File.Open(binPath, FileMode.Create, FileAccess.Write);
									BinaryGalaxySerializer.SerializeGalaxySector(fs, sc);
								}
							}
							catch (Exception)
							{
								return false;
							}

							sectorCache[pos] = sc;
							return true;
						}
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Obtiene los hijos de un cuerpo celeste
		/// </summary>
		/// <param name="body">Cuerpo celeste padre</param>
		/// <returns>Hijos en forma de collecion galactica</returns>
		public GalObjCollection LoadChildrenForObj(CelestialBody body)
		{
			if (body == null) return null;
			if (body.Children == null) return null;
			List<StarData> stars = new List<StarData>();
			List<PlanetData> planets = new List<PlanetData>();
			List<NebulaData> nebulas = new List<NebulaData>();
			List<NovaData> novas = new List<NovaData>();
			List<BaricenterData> baricenters = new List<BaricenterData>();
			foreach (var child in body.Children)
			{
				switch (child[0])
				{
					case 'E':
						stars.Add(LookForStar((BodyID.FromString(child).GetID())));
						break;
					case 'B':
						baricenters.Add(LookForBaricenter((BodyID.FromString(child).GetID())));
						break;
					case 'P':
						planets.Add(LookForPlanet((BodyID.FromString(child).GetID())));
						break;
					case 'N':
						nebulas.Add(LookForNebula((BodyID.FromString(child).GetID())));
						break;
					case 'M':
						novas.Add(LookForNova((BodyID.FromString(child).GetID())));
						break;
				}
			}
			return new GalObjCollection
			{
				Stars = stars,
				planets = planets,
				nebulas = nebulas,
				novas = novas,
				baricenters = baricenters
			};
		}

		public GalObjCollection GetRougueStuffInThisSector(Vector2Int Sector)
		{
			if (!SectorPositions.Contains(Sector))
				return null;
			LoadSector(Sector, out var StarSector);
			List<StarData> Stars = new List<StarData>(ushort.MaxValue);
			foreach (var st in StarSector.Stars)
			{
				if (st == null) continue;
				if (st.IsNull())
					continue;
				if (st.Name == null && st.id == null)
					throw new Exception("Star null");
				if (string.IsNullOrEmpty(st.id))
					throw new Exception($"Star {st.Name} id null");
				if (string.IsNullOrEmpty(st.ParentID))
					throw new Exception($"Star {st.Name} pid null");
				if (st.id[0]== 's' || st.id[0] == 'S')
					Stars.Add(st);
			}
			List<PlanetData> planets = LookForRougePlanetInThisSector(Sector);
			List<NebulaData> nebulas = LookForRougeNebulaInThisSector(Sector);
			List<NovaData> novae = LookForRougeNovaInThisSector(Sector);
			List<BaricenterData> baricenters = LookForRougeBaricenterInThisSector(Sector);
			return new GalObjCollection { Stars = Stars, planets = planets, nebulas = nebulas, baricenters = baricenters, novas = novae };
		}

		// Cargar galaxia (igual que antes)
		public static GalaxyData LoadGalaxy()
		{
			string aa = File.ReadAllText(Path.Combine(Paths.Galaxy, "Galaxy.Json"));
			GalaxyData data = JsonUtility.FromJson<GalaxyData>(aa);
			return data;
		}

		public static bool TryToLoadGalaxy(out GalaxyData data)
		{
			try
			{
				data = LoadGalaxy();
			}
			catch(Exception)
			{
				data = null;	
				return false;
			}
			return data != null;
		}

		// -------------------- Helpers privados para detectar y leer .bin/.json --------------------

		private static bool TryLoadGalaxySectorFiles(Vector2Int sectorPosition, out GalaxySector sector)
		{
			sector = null;
			string binPath = Path.Combine(Paths.GalaxySectors, $"Sector{sectorPosition}.bin");
			string jsonPath = Path.Combine(Paths.GalaxySectors, $"Sector{sectorPosition}.json");

			if (File.Exists(binPath))
			{
				try
				{
					using var fs = File.OpenRead(binPath);
					sector = BinaryGalaxySerializer.DeserializeGalaxySector(fs);
					return true;
				}
				catch { sector = null; return false; }
			}
			else if (File.Exists(jsonPath))
			{
				try
				{
					sector = JsonUtility.FromJson<GalaxySector>(File.ReadAllText(jsonPath));
					return true;
				}
				catch { sector = null; return false; }
			}
			return false;
		}

		private static bool TryLoadBaricenterSectorFiles(Vector2Int sectorPosition, out BaricenterSector sector)
		{
			sector = null;
			string binPath = Path.Combine(Paths.Baricenters, $"BaricenterSector{sectorPosition}.bin");
			string jsonPath = Path.Combine(Paths.Baricenters, $"BaricenterSector{sectorPosition}.json");

			if (File.Exists(binPath))
			{
				try
				{
					using var fs = File.OpenRead(binPath);
					sector = BinaryGalaxySerializer.DeserializeBaricenterSector(fs);
					return true;
				}
				catch { sector = null; return false; }
			}
			else if (File.Exists(jsonPath))
			{
				try
				{
					sector = JsonUtility.FromJson<BaricenterSector>(File.ReadAllText(jsonPath));
					return true;
				}
				catch { sector = null; return false; }
			}
			return false ;
		}

		private static bool TryLoadMiscSectorFiles(Vector2Int sectorPosition, out MiscSector sector)
		{
			sector = null;
			string binPath = Path.Combine(Paths.MiscGalaxy, $"MiscSector{sectorPosition}.bin");
			string jsonPath = Path.Combine(Paths.MiscGalaxy, $"MiscSector{sectorPosition}.json");

			if (File.Exists(binPath))
			{
				try
				{
					using var fs = File.OpenRead(binPath);
					sector = BinaryGalaxySerializer.DeserializeMiscSector(fs);
					return true;
				}
				catch { sector = null; return false; }
			}
			else if (File.Exists(jsonPath))
			{
				try
				{
					sector = JsonUtility.FromJson<MiscSector>(File.ReadAllText(jsonPath));
					return true;
				}
				catch { sector = null; return false; }
			}
			return false;
		}
	}




	/// <summary>
	/// Serializador binario actualizado para CelestialBody.
	/// - Usa un "magic" y versión al inicio.
	/// - Escribe un marker por tipo y después los campos.
	/// - Soporta listas, strings (null-aware), Transform (Pos/Rot/Scale) y Color.
	/// </summary>
	public static class BinaryGalaxySerializer
	{
		const string MAGIC = "EGAL"; // magic header
		const int FORMAT_VERSION = 1;

		#region Public API

		public static void SerializeToStream(Stream stream, CelestialBody body)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Encoding.UTF8.GetBytes(MAGIC));
			writer.Write(FORMAT_VERSION);
			WriteCelestialBody(writer, body);
			writer.Flush();
		}

		public static CelestialBody DeserializeFromStream(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
			var magicBytes = reader.ReadBytes(MAGIC.Length);
			var magicRead = Encoding.UTF8.GetString(magicBytes);
			if (magicRead != MAGIC)
				throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
			int version = reader.ReadInt32();
			if (version != FORMAT_VERSION)
				throw new InvalidDataException($"Versión de formato no soportada: {version}");

			return ReadCelestialBody(reader);
		}

		public static void SerializeList(Stream stream, List<CelestialBody> list)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Encoding.UTF8.GetBytes(MAGIC));
			writer.Write(FORMAT_VERSION);
			writer.Write(list.Count);
			foreach (var b in list) WriteCelestialBody(writer, b);
			writer.Flush();
		}

		public static List<CelestialBody> DeserializeList(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
			var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
			if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
			int version = reader.ReadInt32();
			if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");

			int count = reader.ReadInt32();
			var res = new List<CelestialBody>(count);
			for (int i = 0; i < count; i++) res.Add(ReadCelestialBody(reader));
			return res;
		}

		#endregion

		#region Read / Write dispatch

		private static void WriteCelestialBody(BinaryWriter writer, CelestialBody b)
		{
			if (b == null)
			{
				writer.Write((byte)255); // null marker
				return;
			}

			switch (b)
			{
				case PlanetData p:
					writer.Write((byte)CelestialBodyType.Planet);
					WritePlanet(writer, p);
					break;
				case StarData s:
					writer.Write((byte)CelestialBodyType.Star);
					WriteStar(writer, s);
					break;
				case BaricenterData bc:
					writer.Write((byte)CelestialBodyType.Baricenter);
					WriteBaricenter(writer, bc);
					break;
				case NebulaData n:
					writer.Write((byte)CelestialBodyType.Nebula);
					WriteNebula(writer, n);
					break;
				case NovaData nv:
					writer.Write((byte)CelestialBodyType.Nova);
					WriteNova(writer, nv);
					break;
				default:
					writer.Write((byte)254); // base fallback
					WriteBaseCelestialBody(writer, b);
					break;
			}
		}

		private static CelestialBody ReadCelestialBody(BinaryReader reader)
		{
			byte typeMarker = reader.ReadByte();
			if (typeMarker == 255) return null;

			return typeMarker switch
			{
				(byte)CelestialBodyType.Planet => ReadPlanet(reader),
				(byte)CelestialBodyType.Star => ReadStar(reader),
				(byte)CelestialBodyType.Baricenter => ReadBaricenter(reader),
				(byte)CelestialBodyType.Nebula => ReadNebula(reader),
				(byte)CelestialBodyType.Nova => ReadNova(reader),
				254 => ReadBaseCelestialBody(reader),
				_ => throw new InvalidDataException($"Tipo desconocido al leer CelestialBody: {typeMarker}"),
			};
		}

		#endregion

		#region Specific type read/write (actualizado: Planet usa HMapID en vez de Mesh)

		private static void WriteBaseCelestialBody(BinaryWriter w, CelestialBody b)
		{
			WriteString(w, b.Name);
			WriteString(w, b.Description);
			WriteTransform(w, b.transform);
			WriteStringList(w, b.Children);
			WriteString(w, b.id);
			WriteString(w, b.ParentID);
		}

		private static CelestialBody ReadBaseCelestialBody(BinaryReader r)
		{
			var baseObj = new CelestialBody();
			baseObj.Name = ReadString(r);
			baseObj.Description = ReadString(r);
			baseObj.transform = ReadTransform(r);
			baseObj.Children = ReadStringList(r);
			baseObj.id = ReadString(r);
			baseObj.ParentID = ReadString(r);
			return baseObj;
		}

		private static void WritePlanet(BinaryWriter w, PlanetData p)
		{
			WriteBaseCelestialBody(w, p);

			// Nuevo campo: HMapID (string que referencia heightmap)
			WriteString(w, p.HMapID);

			w.Write(p.IsSaveFile);
			w.Write((int)p.type);
			w.Write(p.radius);

			WriteColorList(w, p.GasColors ?? new List<Color>());

			w.Write(p.Seed);
		}

		private static PlanetData ReadPlanet(BinaryReader r)
		{
			var p = new PlanetData();
			var baseTemp = ReadBaseCelestialBody(r);
			CopyBaseFields(baseTemp, p);

			p.HMapID = ReadString(r);
			p.IsSaveFile = r.ReadBoolean();
			p.type = (PlanetTypes)r.ReadInt32();
			p.radius = r.ReadSingle();
			p.GasColors = ReadColorList(r);
			p.Seed = r.ReadInt32();

			return p;
		}

		private static void WriteStar(BinaryWriter w, StarData s)
		{
			WriteBaseCelestialBody(w, s);
			w.Write((int)s.type);
		}

		private static StarData ReadStar(BinaryReader r)
		{
			var s = new StarData();
			var baseTemp = ReadBaseCelestialBody(r);
			CopyBaseFields(baseTemp, s);
			s.type = (StarTypes)r.ReadInt32();
			return s;
		}

		private static void WriteBaricenter(BinaryWriter w, BaricenterData b)
		{
			WriteBaseCelestialBody(w, b);
		}

		private static BaricenterData ReadBaricenter(BinaryReader r)
		{
			var b = new BaricenterData();
			var baseTemp = ReadBaseCelestialBody(r);
			CopyBaseFields(baseTemp, b);
			return b;
		}

		private static void WriteNebula(BinaryWriter w, NebulaData n)
		{
			WriteBaseCelestialBody(w, n);
			WriteColor(w, n.Color);
			w.Write((int)n.Shape);
		}

		private static NebulaData ReadNebula(BinaryReader r)
		{
			var n = new NebulaData();
			var baseTemp = ReadBaseCelestialBody(r);
			CopyBaseFields(baseTemp, n);
			n.Color = ReadColor(r);
			n.Shape = (GalacticCloudShape)r.ReadInt32();
			return n;
		}

		private static void WriteNova(BinaryWriter w, NovaData nv)
		{
			WriteBaseCelestialBody(w, nv);
			WriteColor(w, nv.Color);
			w.Write((int)nv.Type);
			w.Write((int)nv.Class);
			w.Write((int)nv.Shape);
		}

		private static NovaData ReadNova(BinaryReader r)
		{
			var nv = new NovaData();
			var baseTemp = ReadBaseCelestialBody(r);
			CopyBaseFields(baseTemp, nv);
			nv.Color = ReadColor(r);
			nv.Type = (NovaType)r.ReadInt32();
			nv.Class = (NovaClass)r.ReadInt32();
			nv.Shape = (GalacticCloudShape)r.ReadInt32();
			return nv;
		}

		#endregion

		#region Helpers

		private static void CopyBaseFields(CelestialBody from, CelestialBody to)
		{
			to.Name = from.Name;
			to.Description = from.Description;
			to.transform = from.transform;
			to.Children = from.Children;
			to.id = from.id;
			to.ParentID = from.ParentID;
		}

		private static void WriteString(BinaryWriter w, string s)
		{
			if (s == null) { w.Write(-1); return; }
			var bytes = Encoding.UTF8.GetBytes(s);
			w.Write(bytes.Length);
			w.Write(bytes);
		}

		private static string ReadString(BinaryReader r)
		{
			int len = r.ReadInt32();
			if (len < 0) return null;
			var bytes = r.ReadBytes(len);
			return Encoding.UTF8.GetString(bytes);
		}

		private static void WriteStringList(BinaryWriter w, List<string> list)
		{
			if (list == null) { w.Write(-1); return; }
			w.Write(list.Count);
			foreach (var s in list) WriteString(w, s);
		}

		private static List<string> ReadStringList(BinaryReader r)
		{
			int cnt = r.ReadInt32();
			if (cnt < 0) return null;
			var list = new List<string>(cnt);
			for (int i = 0; i < cnt; i++) list.Add(ReadString(r));
			return list;
		}

		private static void WriteVector3(BinaryWriter w, Vector3 v)
		{
			w.Write(v.x);
			w.Write(v.y);
			w.Write(v.z);
		}

		private static Vector3 ReadVector3(BinaryReader r)
		{
			float x = r.ReadSingle();
			float y = r.ReadSingle();
			float z = r.ReadSingle();
			return new Vector3(x, y, z);
		}

		private static void WriteColor(BinaryWriter w, Color c)
		{
			w.Write(c.r);
			w.Write(c.g);
			w.Write(c.b);
			w.Write(c.a);
		}

		private static Color ReadColor(BinaryReader r)
		{
			float rC = r.ReadSingle();
			float g = r.ReadSingle();
			float b = r.ReadSingle();
			float a = r.ReadSingle();
			return new Color(rC, g, b, a);
		}

		private static void WriteColorList(BinaryWriter w, List<Color> list)
		{
			if (list == null) { w.Write(-1); return; }
			w.Write(list.Count);
			foreach (var c in list) WriteColor(w, c);
		}

		private static List<Color> ReadColorList(BinaryReader r)
		{
			int cnt = r.ReadInt32();
			if (cnt < 0) return null;
			var list = new List<Color>(cnt);
			for (int i = 0; i < cnt; i++) list.Add(ReadColor(r));
			return list;
		}

		private static void WriteTransform(BinaryWriter w, Transform t)
		{
			if (t == null) { w.Write(false); return; }
			w.Write(true);
			WriteVector3(w, t.Pos);
			WriteVector3(w, t.Rot);
			WriteVector3(w, t.Scale);
		}

		private static Transform ReadTransform(BinaryReader r)
		{
			bool has = r.ReadBoolean();
			if (!has) return new();
			var t = new Transform();
			t.Pos = ReadVector3(r);
			t.Rot = ReadVector3(r);
			t.Scale = ReadVector3(r);
			return t;
		}

		#endregion

		// --- AÑADE ESTO A BinaryGalaxySerializer (dentro del mismo tipo) ---

		#region Sector Public API (serializar sectores completos)

		// PlanetSector
		public static void SerializePlanetSector(Stream stream, PlanetSector sector)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Encoding.UTF8.GetBytes(MAGIC));
			writer.Write(FORMAT_VERSION);
			WritePlanetSector(writer, sector);
			writer.Flush();
		}

		public static PlanetSector DeserializePlanetSector(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
			var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
			if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
			int version = reader.ReadInt32();
			if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
			return ReadPlanetSector(reader);
		}

		// BaricenterSector
		public static void SerializeBaricenterSector(Stream stream, BaricenterSector sector)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Encoding.UTF8.GetBytes(MAGIC));
			writer.Write(FORMAT_VERSION);
			WriteBaricenterSector(writer, sector);
			writer.Flush();
		}

		public static BaricenterSector DeserializeBaricenterSector(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
			var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
			if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
			int version = reader.ReadInt32();
			if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
			return ReadBaricenterSector(reader);
		}

		// MiscSector
		public static void SerializeMiscSector(Stream stream, MiscSector sector)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Encoding.UTF8.GetBytes(MAGIC));
			writer.Write(FORMAT_VERSION);
			WriteMiscSector(writer, sector);
			writer.Flush();
		}

		public static MiscSector DeserializeMiscSector(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
			var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
			if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
			int version = reader.ReadInt32();
			if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
			return ReadMiscSector(reader);
		}

		// GalaxySector
		public static void SerializeGalaxySector(Stream stream, GalaxySector sector)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Encoding.UTF8.GetBytes(MAGIC));
			writer.Write(FORMAT_VERSION);
			WriteGalaxySector(writer, sector);
			writer.Flush();
		}

		public static GalaxySector DeserializeGalaxySector(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
			var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
			if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
			int version = reader.ReadInt32();
			if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
			return ReadGalaxySector(reader);
		}

		#endregion

		#region Sector private read/write helpers

		// PlanetSector: array de PlanetData
		private static void WritePlanetSector(BinaryWriter w, PlanetSector sector)
		{
			if (sector == null) { w.Write(false); return; }
			w.Write(true);
			var arr = sector.planets;
			if (arr == null) { w.Write(-1); return; }
			w.Write(arr.Length);
			for (int i = 0; i < arr.Length; i++)
			{
				// Reutilizamos WriteCelestialBody (escribe marker tipo + contenido)
				WriteCelestialBody(w, arr[i]);
			}
		}

		private static PlanetSector ReadPlanetSector(BinaryReader r)
		{
			bool has = r.ReadBoolean();
			if (!has) return null;
			int len = r.ReadInt32();
			if (len < 0) return new PlanetSector { planets = null };
			var arr = new PlanetData[len];
			for (int i = 0; i < len; i++)
			{
				var obj = ReadCelestialBody(r); // puede devolver null o cualquier CelestialBody
				if (obj == null) arr[i] = null;
				else if (obj is PlanetData p) arr[i] = p;
				else throw new InvalidDataException($"Se esperaba PlanetData en PlanetSector[{i}] pero vino {obj.GetType().Name}");
			}
			return new PlanetSector { planets = arr };
		}

		// BaricenterSector: array de BaricenterData
		private static void WriteBaricenterSector(BinaryWriter w, BaricenterSector sector)
		{
			if (sector == null) { w.Write(false); return; }
			w.Write(true);
			var arr = sector.Baricenters;
			if (arr == null) { w.Write(-1); return; }
			w.Write(arr.Length);
			for (int i = 0; i < arr.Length; i++)
				WriteCelestialBody(w, arr[i]);
		}

		private static BaricenterSector ReadBaricenterSector(BinaryReader r)
		{
			bool has = r.ReadBoolean();
			if (!has) return null;
			int len = r.ReadInt32();
			if (len < 0) return new BaricenterSector { Baricenters = null };
			var arr = new BaricenterData[len];
			for (int i = 0; i < len; i++)
			{
				var obj = ReadCelestialBody(r);
				if (obj == null) arr[i] = null;
				else if (obj is BaricenterData b) arr[i] = b;
				else throw new InvalidDataException($"Se esperaba BaricenterData en BaricenterSector[{i}] pero vino {obj.GetType().Name}");
			}
			return new BaricenterSector { Baricenters = arr };
		}

		// MiscSector: Nebulae[] y Supernovae[]
		private static void WriteMiscSector(BinaryWriter w, MiscSector sector)
		{
			if (sector == null) { w.Write(false); return; }
			w.Write(true);

			// Nebulae
			if (sector.Nebulae == null) { w.Write(-1); }
			else
			{
				w.Write(sector.Nebulae.Length);
				for (int i = 0; i < sector.Nebulae.Length; i++) WriteCelestialBody(w, sector.Nebulae[i]);
			}

			// Supernovae
			if (sector.Supernovae == null) { w.Write(-1); }
			else
			{
				w.Write(sector.Supernovae.Length);
				for (int i = 0; i < sector.Supernovae.Length; i++) WriteCelestialBody(w, sector.Supernovae[i]);
			}
		}

		private static MiscSector ReadMiscSector(BinaryReader r)
		{
			bool has = r.ReadBoolean();
			if (!has) return null;

			// Nebulae
			int nebLen = r.ReadInt32();
			NebulaData[] nebulae = null;
			if (nebLen >= 0)
			{
				nebulae = new NebulaData[nebLen];
				for (int i = 0; i < nebLen; i++)
				{
					var obj = ReadCelestialBody(r);
					if (obj == null) nebulae[i] = null;
					else if (obj is NebulaData n) nebulae[i] = n;
					else throw new InvalidDataException($"Se esperaba NebulaData en MiscSector.Nebulae[{i}] pero vino {obj.GetType().Name}");
				}
			}

			// Supernovae
			int snLen = r.ReadInt32();
			NovaData[] sn = null;
			if (snLen >= 0)
			{
				sn = new NovaData[snLen];
				for (int i = 0; i < snLen; i++)
				{
					var obj = ReadCelestialBody(r);
					if (obj == null) sn[i] = null;
					else if (obj is NovaData n) sn[i] = n;
					else throw new InvalidDataException($"Se esperaba NovaData en MiscSector.Supernovae[{i}] pero vino {obj.GetType().Name}");
				}
			}

			return new MiscSector { Nebulae = nebulae, Supernovae = sn };
		}

		// GalaxySector: Position (Vector2Int), Size (Vector2), Stars[] (StarData[])
		private static void WriteGalaxySector(BinaryWriter w, GalaxySector sector)
		{
			if (sector == null) { w.Write(false); return; }
			w.Write(true);

			// Position (Vector2Int)
			w.Write(sector.Position.x);
			w.Write(sector.Position.y);

			// Size (Vector2)
			w.Write(sector.Size.x);
			w.Write(sector.Size.y);

			// Stars array (puede ser fixed-size; escribimos length y cada entrada)
			if (sector.Stars == null) { w.Write(-1); return; }
			w.Write(sector.Stars.Length);
			for (int i = 0; i < sector.Stars.Length; i++)
			{
				WriteCelestialBody(w, sector.Stars[i]); // cada estrella vendrá como StarData (o null)
			}
		}

		private static GalaxySector ReadGalaxySector(BinaryReader r)
		{
			bool has = r.ReadBoolean();
			if (!has) return null;

			var posX = r.ReadInt32();
			var posY = r.ReadInt32();
			var sizeX = r.ReadSingle();
			var sizeY = r.ReadSingle();

			int starLen = r.ReadInt32();
			StarData[] stars = null;
			if (starLen >= 0)
			{
				stars = new StarData[starLen];
				for (int i = 0; i < starLen; i++)
				{
					var obj = ReadCelestialBody(r);
					if (obj == null) stars[i] = null;
					else if (obj is StarData s) stars[i] = s;
					else throw new InvalidDataException($"Se esperaba StarData en GalaxySector.Stars[{i}] pero vino {obj.GetType().Name}");
				}
			}

			var sector = new GalaxySector(new Vector2Int(posX, posY), new Vector2(sizeX, sizeY), stars != null ? stars.Length : 0);
			sector.Stars = stars;
			return sector;
		}

		#endregion

		// --- FIN DE LO QUE AÑADES ---


		#region Async deserialization (solo lectura)

		// Deserializar un CelestialBody desde un Stream (async wrapper)
		public static async Task<CelestialBody> DeserializeFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			return await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				var magicBytes = reader.ReadBytes(MAGIC.Length);
				var magicRead = Encoding.UTF8.GetString(magicBytes);
				if (magicRead != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
				int version = reader.ReadInt32();
				if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión de formato no soportada: {version}");

				return ReadCelestialBody(reader);
			}, cancellationToken).ConfigureAwait(false);
		}

		// Deserializar lista de CelestialBody (async wrapper)
		public static async Task<List<CelestialBody>> DeserializeListAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			return await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
				if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
				int version = reader.ReadInt32();
				if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");

				int count = reader.ReadInt32();
				var res = new List<CelestialBody>(count);
				for (int i = 0; i < count; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();
					res.Add(ReadCelestialBody(reader));
				}
				return res;
			}, cancellationToken).ConfigureAwait(false);
		}

		// PlanetSector async
		public static async Task<PlanetSector> DeserializePlanetSectorAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			return await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
				if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
				int version = reader.ReadInt32();
				if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
				return ReadPlanetSector(reader);
			}, cancellationToken).ConfigureAwait(false);
		}

		// BaricenterSector async
		public static async Task<BaricenterSector> DeserializeBaricenterSectorAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			return await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
				if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
				int version = reader.ReadInt32();
				if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
				return ReadBaricenterSector(reader);
			}, cancellationToken).ConfigureAwait(false);
		}

		// MiscSector async
		public static async Task<MiscSector> DeserializeMiscSectorAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			return await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
				if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
				int version = reader.ReadInt32();
				if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
				return ReadMiscSector(reader);
			}, cancellationToken).ConfigureAwait(false);
		}

		// GalaxySector async
		public static async Task<GalaxySector> DeserializeGalaxySectorAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			return await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				var magic = Encoding.UTF8.GetString(reader.ReadBytes(MAGIC.Length));
				if (magic != MAGIC) throw new InvalidDataException("Archivo no reconocido (magic mismatch).");
				int version = reader.ReadInt32();
				if (version != FORMAT_VERSION) throw new InvalidDataException($"Versión no soportada: {version}");
				return ReadGalaxySector(reader);
			}, cancellationToken).ConfigureAwait(false);
		}

		// Convenience: deserializar múltiples GalaxySectors en paralelo desde una colección de Streams
		public static async Task<GalaxySector[]> DeserializeGalaxySectorsParallelAsync(IEnumerable<Stream> sectorStreams, CancellationToken cancellationToken = default)
		{
			var tasks = sectorStreams.Select(s => DeserializeGalaxySectorAsync(s, cancellationToken));
			return await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		#endregion

	}
}
// Este nombre es temporal luego lo cambiare
// Espero...
namespace aaa
{
	public static class VectorExtensions
	{
		/// <summary>
		/// Convierte un string con formato "(x, y)" a Vector2.
		/// </summary>
		public static Vector2 ParseVector2(this string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				throw new ArgumentException("El string no puede ser null o vacío.");

			// Limpiar paréntesis y espacios
			s = s.Trim('(', ')').Replace(" ", "");

			string[] parts = s.Split(',');
			if (parts.Length != 2)
				throw new FormatException("Formato inválido para Vector2: " + s);

			float x = float.Parse(parts[0]);
			float y = float.Parse(parts[1]);
			return new Vector2(x, y);
		}

		/// <summary>
		/// Convierte un string con formato "(x, y)" a Vector2Int.
		/// </summary>
		public static Vector2Int ParseVector2Int(this string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				throw new ArgumentException("El string no puede ser null o vacío.");

			// Limpiar paréntesis y espacios
			s = s.Trim('(', ')').Replace(" ", "");

			string[] parts = s.Split(',');
			if (parts.Length != 2)
				throw new FormatException("Formato inválido para Vector2Int: " + s);

			int x = int.Parse(parts[0]);
			int y = int.Parse(parts[1]);
			return new Vector2Int(x, y);
		}
	}
}
//HMM esta linea es el año actual al momento de que escribo esto el 6 de diciembre de 2025
//ahora este es el año :) 2026