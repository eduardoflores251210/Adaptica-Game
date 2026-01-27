using StandartUtilities; //basicamente como una libreria personal que tiene funciones utiles 
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
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
	}
	string DestroyMsg = "razon desconocida";
	private void OnDestroy()
	{
		StopAllCoroutines();
		if (DestroyMsg != ("Se está cerrando el juego") && DestroyMsg != ("Se está descargando la escena."))
			Debug.LogError("El generador se ha destruido de manera anomala con la siguiente razón citada:\n " + DestroyMsg);
		else 
			Debug.Log("El generador se ha destruido de manera normal con la siguiente razón citada:\n " + DestroyMsg);
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
					GO = Instantiate(AlgaePrefab, new Vector3(pos.x,0,pos.y), AlgaePrefab.transform.rotation);
					foodComp = GO.GetComponent<FoodComp>();
					foodComp.tipo = TipoDeComida.Alga;
				}
				else
				{
					GO = Instantiate(MeatPrefab, new Vector3(pos.x, 0, pos.y), AlgaePrefab.transform.rotation);
					foodComp = GO.GetComponent<FoodComp>();
					foodComp.tipo = TipoDeComida.Carne;
				}
				foodComp.ExtraOnDelete = delegate { foodCount--; };
				foodCount++;
			}
			yield return new WaitForSeconds(SpawnWaitTime);
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
}
