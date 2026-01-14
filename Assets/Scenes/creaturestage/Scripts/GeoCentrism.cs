using SerializableTypes.Space;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoCentrism : MonoBehaviour
{
	[Header("Config")]
	public bool OverrideSun;
	public Transform CustomSun;
	[Header("Lista de planetas")]
	public StarData Sool;
	public List<PlanetData> Planets;
	public string PlanetID;
	[HideInInspector]
	public List<Transform> planetas; // Todos los planetas
	[HideInInspector]
	public Transform sol;            // Sol
	[Tooltip("Índice del planeta que será el jugador (Tierra). Si es inválido, se selecciona aleatorio.")]
	public int indiceTierra = 0;

	[Header("Velocidades")]
	public float velocidadRotacionPlaneta = 10f;
	public float velocidadOrbitaMin = 10f;
	public float velocidadOrbitaMax = 30f;
	Dictionary<Transform, float> radiosOrbitales = new();
	Dictionary<Transform, float> angulos = new();
	private Transform tierra; // El planeta central

	void Start()
	{
		CrossScenePackageSender Cartero = CrossScenePackageSender.Instance;
		if (Cartero != null)
		{
			if(Cartero.IsThereAnyTypedMailForHim<StarData>("SUN", out var Sun))
			{
				Sool = Sun[0].Contents;
			}
			if (Cartero.IsThereAnyTypedMailForHim<List<PlanetData>>("Planets", out var mail))
			{
				Planets = mail[0].Contents;
			}
			if (Cartero.IsThereAnyTypedMailForHim<string>("Parent", out var MainPlanetID))
			{
				PlanetID = MainPlanetID[0].Contents;
				if (Planets!= null)
				{
					PlanetData Earth = null;
					int i = 0;
					int id = 0;
					foreach (PlanetData Planet in Planets)
					{
						if (Planet.id == PlanetID)
						{
							Earth = Planet;
							id = i;
						}
						i++;
					}
					if (Earth != null)
					{
						indiceTierra = id;
					}else
					{
						indiceTierra = int.MinValue;
					}
				}
			}


		}else
		{
			Debug.LogError("¡NO hay Cartero!");
		}
		if (Sool != null)
		{
			var SolGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			SolGo.name = Sool.Name;
			sol = SolGo.transform;
		}
		if (Planets != null)
		{
			if (planetas != null)
				planetas.Clear();
			else
				planetas = new();
			foreach (PlanetData Planet in Planets)
			{
				var PGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				PGO.name = Planet.Name;
				planetas.Add(PGO.transform);
			}
		}
		sol = OverrideSun? CustomSun : sol;
		// Validar lista de planetas
		if (planetas == null || planetas.Count == 0)
		{
			Debug.LogError("¡No hay planetas en la lista!");
			return;
		}

		// Validar índiceTierra
		if (indiceTierra < 0 || indiceTierra >= planetas.Count)
		{
			indiceTierra = Random.Range(0, planetas.Count);
			Debug.Log("Índice de Tierra inválido. Se eligió aleatoriamente el planeta " + indiceTierra);
		}

		tierra = planetas[indiceTierra];

		// Verificar que haya un Sol asignado
		if (sol == null)
		{
			Debug.LogError("¡No se ha asignado un Sol!");
		}


		float distanciaBase = 50f;
		float separacion = 30f;

		for (int i = 0; i < planetas.Count; i++)
		{
			Transform p = planetas[i];
			if (p == tierra) continue;

			radiosOrbitales[p] = distanciaBase + i * separacion;
			angulos[p] = Random.Range(0f, 360f);
		}

	}

	void Update()
	{
		// El planeta central NUNCA se mueve
		tierra.position = Vector3.zero;
		tierra.Rotate(Vector3.up, velocidadRotacionPlaneta * Time.deltaTime);

		// El Sol tiene una órbita APARENTE alrededor de la Tierra
		float sunAngle = Time.time * velocidadOrbitaMin;
		Vector3 solPosRelativa =
			new Vector3(Mathf.Cos(sunAngle), 0, Mathf.Sin(sunAngle)) * 100f;

		sol.position = solPosRelativa;

		// Los demás planetas orbitan AL SOL, pero calculado en espacio relativo
		foreach (var planeta in planetas)
		{
			if (planeta == tierra) continue;

			angulos[planeta] += Time.deltaTime *
								Mathf.Lerp(velocidadOrbitaMin, velocidadOrbitaMax, 0.5f);

			float a = angulos[planeta] * Mathf.Deg2Rad;
			float r = radiosOrbitales[planeta];

			Vector3 posicionHeliocentrica =
				solPosRelativa +
				new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * r;

			// PASO CLAVE: mover en sistema geocéntrico
			planeta.position = posicionHeliocentrica;
		}
	}

}
