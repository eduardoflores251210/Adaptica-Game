using SerializableTypes;
using SerializableTypes.Biology;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

using Random = UnityEngine.Random;

public class MicrobeSpawneer : MonoBehaviour
{
	public GameObject CellPrefab;
	/// <summary>
	/// microbios maximos (uint de 32 bits)
	/// </summary>
	public uint MaxMicr = 256;
	/// <summary>
	/// cantidad de microbios (uint de 32)
	/// </summary>
	public uint MicrCount = 0;
	/// <summary>
	/// Tiempo de espera entre spawneo
	/// </summary>
	public float SpawnWaitTime = 3.1415926535f;
	/// <summary>
	/// puede spawnear
	/// </summary>
	public bool CanSpawn = true;
	///<summary>
	///Radio de generación
	///</summary>
	public float SpawnRadius;
	List<MicrobeData> Datas = new List<MicrobeData>();
	/// <summary>
	/// inicializa el spawner
	/// </summary>
	void Start()
	{
		Datas = new List<MicrobeData>();
		foreach (var file in Directory.EnumerateFiles(Paths.Cells, "*.json"))
		{
			try
			{
				MicrobeData data = JsonUtility.FromJson<MicrobeData>(File.ReadAllText(file));
				Datas.Add(data);
			}
			catch (System.Exception)
			{
			}
		}
		StartCoroutine(nameof(Spawn));
	}
	public string DestroyMsg = "razon desconocida";
	private void OnDestroy()
	{
		StopAllCoroutines();
		/*if (DestroyMsg != ("Se está cerrando el juego") && DestroyMsg != ("Se está descargando la escena."))
			Debug.LogError("El generador se ha destruido de manera anomala con la siguiente razón citada:\n " + DestroyMsg);
		else
			Debug.Log("El generador se ha destruido de manera normal con la siguiente razón citada:\n " + DestroyMsg);*/
	}
	/// <summary>
	/// genera microbios
	/// </summary>
	IEnumerator Spawn()
	{
		GameObject GO = null;
		Vector2 pos = Vector2.zero;
		while (true)
		{
			if (!CanSpawn)
				yield return null;
			if (MicrCount < MaxMicr)
			{
				GO = null;
				
				pos =  (Random.insideUnitCircle * SpawnRadius);
				var input = DateTime.Now.ToString("o");
				using SHA512 sha = SHA512.Create();
				byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
				// Convertir a hexadecimal
				StringBuilder sb = new StringBuilder();
				foreach (byte b in hashBytes)
					sb.Append(b.ToString("x2"));
				if (CrossScenePackageSender.Instance == null)
				{
					var AAA = new GameObject("Mail");
					var BBB = AAA.AddComponent<CrossScenePackageSender>();
					DontDestroyOnLoad(AAA);
					CrossScenePackageSender.Instance = BBB;
				}
				var Mailman = CrossScenePackageSender.Instance;
				MicrobeData microbe = Datas[Random.Range(0, Datas.Count)];
				// Parche de emergencia elegante: centrar microbio y partes
				microbe.CenterMicrobe();
				microbe.RotateMicrobeEuler(new(0, 90, 0));
				Mailman.SendTypedPackage(gameObject.name, sb.ToString(), microbe, new string[1] { nameof(MicrobeData) });
				GO = Instantiate(CellPrefab);
				GO.transform.position = (Vector3)pos + transform.position;
				GO.name = sb.ToString();
				MicrCount++;
			}
			yield return new WaitForSeconds(SpawnWaitTime);
		}
	}

	void OnEnable()
	{
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}
	void OnDisable()
	{
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}
	void OnSceneUnloaded(Scene scene)
	{
		DestroyMsg = ("Se está descargando la escena.");
	}
	private void OnApplicationQuit()
	{
		DestroyMsg = ("Se está cerrando el juego");
	}


}

