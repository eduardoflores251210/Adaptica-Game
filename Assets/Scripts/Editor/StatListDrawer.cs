using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatList))]
public class StatListDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		SerializedProperty list = property.FindPropertyRelative("stats");
		// Altura total: título + cabeceras + elementos + botones
		return (list.arraySize + 4) * (EditorGUIUtility.singleLineHeight + 4);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty list = property.FindPropertyRelative("stats");
		EditorGUI.BeginProperty(position, label, property);

		position.height = EditorGUIUtility.singleLineHeight;

		// Título
		EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
		position.y += EditorGUIUtility.singleLineHeight + 4;

		// Cabeceras
		float colWidth = (position.width - 10) / 2;
		Rect keyHeader = new Rect(position.x, position.y, colWidth, position.height);
		Rect valHeader = new Rect(position.x + colWidth + 5, position.y, colWidth, position.height);
		EditorGUI.LabelField(keyHeader, "Clave", EditorStyles.miniBoldLabel);
		EditorGUI.LabelField(valHeader, "Valor", EditorStyles.miniBoldLabel);
		position.y += EditorGUIUtility.singleLineHeight + 2;

		// Filas
		for (int i = 0; i < list.arraySize; i++)
		{
			SerializedProperty element = list.GetArrayElementAtIndex(i);
			SerializedProperty key = element.FindPropertyRelative("key");
			SerializedProperty value = element.FindPropertyRelative("value");

			Rect keyRect = new Rect(position.x, position.y, colWidth, EditorGUIUtility.singleLineHeight);
			Rect valRect = new Rect(position.x + colWidth + 5, position.y, colWidth, EditorGUIUtility.singleLineHeight);

			key.stringValue = EditorGUI.TextField(keyRect, key.stringValue);
			value.stringValue = EditorGUI.TextField(valRect, value.stringValue);

			position.y += EditorGUIUtility.singleLineHeight + 4;
		}

		// Botones (abajo a la derecha)
		float buttonWidth = 22;
		float buttonHeight = EditorGUIUtility.singleLineHeight;
		float spacing = 3;

		Rect addButton = new Rect(position.x + position.width - (buttonWidth * 2 + spacing), position.y, buttonWidth, buttonHeight);
		Rect removeButton = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, buttonHeight);

		if (GUI.Button(addButton, "+"))
		{
			list.InsertArrayElementAtIndex(list.arraySize);
		}

		if (GUI.Button(removeButton, "–"))
		{
			if (list.arraySize > 0)
				list.DeleteArrayElementAtIndex(list.arraySize - 1);
		}

		EditorGUI.EndProperty();
	}
}
