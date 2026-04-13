using SerializableTypes.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[Obsolete("ESTO SE REEMPLAZARA CON UN VIDEO")]
public class Adysgsdfhjfdhdfdf : MonoBehaviour //aka SolarSys_Born_Cinematic
{
	public StarData Stardata; // para generar los planetas
	public ParticleSystem ProtoPlanetaryDisk;
	public ParticleSystem SunBeam; // los jets de la protoestrella
	public Material starMat; // para cambiar el color de la estrella en base a su temperatura
	public SphereCollider MyCollider; // para simular el viento solar expulsando polvo y gas
	public GameObject AcretionDisk;
	public Material BlackHoleMat;
	public bool IsWaiting;
	public Canvas FastForward; // canvas para simular avance rápido del tiempo
	public Canvas SuperFastForward;
	public Material planetMaterial; // material simple para los planetas (puedes asignar uno)
	public Material GasMaterial; // material para los planetas gaseosos
	public Material SpNovaMaterial; // material para la super nova
	public string GamePlanetID; //id de lplaneta a donde se movera la camara
	public UnityEngine.Transform PlanetParent;
	private List<GameObject> planetObjects = new List<GameObject>();
	public float GiantSize = 10f;
	public UnityEngine.Transform SuperNova;
	public Camera mainCamera; // Asigna la cámara principal en el Inspector
	public float cameraMoveDuration = 3f; // Duración del movimiento de cámara
	private float growDuration = 10f; // duración para crecer collider y escala
	private float waitTime = 30f; // tiempo de espera antes de empezar a crecer
	private float waitTimeB = 5f; // tiempo de crecimiento de la gigante roja
	private float waitTimeC = 3f; // tiempo de espera antes de la supernova 
	public FadeToBlck blck;
	public AsteroidLauncher launcher;
	private bool inited = false;
	private bool Part2Enable = false;
	[SerializeField]
	private SpaceUtils.SystemObjectsBuilder.InstantiatedSystemData systemData;
	private Dictionary<string, GameObject> Planets_ID = new Dictionary<string, GameObject>();
	private List<PlanetData> planets_Data = new List<PlanetData>();
	void Start()
	{
		if (blck != null)
			blck.StartFadeIn();
		SuperNova.gameObject.SetActive(false);
		transform.localScale = Vector3.one;
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		mainCamera.transform.position = new(0, 23, -50);
		IsWaiting = true;
		if (FastForward != null) FastForward.gameObject.SetActive(false);
		if (SuperNova != null) SuperNova.localScale = Vector3.zero;
		if (AcretionDisk != null) AcretionDisk.SetActive(false);


		if (systemLoader != null)
		{
			systemLoader.BasMat = starMat;
			systemLoader.OpaceMat = planetMaterial;
			systemLoader.GasMaterial = GasMaterial;
			systemLoader.BholMat = BlackHoleMat;
		}

	}
	PlanetData ParentPlanetData;
	bool IsRougueSon = false;

	private void Update()
	{
		if (IsWaiting)
		{
			// Esperar hasta tener datos válidos
			if (!IsRougueSon)
			IsWaiting = Stardata == null || Stardata.Children == null || Stardata.Children.Count == 0;
			if (IsRougueSon)
				IsWaiting = ParentPlanetData == null || ParentPlanetData.Children == null || ParentPlanetData.Children.Count == 0;
			var Mailman = CrossScenePackageSender.Instance;
			if (Mailman != null)
			{
				// Revisar paquetes tipados
				if (Mailman.IsThereAnyTypedMailForMe<StarData>(gameObject, out var starMail))
				{
					Stardata = starMail[0].Contents;
					Mailman.DeleteMyPackage(starMail[0]);
				}
				if (Mailman.IsThereAnyTypedMailForMe<PlanetData>(gameObject, out var PlanetMail))
				{
					ParentPlanetData=PlanetMail[0].Contents;
					Mailman.DeleteMyPackage(PlanetMail[0]);
				}

				if (Mailman.IsThereAnyTypedMailForMe<String>(gameObject, out var idMail))
				{
					GamePlanetID = idMail[0].Contents;
					Mailman.DeleteMyPackage(idMail[0]);
				}
				if (Mailman.IsThereAnyTypedMailForMe<bool>(gameObject, out var SwirchMail))
				{
					if (SwirchMail[0].Contents)
					{
						IsRougueSon = true;

					}
					Mailman.DeleteMyPackage(SwirchMail[0]);

				}
			}
		}
		else if (!inited && !IsRougueSon)
		{
			systemLoader.SolarSystemID = Stardata.id; 
			systemLoader.a(transform); // lo instancia como hijo
			UpdateStarColor();
			StartCoroutine(WaitAndGrowStar());
			inited = true;
			systemData = systemLoader.systemData;

		}
		else if (!inited && IsRougueSon)
		{
			systemLoader.SolarSystemID = ParentPlanetData.id; 
			systemLoader.a(transform); // lo instancia como hijo
			UpdatePlanetColor();
			StartCoroutine(WaitAndGrowPlanet());
			inited = true;
			systemData = systemLoader.systemData;


		}

		if (systemLoader.Done)
		{
			systemData = systemLoader.systemData;
			if (systemLoader.systemData.Datas != null)
			{
				Planets_ID = new Dictionary<string, GameObject>();
				if (systemData.ObjAndIDS != null)
				foreach (var kvp in systemLoader.systemData.ObjAndIDS)
				{
					string id = kvp.Value;
					GameObject obj = kvp.Key;

					if (!Planets_ID.ContainsKey(id)) // evitar duplicados
					{
						Planets_ID.Add(id, obj);
					}
					else
					{
						Debug.LogWarning($"ID duplicado detectado: {id}");
					}
				}
			}
		}
	}
	GameObject GameMePlanet;
	PlanetData GPdata = null;
	GalaxyData g;
	public LoadASYstem systemLoader;


	IEnumerator WaitAndGrowPlanet()
	{
		yield return new WaitForSeconds(waitTime);

		UpdatePlanetColor();

		float elapsed = 0f;

		float colliderStart = 0.5f;
		float colliderEnd = 17f;

		Vector3 scaleStart = Vector3.one;
		Vector3 scaleEnd = Vector3.one * 3.9f;
		float currentTemp = starMat.GetFloat("_Temp_K");



		float ti;
		var emission = SunBeam?.emission;
		float initialRate = emission.HasValue ? emission.Value.rateOverTime.constant : 0f;

		while (elapsed < growDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / growDuration;

			if (MyCollider != null)
				MyCollider.radius = Mathf.Lerp(colliderStart, colliderEnd, t);

			transform.localScale = Vector3.Lerp(scaleStart, scaleEnd, t);

			if (emission.HasValue)
			{
				var e = SunBeam.emission;
				e.rateOverTime = Mathf.Lerp(initialRate, 0f, t);
			}
			
			ti = Mathf.Lerp(currentTemp, 0f, t);
			starMat.SetFloat("_Temp_K", ti);

			yield return null;
		}

		if (MyCollider != null) MyCollider.radius = colliderEnd;
		transform.localScale = scaleEnd;

		if (emission.HasValue)
		{
			var e = SunBeam.emission;
			var ee = ProtoPlanetaryDisk.emission;
			e.rateOverTime = 0f;
			ee.rateOverTime = 0f;// preparacion para implementar la parte 3 en la cual BOMBARDEO DE ASTEROIDES
		}
		GetComponent<Renderer>().material = planetMaterial;

		yield return StartCoroutine(MoveCameraToPlanet());
		
	}
	IEnumerator WaitAndGrowStar()
	{
		yield return new WaitForSeconds(waitTime);

		UpdateStarColor();

		float elapsed = 0f;

		float colliderStart = 0.5f;
		float colliderEnd = 17f;

		Vector3 scaleStart = Vector3.one;
		Vector3 scaleEnd = Vector3.one * 3.9f;

		var emission = SunBeam?.emission;
		float initialRate = emission.HasValue ? emission.Value.rateOverTime.constant : 0f;

		while (elapsed < growDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / growDuration;

			if (MyCollider != null)
				MyCollider.radius = Mathf.Lerp(colliderStart, colliderEnd, t);

			transform.localScale = Vector3.Lerp(scaleStart, scaleEnd, t);

			if (emission.HasValue)
			{
				var e = SunBeam.emission;
				e.rateOverTime = Mathf.Lerp(initialRate, 0f, t);
			}

			yield return null;
		}

		if (MyCollider != null) MyCollider.radius = colliderEnd;
		transform.localScale = scaleEnd;

		if (emission.HasValue)
		{
			var e = SunBeam.emission;
			var ee = ProtoPlanetaryDisk.emission;
			e.rateOverTime = 0f;
			ee.rateOverTime = 0f;// preparacion para implementar la parte 3 en la cual
		}
		
		if (Part2Enable)
		{
			yield return StartCoroutine(HandlePart2Sequence(scaleEnd));
		} else
		{
			yield return StartCoroutine(MoveCameraToPlanet());
		}
	}

	private IEnumerator HandlePart2Sequence(Vector3 scaleEnd)
	{
		float currentTemp = starMat.GetFloat("_Temp_K");
		float targetTempKelvin = StarData.Temperatures[StarTypes.M];

		Vector2 v2currentTemp = new Vector2(currentTemp, 0);
		Vector2 v2targetTempKelvin = new Vector2(targetTempKelvin, 0);
		Vector2 tt = Vector2.zero;

		Vector3 GiantSizeVector3 = Vector3.one * GiantSize;

		if (FastForward != null) FastForward.gameObject.SetActive(true);
		yield return new WaitForSecondsRealtime(20f);
		if (FastForward != null) FastForward.gameObject.SetActive(false);

		float elapsed = 0f;
		while (elapsed < waitTimeB)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / waitTimeB;

			transform.localScale = Vector3.Lerp(scaleEnd, GiantSizeVector3, t);
			tt = Vector2.Lerp(v2currentTemp, v2targetTempKelvin, t);
			starMat.SetFloat("_Temp_K", tt.x);

			yield return null;
		}

		yield return new WaitForSeconds(waitTimeC);

		elapsed = 0f;
		targetTempKelvin = Stardata.type switch
		{
			StarTypes.X => 0,
			StarTypes.EB or StarTypes.NS => StarData.Temperatures[Stardata.type],
			_ => StarData.Temperatures[StarTypes.EB]
		};

		currentTemp = starMat.GetFloat("_Temp_K");
		v2currentTemp = new Vector2(currentTemp, 0);
		v2targetTempKelvin = new Vector2(targetTempKelvin, 0);
		SuperNova.gameObject.SetActive(true);
		while (elapsed < waitTimeB)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / waitTimeB;

			transform.localScale = Vector3.Lerp(GiantSizeVector3, scaleEnd, t);
			tt = Vector2.Lerp(v2currentTemp, v2targetTempKelvin, t);
			starMat.SetFloat("_Temp_K", tt.x);
			//999999
			if (SuperNova != null)
			{
				SuperNova.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 99, t);
				if(SpNovaMaterial != null)
				{
					SpNovaMaterial.SetFloat("_Temp_K", Lerp(35000, 0,t));
				}
			}
			yield return null;
		}

		if (Stardata.type != StarTypes.EN)
			starMat.SetFloat("_Temp_K", targetTempKelvin);

		if (Stardata.type == StarTypes.EN)
		{
			var itemp = new Vector2(targetTempKelvin, 0);
			Vector2 temp;
			if (SuperFastForward != null) SuperFastForward.gameObject.SetActive(true);

			elapsed = 0f;
			while (elapsed < waitTimeB)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / waitTimeB;

				temp = Vector2.Lerp(itemp, Vector2.zero, t);
				starMat.SetFloat("_Temp_K", temp.x);

				yield return null;
			}

			if (SuperFastForward != null) SuperFastForward.gameObject.SetActive(false);
		}
		else if (Stardata.type == StarTypes.X)
		{
			var r = GetComponent<MeshRenderer>();
			if (r != null && BlackHoleMat != null)
				r.material = BlackHoleMat;

			if (AcretionDisk != null)
				AcretionDisk.SetActive(true);
		}
		SuperNova.gameObject.SetActive(false);
		yield return StartCoroutine(MoveCameraToPlanet());
	}

	void UpdatePlanetColor()
	{
		if (ParentPlanetData == null || starMat == null) return;

		if (!StarData.Temperatures.TryGetValue(StarTypes.G, out float tempKelvin))
		{
			Debug.LogWarning($"No temperature found for star type {Stardata.type}");
			return;
		}


		starMat.SetFloat("_Temp_K", tempKelvin);
		Part2Enable = false;


	}
	void UpdateStarColor()
	{
		if (Stardata == null || starMat == null) return;

		if (!StarData.Temperatures.TryGetValue(Stardata.type, out float tempKelvin))
		{
			Debug.LogWarning($"No temperature found for star type {Stardata.type}");
			return;
		}

		if (Stardata.type != StarTypes.X && Stardata.type != StarTypes.EB && Stardata.type != StarTypes.EN && Stardata.type != StarTypes.NS)
		{
			starMat.SetFloat("_Temp_K", tempKelvin);
			Part2Enable = false;
		}
		else
		{
			float val = Stardata.type switch
			{
				StarTypes.X or StarTypes.NS => StarData.Temperatures[StarTypes.O], // neutrones y agujero negro
				StarTypes.EB or StarTypes.EN => StarData.Temperatures[StarTypes.G], // enana blanca y negra
				_ => StarData.Temperatures[StarTypes.K]
			};
			starMat.SetFloat("_Temp_K", val);
			Part2Enable = true;
		}
	}
	IEnumerator MoveCameraToPlanet()
	{
		GameObject targetPlanet = Planets_ID[GamePlanetID];

		Vector3 startPos = mainCamera.transform.position;
		Quaternion startRot = mainCamera.transform.rotation;

		// Definir un offset para que la cámara no quede justo dentro del planeta
		Vector3 offset = new Vector3(0, 2, -5);
		Vector3 targetPos = targetPlanet.transform.position + offset;

		// Mira hacia el planeta
		Quaternion targetRot = Quaternion.LookRotation(targetPlanet.transform.position - targetPos);

		float elapsed = 0f;
		while (elapsed < cameraMoveDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / cameraMoveDuration;

			mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
			mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
			mainCamera.transform.position.GetHashCode();
			yield return null;
		}

		mainCamera.transform.position = targetPos;
		mainCamera.transform.rotation = targetRot;
		MyCollider.radius = 0.5f;
		launcher.Target = targetPlanet;
		launcher.IsWaiting = false;
		yield return new WaitForSeconds(30);
		launcher.IsWaiting = true;
		yield return new WaitForSeconds(10);
		blck.StartFadeOut();
		yield return new WaitForSeconds(1.5f);
		CrossScenePackageSender.Instance.SendTypedPackage("BEginCInematic", "CellSaver", true, new string[2] { nameof(Boolean), "LodStg" });
		CrossScenePackageSender.Instance.SendTypedPackage("BEginCInematic", "CellSaver", GPdata, new string[1] { nameof(PlanetData) });
		yield return StartCoroutine(LoadScene(1)); 

	}
	public IEnumerator LoadScene(int id)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(id);
		yield break;
	}
	public static float Lerp(float a, float b, float t) //NO SABIA LA EXISTENCIA DE Mathf.Lerp OK
	{
		t = Mathf.Clamp01(t);
		return a + (b - a) * t;
	}
}

public static class MicrobeUnitConversion
{
	public static float MapInternalToMeter(float val)
	{
		return SizeMapper.InverseLogMap(val);
	}
	public static float Map_Meter_ToInternal(float val)
	{
		return SizeMapper.LogMap(val);
	}
}

public static class SizeMapper
{
	/// <summary>
	/// Devuelve el tamaño real x correspondiente al valor normalizado y
	/// usando la inversa de la transformación logarítmica:
	/// x = xmin * (xmax/xmin)^y
	/// </summary>
	/// <param name="y">Valor normalizado en [0,1]</param>
	/// <param name="xmin">Tamaño mínimo (debe ser > 0)</param>
	/// <param name="xmax">Tamaño máximo (debe ser > xmin)</param>
	/// <returns>Tamaño real (misma unidad que xmin/xmax)</returns>
	public static float InverseLogMap(float y, float xmin = 0.0001f, float xmax = 0.10f)
	{
		if (xmin <= 0f) throw new ArgumentException("xmin must be > 0", nameof(xmin));
		if (xmax <= xmin) throw new ArgumentException("xmax must be > xmin", nameof(xmax));

		// Clamp de y por seguridad
		y = Mathf.Clamp01(y);

		float ratio = xmax / xmin;
		return xmin * Mathf.Pow(ratio, y);
	}

	// Variante con double si necesitas más precisión (no común en Unity runtime)
	public static double InverseLogMapDouble(double y, double xmin = 0.0001d, double xmax = 0.10d)
	{
		if (xmin <= 0.0) throw new ArgumentException("xmin must be > 0", nameof(xmin));
		if (xmax <= xmin) throw new ArgumentException("xmax must be > xmin", nameof(xmax));

		y = Math.Max(0.0, Math.Min(1.0, y));
		double ratio = xmax / xmin;
		return xmin * Math.Pow(ratio, y);
	}
	// Función inversa de InverseLogMap: de x a y normalizado
	public static float LogMap(float x, float xmin = 0.0001f, float xmax = 0.10f)
	{
		if (xmin <= 0f) throw new ArgumentException("xmin must be > 0", nameof(xmin));
		if (xmax <= xmin) throw new ArgumentException("xmax must be > xmin", nameof(xmax));

		// Clamp de x por seguridad
		x = Mathf.Clamp(x, xmin, xmax);

		float ratio = xmax / xmin;
		return Mathf.Log(x / xmin) / Mathf.Log(ratio);
	}

	// Variante con double para mayor precisión
	public static double LogMapDouble(double x, double xmin = 0.0001d, double xmax = 0.10d)
	{
		if (xmin <= 0.0) throw new ArgumentException("xmin must be > 0", nameof(xmin));
		if (xmax <= xmin) throw new ArgumentException("xmax must be > xmin", nameof(xmax));

		x = Math.Max(xmin, Math.Min(xmax, x));
		double ratio = xmax / xmin;
		return Math.Log(x / xmin) / Math.Log(ratio);
	}
}
