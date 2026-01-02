using SerializableTypes.Space;
using StandartUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Convertir el miembro en 'readonly'", Justification = "<pendiente>")]

public class SpaceshipCtr : MonoBehaviour
{
	public LayerMask raycastMask;
	public InputActionAsset inputActions;
	public Camera cam;
	public GameObject Galaxy;
	private InputAction CursAction;
	private InputAction Clcik;
	float TimeSinceLastClick;
	SpaceStageStar LastStarClicked;
	public GameObject CurrentStar;
	InputActionMap map;
	GameObject BigStar;
	public Material BholMat; //este material distorsion la luz :)
	Vector2 MapCamZOOM;
	float MapCamZoomSpeed;
	bool IsInSystem = false;
	GameObject AcrecionDisk;
	public List<string> SelectedStarChildren;
	private void OnEnable()
	{
		map = inputActions.FindActionMap("GC", true);
		map.Enable();

		CursAction = map.FindAction("PPos", true);
		Clcik = map.FindAction("Click", true);

	}
	bool startedTemp = false;
	int Neg1Count;
	// Update is called once per frame
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Declaración de variables alineada", Justification = "<pendiente>")]
	void Update()
	{
		if (!IsInSystem)
		{
			if (Clcik.WasPressedThisFrame())
			{

				// Un solo raycast por frame
				Vector2 cursorPos = CursAction.ReadValue<Vector2>();
				Ray ray = cam.ScreenPointToRay(cursorPos);
				RaycastHit hit;
				bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask);
				GameObject hitGO = hitSomething ? hit.collider.gameObject : null;

				if (hitSomething)
				{
					if (hitGO.TryGetComponent<SpaceStageStar>(out var star))
					{
						var GGG = FindAnyObjectByType<CameraOrbitController>();
						if (GGG != null)
						{
							if (GGG.Camera == cam)
							{
								var StarVIs = FindAnyObjectByType<StarVisualizer>();

								GGG.Target = hitGO.transform;
								SelectedStarChildren = StarVIs.galaxy.LookForStar(BodyID.FromString(star.ID).GetID()).Children;
							}
						}
						if (LastStarClicked == star)
						{
							if (TimeSinceLastClick < 2 && TimeSinceLastClick > 0.1)
							{
								var StarVIs = FindAnyObjectByType<StarVisualizer>();
								if (BigStar == null)
									BigStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
								BigStar.SetActive(true);
								BigStar.transform.localScale = Vector3.one * 5;
								var F = BigStar.GetComponent<MeshRenderer>();
								F.material = StarVIs.Mats[star.Type];
								if (star.Type == StarTypes.X)
								{
									AcrecionDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
									AcrecionDisk.GetComponent<MeshRenderer>().material = new(F.material);
									F.material = BholMat;

									AcrecionDisk.transform.localScale = new Vector3(10, 0.025f, 10);

								}
								MapCamZOOM = (GGG.ZoomLimits);
								MapCamZoomSpeed = GGG.ZoomSpeed;
								GGG.ZoomLimits = new(5.1f, 100);
								GGG.ZoomSpeed = 2.5f;
								GGG.Target = BigStar.transform;
								GGG.currentZoom = 99;
								Galaxy.transform.localScale = Vector3.zero;

								foreach (var PS in StarVIs.Particles.Values)
								{
									PS.transform.localScale = Vector3.zero;
								}
								int count = SelectedStarChildren.Count;
								int index = 0;
								List<string> ChildCopy = new List<string>();
								if (count > 0)
								{
									foreach (var Child in SelectedStarChildren)
									{
										ChildCopy.Add(Child);
									}
									foreach (var Child in ChildCopy)
									{
										float t = count == 1 ? 0.5f : (float)index / (count - 1);
										float xPos = Mathf.Lerp(10f, 80f, t);

										if (!Child.StartsWith('P'))
										{
											Debug.Log($"{Child} no es un planeta.");
											index++;
											continue;
										}

										ulong planetID = BodyID.FromString(Child).GetID();
										PlanetData Data = null;

										// --- 1) Intentar cargar en el sector actual ---
										if (!StarVIs.galaxy.TryToLookForPlanet(planetID, star.SectorPos, out Data))
										{
											// --- 2) Buscar en los 8 sectores vecinos ---
											bool found = false;

											for (int dx = -1; dx <= 1 && !found; dx++)
											{
												for (int dy = -1; dy <= 1 && !found; dy++)
												{
													Vector2Int neighbor = new Vector2Int(star.SectorPos.x + dx, star.SectorPos.y + dy);

													if (StarVIs.galaxy.TryToLookForPlanet(planetID, neighbor, out Data))
													{
														found = true;
														Debug.LogWarning($"{Child} no se cargó en este sector pero si en uno cercano");

													}
												}
											}
											// --- 3) Buscar en toda la galaxia
											if (!found)
											{




												if (StarVIs.galaxy.TryToLookForPlanet(planetID, out Data, out var  foundPos))
												{
													found = true;
												}
												Debug.LogWarning($"{Child} no se cargó en ningún sector cercano pero si uno lejano {foundPos}");

											}
											// --- 4) Si ni así se encontró → limpiamos el Child inválido ---
											if (!found)
											{
												Debug.LogWarning($"{Child} no se cargó en ningún sector. Eliminando del star.Children...");

												StarVIs.galaxy.UpdateStar(BodyID.FromString(star.name).GetID(), sd =>
												{
													sd.Children.Remove(Child);
												});

												index++;
												continue; // NO generar esfera
											}
										}

										if (Data.ParentID != star.ID)
										{
											Debug.Log("????????");
										}

										// --- Si llegamos aquí, ¡sí existe el planeta! ---
										GameObject game = GameObject.CreatePrimitive(PrimitiveType.Sphere);
										game.name = Data.Name;
										game.transform.position = new Vector3(xPos, 0, 0);

										Planets.Add(game);
										index++;
									}

								}
								IsInSystem = true;
							}
						}
						LastStarClicked = star;
					}
				}
				TimeSinceLastClick = 0;
			}
			if (LastStarClicked != null)
				CurrentStar = LastStarClicked.gameObject;
			TimeSinceLastClick += Time.deltaTime;
		}
		else
		{
			var GGG = FindAnyObjectByType<CameraOrbitController>();
			if (GGG != null)
			{

				if (!startedTemp && Neg1Count == 0)
				{
					if (GGG.ZoomOutput == -1)
					{
						Neg1Count = 1;
						startedTemp = true;
						TimeSinceLastClick = 0;
					}
				}
				else
				if (startedTemp)
				{
					if (GGG.ZoomOutput == -1)
					{
						Neg1Count++;


					}
					if (TimeSinceLastClick > 2)
					{
						if (Neg1Count > 3)
						{
							startedTemp = false;
							Neg1Count = 0;
							UnLoad(GGG);
						}
						else
						{
							startedTemp = false;
							Neg1Count = 0;
						}
					}
					TimeSinceLastClick += Time.deltaTime;
				}

			}
		}

	}
	List<GameObject> Planets = new List<GameObject>();
	void UnLoad(CameraOrbitController GGG)
	{
		BigStar.SetActive(false);
		GGG.ZoomSpeed = MapCamZoomSpeed;
		GGG.ZoomLimits = MapCamZOOM;
		GGG.Target = LastStarClicked.transform;
		GGG.currentZoom = 2.2f;
		Galaxy.transform.localScale = Vector3.one;
		IsInSystem = false;
		TimeSinceLastClick = 0f; // reiniciamos temporizador
		Neg1Count = 0;
		var StarVIs = FindAnyObjectByType<StarVisualizer>();
		foreach (var PS in StarVIs.Particles.Values)
		{
			PS.transform.localScale = Vector3.one;
		}
		foreach (var P in Planets)
		{
			Destroy(P);
		}
		if (AcrecionDisk != null)
			Destroy(AcrecionDisk);
	}
}
