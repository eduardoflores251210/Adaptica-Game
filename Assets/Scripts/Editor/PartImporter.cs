using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using EGL = UnityEditor.EditorGUILayout;
using EGL2 = UnityEngine.GUILayout;
/// <summary>
/// Ventana para importar partes
/// 
/// actua a veces como el Chavo del 8 como puedes ver
/// </summary>
public class PartImporter :  EditorWindow
{
    public GameObject PärtPrefab; // si uso ä por que es raro ademas esto es herramienta interna asi que no lo va a ver nadie mas que yo y asi me evito problemas de colisiones de nombres con otras variables o cosas asi
	public PartTypes type;
	public string PartName;
	public string PartDescription;
	// Update is called once per frame
	void Update()
    {
        
    }

    [MenuItem("Tools/Part Importer")]
	public static void ShowWindow()
    {
        GetWindow<PartImporter>("Part Importer");
	}
	private void OnGUI()
	{
		EGL.LabelField("Importar parte desde un mesh", EditorStyles.boldLabel);
		EGL.LabelField("NO USES ESTO PARA ACTUALIZAR PARTES EXISTENTES, SOLO PARA CREAR PARTES NUEVAS, SI QUIERES ACTUALIZAR PARTES EXISTENTES HAZLO MANUALMENTE EDITANDO EL ASSET DE LA PARTE EN CUESTION -atte el chavo");
		EGL.Space(30);
		EGL.LabelField("Nombre de la parte:");
		PartName = EGL.TextField(PartName);

		EGL.LabelField("Descripción de la parte:");
		PartDescription = EGL.TextField(PartDescription);
		type = (PartTypes)EGL.EnumPopup("Tipo de parte:", type);
		PärtPrefab = (GameObject)EGL.ObjectField("Prefab de la parte:", PärtPrefab, typeof(GameObject), false);
		EGL.Space(20);
		string SHA512first10hexdigits = System.BitConverter.ToString(System.Security.Cryptography.SHA512.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(PartName + PartDescription + type.ToString()))).Replace("-", "").Substring(0, 10); //no se por si alguien hace tonterias con el nombre de la parte o la descripcion de la parte o el tipo de parte, asi que le pongo un hash para evitar problemas de colisiones de nombres o cosas asi
																																																													//ahora solo falta el boton de guardar para que lo Guarde en Assets/Parts/Vehicle o Assets/Parts/Cell_creature o Assets/Parts/Plant dependiendo del tipo de parte que sea
		if (EGL2.Button("Guardar parte"))
		{                                                                                                                                                                                                                                       //con un boton de guardar que cree un asset con el nombre de la parte y la descripcion de la parte y el mesh de la parte en la carpeta correspondiente

			BasePart @base = null;
			string assetPath = "";
			if (type == PartTypes.Animal)
			{

				string folderPath = $"Assets/Parts/Cell_creature"; //si es incosistente pero es por que  es muy dificil cambiarle nombre al tipo BiologicalPart que a su vez es exclusicvo de celulas y criaturas, asi que lo deje asi para no romper nada
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}
				assetPath = $"{folderPath}/{PartName + SHA512first10hexdigits}.asset";
				BiologicalPart partData = ScriptableObject.CreateInstance<BiologicalPart>();
				partData.displayName = partData.name = PartName;
				partData.description = PartDescription;
				partData.prefab = PärtPrefab;

				@base = partData;
			}
			else if (type == PartTypes.Vehicle)
			{


				string folderPath = $"Assets/Parts/Vehicle";
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}
				assetPath = $"{folderPath}/{PartName + SHA512first10hexdigits}.asset";
				VehiclePart partData = ScriptableObject.CreateInstance<VehiclePart>();
				partData.displayName = partData.name = PartName;
				partData.description = PartDescription;
				partData.prefab = PärtPrefab;

				@base = partData;
			}
			else if (type == PartTypes.Vegetal)
			{

				string folderPath = $"Assets/Parts/Vegetal";
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}
				assetPath = $"{folderPath}/{PartName + SHA512first10hexdigits}.asset";
				VegetalPart partData = ScriptableObject.CreateInstance<VegetalPart>();
				partData.displayName = partData.name = PartName;
				partData.description = PartDescription;
				partData.prefab = PärtPrefab;



				@base = partData;
			}else
			{
				EditorUtility.DisplayDialog("Error", " Se te chispoteo ese tipo de parte no esta soportado", "OK");//si asi es use una frase del Chavo
				return;
			}
				AssetDatabase.CreateAsset(@base, assetPath);
			AssetDatabase.SaveAssets();
			EditorUtility.DisplayDialog("Parte guardada", $"La parte '{PartName}' ha sido guardada en '{assetPath}' NOTA tu tienes que añadirla a la base de datos correspondiente No soy mago", "OK");
		}
	}
}
