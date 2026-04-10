using ActualUtils;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class GalaxyGenerator : MonoBehaviour
{
	#region Campos 
	[Header("Dependencias Opcionales")]
	public StarVisualizer visualizer;
	private Random Random;
	[Header("Parámetros de la galaxia")]
	public float galaxyRadius = 15f; // radio de la galaxia
	public Vector2Double sectorSize = new Vector2Double(1, 1); // tamaño de cada sector
	public int starCountPerSector = 250;
	byte numArms = 0; //numero de brazos en Espiral
	public int totalSectoresX;
	public int totalSectoresY;

	[Header("Bool util")]
	
	public bool DoesTheGalaxyExist;
	public bool IsGenerating;
	public bool RegenNow;

	[Header("Depuración")]
	
	public long globalStarId;
	public long globalPlanetId;
	public bool IsTestingGeneration;
	public bool ForceType = false;
	public GalaxyTypes ForcedType;
	public bool ForceArms = false;
	public byte ForcedArms;
	private bool aaa = true;

	[Header("StarGenStuff")]
	public bool AllowRogues = true;
	[Tooltip("de aucerdo con las observaciones reales 95% es la probabilidad que un objeto sea un planeta")]
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
	[Header("Spiral Settings")]
	public float spiralBackgroundDensity = 0.015f;
	public float spiralTightness = 0.15f;
	public float SpiralArmWidthMultiplier = 0.08f;
	public float SpiralScaleMultiplier = 0.2f;
	public float spiralAngularStretch = 0.35f;
	[Header("Eliptica Settings")]
	public float irregularNoiseScale = 0.08f;
	public float irregularNoiseOffsetX = 123.45f;
	public float irregularNoiseOffsetY = 678.9f;
	public float irregularNoiseOffsetZ = 1011.12f;
	public float irregularClumpScale = 0.03f;
	public float irregularCoreFalloff = 1.6f;
	public float irregularVoidScale = 0.015f;
	#endregion
	#region Unity Messages
	void Start()
	{

		Soap soap = new Soap();
		int seed = GetSeed();
		Random = new Random(seed);
		Debug.Log($"SEED {seed}");
		if (!IsTestingGeneration)
			DoesTheGalaxyExist = GalaxyExists();
		else
		{
			DoesTheGalaxyExist = false;
			if (Directory.Exists(Paths.Galaxy))
			{
				Directory.Delete(Paths.Galaxy, true );
			}
		}
		CalculateTotalSectors();
		//Debug.Log($"GalaxyGenerator init: totalSectoresX={totalSectoresX}, totalSectoresY={totalSectoresY}");
	}
	void Update()
	{
		if (RegenNow)
		{
			if (IsGenerating)
			{
				RegenNow = false;
			}
			else
			{
				DoesTheGalaxyExist = false;
				if (Directory.Exists(Paths.Galaxy))
				{
					Directory.Delete(Paths.Galaxy, true);
				}
				RegenNow = false;
				int seed = GetSeed();
				Random = new Random(seed);
			}
		}

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
	int GetSeed()
	{
		Span<byte> b = stackalloc byte[4];
		System.Security.Cryptography.RandomNumberGenerator.Fill(b);
		return BitConverter.ToInt32(b);
	}
	#endregion
	#region Main
	[ConsoleCommand(Name = "!reggal", Description = "Regenera la galaxia", IsEgg = true)]
	public static void RegenGalaxy()
	{
		GalaxyGenerator generator = FindAnyObjectByType<GalaxyGenerator>();
		if (generator != null)
		{
			Debug.Log("REGENERANDO GALAXIA");
			generator.RegenNow = true;
		}

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
		if (ForceType)
			galaxy.Type = ForcedType;
		if (galaxy.Type == GalaxyTypes.Spiral)
		{
			numArms = (byte)Random.Range(1, 6);
		}
		if (ForceArms)
		{
			numArms = ForcedArms;
		}
		galaxy.Description = GenerateGalaxyDescription(galaxy.Type);

		if (visualizer != null)
			visualizer.StopAllCoroutines();

		Debug.Log("Generando galaxia del tipo " + galaxy.Type);
		if (galaxy.Type == GalaxyTypes.Spiral)
			Debug.Log($"NUMERO DE BRAZOS {numArms}");
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
				sector = CreateCentralSector(galaxy.Name, sectorIndex, (Vector2)sectorSize, galaxy.Type);

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
				galaxy: galaxy.Type, sectorIndex: sectorIndex
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
	#endregion
	#region Generation Functions
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
	private GalaxySector CreateCentralSector(string galaxyName, int sectorIndex, Vector2 sectorSize, GalaxyTypes type)
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
			float Y = (type == GalaxyTypes.Eliptica) ? Random.Range(-(float)sectorSize.x / 2f, (float)sectorSize.x / 2f) : 0;
			Vector3 starLocalPos = new Vector3(
				Random.Range(-(float)sectorSize.x / 2f, (float)sectorSize.x / 2f),
				Y,
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
	public static float PerlinNoise3D(float x, float y, float z)
	{
		float xy = Mathf.PerlinNoise(x, y);
		float xz = Mathf.PerlinNoise(x, z);
		float yz = Mathf.PerlinNoise(y, z);
		float yx = Mathf.PerlinNoise(y, x);
		float zx = Mathf.PerlinNoise(z, x);
		float zy = Mathf.PerlinNoise(z, y);

		return (xy + xz + yz + yx + zx + zy) / 6f;
	}



	Vector3 GenerateElipticalPosition(
Vector3 sectorOrigin,
float halfX,
float halfY,
float halfZ,
float galaxyRadius, out bool Exeded
)
	{
		Exeded = false;
	




			Vector3 localSample = new Vector3(
				Random.Range(-halfX, halfX),
				Random.Range(-halfY, halfY),
				Random.Range(-halfZ, halfZ)
			);
			

				var worldSample = localSample+ sectorOrigin;
					if (Vector3.Distance(worldSample, Vector3.zero) > galaxyRadius * Mathf.Lerp(0.85f, 0.90f, Random.Value()))
					{
						Exeded = true;
					}
					return worldSample;
			

	}

	public List<StarData> GenStarsInSector(
		int sectorId,
		Vector3 sectorOrigin,
		Vector3 sectorSize,
		Vector2Int SectorPos,
		long starCount,
		GalaxyTypes galaxy, long sectorIndex = 0
	)
	{
		var stars = new List<StarData>();
		planetDataList = new List<PlanetData>();

	/*	if (AAAAAAAAA == null) AAAAAAAAA = "";
		AAAAAAAAA += SectorPos + " " + sectorOrigin + '\n';*/

		float halfX = sectorSize.x / 2f;
		float halfZ = sectorSize.z / 2f;
		float halfY = sectorSize.y / 2f;

		float ellipseRadiusX = Mathf.Max(0.0001f, galaxyRadius);
		float ellipseRadiusZ = Mathf.Max(0.0001f, galaxyRadius * Mathf.Max(0.0001f, ellipseRatio));
		float invEllipseRadiusX = 1f / ellipseRadiusX;
		float invEllipseRadiusZ = 1f / ellipseRadiusZ;

		float spiralScale = Mathf.Max(0.0001f, galaxyRadius * SpiralScaleMultiplier);

		float spiralCoreEpsilon = Mathf.Max(0.5f, galaxyRadius * 0.3f);
		float spiralArmWidth = Mathf.Max(0.5f, galaxyRadius *  SpiralArmWidthMultiplier);


		float anglePerArm = numArms > 0 ? (Mathf.PI * 2f / numArms) : (Mathf.PI * 2f);

		Vector3 GenerateFallbackPosition()
		{
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

				float rNorm = Mathf.Sqrt(
					Mathf.Pow(worldSample.x * invEllipseRadiusX, 2f) +
					Mathf.Pow(worldSample.z * invEllipseRadiusZ, 2f)
				);

				if (rNorm <= 1f)
				{
					acceptedWorldPos = worldSample;
					accepted = true;
					break;
				}
				else
				{
					float p = Mathf.Exp(-ellipseEdgeFalloff * (rNorm - 1f));
					if (Random.Value() < p)
					{
						acceptedWorldPos = worldSample;
						accepted = true;
						break;
					}
				}
			}

			if (!accepted)
			{
				Vector3 localSample = new Vector3(
					Random.Range(-halfX, halfX),
					Random.Range(-halfY, halfY),
					Random.Range(-halfZ, halfZ)
				);

				Vector3 worldSample = sectorOrigin + localSample;
				float rNorm = Mathf.Sqrt(
					Mathf.Pow(worldSample.x * invEllipseRadiusX, 2f) +
					Mathf.Pow(worldSample.z * invEllipseRadiusZ, 2f)
				);

				if (rNorm <= 0f) rNorm = 1f;
				float s = 1f / rNorm;

				acceptedWorldPos = new Vector3(
					worldSample.x * s,
					worldSample.y,
					worldSample.z * s
				);
			}

			return acceptedWorldPos;
		}
		Vector3 GenerateInterArmPosition(
	Vector3 sectorOrigin,
	float halfX,
	float halfY,
	float halfZ,
	float spiralScale,
	float spiralTightness,
	float anglePerArm
)
		{
			for (int attempt = 0; attempt < starPlacementMaxTries; attempt++)
			{
				Vector3 localSample = new Vector3(
					Random.Range(-halfX, halfX),
					Random.Range(-halfY, halfY),
					Random.Range(-halfZ, halfZ)
				);

				Vector3 worldSample = sectorOrigin + localSample;

				float x = worldSample.x;
				float z = worldSample.z;

				float r = Mathf.Sqrt(x * x + z * z);
				if (r <= 0.001f) continue;

				float angle = Mathf.Atan2(z, x);
				if (angle < 0f) angle += Mathf.PI * 2f;

				int armIndex = Mathf.FloorToInt(angle / anglePerArm);
				float armBaseAngle = armIndex * anglePerArm;

				float expectedR = spiralScale * Mathf.Exp(spiralTightness * angle);

				// distancia al brazo
				float distToArm = Mathf.Abs(r - expectedR);

				// queremos que esté LEJOS del brazo
				float minDist = spiralScale * SpiralArmWidthMultiplier * 2f;

				if (distToArm > minDist)
				{
					return worldSample;
				}
			}

			// fallback seguro
			return sectorOrigin + new Vector3(
				Random.Range(-halfX, halfX),
				Random.Range(-halfY, halfY),
				Random.Range(-halfZ, halfZ)
			);
		}

		Vector3 GenerateHaloPosition(
	Vector3 sectorOrigin,
	float halfX,
	float halfY,
	float halfZ,
	float galaxyRadius
)
		{
			for (int attempt = 0; attempt < starPlacementMaxTries; attempt++)
			{
				Vector3 localSample = new Vector3(
					Random.Range(-halfX, halfX),
					Random.Range(-halfY, halfY),
					Random.Range(-halfZ, halfZ)
				);

				Vector3 worldSample = sectorOrigin + localSample;

				float x = worldSample.x;
				float z = worldSample.z;

				float r = Mathf.Sqrt(x * x + z * z);

				// halo = fuera del disco principal
				float minHaloRadius = galaxyRadius * 0.6f;

				if (r >= minHaloRadius)
				{
					// darle más altura (clave para halo)
					worldSample.y += Random.Range(galaxyRadius * 0.1f, galaxyRadius * 0.4f);
					return worldSample;
				}
			}

			// fallback
			Vector3 fallback = sectorOrigin + new Vector3(
				Random.Range(-halfX, halfX),
				Random.Range(-halfY, halfY),
				Random.Range(-halfZ, halfZ)
			);

			fallback.y += galaxyRadius * 0.2f;
			return fallback;
		}


		if (numArms == 0) numArms = 255;
		int spiralHits = 0;
		int spiralFallbacks = 0;
		int CenterspiralFallbacks = 0;

		float distToCenter = new Vector2(sectorOrigin.x, sectorOrigin.z).magnitude;
		bool useSpiralLike = galaxy == GalaxyTypes.Spiral || galaxy == GalaxyTypes.Lenticular;

		float localSpiralBackgroundDensity = galaxy == GalaxyTypes.Lenticular ? 0.01f : spiralBackgroundDensity;
		float localSpiralTightness = galaxy == GalaxyTypes.Lenticular ? 0.35f : spiralTightness;
		float localSpiralArmWidth = galaxy == GalaxyTypes.Lenticular ? Mathf.Max(0.0001f, galaxyRadius * 0.2f) : spiralArmWidth;
		float localSpiralScale = galaxy == GalaxyTypes.Lenticular ? Mathf.Max(0.0001f, galaxyRadius * 0.2f) : spiralScale;
		float localSpiralAngularStretch = galaxy == GalaxyTypes.Lenticular ? 7f : spiralAngularStretch;

		byte localNumArms = galaxy == GalaxyTypes.Lenticular ? (byte)100 : numArms;
		float localAnglePerArm = localNumArms > 0 ? (Mathf.PI * 2f / localNumArms) : (Mathf.PI * 2f);
		for (long i = 0; i < starCount; i++)
		{
			bool generateRogue = AllowRogues && Random.Value() < Probability;
			bool ImAlreadyAHaloStarPleaseGoAway = false;
			Vector3 acceptedWorldPos = Vector3.zero;
			bool Discard = false;
			// 🌀 GALACTIC JETS (compartido por Spiral y Lenticular)
			if (useSpiralLike &&
				distToCenter < galaxyRadius * 0.2f &&
				Random.Value() < 0.005f)
			{
				Vector3 localSample = new Vector3(
					Random.Range(-halfX, halfX),
					Random.Range(-halfY, halfY),
					Random.Range(-halfZ, halfZ)
				);

				Vector3 worldSample = sectorOrigin + localSample;

				float x = worldSample.x;
				float z = worldSample.z;

				float radial = Mathf.Sqrt(x * x + z * z);

				// cono: y = ±√(x² + z²)
				float y = radial;

				if (Random.Value() < 0.5f)
					y = -y;

				worldSample.y = y;

				acceptedWorldPos = worldSample;

				ImAlreadyAHaloStarPleaseGoAway = true;

				// saltar lógica espiral/elíptica
			}
			else
			if (useSpiralLike)
			{
				if (distToCenter <= spiralCoreEpsilon && galaxy == GalaxyTypes.Spiral)
				{
					acceptedWorldPos = GenerateFallbackPosition();
					CenterspiralFallbacks++;
				}
				else
				{
					bool found = false;

					for (int attempt = 0; attempt < starPlacementMaxTries; attempt++)
					{
						Vector3 localSample = new Vector3(
							Random.Range(-halfX, halfX),
							Random.Range(-halfY, halfY),
							Random.Range(-halfZ, halfZ)
						);

						Vector3 worldSample = sectorOrigin + localSample;

						float x = worldSample.x;
						float z = worldSample.z;

						float r = Mathf.Sqrt(x * x + z * z);
						if (r <= 0.001f) continue;

						float angle = Mathf.Atan2(z, x);
						if (angle < 0f) angle += Mathf.PI * 2f;

						int armIndex = Mathf.FloorToInt(angle / localAnglePerArm);
						int prevArm = (armIndex - 1 + (int)localNumArms) % (int)localNumArms;
						int nextArm = (armIndex + 1) % (int)localNumArms;

						int pick = Random.Range(0, 3);
						int brazo = pick == 0 ? armIndex : (pick == 1 ? prevArm : nextArm);

						float offsetBrazo = brazo * localAnglePerArm;

						float localAngle = angle - offsetBrazo;
						if (localAngle < 0f) localAngle += Mathf.PI * 2f;
						localAngle *= localSpiralAngularStretch;

						float expectedR = localSpiralScale * Mathf.Exp(localSpiralTightness * localAngle);

						float distToArm = Mathf.Abs(r - expectedR);
						float density = Mathf.Exp(-(distToArm * distToArm) / (2f * localSpiralArmWidth * localSpiralArmWidth));

						float acceptChance = Mathf.Clamp01(density + localSpiralBackgroundDensity);

						if (Random.Value() < acceptChance)
						{
							acceptedWorldPos = worldSample;
							found = true;
							break;
						}
					}

					if (!found)
					{
						float roll = Random.Value();

						if (roll < 0.95f)
						{
							continue;
						}
						else if (roll < 0.97f)
						{
							generateRogue = true;
							acceptedWorldPos = GenerateFallbackPosition();
						}
						else if (roll < 0.975f)
						{
							acceptedWorldPos = GenerateInterArmPosition(
								sectorOrigin,
								halfX,
								halfY,
								halfZ,
								localSpiralScale,
								localSpiralTightness,
								localAnglePerArm
							);
						}
						else
						{
							acceptedWorldPos = GenerateHaloPosition(
								sectorOrigin,
								halfX,
								halfY,
								halfZ,
								galaxyRadius
							);
							ImAlreadyAHaloStarPleaseGoAway = true;
						}
					}

					if (found) spiralHits++;
					else spiralFallbacks++;
				}
			}
			else if(galaxy == GalaxyTypes.Eliptica)
			{
				acceptedWorldPos = GenerateElipticalPosition(
	sectorOrigin,
	halfX,
	halfY,
	halfZ,
	galaxyRadius,
	out Discard
);


			} else
			{

			}
			if (Discard)
				continue;

			if (generateRogue)
			{
				var roguePlanet = GenRoguePlanet(sectorId, globalPlanetId++);
				roguePlanet.ParentID = $"S{sectorIndex}";
				roguePlanet.transform = new StdUtils.Serializable.Transform(
					acceptedWorldPos,
					Random.rotation().eulerAngles,
					Vector3.one
				);
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
					transform = new StdUtils.Serializable.Transform(
						acceptedWorldPos,
						Random.rotation().eulerAngles,
						Vector3.one
					),
					ParentID = $"S{sectorIndex}"
				};

				if (galaxy != GalaxyTypes.Eliptica)
				{
					if (ImAlreadyAHaloStarPleaseGoAway)
					{ }
					else
					{
						float haloChance = 0.05f;
						var pos = star.transform.Pos;
						if (Random.Value() <= haloChance)
							pos = GenerateHaloPosition(sectorOrigin, halfX, halfY, halfZ, galaxyRadius);
						else
							pos.y = Random.Range(-1,1)*0.5f;
						star.transform.Pos = pos;
					}
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

		//Debug.Log($"Sector {SectorPos} -> spiralHits={spiralHits}, spiralFallbacks={spiralFallbacks}, CenterFallbacks {CenterspiralFallbacks}");

		planetDataList.Clear();
		return stars;
	}

	public PlanetData GenPlanet(StarData parentStar, long planetId, short planetIndex, short totalPlanets)
	{
		PlanetTypes type = PlanetTypes.None;
		if (parentStar == null) throw new System.ArgumentNullException(nameof(parentStar));
		if (parentStar.type != StarTypes.EN)
			type = GetPlanetTypeByOrbit(planetIndex, totalPlanets); 
		else
			type = GetPlanetTypeByOrbitBLACK(planetIndex, totalPlanets);

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
	/// <summary>
	/// genera el tipo de planeta basandose en su orbita
	/// EN UNA ENANA NEGRA O PLANETA ERRANTE
	/// </summary>
	/// <param name="index"></param>
	/// <param name="total"></param>
	/// <returns></returns>
	public PlanetTypes GetPlanetTypeByOrbitBLACK(int index, int total)
	{
		PlanetTypes[] calientes = { PlanetTypes.VenusLike, PlanetTypes.Toxic, PlanetTypes.Deserted };
		PlanetTypes[] templados = { PlanetTypes.WaterWorld, PlanetTypes.MarsLike, PlanetTypes.ExTerra };
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
			GalaxyTypes.Lenticular => "Una galaxia lenticular, disco de estrellas y gas.",
			GalaxyTypes.Eliptica => "Galaxia Eliptica tiene formaa esferoidal",// por algun motivo son esfericas
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
	#endregion
	#region Misc
	private void EnsureDirectoriesAreReal()
	{
		Directory.CreateDirectory(Paths.Galaxy);
		Directory.CreateDirectory(Paths.GalaxySectors);
		Directory.CreateDirectory(Paths.Planets);
		Directory.CreateDirectory(Paths.Baricenters);
		Directory.CreateDirectory(Paths.MiscGalaxy);
		Directory.CreateDirectory(Paths.SaveFiles);
	}
	#endregion
}




/// <summary>
/// inplemebnta metodos de Random de unity a Random de System
/// </summary>
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
		float S = r.Range(saturationMin, saturationMax);

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
				Debug.LogWarning($"Error leyendo {f}: {ex.Message}");//ups se me chispoteo
			}
		}

		var duplicates = map.Where(kv => kv.Value.Count > 1).ToList();
		if (duplicates.Count == 0)
		{
			//Debug.Log("No se encontraron IDs duplicadas entre sectores.");
			return;
		}

		Debug.Log($"Encontradas {duplicates.Count} IDs duplicadas:");//es que no me tienen paciencia 
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

