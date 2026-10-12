using aaa;
using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// creo que carga la criatura en el visualizador de criaturas
/// aunque deveria usar el termino Microbio en vez de criatura ahora que el estadio creatura
/// esta en desarrollo
/// </summary>
public class CreatureLoader : MonoBehaviour
{
    public string Path;
    public PartsDatabase Parts;
    public GéneroBiológico ActiveGender = GéneroBiológico.Female;
    public Material FMat;
    public Material MMat;
	public Vector3 Rot;
	public bool Reset;
	Color Normalcolor;
	public MicrobeData BaseData;
	/// <summary>
	/// Configura el/la Microbi@ 
	/// 
	/// 
	/// 
	/// 
	/// 
	/// pd si uso Aroba 
	/// en vez de X para genero neutral :) ademas Microbix es raro
	/// </summary>
	void SetupMicrobe()
	{
		Vector3 OldPos = transform.position;
		BaseData = LoadMicrobe();
		if (BaseData.DATA_VERSION == -1)
		{
			Debug.LogWarning("ESTAS cargando un microbio de Alpha 3.1 o anterior, puede que no funcione correctamente");
		}
		else if (BaseData.DATA_VERSION > DevInfo.CurrentCreatureDataVersion)
		{
			Debug.LogWarning("El Microbio que estas intentando cargar es de una version mas nueva que la del juego, puede que no funcione correctamente");
			
		}
		if (BaseData.DATA_VERSION != -1)
			BaseData.RotateMicrobeEuler(Rot);
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		if (renderer == null)
			renderer = gameObject.AddComponent<MeshRenderer>();
		GéneroBiológico CurrentGen;
		Material mat = new Material(FMat);
		transform.position = Vector3.zero;
		// Determinar género si hay ambos disponibles

		CurrentGen = ActiveGender;


		// Seleccionar color y partes según género

		var partsList = new List<SerializedPartData>();

		if (CurrentGen == GéneroBiológico.Female || CurrentGen == GéneroBiológico.None)
		{
			Normalcolor = BaseData.FemaleColor;
			partsList = BaseData.PartsF;
		}
		else // GéneroBiológico.Male
		{
			Normalcolor = BaseData.MaleColor;
			partsList = BaseData.PartsM;
		}

		mat.color = Normalcolor;
		renderer.material = mat;
		int HeCount = 0; //no no es cuenta de helio
		int CaCount = 0; // no no es cuenta calcio
		int OmCount = 0;
		int i = 0;
		// Instanciar las partes
		foreach (var ff in partsList)
		{
			var GOP = Parts.GetPartByID(ff.Id).prefab;
			if (GOP.activeInHierarchy == false)
				GOP.SetActive(true);
			var meshI = GOP.GetComponent<MeshFilter>();
			var Ren = GOP.GetComponent<MeshRenderer>();
			var GO = new GameObject(i.ToRoman());
			GO.transform.SetPositionAndRotation(ff.transform.Pos, Quaternion.Euler(ff.transform.Rot));
			GO.transform.localScale = ff.transform.Scale;
			GO.transform.SetParent(transform, true);
			var red = GO.AddComponent<MeshRenderer>();
			var meshf = GO.AddComponent<MeshFilter>();
			meshf.mesh = meshI.sharedMesh;
			red.materials = Ren.sharedMaterials;
			//try commentado por que es mas facil ver la stacktrace sin el try catch aunque no es tan seguro pero bueno
			//try

			i++;
		}

		// Configurar mesh
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();
		// Mesh --> Mesh......... confuso ¿verdad?
		var mesh = (UnityEngine.Mesh)BaseData.Mesh; // mesh es del tipo StandartUtilities.StdUtils.Serializable.Mesh
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.RecalculateTangents();
		meshFilter.mesh = mesh;
	
		transform.position = OldPos;


		
		
	}

	/// <summary>
	/// Carga El Microbio en base al paquete recibido
	/// o crea uno nuevo para casos de emergencia
	/// </summary>
	/// <returns>Un Microbio nueva o cargada</returns>
	MicrobeData LoadMicrobe()
	{
		string BAseOBJ = "# 8 vertices del cubo\r\nv -1 -1 -1\r\nv  1 -1 -1\r\nv  1  1 -1\r\nv -1  1 -1\r\nv -1 -1  1\r\nv  1 -1  1\r\nv  1  1  1\r\nv -1  1  1\r\n\r\n# 12 triángulos (2 por cara) - normales invertidas\r\n# Cara frontal\r\nf 3 2 1\r\nf 4 3 1\r\n\r\n# Cara trasera\r\nf 7 8 5\r\nf 6 7 5\r\n\r\n# Cara izquierda\r\nf 8 4 1\r\nf 5 8 1\r\n\r\n# Cara derecha\r\nf 7 6 2\r\nf 3 7 2\r\n\r\n# Cara superior\r\nf 7 3 4\r\nf 8 7 4\r\n\r\n# Cara inferior\r\nf 6 5 1\r\nf 2 6 1\r\n";
		MicrobeData Tempdata;
		var EmergencyMEsh = StandartUtilities.StdUtils.Serializable.Mesh.FromObjString(BAseOBJ);
		Tempdata = new MicrobeData("AAAAA",
								  "AAAA",
								  false,
								  reproductionTypes.SingleCell,
								  ReproductionMethod.SplitIn2,
								  new List<SerializedPartData>() {
									  new("0", new StdUtils.Serializable.Transform(Vector3.forward, Quaternion.identity.eulerAngles, Vector3.one)),
									  new("-1", new StdUtils.Serializable.Transform(Vector3.up, Quaternion.identity.eulerAngles, Vector3.one))
								  },
								  null,
								  Color.magenta,
								  Color.blue,
								  new() { new() { radius = 3 } }, mesh: EmergencyMEsh);

		if (!File.Exists(Path)) return Tempdata;
		else if (File.Exists(Path))
		{
			try
			{
				string json = File.ReadAllText(Path);
				MicrobeData data = JsonUtility.FromJson<MicrobeData>(json);
				return data;
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error al cargar el microbio desde {Path}: {ex.Message}");
				return Tempdata;
			}
		}
		else return Tempdata;
	}
	private void Start()
    {
       SetupMicrobe();
	}
	private void Update()
	{
		if (Reset)
		{
			Reset = false;
			DeleteChildren();
			SetupMicrobe();
		}
	}
	void DeleteChildren()
	{
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}
	}
}
