#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

[CustomEditor(typeof(CrossScenePackageSender))]
public class CrossScenePackageSenderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		CrossScenePackageSender carteroScript = (CrossScenePackageSender)target;

		EditorGUILayout.Space(10);
		GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
		EditorGUILayout.BeginVertical("box");
		GUI.backgroundColor = Color.white;

		EditorGUILayout.LabelField("📦 EL CORREO RECURSIVO DE TANGAMANDAPIO", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Este inspector ve todo: desde la agresividad de tu criatura con flojera infinita hasta lo que Dios nos mande.", MessageType.Info);

		if (carteroScript.Packages == null || carteroScript.Packages.Count == 0)
		{
			EditorGUILayout.LabelField("No hay paquetes. ¡Todos se fueron a ver la película de Chucho el Roto!", EditorStyles.centeredGreyMiniLabel);
			EditorGUILayout.EndVertical();
			return;
		}

		EditorGUILayout.LabelField($"Total de paquetes en la saca: {carteroScript.Packages.Count}", EditorStyles.miniBoldLabel);
		EditorGUILayout.Space(5);

		for (int i = 0; i < carteroScript.Packages.Count; i++)
		{
			var pkg = carteroScript.Packages[i];

			EditorGUILayout.BeginVertical("helpBox");
			if (pkg != null)
			{
				EditorGUILayout.LabelField($"Paquete [{i}] - [{pkg.GetType().Name}]", EditorStyles.boldLabel);
				EditorGUILayout.LabelField($"📤 De: {pkg.Sender} | 📥 Para: {pkg.Reciever}");

				if (pkg.Tags != null && pkg.Tags.Length > 0)
				{
					EditorGUILayout.LabelField($"🏷️ Tags: {string.Join(", ", pkg.Tags)}");
				}

				// Magia recursiva para sacar el Contents sea lo que sea
				var contentsProp = pkg.GetType().GetProperty("Contents");
				if (contentsProp != null)
				{
					var contentValue = contentsProp.GetValue(pkg);
					EditorGUILayout.Space(2);
					EditorGUILayout.LabelField("🎁 Contenido Detallado:", EditorStyles.miniBoldLabel);

					// ¡Aquí entra la recursividad como el Chavo metiéndose a donde no lo llaman!
					EditorGUI.indentLevel++;
					DrawObjectRecursively(contentValue);
					EditorGUI.indentLevel--;
				}
			}
			else
			{
				EditorGUILayout.LabelField($"Paquete [{i}]: ¡Se cayó al lodo! (Es null)", EditorStyles.boldLabel);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(4);
		}

		EditorGUILayout.EndVertical();

		if (Application.isPlaying)
		{
			Repaint();
		}
	}

	// --- FUNCIÓN RECURSIVA MULTIPROPÓSITO ---
	private void DrawObjectRecursively(object obj, string label = "")
	{
		if (obj == null)
		{
			EditorGUILayout.LabelField(string.IsNullOrEmpty(label) ? "Valor" : label, "¡Null! (Desapareció como el Chavo en el callejón)");
			return;
		}

		Type type = obj.GetType();

		// 1. Si es un tipo primitivo, string o un struct de Unity (como Vector3)
		if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.Namespace?.StartsWith("UnityEngine") == true)
		{
			string displayLabel = string.IsNullOrEmpty(label) ? "Valor" : label;
			EditorGUILayout.LabelField($"{displayLabel}: {obj}");
			return;
		}

		// 2. Si es una lista, arreglo o colección (ej. List<T>, arrays)
		if (obj is IEnumerable enumerable && !(obj is string))
		{
			string listLabel = string.IsNullOrEmpty(label) ? "Colección" : label;
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField($"📁 {listLabel} (Lista)", EditorStyles.boldLabel);

			EditorGUI.indentLevel++;
			int index = 0;
			foreach (var item in enumerable)
			{
				DrawObjectRecursively(item, $"[{index}]");
				index++;
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
			return;
		}

		// 3. Si es una clase o struct personalizada (como tu BehaviourStats)
		string structLabel = string.IsNullOrEmpty(label) ? type.Name : label;
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField($"📦 {structLabel} ({type.Name})", EditorStyles.boldLabel);

		EditorGUI.indentLevel++;

		// Inspeccionar campos públicos
		var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		foreach (var field in fields)
		{
			try
			{
				var val = field.GetValue(obj);
				DrawObjectRecursively(val, field.Name);
			}
			catch (Exception ex)
			{
				EditorGUILayout.LabelField(field.Name, $"Error: {ex.Message}");
			}
		}

		// Inspeccionar propiedades públicas con getter
		var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		foreach (var prop in properties)
		{
			if (prop.CanRead && prop.GetIndexParameters().Length == 0)
			{
				try
				{
					var val = prop.GetValue(obj);
					DrawObjectRecursively(val, prop.Name);
				}
				catch { /* Ignorar propiedades incompatibles */ }
			}
		}

		EditorGUI.indentLevel--;
		EditorGUILayout.EndVertical();
	}
}
#endif