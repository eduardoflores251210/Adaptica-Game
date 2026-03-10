using SerializableTypes;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class GalaxyGenerator : MonoBehaviour
{
	[Header("Dependencias Opcionales")]
	public StarVisualizer visualizer;
	private Random Random;
	[Header("Parámetros de la galaxia")]
	public float galaxyRadius = 15f; // radio de la galaxia
	public Vector2Double sectorSize = new Vector2Double(1, 1); // tamaño de cada sector
	public int starCountPerSector = 250;

	public int totalSectoresX;
	public int totalSectoresY;

	[Header("Bool util")]
	public bool DoesTheGalaxyExist;
	public bool IsGenerating;

	[Header("Depuración")]
	public string AAAAAAAAA;
	public string BBBBBBBBB;
	public long globalStarId;
	public long globalPlanetId;

	private bool aaa = true;

	[Header("StarGenStuff")]
	public bool AllowRogues = true;
	[Tooltip ("de aucerdo con las observaciones reales 95% es la probabilidad que un objeto sea un planeta")]
	public float Probability = 0.95f;
	public List<PlanetData> planetDataList;
	public event Action OnGalaxyGenerated;
	// --- Parámetros para redondeo elíptico (ajustables) ---
	[Header("Rounding / Shape")]
	[Tooltip("Ratio para el eje Z respecto a X: 1 -> círculo. <1 hace elipse achatada en Z, >1 estirada.")]
	public float ellipseRatio = 1.0f;
	[Tooltip("Cuánto penalizar posiciones fuera de la elipse (mayor = menos fuera).")]
	public float ellipseEdgeFalloff = 100f;
	[Tooltip("Máximo intentos de muestreo por estrella para respetar la elipse antes de proyectar.")]
	public int starPlacementMaxTries = 8;

	void Start()
	{

		Soap soap = new Soap();
		int seed = GetSeed();
		Random = new Random(seed);
		Debug.Log($"SEED {seed}"); 
		DoesTheGalaxyExist = GalaxyExists();
		CalculateTotalSectors();
		//Debug.Log($"GalaxyGenerator init: totalSectoresX={totalSectoresX}, totalSectoresY={totalSectoresY}");
	}
	int GetSeed()
	{
		Span<byte> b = stackalloc byte[4];
		System.Security.Cryptography.RandomNumberGenerator.Fill(b);
		return BitConverter.ToInt32(b);
	}
	void Update()
	{
		if (!DoesTheGalaxyExist)
		{
			StartCoroutine(GenerateGalaxy());
			DoesTheGalaxyExist = true;
		}
		else
		{
			if (aaa)
			{
				GalaxyDiagnostics.FindDuplicateStarIDs(Paths.GalaxySectors);
				aaa = false;
			}
		}
	}

	private void CalculateTotalSectors()
	{
		float diametro = galaxyRadius * 2;
		totalSectoresX = Mathf.Max(1, (int)Math.Round(diametro / sectorSize.x));
		totalSectoresY = Mathf.Max(1, (int)Math.Round(diametro / sectorSize.y));

		// Garantizar un único sector central (hacer impares)
		if (totalSectoresX % 2 == 0) totalSectoresX += 1;
		if (totalSectoresY % 2 == 0) totalSectoresY += 1;

		//Debug.Log($"Total sectores calculados: X={totalSectoresX}, Y={totalSectoresY}");
	}

	private bool GalaxyExists()
	{
		try
		{
			if (!Directory.Exists(Paths.Galaxy) ||
				!Directory.Exists(Paths.GalaxySectors) ||
				!Directory.Exists(Paths.Planets))
			{
				return false;
			}

			string galaxyPath = Path.Combine(Paths.Galaxy, "Galaxy.Json");
			if (!File.Exists(galaxyPath))
				return false;

			var json = File.ReadAllText(galaxyPath);
			var galaxyData = JsonUtility.FromJson<GalaxyData>(json);
			return galaxyData != null && galaxyData.SectorPositions != null && galaxyData.SectorPositions.Count > 0;
		}
		catch
		{
			return false;
		}
	}


	/// <summary>
	/// Genera posiciones de sectores en anillos, en ORDEN HORARIO (clockwise),
	/// asegurando que cada anillo cierre completamente (en el orden angular),
	/// y respetando los límites startX..endX, startY..endY.
	/// </summary>
	private List<Vector2Int> GenerateSectorPositionsInRingsClockwise(int startX, int endX, int startY, int endY)
	{
		var result = new List<Vector2Int>();
		int centerX = (startX + endX) / 2;
		int centerY = (startY + endY) / 2;

		int maxRadius = Math.Max(Math.Max(Math.Abs(startX - centerX), Math.Abs(endX - centerX)),
								 Math.Max(Math.Abs(startY - centerY), Math.Abs(endY - centerY)));

		// r = 0 -> centro (si está dentro de la caja)
		if (centerX >= startX && centerX <= endX && centerY >= startY && centerY <= endY)
			result.Add(new Vector2Int(centerX, centerY));

		// Para cada radio r>0:
		for (int r = 1; r <= maxRadius; r++)
		{
			var ringCells = new List<(Vector2Int pos, float angle)>();

			// Recoger todas las celdas con max(|dx|,|dy|) == r
			for (int dx = -r; dx <= r; dx++)
			{
				for (int dy = -r; dy <= r; dy++)
				{
					if (Mathf.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue; // sólo la periferia del anillo
					int x = centerX + dx;
					int y = centerY + dy;
					if (x < startX || x > endX || y < startY || y > endY) continue;

					// calcular ángulo relativo para orden horario empezando en el Este (0 rad)
					// Atan2 devuelve ángulo en [-PI,PI] donde 0 es en el Este; convertimos a [0,2PI)
					float ang = Mathf.Atan2(dy, dx);
					if (ang < 0f) ang += Mathf.PI * 2f;
					// Convertir a orden horario empezando en East: clockwiseAngle = (2PI - ang) % 2PI
					float cwAng = (Mathf.PI * 2f - ang) % (Mathf.PI * 2f);

					ringCells.Add((new Vector2Int(x, y), cwAng));
				}
			}

			// ordenar por cwAng ascendente (esto produce secuencia clockwise empezando en East)
			ringCells.Sort((a, b) => a.angle.CompareTo(b.angle));

			// añadir al resultado en orden (sin duplicados)
			foreach (var item in ringCells)
			{
				// evitar duplicados accidentales
				if (!result.Contains(item.pos))
					result.Add(item.pos);
			}
		}
		
		return result;
	}


	public IEnumerator GenerateGalaxy()
	{
		IsGenerating = true;
		EnsureDirectoriesAreReal();
		SpaceUtils.Naming.Random = Random;
		GalaxyData galaxy = new GalaxyData
		{
			Name = SpaceUtils.Naming.GenerateGalaxyName(),
			Type = StdUtils.Randomness.GetRandomEnumValue<GalaxyTypes>(),
			NucleusColor = Random.ColorHSV(),
			NebulaColor = Random.ColorHSV(),
			SectorPositions = new List<Vector2Int>()
		};
		galaxy.Description = GenerateGalaxyDescription(galaxy.Type);

		if (visualizer != null)
			visualizer.StopAllCoroutines();

		Debug.Log("Generando galaxia del tipo " + galaxy.Type);

		int startX = -(totalSectoresX / 2);
		int startY = -(totalSectoresY / 2);
		int endX = startX + totalSectoresX - 1;
		int endY = startY + totalSectoresY - 1;

		var sectorPositions = GenerateSectorPositionsInRingsClockwise(startX, endX, startY, endY);

		globalStarId = 0;
		globalPlanetId = 0;
		int sectorIndex = 0;

		foreach (var sectorPos in sectorPositions)
		{
			Vector3 origin = new Vector3(sectorPos.x * (float)sectorSize.x, 0f, sectorPos.y * (float)sectorSize.y);

			GalaxySector sector;
			// central
			if (sectorPos == Vector2Int.zero)
			{
				sector = CreateCentralSector(galaxy.Name, sectorIndex, (Vector2)sectorSize);

				globalStarId += sector.Stars.Count(s => s != null);

				string centralPlanetPath = Path.Combine(Paths.Planets, $"SectorPlanet{sector.Position}.bin");
				if (File.Exists(centralPlanetPath))
				{
					PlanetSector centralPlanetSector;
					using (var fs = File.Open(centralPlanetPath, FileMode.Open, FileAccess.Read))
						centralPlanetSector = BinaryGalaxySerializer.DeserializePlanetSector(fs);

					if (centralPlanetSector?.planets != null)
						globalPlanetId += centralPlanetSector.planets.Length;
				}

				galaxy.SectorPositions.Add(sector.Position);
				using (var fs = File.Open(Path.Combine(Paths.GalaxySectors, $"Sector{sector.Position}.bin"), FileMode.Create, FileAccess.Write))
					BinaryGalaxySerializer.SerializeGalaxySector(fs, sector);

				sectorIndex++;
				yield return new WaitForFixedUpdate();
				continue;
			}

			sector = new GalaxySector(sectorPos, (Vector2)sectorSize, starCountPerSector);

			var stars = GenStarsInSector(
				sectorId: sectorIndex,
				sectorOrigin: origin,
				sectorSize: new Vector3((float)sectorSize.x, galaxyRadius * 2f, (float)sectorSize.y),
				SectorPos: sector.Position,
				starCount: starCountPerSector,
				galaxy: galaxy.Type,
				sectorIndex: sectorIndex
			);

			for (int i = 0; i < stars.Count && i < sector.Stars.Length; i++)
				sector.Stars[i] = stars[i];

			galaxy.SectorPositions.Add(sector.Position);

			using (var fs = File.Open(Path.Combine(Paths.GalaxySectors, $"Sector{sector.Position}.bin"), FileMode.Create, FileAccess.Write))
				BinaryGalaxySerializer.SerializeGalaxySector(fs, sector);

			sectorIndex++;
			yield return new WaitForFixedUpdate();
		}

		// Diagnóstico rápido (opcional)
		try
		{
			float minOriginX = float.MaxValue, maxOriginX = float.MinValue;
			float minStarX = float.MaxValue, maxStarX = float.MinValue;
			foreach (var secPos in galaxy.SectorPositions)
			{
				Vector3 secOrigin = new Vector3(secPos.x * (float)sectorSize.x, 0f, secPos.y * (float)sectorSize.y);
				minOriginX = Mathf.Min(minOriginX, secOrigin.x);
				maxOriginX = Mathf.Max(maxOriginX, secOrigin.x);

				string sectorPath = Path.Combine(Paths.GalaxySectors, $"Sector{secPos}.bin");
				if (File.Exists(sectorPath))
				{
					using (var fs = File.Open(sectorPath, FileMode.Open, FileAccess.Read))
					{
						var sector = BinaryGalaxySerializer.DeserializeGalaxySector(fs);
						if (sector?.Stars != null)
						{
							foreach (var s in sector.Stars)
							{
								if (s == null) continue;
								float x = s.transform.Pos.x;
								minStarX = Mathf.Min(minStarX, x);
								maxStarX = Mathf.Max(maxStarX, x);
							}
						}
					}
				}
			}
			//Debug.Log($"Diagnóstico: originX range [{minOriginX}, {maxOriginX}], starX range [{minStarX}, {maxStarX}]");
		}
		catch (Exception ex) { Debug.LogWarning("Diagnóstico falló: " + ex.Message); }

		// Guardar galaxia final
		string galaxyJson = JsonUtility.ToJson(galaxy, true);
		File.WriteAllText(Path.Combine(Paths.Galaxy, "Galaxy.Json"), galaxyJson);
		IsGenerating = false;
		Debug.Log($"Galaxia generada: {galaxy.Name}");
		if (visualizer != null)
		{
			yield return StartCoroutine(visualizer.LookCoroutine());
		}
		OnGalaxyGenerated?.Invoke();
	}

	private void EnsureDirectoriesAreReal()
	{
		Directory.CreateDirectory(Paths.Galaxy);
		Directory.CreateDirectory(Paths.GalaxySectors);
		Directory.CreateDirectory(Paths.Planets);
		Directory.CreateDirectory(Paths.Baricenters);
		Directory.CreateDirectory(Paths.MiscGalaxy);
		Directory.CreateDirectory(Paths.SaveFiles);
	}

	private GalaxySector CreateCentralSector(string galaxyName, int sectorIndex, Vector2 sectorSize)
	{
		int totalStars = starCountPerSector;
		var sector = new GalaxySector(Vector2Int.zero, sectorSize, totalStars);

		PlanetData controlPlanet = new PlanetData
		{
			id = "P0",
			Name = "CtrB",
			Description = "Planeta de emergencia, siempre Terra.",
			ParentID = "E0",
			type = PlanetTypes.Terra,
			radius = 0.58f,
			GasColors = new List<Color> { Color.green, Color.blue },
			Seed = "CtrB".GetHashCode()
		};

		StarData centralStar = new StarData
		{
			id = "E0",
			Name = $"Center {galaxyName}",
			Description = "Agujero negro masivo central",
			Children = new List<string> { controlPlanet.id },
			transform = new StdUtils.Serializable.Transform(Vector3.zero, Vector3.zero, Vector3.one),
			type = StarTypes.X,
			ParentID = $"S{sectorIndex}"
		};

		sector.Stars[0] = centralStar;

		List<PlanetData> allPlanets = new List<PlanetData> { controlPlanet };
		for (int i = 1; i < totalStars; i++)
		{
			Vector3 starLocalPos = new Vector3(
				Random.Range(-(float)sectorSize.x / 2f, (float)sectorSize.x / 2f),
				0f,
				Random.Range(-(float)sectorSize.y / 2f, (float)sectorSize.y / 2f)
			);

			StarData star = new StarData
			{
				id = "E" + i,
				Name = $"G Star {i}",
				Description = GenerateStarDescription(StarTypes.G),
				Children = new List<string> { "P" + i },
				transform = new StdUtils.Serializable.Transform(starLocalPos, Vector3.zero, Vector3.one),
				type = StarTypes.G,
				ParentID = $"S{sectorIndex}"
			};

			PlanetData terra = new PlanetData
			{
				id = "P" + i,
				Name = $"Terra_{i}",
				Description = controlPlanet.Description,
				ParentID = star.id,
				type = PlanetTypes.Terra,
				radius = controlPlanet.radius,
				GasColors = new List<Color>(controlPlanet.GasColors),
				Seed = $"Terra_{i}".GetHashCode()
			};

			allPlanets.Add(terra);
			sector.Stars[i] = star;
		}

		PlanetSector planetSector = new PlanetSector { planets = allPlanets.ToArray() };
		string planetPath = Path.Combine(Paths.Planets, $"SectorPlanet{sector.Position}.bin");
		using (var fs = File.Open(planetPath, FileMode.Create, FileAccess.Write))
			BinaryGalaxySerializer.SerializePlanetSector(fs, planetSector);

		return sector;
	}

	public List<StarData> GenStarsInSector(
	 int sectorId,
	 Vector3 sectorOrigin,
	 Vector3 sectorSize,
	 Vector2Int SectorPos,
	 int starCount,
	 GalaxyTypes galaxy,
	 int sectorIndex = 0
	)
	{
		var stars = new List<StarData>();
		planetDataList = new List<PlanetData>();
		if (AAAAAAAAA == null) AAAAAAAAA = "";
		AAAAAAAAA += SectorPos + " " + sectorOrigin + '\n'; // logs

		// half extents (X, Z) dentro del sector
		float halfX = sectorSize.x / 2f;
		float halfZ = sectorSize.z / 2f;
		float halfY = sectorSize.y / 2f; // altura

		// parámetros para la elipse global (eje X = galaxyRadius, eje Z = galaxyRadius * ellipseRatio)
		float a = Mathf.Max(0.0001f, galaxyRadius);
		float b = Mathf.Max(0.0001f, galaxyRadius * Mathf.Max(0.0001f, ellipseRatio));
		float invA = 1f / a;
		float invB = 1f / b;

		for (long i = 0; i < starCount; i++)
		{
			bool generateRogue = AllowRogues && Random.Value() < Probability;

			// Muestreo local con "redondeo/elipse" aplicado:
			Vector3 acceptedWorldPos = Vector3.zero;
			bool accepted = false;

			for (int attempt = 0; attempt < starPlacementMaxTries; attempt++)
			{
				Vector3 localSample = new Vector3(
					Random.Range(-halfX, halfX),
					Random.Range(-halfY, halfY),
					Random.Range(-halfZ, halfZ)
				);
				Vector3 worldSample = sectorOrigin + localSample;

				// calcular norma respecto a la elipse: rNorm = sqrt((x/a)^2 + (z/b)^2)
				float rNorm = Mathf.Sqrt(Mathf.Pow(worldSample.x * invA, 2f) + Mathf.Pow(worldSample.z * invB, 2f));

				if (rNorm <= 1f)
				{
					// dentro de la elipse
					acceptedWorldPos = worldSample;
					accepted = true;
					break;
				}
				else
				{
					// fuera: aceptar con probabilidad decreciente
					float p = Mathf.Exp(-ellipseEdgeFalloff * (rNorm - 1f));
					if (Random.Value() < p)
					{
						acceptedWorldPos = worldSample;
						accepted = true;
						break;
					}
				}
			}

			// si no aceptó tras varios intentos, proyectar al borde elíptico (clamp)
			if (!accepted)
			{
				// último muestreo simple (para tener algo estable)
				Vector3 localSample = new Vector3(
					Random.Range(-halfX, halfX),
					Random.Range(-halfY, halfY),
					Random.Range(-halfZ, halfZ)
				);
				Vector3 worldSample = sectorOrigin + localSample;
				float rNorm = Mathf.Sqrt(Mathf.Pow(worldSample.x * invA, 2f) + Mathf.Pow(worldSample.z * invB, 2f));
				if (rNorm <= 0f) rNorm = 1f;
				float s = 1f / rNorm; // factor de escala para proyectar sobre la elipse
				Vector3 projected = new Vector3(worldSample.x * s, worldSample.y, worldSample.z * s);
				acceptedWorldPos = projected;
			}

			if (generateRogue)
			{
				var roguePlanet = GenRoguePlanet(sectorId, globalPlanetId++);
				roguePlanet.ParentID = $"S{sectorIndex}";
				roguePlanet.transform = new StdUtils.Serializable.Transform(acceptedWorldPos, Random.rotation().eulerAngles, Vector3.one);
				planetDataList.Add(roguePlanet);

				StarData fakeStar = new StarData
				{
					id = "E_fake_" + roguePlanet.id,
					Name = roguePlanet.Name,
					type = StarTypes.EN,
					transform = new StdUtils.Serializable.Transform(acceptedWorldPos, Vector3.zero, Vector3.one)
				};

				var (moons, moonCount) = GenPlanets(fakeStar, globalPlanetId);

				foreach (var moon in moons)
				{
					moon.ParentID = roguePlanet.id;
					moon.id = "P" + (globalPlanetId++);
					planetDataList.Add(moon);
				}

				roguePlanet.Children = moons.Select(m => m.id).ToList();
				roguePlanet.Description = "Un planeta solitario con unas cuantas lunas generadas como mini-sistema solar.";
			}
			else
			{
				StarData star = new StarData
				{
					id = "E" + (globalStarId),
					Name = SpaceUtils.Naming.GenerateStarName_NASAStyle(),
					type = StdUtils.Randomness.GetRandomEnumValue<StarTypes>(),
					transform = new StdUtils.Serializable.Transform(acceptedWorldPos, Random.rotation().eulerAngles, Vector3.one),
					ParentID = $"S{sectorIndex}"
				};

				if (galaxy != GalaxyTypes.Irregular)
				{
					float haloChance = 0.05f;
					var pos = star.transform.Pos;
					if (Random.Value() >= haloChance)
						pos.y = Mathf.Clamp(pos.y, 0f, 0.1f);
					star.transform.Pos = pos;
				}

				var (planets, planetCount) = GenPlanets(star, globalPlanetId);
				foreach (var p in planets)
				{
					p.ParentID = star.id;
					p.id = "P" + (globalPlanetId++);
					planetDataList.Add(p);
				}

				star.Children = planets.Select(p => p.id).ToList();
				star.Description = GenerateStarDescription(star.type);
				stars.Add(star);

				globalStarId++;
			}
		}

		PlanetSector sectorPlanets = new PlanetSector { planets = planetDataList.ToArray() };
		string path = Path.Combine(Paths.Planets, $"SectorPlanet{SectorPos}.bin");
		using (var fs = File.Open(path, FileMode.Create, FileAccess.Write))
			BinaryGalaxySerializer.SerializePlanetSector(fs, sectorPlanets);

		planetDataList.Clear();
		return stars;
	}

	public PlanetData GenPlanet(StarData parentStar, long planetId, short planetIndex, short totalPlanets)
	{
		if (parentStar == null) throw new System.ArgumentNullException(nameof(parentStar));

		PlanetTypes type = GetPlanetTypeByOrbit(planetIndex, totalPlanets);

		int attempts = 0;
		while (type == PlanetTypes.None && attempts < 30)
		{
			type = GetPlanetTypeByOrbit(planetIndex, totalPlanets);
			attempts++;
		}
		if (type == PlanetTypes.None) type = PlanetTypes.MoonLike;

		List<Color> gasColors = new List<Color>();
		if (type == PlanetTypes.BasicGas || type == PlanetTypes.IceGas)
		{
			for (int i = 0; i < 5; i++)
				gasColors.Add(Random.ColorHSV());
		}
		else
		{
			gasColors.Add(Random.ColorHSV());
			gasColors.Add(Random.ColorHSV(0.45f, 0.833f, 0, 1));
		}

		var planetName = SpaceUtils.Naming.GeneratePlanetName_NASAStyle(parentStar.Name, planetIndex + 1);
		float radius = Mathf.Clamp(Random.Value(), 0.01f, 0.99f);

		return new PlanetData
		{
			id = "P" + planetId,
			ParentID = parentStar.id,
			Name = planetName,
			type = type,
			radius = radius,
			GasColors = gasColors,
			Seed = radius.GetHashCode() ^ planetId.GetHashCode() ^ parentStar.id.GetHashCode() ^ planetName.GetHashCode(),
			Description = GetPlanetDescription(type)
		};
	}

	public PlanetData GenRoguePlanet(int sectorID, long planetId)
	{
		PlanetTypes[] Validos = { PlanetTypes.IceRock, PlanetTypes.IceGas, PlanetTypes.BasicGas, PlanetTypes.MoonLike, PlanetTypes.Barren };
		PlanetTypes type = Validos[Random.Range(0, Validos.Length)];

		List<Color> gasColors = new List<Color>();
		if (type == PlanetTypes.BasicGas || type == PlanetTypes.IceGas)
		{
			for (int i = 0; i < 5; i++)
				gasColors.Add(Random.ColorHSV());
		}
		else
		{
			gasColors.Add(Random.ColorHSV());
			gasColors.Add(Random.ColorHSV(0.45f, 0.833f, 0, 1));
		}

		var planetName = SpaceUtils.Naming.GenerateStarName_NASAStyle();
		float radius = Mathf.Clamp(Random.Value(), 0.01f, 0.99f);

		return new PlanetData
		{
			id = "P" + planetId,
			ParentID = $"S{sectorID}",
			Name = planetName,
			type = type,
			radius = radius,
			GasColors = gasColors,
			Seed = radius.GetHashCode() ^ planetId.GetHashCode() ^ sectorID.GetHashCode() ^ planetName.GetHashCode(),
			Description = GetPlanetDescription(type)
		};
	}

	public (List<PlanetData>, int) GenPlanets(StarData star, long planetOffset)
	{
		if (star == null) throw new System.ArgumentNullException(nameof(star));
		List<PlanetData> planets = new List<PlanetData>();
		byte maxPlanets = (byte)Mathf.FloorToInt(Random.Range(0f, 11f));
		for (byte i = 0; i < maxPlanets; i++)
		{
			long planetId = planetOffset + i;
			PlanetData planet = GenPlanet(star, planetId, i, maxPlanets);
			planets.Add(planet);
		}
		return (planets, maxPlanets);
	}

	public PlanetTypes GetPlanetTypeByOrbit(int index, int total)
	{
		PlanetTypes[] calientes = { PlanetTypes.VenusLike, PlanetTypes.Toxic, PlanetTypes.Deserted };
		PlanetTypes[] templados = { PlanetTypes.Terra, PlanetTypes.Jungle, PlanetTypes.WaterWorld, PlanetTypes.MarsLike, PlanetTypes.ExTerra };
		PlanetTypes[] frios = { PlanetTypes.IceRock, PlanetTypes.IceGas };
		PlanetTypes[] neutros = { PlanetTypes.BasicGas, PlanetTypes.MoonLike, PlanetTypes.Barren, PlanetTypes.None };

		if (Random.Value() < 0.005f) return PlanetTypes.SPAMTON; // easter egg ultra raro

		if (total == 0) return PlanetTypes.None;
		if (total == 1) return (PlanetTypes)StdUtils.Randomness.GetRandomEnumValue<PlanetTypesNoEgg>(); // por que no quremos a veces un spamton gritandonos "OFERTAS ESPECIALES SOLO POR [CANTIDAD NO ESPECIFICADA]"
		if (total == 2) return index == 0 ? calientes[Random.Range(0, calientes.Length)] : frios[Random.Range(0, frios.Length)];

		float t = (float)index / (total - 1); //matematicas raras para que el ultimo planeta sea 1.0f y el primero 0.0f
		if (Random.Value() < 0.15f) return neutros[Random.Range(0, neutros.Length)];
		if (t < 0.33f) return calientes[Random.Range(0, calientes.Length)];
		else if (t < 0.66f) return templados[Random.Range(0, templados.Length)];
		else return frios[Random.Range(0, frios.Length)];
	}

	/// <summary>
	/// Descripción de la estrella según su tipo.
	/// </summary>
	public static string GenerateStarDescription(StarTypes type)
	{
		return type switch
		{
			StarTypes.X => "Un agujero negro: la estrella que nunca envejece porque todo desaparece dentro de ella. Oscuridad elegante.",
			StarTypes.O => "Estrella O: gigante azul y brillante. Muy joven y energética, con ego cósmico incluido.",
			StarTypes.B => "Estrella B: azul intensa, más elegante que la O, perfecta para selfies estelares… pero cuidado con la radiación.",
			StarTypes.A => "Estrella A: blanca y brillante, todavía relativamente joven. Ideal para sistemas solares con estilo.",
			StarTypes.F => "Estrella F: blanca-amarillenta, tranquila y confiable. Como esa tía que nunca te falla.",
			StarTypes.G => "Estrella G: amarilla, como nuestro Sol. Equilibrada y cálida, perfecta para planetas con vida (o para café galáctico).",
			StarTypes.K => "Estrella K: naranja y más fría que la G. Elegante y longeva, con paciencia cósmica infinita.",
			StarTypes.M => "Estrella M: roja y modesta, pero duradera. Si quieres aventuras, los planetas la rodean por miles de millones de años.",
			StarTypes.L => "Estrella L: marrón y tenue, más como un farol apagado. Difícil de ver, fácil de ignorar.",
			StarTypes.T => "Estrella T: muy fría y débil, casi un cometa atrapado en forma de estrella. Perfecta para amantes de lo misterioso.",
			StarTypes.EB => "Enana blanca: lo que queda cuando una estrella termina su drama. Brilla con nostalgia.",
			StarTypes.NS => "Estrella de neutrones: ultra densa y pequeña, con gravedad que te aplastaría en un segundo. Brutalmente compacta.",
			StarTypes.EN => "Enana negra: una estrella apagada, el fantasma de su antiguo yo. Silencio absoluto en el cosmos.",
			_ => "Tipo desconocido de estrella. Misterio cósmico asegurado o bug."
		};
	}

	public static string GenerateGalaxyDescription(GalaxyTypes type)
	{
		return type switch
		{
			GalaxyTypes.Spiral => "Una galaxia espiral clásica.",
			GalaxyTypes.Eliptical => "Una galaxia elíptica, disco de estrellas y gas.",
			GalaxyTypes.Irregular => "Galaxia irregular, caótica y única.",// por algun motivo son raras
			_ => "Galaxia desconocida, posiblemente corrupta." // [insertar Sonido Dial Up aquí]
		};
	}

	public static string GetPlanetDescription(PlanetTypes type)
	{
		return type switch
		{
			PlanetTypes.None => "Todavía es un lienzo en blanco cósmico… ¿qué formas de vida surgirán aquí?",
			PlanetTypes.Terra => "Un mundo vibrante lleno de ecosistemas. Perfecto para colonizar o iniciar una partida nueva",
			PlanetTypes.Barren => "Rocas y polvo dominan el paisaje. Ideal para construir bases mineras… si sobrevives al viento.",
			PlanetTypes.ExTerra => "Un planeta que albergó civilizaciones. Ruinas y secretos esperan a los exploradores audaces.",
			PlanetTypes.BasicGas => "Un gigante gaseoso sin superficie sólida. Solo podrás flotar entre nubes multicolores o buggearte como kerbal.",
			PlanetTypes.IceGas => "Corrientes de gas helado y tormentas glaciares. Los más valientes sobreviven solo con escudos térmicos.",
			PlanetTypes.IceRock => "Rocoso y helado. Paisajes brillantes de hielo, perfecto para COnseguir hielo para hacerse un buen raspado.",
			PlanetTypes.MarsLike => "Casí siempre Rojo. y polvoriento, con misteriosos cañones y montañas. Ideal para experimentos de terraformación.",
			PlanetTypes.VenusLike => "Un infierno ácido con nubes tóxicas. Solo los exploradores mejor equipados se atreverán a pisarlo.",
			PlanetTypes.Toxic => "Veneno y radiación por doquier. Lugar perfecto para criaturas mutantes o experimentos arriesgados.",
			PlanetTypes.WaterWorld => "Océanos interminables y tormentas marinas. Habitat para peces y mas peces.",
			PlanetTypes.Jungle => "Selvas densas y húmedas, hogar de flora y fauna salvaje. O tu partida olvidada de hace 10 meses",
			PlanetTypes.Deserted => "Un mundo desolado, solo arena y viento. Los exploradores encontrarán soledad… y oportunidades.",
			PlanetTypes.MoonLike => "Pequeño y rocoso, orbitando un gigante. Ideal para establecer colonias científicas o mirar las estrellas.",
			PlanetTypes.SPAMTON => "¡UN PLANETA DE OFERTAS! COMPRA AHORA POR [CANTIDAD NO ESPECIFICADA] O ARREPENTIRÁS POR SIEMPRE. ¡OFERTA LIMITADA!",
			_ => "Un planeta que desafía toda imaginación. Cada visita revela un misterio inesperado. o es un bug",
		};
	}
}



public static class GalaxyDiagnostics
{
	public static void FindDuplicateStarIDs(string sectorsDir)
	{
		if (!Directory.Exists(sectorsDir))
		{
			Debug.Log("No existe carpeta de sectores: " + sectorsDir);
			return;
		}

		var map = new Dictionary<string, List<string>>(); // starId -> list of sector paths
		var files = Directory.GetFiles(sectorsDir, "*.bin");
		foreach (var f in files)
		{
			try
			{
				using var fs = File.OpenRead(f);
				var sector = BinaryGalaxySerializer.DeserializeGalaxySector(fs);
				if (sector?.Stars == null) continue;
				foreach (var s in sector.Stars)
				{
					if (s == null) continue;
					if (string.IsNullOrEmpty(s.id)) continue;
					if (!map.TryGetValue(s.id, out var list)) { list = new List<string>(); map[s.id] = list; }
					list.Add(Path.GetFileName(f));
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Error leyendo {f}: {ex.Message}");
			}
		}

		var duplicates = map.Where(kv => kv.Value.Count > 1).ToList();
		if (duplicates.Count == 0)
		{
			//Debug.Log("No se encontraron IDs duplicadas entre sectores.");
			return;
		}

		Debug.Log($"Encontradas {duplicates.Count} IDs duplicadas:");
		foreach (var kv in duplicates)
		{
			Debug.Log($"ID {kv.Key} aparece en: {string.Join(", ", kv.Value)}");
		}
	}

}

[Serializable]
public struct Vector2Double
{
	public double x;
	public double y;

	public Vector2Double(double x, double y)
	{
		this.x = x;
		this.y = y;
	}
	public static Vector2Double operator+(Vector2Double a, Vector2Double b)
	{
		return new(a.x+b.x, a.y+b.y);
	}
	public static Vector2Double operator-(Vector2Double a, Vector2Double b)
	{
		return new(a.x-b.x, a.y-b.y);
	}
	public static Vector2Double operator-(Vector2Double a)
	{
		return new(-a.x, -a.y);
	}
	public static Vector2Double operator*(Vector2Double a, Vector2Double b)
	{
		return new(a.x*b.x, a.y*b.y);
	}
	public static Vector2Double operator/(Vector2Double a, Vector2Double b)
	{
		return new(a.x/b.x, a.y/b.y);
	}
	public static explicit operator Vector2(Vector2Double aaaa)
	{
		return new((float)aaaa.x, (float)aaaa.y);
	}
	public static implicit operator Vector2Double(Vector2 aaaa)
	{
		return new((double)aaaa.x, (double)aaaa.y);
	}
}

public static class gdfdgfdfg
{
	public static float Value(this Random r)
	{
		return (float)r.NextDouble();
	}
	public static Color ColorHSV(this Random r)
	{
		float H = r.Value();
		float S = r.Value();
		float V = r.Value();
		return Color.HSVToRGB(H, S, V);
	}
	public static Color ColorHSV(this Random r, float hueMin, float hueMax, float saturationMin, float saturationMax)
	{
		// Genera un hue dentro del rango dado
		float H = r.Range(hueMin, hueMax);

		// Genera una saturación dentro del rango dado
		float S = r.Range(saturationMin,saturationMax);

		// Valor (V) aleatorio completo de 0 a 1
		float V = r.Value();

		return Color.HSVToRGB(H, S, V);
	}
	// Para float, similar a Unity
	public static float Range(this Random r, float min, float max)
	{
		return min + (max - min) * (float)r.NextDouble();
	}


	public static int Range(this Random r, int min, int max)
	{
		return r.Next(min, max); // r.Next(min, max) ya es [min, max)
	}
	public static Quaternion rotation(this Random r)
	{
		float x = r.Range(0f, 360f);
		float y = r.Range(0f, 360f);
		float z = r.Range(0f, 360f);
		return Quaternion.Euler(x, y, z);
	}

}