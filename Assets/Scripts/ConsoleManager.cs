using ActualUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConsoleManager : MonoBehaviour
{
	public static ConsoleManager Instance { get; private set; }

	[Header("UI")]
	[SerializeField] private GameObject consoleUI;
	[SerializeField] private TMPro.TextMeshProUGUI outputText;
	[SerializeField] private TMPro.TMP_InputField inputField;
	[SerializeField] private int maxLines = 15;
	public InputActionAsset InputActions;
	private InputAction Ct;
	private InputAction sh;
	private InputAction C;


	private readonly List<string> lines = new();

	private readonly Dictionary<string, CommandInfo> commands = new();

	#region Unity Lifecycle

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(consoleUI);
	}

	void OnEnable()
	{
		Application.logMessageReceived += OnUnityLog;
	}

	void OnDisable()
	{
		Application.logMessageReceived -= OnUnityLog;
	}

	void Start()
	{
		var map = InputActions.FindActionMap("DEBUG")
			;
		Ct = map.FindAction("Ctrl");
		sh = map.FindAction("Shift");
		C = map.FindAction("C");
		Ct.performed += Ct_performed;
		sh.performed += Sh_performed;
		C.performed += C_performed;
		map.Enable();	
		outputText.text = "";

		RegisterAllCommands();
		Debug.Log("🧠 Console ready. Type 'help'");
	}

	private void C_performed(InputAction.CallbackContext obj)
	{

		if (Ct.inProgress && sh.inProgress)
		{
			Toggle();
		}
	}

	private void Sh_performed(InputAction.CallbackContext obj)
	{

		if (C.inProgress && Ct.inProgress)
		{
			Toggle();
		}
	}

	private void Ct_performed(InputAction.CallbackContext obj)
	{

		if (C.inProgress && sh.inProgress)
		{
			Toggle();
		}
	}

	#endregion

	#region Public API

	public void Toggle()
	{
		consoleUI.SetActive(!consoleUI.activeSelf);
		if (consoleUI.activeSelf)
			inputField.ActivateInputField();
	}

	public void ExecuteInput()
	{
		string input = inputField.text;
		inputField.text = "";
		Execute(input);
	}

	public void Log(string msg)
	{
		lines.Add(msg);

		if (lines.Count > maxLines)
			lines.RemoveAt(0); // la más vieja muere aquí ☠️

		outputText.text = string.Join("\n", lines);
	}


	#endregion

	#region Command System

	void RegisterAllCommands()
	{
		commands.Clear();

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();

		foreach (var asm in assemblies)
		{
			foreach (var type in asm.GetTypes())
			{
				foreach (var method in type.GetMethods(
					BindingFlags.Static |
					BindingFlags.Public |
					BindingFlags.NonPublic))
				{
					var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
					if (attr == null) continue;

					commands[attr.Name] = new CommandInfo
					{
						Method = method,
						Attribute = attr
					};
				}
			}
		}

		Debug.Log($"📜 {commands.Count} comandos registrados");
	}

	public void Execute(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return;

		Debug.Log($"> {input}");

		var parts = ParseInput(input);
		var cmdName = parts[0].ToLower();
		var args = parts.Skip(1).ToArray();



		if (!commands.TryGetValue(cmdName, out var cmd))
		{
			Debug.Log("X Comando desconocido");
			return;
		}

		InvokeCommand(cmd.Method, args);
	}

	void InvokeCommand(MethodInfo method, string[] args)
	{
		var parameters = method.GetParameters();

		if (args.Length > parameters.Length)
		{
			Debug.Log($"❌ Demasiados argumentos (esperados {parameters.Length})");
			return;
		}

		object[] finalArgs = new object[parameters.Length];

		for (int i = 0; i < parameters.Length; i++)
		{
			if (i >= args.Length)
			{
				finalArgs[i] = parameters[i].HasDefaultValue
					? parameters[i].DefaultValue
					: null;
				continue;
			}

			try
			{
				finalArgs[i] = Convert.ChangeType(
					args[i],
					parameters[i].ParameterType);
			}
			catch
			{
				Debug.Log($"❌ Argumento inválido: {args[i]}");
				return;
			}
		}

		method.Invoke(null, finalArgs);
	}

	#endregion

	#region Helpers
	[ConsoleCommand("help")]
	public static void ShowHelp()
	{
		Debug.Log("📖 Comandos disponibles:");
		foreach (var cmd in Instance.commands.Values)
			Debug.Log($" - {cmd.Attribute.Name}");
	}
	void clear()
	{
		outputText.text = "";
		lines.Clear();
	}

	[ConsoleCommand("cls")]
	public static void ClearConsole()
	{
		Instance.clear();
	}
	static string[] ParseInput(string input)
	{
		return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
			.Select(m => m.Value.Trim('"'))
			.ToArray();
	}

	void OnUnityLog(string message, string stack, LogType type)
	{
		foreach (var line in message.Split('\n'))
			Log($"[{type}] {message}");
	}

	#endregion

	#region Internal Types

	class CommandInfo
	{
		public MethodInfo Method;
		public ConsoleCommandAttribute Attribute;
	}

	#endregion
}
