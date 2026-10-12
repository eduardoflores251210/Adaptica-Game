
using AdapticaDebugStuff;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace SerializableTypes.Space
{
	/// <summary>
	/// API asíncrona de acceso a una galaxia.
	/// 
	/// Espeja las operaciones de lectura principales de GalaxyData,
	/// pero utiliza BinaryGalaxySerializer para deserializar los datos
	/// sin bloquear el hilo principal durante la lectura.
	///
	/// NOTA:
	/// - Esta API está orientada a lectura.
	/// - Las operaciones de escritura siguen perteneciendo a GalaxyData.
	/// - Los métodos async utilizan los deserializadores async de
	///   BinaryGalaxySerializer.
	/// </summary>
	public static class AsyncGalaxyAPI
	{
		#region Configuración / Helpers

		private static readonly Dictionary<Vector2Int, Task<GalaxySector>> galaxySectorCache
			= new Dictionary<Vector2Int, Task<GalaxySector>>();

		private static readonly Dictionary<Vector2Int, Task<BaricenterSector>> baricenterSectorCache
			= new Dictionary<Vector2Int, Task<BaricenterSector>>();

		private static readonly Dictionary<Vector2Int, Task<MiscSector>> miscSectorCache
			= new Dictionary<Vector2Int, Task<MiscSector>>();

		private static readonly Dictionary<string, Task<PlanetSector>> planetSectorCache
			= new Dictionary<string, Task<PlanetSector>>();

		#endregion


		#region Galaxy

		/// <summary>
		/// Carga la definición de la galaxia.
		/// Galaxy.Json sigue siendo JSON porque GalaxyData no tiene
		/// actualmente un serializador binario para la propia metadata
		/// de GalaxyData.
		/// </summary>
		public static async Task<GalaxyData> LoadGalaxyAsync(
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string path = Path.Combine(Paths.Galaxy, "Galaxy.Json");

			if (!File.Exists(path))
				return null;

			string json = await Task.Run(
				() => File.ReadAllText(path),
				cancellationToken
			).ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();

			GalaxyData data = JsonUtility.FromJson<GalaxyData>(json);

			if (data == null)
				return null;

			if (data.CreatedWithVersion == null)
				data.CreatedWithVersion = "Alpha 0.0.0- 00n00a";

			bool isNewer =
				MetaUtils.CompareVersions(
					Application.version,
					data.CreatedWithVersion
				) == -1;

			if (isNewer)
				throw new Exception("VERSION INCOMPATIBLE");

			return data;
		}

		/// <summary>
		/// Variante async equivalente a TryToLoadGalaxy.
		/// </summary>
		public static async Task<(bool Success, GalaxyData Data)> TryToLoadGalaxyAsync(
			CancellationToken cancellationToken = default)
		{
			try
			{
				var data = await LoadGalaxyAsync(cancellationToken);
				return (data != null, data);
			}
			catch
			{
				return (false, null);
			}
		}

		#endregion


		#region GalaxySector

		public static async Task<GalaxySector> LoadSectorAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			string binPath = Path.Combine(
				Paths.GalaxySectors,
				$"Sector{sectorPosition}.bin"
			);

			string jsonPath = Path.Combine(
				Paths.GalaxySectors,
				$"Sector{sectorPosition}.json"
			);

			if (File.Exists(binPath))
			{
				cancellationToken.ThrowIfCancellationRequested();

				using var fs = new FileStream(
					binPath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					4096,
					useAsync: true
				);

				return await BinaryGalaxySerializer
					.DeserializeGalaxySectorAsync(fs, cancellationToken)
					.ConfigureAwait(false);
			}

			// Compatibilidad con sectores antiguos JSON.
			if (File.Exists(jsonPath))
			{
				string json = await ReadAllTextAsync(
					jsonPath,
					cancellationToken
				).ConfigureAwait(false);

				return JsonUtility.FromJson<GalaxySector>(json);
			}

			return null;
		}

		public static async Task<GalaxySector> LoadSectorCachedAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			Task<GalaxySector> task;

			lock (galaxySectorCache)
			{
				if (!galaxySectorCache.TryGetValue(
					sectorPosition,
					out task))
				{
					task = LoadSectorAsync(
						sectorPosition,
						cancellationToken
					);

					galaxySectorCache[sectorPosition] = task;
				}
			}

			return await task.ConfigureAwait(false);
		}

		public static void ClearSectorCache()
		{
			lock (galaxySectorCache)
			{
				galaxySectorCache.Clear();
			}

			lock (baricenterSectorCache)
			{
				baricenterSectorCache.Clear();
			}

			lock (miscSectorCache)
			{
				miscSectorCache.Clear();
			}

			lock (planetSectorCache)
			{
				planetSectorCache.Clear();
			}
		}

		#endregion


		#region BaricenterSector

		public static async Task<BaricenterSector> LoadBaricenterSectorAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			string binPath = Path.Combine(
				Paths.Baricenters,
				$"BaricenterSector{sectorPosition}.bin"
			);

			string jsonPath = Path.Combine(
				Paths.Baricenters,
				$"BaricenterSector{sectorPosition}.json"
			);

			if (File.Exists(binPath))
			{
				using var fs = new FileStream(
					binPath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					4096,
					useAsync: true
				);

				return await BinaryGalaxySerializer
					.DeserializeBaricenterSectorAsync(fs, cancellationToken)
					.ConfigureAwait(false);
			}

			if (File.Exists(jsonPath))
			{
				string json = await ReadAllTextAsync(
					jsonPath,
					cancellationToken
				).ConfigureAwait(false);

				return JsonUtility.FromJson<BaricenterSector>(json);
			}

			return null;
		}

		public static async Task<BaricenterSector> LoadBaricenterSectorCachedAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			Task<BaricenterSector> task;

			lock (baricenterSectorCache)
			{
				if (!baricenterSectorCache.TryGetValue(
					sectorPosition,
					out task))
				{
					task = LoadBaricenterSectorAsync(
						sectorPosition,
						cancellationToken
					);

					baricenterSectorCache[sectorPosition] = task;
				}
			}

			return await task.ConfigureAwait(false);
		}

		#endregion


		#region MiscSector

		public static async Task<MiscSector> LoadMiscSectorAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			string binPath = Path.Combine(
				Paths.MiscGalaxy,
				$"MiscSector{sectorPosition}.bin"
			);

			string jsonPath = Path.Combine(
				Paths.MiscGalaxy,
				$"MiscSector{sectorPosition}.json"
			);

			if (File.Exists(binPath))
			{
				using var fs = new FileStream(
					binPath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					4096,
					useAsync: true
				);

				return await BinaryGalaxySerializer
					.DeserializeMiscSectorAsync(fs, cancellationToken)
					.ConfigureAwait(false);
			}

			if (File.Exists(jsonPath))
			{
				string json = await ReadAllTextAsync(
					jsonPath,
					cancellationToken
				).ConfigureAwait(false);

				return JsonUtility.FromJson<MiscSector>(json);
			}

			return null;
		}

		public static async Task<MiscSector> LoadMiscSectorCachedAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			Task<MiscSector> task;

			lock (miscSectorCache)
			{
				if (!miscSectorCache.TryGetValue(
					sectorPosition,
					out task))
				{
					task = LoadMiscSectorAsync(
						sectorPosition,
						cancellationToken
					);

					miscSectorCache[sectorPosition] = task;
				}
			}

			return await task.ConfigureAwait(false);
		}

		#endregion


		#region PlanetSector

		public static async Task<PlanetSector> LoadPlanetSectorAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			string baseName = $"SectorPlanet{sectorPosition}";

			string binPath = Path.Combine(
				Paths.Planets,
				baseName + ".bin"
			);

			string jsonPath = Path.Combine(
				Paths.Planets,
				baseName + ".json"
			);

			if (File.Exists(binPath))
			{
				using var fs = new FileStream(
					binPath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					4096,
					useAsync: true
				);

				return await BinaryGalaxySerializer
					.DeserializePlanetSectorAsync(fs, cancellationToken)
					.ConfigureAwait(false);
			}

			if (File.Exists(jsonPath))
			{
				string json = await ReadAllTextAsync(
					jsonPath,
					cancellationToken
				).ConfigureAwait(false);

				return JsonUtility.FromJson<PlanetSector>(json);
			}

			return null;
		}

		public static async Task<PlanetSector> LoadPlanetSectorCachedAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			string key = sectorPosition.ToString();

			Task<PlanetSector> task;

			lock (planetSectorCache)
			{
				if (!planetSectorCache.TryGetValue(key, out task))
				{
					task = LoadPlanetSectorAsync(
						sectorPosition,
						cancellationToken
					);

					planetSectorCache[key] = task;
				}
			}

			return await task.ConfigureAwait(false);
		}

		#endregion


		#region Busqueda de estrella

		public static async Task<StarData> LookForStarAsync(
			GalaxyData galaxy,
			ulong id,
			CancellationToken cancellationToken = default)
		{
			if (galaxy == null || galaxy.SectorPositions == null)
				return null;

			foreach (var pos in galaxy.SectorPositions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var sector = await LoadSectorCachedAsync(
					pos,
					cancellationToken
				);

				if (sector?.Stars == null)
					continue;

				foreach (var star in sector.Stars)
				{
					if (star == null)
						continue;

					if (star.IsNull())
						continue;

					if (BodyID.FromString(star.id).GetID() == id)
						return star;
				}
			}

			return null;
		}

		public static async Task<(bool Success, StarData Star)> TryToLookForStarAsync(
			GalaxyData galaxy,
			ulong id,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var star = await LookForStarAsync(
					galaxy,
					id,
					cancellationToken
				);

				return (star != null, star);
			}
			catch
			{
				return (false, null);
			}
		}

		#endregion


		#region Busqueda de baricentro

		public static async Task<BaricenterData> LookForBaricenterAsync(
			GalaxyData galaxy,
			ulong id,
			CancellationToken cancellationToken = default)
		{
			if (galaxy == null || galaxy.SectorPositions == null)
				return null;

			foreach (var pos in galaxy.SectorPositions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var sector =
					await LoadBaricenterSectorCachedAsync(
						pos,
						cancellationToken
					);

				if (sector?.Baricenters == null)
					continue;

				foreach (var b in sector.Baricenters)
				{
					if (b == null)
						continue;

					if (BodyID.FromString(b.id).GetID() == id)
						return b;
				}
			}

			return null;
		}

		public static async Task<(bool Success, BaricenterData Baricenter)>
			TryToLookForBaricenterAsync(
				GalaxyData galaxy,
				ulong id,
				CancellationToken cancellationToken = default)
		{
			try
			{
				var b = await LookForBaricenterAsync(
					galaxy,
					id,
					cancellationToken
				);

				return (b != null, b);
			}
			catch
			{
				return (false, null);
			}
		}

		#endregion


		#region Busqeuda de nebulosas

		public static async Task<NebulaData> LookForNebulaAsync(
			GalaxyData galaxy,
			ulong id,
			CancellationToken cancellationToken = default)
		{
			if (galaxy == null || galaxy.SectorPositions == null)
				return null;

			foreach (var pos in galaxy.SectorPositions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var sector =
					await LoadMiscSectorCachedAsync(
						pos,
						cancellationToken
					);

				if (sector?.Nebulae == null)
					continue;

				foreach (var n in sector.Nebulae)
				{
					if (n == null)
						continue;

					if (BodyID.FromString(n.id).GetID() == id)
						return n;
				}
			}

			return null;
		}

		public static async Task<(bool Success, NebulaData Nebula)>
			TryToLookForNebulaAsync(
				GalaxyData galaxy,
				ulong id,
				CancellationToken cancellationToken = default)
		{
			try
			{
				var nebula = await LookForNebulaAsync(
					galaxy,
					id,
					cancellationToken
				);

				return (nebula != null, nebula);
			}
			catch
			{
				return (false, null);
			}
		}

		#endregion


		#region Bisqueda de Novas

		public static async Task<NovaData> LookForNovaAsync(
			GalaxyData galaxy,
			ulong id,
			CancellationToken cancellationToken = default)
		{
			if (galaxy == null || galaxy.SectorPositions == null)
				return null;

			foreach (var pos in galaxy.SectorPositions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var sector =
					await LoadMiscSectorCachedAsync(
						pos,
						cancellationToken
					);

				if (sector?.Supernovae == null)
					continue;

				foreach (var nova in sector.Supernovae)
				{
					if (nova == null)
						continue;

					if (BodyID.FromString(nova.id).GetID() == id)
						return nova;
				}
			}

			return null;
		}

		public static async Task<(bool Success, NovaData Nova)>
			TryToLookForNovaAsync(
				GalaxyData galaxy,
				ulong id,
				CancellationToken cancellationToken = default)
		{
			try
			{
				var nova = await LookForNovaAsync(
					galaxy,
					id,
					cancellationToken
				);

				return (nova != null, nova);
			}
			catch
			{
				return (false, null);
			}
		}

		#endregion


		#region Busqueda: planetas

		public static async Task<PlanetData> LookForPlanetAsync(
			GalaxyData galaxy,
			ulong id,
			CancellationToken cancellationToken = default)
		{
			if (galaxy == null || galaxy.SectorPositions == null)
				return null;

			foreach (var pos in galaxy.SectorPositions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var sector =
					await LoadPlanetSectorCachedAsync(
						pos,
						cancellationToken
					);

				if (sector?.planets == null)
					continue;

				foreach (var planet in sector.planets)
				{
					if (planet == null)
						continue;

					if (BodyID.FromString(planet.id).GetID() == id)
						return planet;
				}
			}

			return null;
		}

		public static async Task<(bool Success, PlanetData Planet)>
			TryToLookForPlanetAsync(
				GalaxyData galaxy,
				ulong id,
				CancellationToken cancellationToken = default)
		{
			try
			{
				var planet = await LookForPlanetAsync(
					galaxy,
					id,
					cancellationToken
				);

				return (planet != null, planet);
			}
			catch
			{
				return (false, null);
			}
		}

		public static async Task<PlanetData> LoadPlanetAsync(
			ulong id,
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			var sector = await LoadPlanetSectorCachedAsync(
				sectorPosition,
				cancellationToken
			);

			if (sector?.planets == null)
				return null;

			foreach (var planet in sector.planets)
			{
				if (planet == null)
					continue;

				if (BodyID.FromString(planet.id).GetID() == id)
					return planet;
			}

			return null;
		}

		public static async Task<(PlanetData Planet, Vector2Int SectorPos)>
			LoadPlanetWithPositionAsync(
				GalaxyData galaxy,
				ulong id,
				CancellationToken cancellationToken = default)
		{
			if (galaxy?.SectorPositions == null)
				return (null, new Vector2Int(9999999, 999999));

			foreach (var pos in galaxy.SectorPositions)
			{
				var planet = await LoadPlanetAsync(
					id,
					pos,
					cancellationToken
				);

				if (planet != null)
					return (planet, pos);
			}

			return (null, new Vector2Int(9999999, 999999));
		}

		#endregion


		#region Busqueda por topo de planeta

		public static async Task<List<PlanetData>> LookForTypePlanetAsync(
			GalaxyData galaxy,
			PlanetTypes type,
			CancellationToken cancellationToken = default)
		{
			var result = new List<PlanetData>();

			if (galaxy == null || galaxy.SectorPositions == null)
				return result;

			foreach (var pos in galaxy.SectorPositions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var sector =
					await LoadPlanetSectorCachedAsync(
						pos,
						cancellationToken
					);

				if (sector?.planets == null)
					continue;

				foreach (var planet in sector.planets)
				{
					if (planet == null)
						continue;

					if (planet.type == type)
						result.Add(planet);
				}
			}

			return result;
		}

		public static async Task<(bool Success, List<PlanetData> Planets)>
			TryToLookForTypePlanetAsync(
				GalaxyData galaxy,
				PlanetTypes type,
				CancellationToken cancellationToken = default)
		{
			var planets = await LookForTypePlanetAsync(
				galaxy,
				type,
				cancellationToken
			);

			return (planets.Count != 0, planets);
		}

		#endregion


		#region Cuerpos celestes hijos

		public static async Task<GalObjCollection> LoadChildrenForObjAsync(
			GalaxyData galaxy,
			CelestialBody body,
			CancellationToken cancellationToken = default)
		{
			if (body == null || body.Children == null)
				return null;

			var stars = new List<StarData>();
			var planets = new List<PlanetData>();
			var nebulas = new List<NebulaData>();
			var novas = new List<NovaData>();
			var baricenters = new List<BaricenterData>();

			/*
			 * Aquí deliberadamente NO hacemos un await por hijo
			 * de forma secuencial.
			 *
			 * Primero construimos las tareas para poder cargar
			 * objetos simultáneamente.
			 */

			var starTasks = new List<Task<StarData>>();
			var planetTasks = new List<Task<PlanetData>>();
			var nebulaTasks = new List<Task<NebulaData>>();
			var novaTasks = new List<Task<NovaData>>();
			var baricenterTasks = new List<Task<BaricenterData>>();

			foreach (var child in body.Children)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (string.IsNullOrEmpty(child))
					continue;

				switch (char.ToUpperInvariant(child[0]))
				{
					case 'E':
						starTasks.Add(
							LookForStarAsync(
								galaxy,
								BodyID.FromString(child).GetID(),
								cancellationToken
							)
						);
						break;

					case 'B':
						baricenterTasks.Add(
							LookForBaricenterAsync(
								galaxy,
								BodyID.FromString(child).GetID(),
								cancellationToken
							)
						);
						break;

					case 'P':
						planetTasks.Add(
							LookForPlanetAsync(
								galaxy,
								BodyID.FromString(child).GetID(),
								cancellationToken
							)
						);
						break;

					case 'N':
						nebulaTasks.Add(
							LookForNebulaAsync(
								galaxy,
								BodyID.FromString(child).GetID(),
								cancellationToken
							)
						);
						break;

					case 'M':
						novaTasks.Add(
							LookForNovaAsync(
								galaxy,
								BodyID.FromString(child).GetID(),
								cancellationToken
							)
						);
						break;
				}
			}

			var starsResult = await Task.WhenAll(starTasks);
			var planetsResult = await Task.WhenAll(planetTasks);
			var nebulasResult = await Task.WhenAll(nebulaTasks);
			var novasResult = await Task.WhenAll(novaTasks);
			var baricentersResult = await Task.WhenAll(baricenterTasks);

			stars.AddRange(starsResult.Where(x => x != null));
			planets.AddRange(planetsResult.Where(x => x != null));
			nebulas.AddRange(nebulasResult.Where(x => x != null));
			novas.AddRange(novasResult.Where(x => x != null));
			baricenters.AddRange(baricentersResult.Where(x => x != null));

			return new GalObjCollection
			{
				Stars = stars,
				planets = planets,
				nebulas = nebulas,
				novas = novas,
				baricenters = baricenters
			};
		}

		#endregion


		#region Objetos Errantes

		public static async Task<List<StarData>> LookForRougeStarsInThisSectorAsync(
			Vector2Int sectorPosition,
			CancellationToken cancellationToken = default)
		{
			var result = new List<StarData>();

			var sector = await LoadSectorCachedAsync(
				sectorPosition,
				cancellationToken
			);

			if (sector?.Stars == null)
				return result;

			foreach (var star in sector.Stars)
			{
				if (star == null)
					continue;

				if (star.IsNull())
					continue;

				if (string.IsNullOrEmpty(star.id))
					continue;

				if (star.id[0] == 's' ||
					star.id[0] == 'S')
				{
					result.Add(star);
				}
			}

			return result;
		}

		public static async Task<List<PlanetData>>
			LookForRougePlanetInThisSectorAsync(
				Vector2Int sectorPosition,
				CancellationToken cancellationToken = default)
		{
			var result = new List<PlanetData>();

			var sector = await LoadPlanetSectorCachedAsync(
				sectorPosition,
				cancellationToken
			);

			if (sector?.planets == null)
				return result;

			foreach (var planet in sector.planets)
			{
				if (planet == null)
					continue;

				if (string.IsNullOrEmpty(planet.ParentID))
					continue;

				if (planet.ParentID[0] == 's' ||
					planet.ParentID[0] == 'S')
				{
					result.Add(planet);
				}
			}

			return result;
		}

		public static async Task<List<NebulaData>>
			LookForRougeNebulaInThisSectorAsync(
				Vector2Int sectorPosition,
				CancellationToken cancellationToken = default)
		{
			var result = new List<NebulaData>();

			var sector = await LoadMiscSectorCachedAsync(
				sectorPosition,
				cancellationToken
			);

			if (sector?.Nebulae == null)
				return result;

			foreach (var nebula in sector.Nebulae)
			{
				if (nebula == null)
					continue;

				if (string.IsNullOrEmpty(nebula.ParentID))
					continue;

				if (nebula.ParentID[0] == 's' ||
					nebula.ParentID[0] == 'S')
				{
					result.Add(nebula);
				}
			}

			return result;
		}

		public static async Task<List<NovaData>>
			LookForRougeNovaInThisSectorAsync(
				Vector2Int sectorPosition,
				CancellationToken cancellationToken = default)
		{
			var result = new List<NovaData>();

			var sector = await LoadMiscSectorCachedAsync(
				sectorPosition,
				cancellationToken
			);

			if (sector?.Supernovae == null)
				return result;

			foreach (var nova in sector.Supernovae)
			{
				if (nova == null)
					continue;

				if (string.IsNullOrEmpty(nova.ParentID))
					continue;

				if (nova.ParentID[0] == 's' ||
					nova.ParentID[0] == 'S')
				{
					result.Add(nova);
				}
			}

			return result;
		}

		public static async Task<List<BaricenterData>>
			LookForRougeBaricenterInThisSectorAsync(
				Vector2Int sectorPosition,
				CancellationToken cancellationToken = default)
		{
			var result = new List<BaricenterData>();

			var sector = await LoadBaricenterSectorCachedAsync(
				sectorPosition,
				cancellationToken
			);

			if (sector?.Baricenters == null)
				return result;

			foreach (var baricenter in sector.Baricenters)
			{
				if (baricenter == null)
					continue;

				if (string.IsNullOrEmpty(baricenter.ParentID))
					continue;

				if (baricenter.ParentID[0] == 's' ||
					baricenter.ParentID[0] == 'S')
				{
					result.Add(baricenter);
				}
			}

			return result;
		}

		public static async Task<GalObjCollection>
			GetRougueStuffInThisSectorAsync(
				GalaxyData galaxy,
				Vector2Int sectorPosition,
				CancellationToken cancellationToken = default)
		{
			if (galaxy == null)
				return null;

			if (!galaxy.SectorPositions.Contains(sectorPosition))
				return null;

			/*
			 * Los cinco tipos se cargan concurrentemente.
			 * El disco entra oficialmente en fase de "¿POR QUÉ?"
			 */

			var starsTask =
				LookForRougeStarsInThisSectorAsync(
					sectorPosition,
					cancellationToken
				);

			var planetsTask =
				LookForRougePlanetInThisSectorAsync(
					sectorPosition,
					cancellationToken
				);

			var nebulasTask =
				LookForRougeNebulaInThisSectorAsync(
					sectorPosition,
					cancellationToken
				);

			var novasTask =
				LookForRougeNovaInThisSectorAsync(
					sectorPosition,
					cancellationToken
				);

			var baricentersTask =
				LookForRougeBaricenterInThisSectorAsync(
					sectorPosition,
					cancellationToken
				);

			await Task.WhenAll(
				starsTask,
				planetsTask,
				nebulasTask,
				novasTask,
				baricentersTask
			);

			return new GalObjCollection
			{
				Stars = await starsTask,
				planets = await planetsTask,
				nebulas = await nebulasTask,
				novas = await novasTask,
				baricenters = await baricentersTask
			};
		}

		#endregion


		#region Precarga paralela

		/// <summary>
		/// Precarga todos los sectores de estrellas en paralelo.
		/// Utiliza DeserializeGalaxySectorsParallelAsync de
		/// BinaryGalaxySerializer.
		/// </summary>
		public static async Task<GalaxySector[]> PreloadGalaxySectorsAsync(
			GalaxyData galaxy,
			CancellationToken cancellationToken = default)
		{
			if (galaxy?.SectorPositions == null ||
				galaxy.SectorPositions.Count == 0)
			{
				return Array.Empty<GalaxySector>();
			}

			var streams = new List<Stream>();

			try
			{
				foreach (var pos in galaxy.SectorPositions)
				{
					cancellationToken.ThrowIfCancellationRequested();

					string binPath = Path.Combine(
						Paths.GalaxySectors,
						$"Sector{pos}.bin"
					);

					if (File.Exists(binPath))
					{
						streams.Add(
							new FileStream(
								binPath,
								FileMode.Open,
								FileAccess.Read,
								FileShare.Read
							)
						);
					}
				}

				if (streams.Count == 0)
					return Array.Empty<GalaxySector>();

				var result =
					await BinaryGalaxySerializer
						.DeserializeGalaxySectorsParallelAsync(
							streams,
							cancellationToken
						)
						.ConfigureAwait(false);

				for (int i = 0; i < result.Length; i++)
				{
					if (result[i] != null)
					{
						lock (galaxySectorCache)
						{
							galaxySectorCache[result[i].Position] =
								Task.FromResult(result[i]);
						}
					}
				}

				return result;
			}
			finally
			{
				foreach (var stream in streams)
					stream.Dispose();
			}
		}

		#endregion


		#region File Helper Interno

		private static async Task<string> ReadAllTextAsync(
			string path,
			CancellationToken cancellationToken)
		{
			using var fs = new FileStream(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				4096,
				useAsync: true
			);

			using var reader = new StreamReader(fs);

			return await reader
				.ReadToEndAsync()
				.ConfigureAwait(false);
		}

		#endregion
	}
}
