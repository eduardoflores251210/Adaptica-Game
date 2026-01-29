using ActualUtils;
using SerializableTypes;
using UnityEngine;
using UnityEngine.SceneManagement;

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
		}
		public static class Cheats
		{
			[ConsoleCommand("Motherlode", true)]
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
		}
		public static class EasterEggCommands
		{
			[ConsoleCommand("dir")]
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
			[ConsoleCommand("rm")]
			public static void rm()
			{
				Debug.Log("lo siento pero no puedes hacer rm\n usa una terminal real\n ni te dejare eliminargame objects ");
			}
			[ConsoleCommand("ren")]
			public static void ren()
			{
				Debug.Log("lo siento pero no puedes hacer ren\n usa una terminal real");
			}
			[ConsoleCommand("rmdir")]
			public static void rmAll()
			{
				{
					Debug.Log("lo siento pero no puedes hacer rmdir\n usa una terminal real");
				}
			}
			[ConsoleCommand("del")]
			public static void del()
			{
				{
					Debug.Log("lo siento pero no puedes hacer del\n usa una terminal real");
				}
			}
			[ConsoleCommand("pwd")]
			public static void PWD()
			{
				Debug.Log(SceneManager.GetActiveScene().name);
			}
			[ConsoleCommand("/gamemode")]
			public static void Gamemode()
			{
				Debug.Log("esto no es minecraft");
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
}
