using NUnit.Framework;
using SerializableTypes.Biology;
using StandartUtilities; //basicamente como una libreria personal que tiene funciones utiles 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



/// <summary>
/// El generador de comida del estadio celula solo puede generar 2 tipos de comida por que no es criatura que tiene variedad
/// </summary>
public class FoodSpawner : MonoBehaviour
{
	/// <summary>
	/// prefab de la carne requiere foodcomp
	/// </summary>
	public GameObject MeatPrefab;
	/// <summary>
	/// prefab de las agas requiere foodcomp
	/// </summary>
	public GameObject AlgaePrefab;
	/// <summary>
	/// comida maxima (uint de 16 bits)
	/// </summary>
	public ushort MaxFood = 256;
	/// <summary>
	/// cantidad de comida (uint de 16)
	/// </summary>
	public ushort foodCount = 0;
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
	List<FoodComp> Food = new();


	/// <summary>
	/// inicializa el spawbner
	/// </summary>
	void Start()
	{
		if (!MeatPrefab.TryGetComponent<FoodComp>(out _))
		{
			DestroyMsg = "DESAROLLADOR CONFIGURA BIEN TUS PREFABS (Carne)!!!\n";
			DestroyMsg += "Falta el FoodComp";
			Destroy(this);
			return;
		}

		if (!AlgaePrefab.TryGetComponent<FoodComp>(out _))
		{
			DestroyMsg = "DESAROLLADOR CONFIGURA BIEN TUS PREFABS (Alga)!!!\n";
			DestroyMsg += "Falta el FoodComp";
			Destroy(this);
			return;
		}
		StartCoroutine(nameof(Spawn));
		StartCoroutine(nameof(N));
	}
	string DestroyMsg = "razon desconocida";
	private void OnDestroy()
	{
		StopAllCoroutines();
		/*if (DestroyMsg != ("Se está cerrando el juego") && DestroyMsg != ("Se está descargando la escena."))
			Debug.LogError("El generador se ha destruido de manera anomala con la siguiente razón citada:\n " + DestroyMsg);
		else 
			Debug.Log("El generador se ha destruido de manera normal con la siguiente razón citada:\n " + DestroyMsg);*/
	}
	/// <summary>
	/// genera comida 
	/// </summary>
	IEnumerator Spawn()
	{
		GameObject GO = null;
		FoodComp foodComp = null;
		Vector2 pos = Vector2.zero;
		while (true)
		{
			if (!CanSpawn)
				yield return null;
			if (foodCount < MaxFood)
			{
				GO = null;
				foodComp = null;
				pos = Random.insideUnitCircle * SpawnRadius;
				if (StdUtils.Randomness.CoinFlip()) //mi random true false
				{
					GO = Instantiate(AlgaePrefab, transform.position + new Vector3(pos.x, 0, pos.y), AlgaePrefab.transform.rotation);
					foodComp = GO.GetComponent<FoodComp>();
					foodComp.tipo = TipoDeComida.Alga;
					Food.Add(foodComp);
				}
				else
				{
					GO = Instantiate(MeatPrefab, transform.position + new Vector3(pos.x, 0, pos.y), AlgaePrefab.transform.rotation);
					foodComp = GO.GetComponent<FoodComp>();
					foodComp.tipo = TipoDeComida.Carne;
					Food.Add(foodComp);

				}
				foodComp.ExtraOnDelete = delegate { foodCount--; Food.Remove(foodComp); };
				foodCount++;
			}
			else if (Food.Count > 0)
			{
				int i = 0;
				List<int> ints = new();
				foreach (var food in Food.ToList())
				{
					if(food  == null)
					{
						ints.Add(i);
						continue;
					}

					if (Vector3.Distance(transform.position, food.transform.position) < SpawnRadius)
					{
						 food .transform.position = Random.insideUnitCircle* SpawnRadius;

					}
					if (i%2 == 0)
						yield return null;
					i++;
				}

			}
			yield return new WaitForSeconds(SpawnWaitTime);
		}
	}
	IEnumerator N()
	{ 
		while (true)
		{
			var objs = FindObjectsByType<FoodComp>(FindObjectsSortMode.None);
			if (Food.Count != objs.Length)
			{
				Food.Clear();
				Food = objs.ToList();
			}
			yield return new WaitForSecondsRealtime(10);
		}
	}

	void OnEnable()
	{
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	void OnSceneUnloaded(Scene scene)
	{
		DestroyMsg = ("Se está descargando la escena." );
	}
	private void OnApplicationQuit()
	{
		DestroyMsg = ("Se está cerrando el juego");
	}

	internal void SpawnFood(Vector3 position, int v, Diets FoodType)
	{
		GameObject GO = null;
		FoodComp foodComp = null;
		Vector2 pos = new Vector2(position.x, position.z)+ Random.insideUnitCircle*2;
		foodComp = null;
		pos += Random.insideUnitCircle * 1.25f;
		for (int i = 0; i < v; i++)
		{
			if (FoodType == Diets.Herbivore) //mi random true false
			{
				GO = Instantiate(AlgaePrefab, transform.position + new Vector3(pos.x, 0, pos.y), AlgaePrefab.transform.rotation);
				foodComp = GO.GetComponent<FoodComp>();
				foodComp.tipo = TipoDeComida.Alga;
				Food.Add(foodComp);

			}
			else
			{
				GO = Instantiate(MeatPrefab, transform.position + new Vector3(pos.x, 0, pos.y), AlgaePrefab.transform.rotation);
				foodComp = GO.GetComponent<FoodComp>();
				foodComp.tipo = TipoDeComida.Carne;
				Food.Add(foodComp);

			}
			foodComp.ExtraOnDelete = delegate { foodCount--; Food.Remove(foodComp); };
			foodCount++;
		}

	}
}
