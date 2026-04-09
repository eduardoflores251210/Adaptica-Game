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
				if (saved.CreatureName == name)
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
	/// XDdddd
	/// 
	/// SIMBOLOGIA DE LOS COMANDOS DE LA CONSOLA:
	/// ! -comando debug o de una funcion en pruebas ej
	///		!editmic - abre el editor de micrboios desde el juego en el futuro cuando esta funcion sea estable se eliminara el ! y quedara solo EditMic o el comando desaparecera y el boton del menu se encargara de abrir el editor pero por ahora esta asi para evitar que alguien use un comando que no esta
	///		!loadpt -carga la generacion test de planeta es MUY debug
	/// si no hay ! o es truco o es un comando normal que no es ni truco ni debug, por ejemplo:
	///		fbxexport -exportaria o (a lo mejor jamas llegue) el modelo del jugador en formato fbx para usarlo en otros programas de modelado o lo que sea, este comando no es ni truco ni debug es solo una utilidad que podria ser util para los jugadores pero no es algo que se quiera que se use sin saber lo que hace por eso no es un comando normal sin simbolos ni nada
	///		help - muestra una lista de comandos disponibles y su descripcion, este comando es un comando normal que no es ni truco ni debug por que no hace nada malo ni es algo que se quiera ocultar a los jugadores pero tampoco es algo que se quiera que se use sin saber lo que hace por eso no es un comando normal sin simbolos ni nada
	///		
	///  no mayusculas solo minusculas por favor, gracias por su coomprension :D
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
		public static void RegisterEditor(MonoBehaviour @object, Editors editor)
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
			if (isInEditor) Player = null;
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
