#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class ForbiddenUnityEditorNamespaceGuard : IPreprocessBuildWithReport
{
	public int callbackOrder => -1000;

	private static readonly string[] ForbiddenFragments =
	{
		"using UnityEditor.Localization.Plugins",
		"using UnityEditor.Localization",
		"using UnityEditor.Experimental.GraphView",
	};

	public void OnPreprocessBuild(BuildReport report)
	{
		var hits = ScanProjectForForbiddenUsings();

		if (hits.Count == 0)
		{
			Debug.Log("Guard: no se encontraron usings prohibidos en scripts de runtime.");
			return;
		}

		var message =
			"Build bloqueado: se encontraron usings en scripts que no son de Editor:\n" +
			string.Join("\n", hits.Select(p => " - " + p));

		Debug.LogError(message);
		throw new BuildFailedException(message);
	}

	[MenuItem("Tools/Checks/Buscar usings UnityEditor.Localization prohibidos")]
	private static void RunManualCheck()
	{
		var hits = ScanProjectForForbiddenUsings();

		if (hits.Count == 0)
		{
			Debug.Log("No se encontraron coincidencias.");
			return;
		}

		Debug.LogError(
			"Se encontraron coincidencias:\n" +
			string.Join("\n", hits.Select(p => " - " + p)));
	}

	private static List<string> ScanProjectForForbiddenUsings()
	{
		var results = new List<string>();

		var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
		var assetsRoot = Application.dataPath;

		foreach (var file in Directory.GetFiles(assetsRoot, "*.cs", SearchOption.AllDirectories))
		{
			if (IsInsideEditorFolder(file))
				continue;

			string text;
			try
			{
				text = File.ReadAllText(file);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"No se pudo leer '{file}': {e.Message}");
				continue;
			}

			if (ForbiddenFragments.Any(fragment => text.Contains(fragment)))
			{
				results.Add(NormalizePath(file, projectRoot));
			}
		}

		return results;
	}

	private static bool IsInsideEditorFolder(string filePath)
	{
		var normalized = filePath.Replace('\\', '/');
		return normalized.Contains("/Editor/");
	}

	private static string NormalizePath(string path, string projectRoot)
	{
		var normalized = path.Replace('\\', '/');
		var root = projectRoot.Replace('\\', '/').TrimEnd('/');
		return normalized.StartsWith(root, StringComparison.OrdinalIgnoreCase)
			? normalized.Substring(root.Length + 1)
			: normalized;
	}
}
#endif