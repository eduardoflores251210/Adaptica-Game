using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace AdapticaDebugStuff
{
	public static class Commands
	{
		public static class UtiliraryComands
		{
			[ConsoleCommand("fbxexport")]
			public static void FBX_Export()
			{
				Debug.LogError("No disponibe");
			}
			[ConsoleCommand("ver")]
			public static void Version()
			{
				string VVV = $@"Adaptica {Application.version}
compilado para {Application.platform}
creado por {Application.companyName}";
				
				Debug.Log(VVV);
			}
			[ConsoleCommand("pos")]
			public static void GetPosition()
			{
				string msg = "";
				if (PlayerManager.Player != null)
				{
					if (PlayerManager.isInEditor)
					{
						msg += "NO puedes saber poscion en editor";
					}
					else
					{
						msg += PlayerManager.Player.transform.position;
					}
				}
				else
				{
					msg += "NO HAY JUGADOR";
				}
				Debug.Log(msg);
			}
			[ConsoleCommand("listsaves")]
			public static void listALlSaves()
			{
				string SavesFolderPath = Paths.SaveFiles;
				if (!Directory.Exists(SavesFolderPath))
				{
					Debug.LogWarning($"La carpeta {SavesFolderPath} no existe, se creará una nueva.");
					Directory.CreateDirectory(SavesFolderPath);
					return;
				}

				int buttonCount = 0;
				var ACTUALSAVES = Saver.ListSavefiles();

				foreach (var fil in ACTUALSAVES)
				{
					string fil2 = Path.Combine(SavesFolderPath, fil);
					string file = Path.Combine(fil2, "Save.json"); //ahora cada guardado es una carpeta con varios archivos dentro
					/*Debug.Log(file);*/
					try
					{
						string content = File.ReadAllText(file).Trim();
						if (TryRepair(content, file))
						{

							var text = Saver.GetSaveNameForFolder(fil); //por que nadie quiere un SHA512 en su cara jeje XD
							Vector3 a = new(1, 1, 1);

							Debug.Log(text);

							// Añadir al contenedor

							buttonCount++;
						}
					}
					catch (System.Exception e)
					{
						Debug.LogError($"Error al leer el archivo {file}: {e.Message}");
					}


			static bool validateSave(string Json, out SavedGame game)
			{
				game = null;
				if (string.IsNullOrEmpty(Json))
				{
					return false;
				}
				SavedGame savefile = JsonUtility.FromJson<SavedGame>(Json);
				if (savefile != null)
				{
					game = savefile;
					if (savefile.CreatureName == null)
					{
						return false;
					}
					if (savefile.isCPUEmpire)
					{
						return false;
					}
					return true;
				}
				else
					return false;
			}
			static bool IsValidNameForRepair(string FileName)
			{

				if (!string.IsNullOrEmpty(FileName))
				{
					string NoExtFilNam = FileName.Replace(".json", "");
					string NoPrefFilNam = NoExtFilNam.Replace("Game", "");
					if (int.TryParse(NoPrefFilNam, out var id))
					{
						return true;
					}
					else return false;
				}
				else return false;
			}
			static bool TryRepair(string contents, string FilePath)
			{
				try
				{
					if (!validateSave(contents, out var game))
					{
						if (game == null)
						{
							string FileName = Path.GetFileName(FilePath);
							if (IsValidNameForRepair(FileName))
							{

								string NoExtFilNam = FileName.Replace(".json", "");
								string NoPrefFilNam = NoExtFilNam.Replace("Game", "");
								ulong id = ulong.Parse(NoPrefFilNam);
								string[] cells = Directory.GetFiles(Paths.Cells);
								if (cells.Length > 0)
								{
									string rnd = cells[Random.Range(0, cells.Length)];
									string Cell = Path.GetFileNameWithoutExtension(rnd);
									game = new SavedGame(id, false, Stages.Microbe, Cell, new List<HistoryActions>(), Diets.Omnivore, 0d);
									var newContent = JsonUtility.ToJson(game);
									File.WriteAllText(FilePath, newContent);
									return true;
								}
								else return false;

							}
							else
							{
								return false;
							}
						}
						else
						{
							if (game.CreatureName == null)
							{
								string[] cells = Directory.GetFiles(Paths.Cells);
								if (cells.Length > 0)
								{
									string rnd = cells[Random.Range(0, cells.Length)];
									string Cell = Path.GetFileNameWithoutExtension(rnd);
									game.CreatureName = Cell;
									var newContent = JsonUtility.ToJson(game);
									File.WriteAllText(FilePath, newContent);
									return true;
								}
								else return false;
							}
							else if (game.isCPUEmpire)
							{
								return false; // No queremos imperios de CPU en el menu de guardar causaria caos
							}
							else return false; //no se que pasa;

						}
					}
					else return true; //el juego esta bien
				}
				catch (System.Exception) { return false; }
			}
		}
			}
			[ConsoleCommand("loadsave")]
			public static void LoadSave(string name)
			{
				string fold1 = Saver.GetFolderForSaveName(name);
				Debug.Log(fold1);
				string fold2 = Path.GetFileName(fold1);
				Debug.Log(fold2);
				Saver.LoadGameComplete(fold2);
			}
			[ConsoleCommand("skipintro")]
			public static void SkipIntro()
			{
				var sc =SceneManager.GetActiveScene();
				if (sc.name == "MainMenui" )
				{
					var gOL = sc.GetRootGameObjects();
					foreach (var gol in gOL)
					{
						if (gol.name == "NEG" || gol.name == "VID")
						{
							gol.SetActive(false);
						}

					}
				}
			}
		}
		public static class Cheats
		{
			[ConsoleCommand("motherlode", true)]
			public static void Motherlode()
			{
				int amnt = 50_000;
				if (IsInPlay())
				{
					if (Stages.Microbe != PlayerManager.Current_Stage)
					{
						Debug.Log("INVALIDO");
					}
					else
					{
						if (PlayerManager.Player is not CellController a)
						{
							Debug.Log("INVALIDO");

						}
						else
						{
							a.CurrentEvoPoints += amnt;
							a.MaxEvoPointsGotStat += amnt;
							a.StageProgress += amnt;
						}
					}
				}
				else Debug.Log("INVALIDO");
			}
			[ConsoleCommand("money", true)]
			public static void Money(int amnt)
			{
				if (IsInPlay())
				{
					if (Stages.Microbe != PlayerManager.Current_Stage)
					{
						Debug.Log("INVALIDO");
					}
					else
					{
						if (PlayerManager.Player is not CellController a)
						{
							Debug.Log("INVALIDO");

						}
						else
						{
							a.CurrentEvoPoints += amnt;
							a.MaxEvoPointsGotStat += amnt;
							a.StageProgress += amnt;
						}
					}
				}
				else Debug.Log("INVALIDO");
			}
			[ConsoleCommand("entereditor", true)]
			public static void ED (int editor)
			{
				Debug.Log("Entando a editor " + (Editors )editor);
				EditorLoader.EnterEditor((Editors)editor);
			}


		}
		public static class EasterEggCommands
		{
			[ConsoleCommand("dir", IsEgg = true)]
			public static void DIR()
			{
				Debug.Log("lo siento pero no puedes hacer dir\n usa una terinal real");
				Debug.Log("pero esta bien en vez te mostrare los objetos root");
				var f = SceneManager.GetActiveScene().GetRootGameObjects();
				if (f != null)
				{
					foreach (var obj in f)
					{
						Debug.Log($"obj {obj.name}");
					}
				}
			}
			[ConsoleCommand("rm", IsEgg = true)]
			public static void rm()
			{
				Debug.Log("lo siento pero no puedes hacer rm\n usa una terminal real\n ni te dejare eliminargame objects ");
			}
			[ConsoleCommand("ren", IsEgg = true)]
			public static void ren()
			{
				Debug.Log("lo siento pero no puedes hacer ren\n usa una terminal real");
			}
			[ConsoleCommand("rmdir", IsEgg = true)]
			public static void rmAll()
			{
				{
					Debug.Log("lo siento pero no puedes hacer rmdir\n usa una terminal real");
				}
			}
			[ConsoleCommand("del", IsEgg = true)]
			public static void del()
			{
				{
					Debug.Log("lo siento pero no puedes hacer del\n usa una terminal real");
				}
			}
			[ConsoleCommand("pwd", IsEgg = true)]
			public static void PWD()
			{
				Debug.Log(SceneManager.GetActiveScene().name);
			}
			[ConsoleCommand("/gamemode", IsEgg = true)]
			public static void Gamemode(string mode)
			{

				string moda = mode;
				if (int.TryParse(mode, out var modo))
				{
					moda = ((minecraftGamemodes)modo).ToString();
					Debug.Log($"esto no es minecraft no puedes entrar a modo {moda} POR QUE NO EXISTE aqui");
				}
				else
					Debug.Log($"esto no es minecraft no puedes hacer /gamemode {moda}");
			}
		}
		public static bool IsInPlay()
		{
			if (PlayerManager.Player != null)
			{
				if (PlayerManager.isInEditor)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			return false;
		}
	}
	public static class MetaUtils
	{

		/// <summary>
		/// Compara dos cadenas de versión.
		/// Devuelve: -1 si version1 &lt; version2, 0 si iguales, 1 si version1 &gt; version2.
		/// Soporta prefijos "Alpha", "Beta" (case-insensitive), y sufijos "RC ####" o "pre####" (con o sin espacio).
		/// Orden (de menor a mayor): Alpha &lt; Beta &lt; Release. Dentro del mismo grupo: RC &lt; pre &lt; normal.
		/// </summary>
		public static int CompareVersions(string version1, string version2)
		{
			if (ReferenceEquals(version1, version2)) return 0;
			if (version1 is null) return -1;
			if (version2 is null) return 1;

			string v1 = version1.Trim();
			string v2 = version2.Trim();

			// 1. Extraer bases (lo que está antes del '-')
			bool isSnap1 = v1.Contains("-");
			bool isSnap2 = v2.Contains("-");

			string base1 = isSnap1 ? v1.Split('-')[0].Trim() : v1;
			string base2 = isSnap2 ? v2.Split('-')[0].Trim() : v2;

			// 2. Comparar Fase Principal (Alpha/Beta/Release)
			var phase1 = GetPhase(base1);
			var phase2 = GetPhase(base2);

			if (phase1 != phase2)
				return phase1.CompareTo(phase2);

			// 3. Comparar Números de versión (3.1.0)
			int numComparison = CompareNumericParts(base1, base2);
			if (numComparison != 0)
				return numComparison;

			// 4. Si la base numérica y la fase son IGUALES, desempatamos por Subfase
			var sub1 = GetSubPhase(v1);
			var sub2 = GetSubPhase(v2);

			if (sub1 != sub2)
				return sub1.CompareTo(sub2);

			// 5. Si AMBAS son Snapshots de la misma base, comparamos el código cronológico
			if (isSnap1 && isSnap2)
			{
				return CompareSnapshotCodes(v1, v2);
			}

			return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
		}

		private static int CompareNumericParts(string v1, string v2)
		{
			var verRegex = new Regex(@"\d+(?:\.\d+)*");
			var m1 = verRegex.Match(v1);
			var m2 = verRegex.Match(v2);

			if (!m1.Success || !m2.Success)
				return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);

			var parts1 = m1.Value.Split('.');
			var parts2 = m2.Value.Split('.');

			int maxLen = Math.Max(parts1.Length, parts2.Length);
			for (int i = 0; i < maxLen; i++)
			{
				// Usamos TryParse para evitar errores si hay basura en el string
				int p1 = (i < parts1.Length && int.TryParse(parts1[i], out int n1)) ? n1 : 0;
				int p2 = (i < parts2.Length && int.TryParse(parts2[i], out int n2)) ? n2 : 0;

				if (p1 > p2) return 1;
				if (p1 < p2) return -1;
			}
			return 0;
		}

		private static int CompareSnapshotCodes(string v1, string v2)
		{
			var snapRegex = new Regex(@"(\d{2})n(\d+)([a-z]?)", RegexOptions.IgnoreCase);
			var m1 = snapRegex.Match(v1);
			var m2 = snapRegex.Match(v2);

			// Si una no cumple el formato snapshot, la que SÍ cumple es considerada más nueva
			if (m1.Success && !m2.Success) return 1;
			if (!m1.Success && m2.Success) return -1;
			if (!m1.Success && !m2.Success) return 0;

			// Comparar Año
			if (int.TryParse(m1.Groups[1].Value, out int y1) && int.TryParse(m2.Groups[1].Value, out int y2))
			{
				if (y1 != y2) return y1.CompareTo(y2);
			}

			// Comparar Número
			if (int.TryParse(m1.Groups[2].Value, out int n1) && int.TryParse(m2.Groups[2].Value, out int n2))
			{
				if (n1 != n2) return n1.CompareTo(n2);
			}

			// Comparar Letra
			return string.Compare(m1.Groups[3].Value, m2.Groups[3].Value, StringComparison.OrdinalIgnoreCase);
		}

		private static AdapticaDevPhases GetPhase(string v)
		{
			if (v.StartsWith("Alpha", StringComparison.OrdinalIgnoreCase)) return AdapticaDevPhases.Alpha;
			if (v.StartsWith("Beta", StringComparison.OrdinalIgnoreCase)) return AdapticaDevPhases.Beta;
			return AdapticaDevPhases.Realese;
		}

		private static VersionSubPhase GetSubPhase(string v)
		{
			// Si tiene guion, es Snapshot (la fase más reciente post-release de esa versión)
			if (v.Contains("-")) return VersionSubPhase.Snapshot;

			bool hasPre = v.Contains("pre", StringComparison.OrdinalIgnoreCase);
			bool hasRC = v.Contains("RC", StringComparison.OrdinalIgnoreCase);

			if (hasPre && hasRC) return VersionSubPhase.PreRC;
			if (hasPre) return VersionSubPhase.Pre;
			if (hasRC) return VersionSubPhase.RC;

			return VersionSubPhase.Normal;
		}
		private enum VersionSubPhase
		{
			Pre,
			PreRC,
			RC,
			Normal,
			Snapshot
		}


	}
	enum minecraftGamemodes//solo para el comando de huevo de pascua /gamemodes
	{
		survival = 0,
		creative = 1,
		adventure = 2,
		spectator = 3,
	}
}
