using SerializableTypes.Space;
using StandartUtilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipCtr : MonoBehaviour
{
	[Header("Input")]
	public InputActionAsset inputActions;
	private InputAction cursorAction;
	private InputAction clickAction;
	private InputActionMap map;

	[Header("Scene")]
	public Camera cam;
	public GameObject Galaxy;
	public LayerMask raycastMask;

	[Header("Black Hole")]//si hay un agujero negro 
	public Material BholMat;
	public Material BholDiskMat;

	[Header("Star")]
	private SpaceStageStar lastStarClicked;
	public GameObject CurrentStar;
	public bool ENTER_ANYWAY = false;//debug
	public Mesh Sphere;
	private float timeSinceLastClick;
	private bool isInSystem;

	private GameObject bigStar;
	private GameObject acretionDisk;
	private readonly List<GameObject> planets = new();

	private Vector2 savedZoomLimits;
	private float savedZoomSpeed;

	private bool zoomExitStarted;
	private int zoomExitCount;

	public List<string> SelectedStarChildren;

	#region Unity

	private void OnEnable()
	{
		map = inputActions.FindActionMap("GC", true);// no, no se llama GC por Garbage Collection sino por que pense en el Nintendo GameCube
		map.Enable();

		cursorAction = map.FindAction("PPos", true);
		clickAction = map.FindAction("Click", true);
	}

	private void Update()
	{
		if (!isInSystem)
			HandleGalaxyInput();
		else
			HandleSystemExit();
	}

	#endregion

	#region Galaxy Logic

	private void HandleGalaxyInput()
	{
		timeSinceLastClick += Time.deltaTime;

		if (!clickAction.WasPressedThisFrame())
		{
			if (ENTER_ANYWAY)
				EnterStarSystem(lastStarClicked);
			return;
		}
		var star = RaycastStarUnderCursor();
		if (star == null)
			return;

		FocusCameraOnStar(star);

		if ((star == lastStarClicked && IsDoubleClick())|| ENTER_ANYWAY)
			EnterStarSystem(star);

		lastStarClicked = star;
		CurrentStar = star.gameObject;
		timeSinceLastClick = 0f;
	}

	private SpaceStageStar RaycastStarUnderCursor()
	{
		Ray ray = cam.ScreenPointToRay(cursorAction.ReadValue<Vector2>());
		return Physics.Raycast(ray, out var hit, Mathf.Infinity, raycastMask)
			? hit.collider.GetComponent<SpaceStageStar>()
			: null;
	}

	private bool IsDoubleClick()
		=> timeSinceLastClick is > 0.1f and < 2f;

	#endregion

	#region Camera / Star

	private void FocusCameraOnStar(SpaceStageStar star)
	{
		if (orbit ==  null)
		orbit = FindAnyObjectByType<CameraOrbitController>();
		var vis = FindAnyObjectByType<StarVisualizer>();

		if (orbit == null || orbit.Camera != cam)
			return;

		orbit.Target = star.transform;
		SelectedStarChildren =
			vis.galaxy.LookForStar(BodyID.FromString(star.ID).GetID()).Children;
	}

	private void EnterStarSystem(SpaceStageStar star)
	{
		if (orbit == null)
			orbit = FindAnyObjectByType<CameraOrbitController>();
		var vis = FindAnyObjectByType<StarVisualizer>();

		CreateBigStar(star, vis);
		SpawnPlanets(star, vis);

		SaveCameraSettings(orbit);
		SetupSystemCamera(orbit);

		

		ENTER_ANYWAY = false;
		isInSystem = true;
	}

	#endregion

	#region System Creation

	private void CreateBigStar(SpaceStageStar star, StarVisualizer vis)
	{
		if (bigStar == null)
			bigStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		bigStar.SetActive(true);
		bigStar.transform.localScale = Vector3.one * 5;
		bigStar.transform.position = Vector3.one * 99999f;

		var renderer = bigStar.GetComponent<MeshRenderer>();
		renderer.material = vis.Mats[star.Type];

		if (star.Type == StarTypes.X)
			CreateBlackHoleDisk(renderer);
	}

	private void CreateBlackHoleDisk(MeshRenderer starRenderer)
	{
		acretionDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		acretionDisk.transform.localScale = new Vector3(10, 0.025f, 10);
		acretionDisk.transform.position = Vector3.one * 99999f;
		acretionDisk.GetComponent<MeshRenderer>().material = (BholDiskMat);
		starRenderer.material = BholMat;
	}

	private void SpawnPlanets(SpaceStageStar star, StarVisualizer vis)
	{
		if (SelectedStarChildren.Count == 0)
			return;

		int index = 0;
		int count = SelectedStarChildren.Count;

		foreach (var child in new List<string>(SelectedStarChildren))
		{
			if (!child.StartsWith('P'))
			{
				index++;
				continue;
			}

			if (!TryGetPlanetData(child, star, vis, out var data))
			{
				//RemoveInvalidPlanet(child, star, vis);
				index++;
				continue;
			}

			float t = count == 1 ? 0.5f : (float)index / (count - 1);
			float xPos = Mathf.Lerp(10f, 80f, t);

			var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			planet.name = data.Name;
			planet.transform.position = Vector3.one * 99999f + new Vector3(xPos, 0, 0);

			planets.Add(planet);
			index++;
		}
	}

	#endregion

	#region Planet Lookup

	private bool TryGetPlanetData(
		string child,
		SpaceStageStar star,
		StarVisualizer vis,
		out PlanetData data)
	{
		ulong planetID = BodyID.FromString(child).GetID();

		if (vis.galaxy.TryToLookForPlanet(planetID, star.SectorPos, out data))
			return true;

		for (int dx = -1; dx <= 1; dx++)
			for (int dy = -1; dy <= 1; dy++)
				if (vis.galaxy.TryToLookForPlanet(
					planetID,
					star.SectorPos + new Vector2Int(dx, dy),
					out data))
					return true;

		return false;
	}

	private void RemoveInvalidPlanet(string child, SpaceStageStar star, StarVisualizer vis)
	{
		vis.galaxy.UpdateStar(
			BodyID.FromString(star.name).GetID(),
			sd => sd.Children.Remove(child)
		);
	}

	#endregion

	#region Exit System
	public CameraOrbitController orbit = null;
	private void HandleSystemExit()
	{

		if (orbit == null)
		 orbit = FindAnyObjectByType<CameraOrbitController>();
		if (orbit == null)
			return;

		if (!zoomExitStarted && orbit.ZoomOutput == -1)
		{
			zoomExitStarted = true;
			zoomExitCount = 1;
			timeSinceLastClick = 0;
			return;
		}

		if (!zoomExitStarted)
			return;

		if (orbit.ZoomOutput == -1)
			zoomExitCount++;

		timeSinceLastClick += Time.deltaTime;

		if (timeSinceLastClick > 2f && zoomExitCount > 3)
			UnloadSystem(orbit);
	}

	private void UnloadSystem(CameraOrbitController orbit)
	{
		bigStar.SetActive(false);

		orbit.ZoomSpeed = savedZoomSpeed;
		orbit.ZoomLimits = savedZoomLimits;
		orbit.Target = lastStarClicked.transform;
		orbit.currentZoom = 2.2f;

		Galaxy.transform.position = Vector3.zero;
		isInSystem = false;

		foreach (var p in planets)
			Destroy(p);

		planets.Clear();

		if (acretionDisk != null)
			Destroy(acretionDisk);

		zoomExitStarted = false;
		zoomExitCount = 0;
	}

	#endregion

	#region Camera Helpers

	private void SaveCameraSettings(CameraOrbitController orbit)
	{
		savedZoomLimits = orbit.ZoomLimits;
		savedZoomSpeed = orbit.ZoomSpeed;
	}

	private void SetupSystemCamera(CameraOrbitController orbit)
	{
		orbit.ZoomLimits = new Vector2(5.1f, 100);
		orbit.ZoomSpeed = 2.5f;
		orbit.Target = bigStar.transform;
		orbit.currentZoom = 99;
	}

	private void HideGalaxyParticles(StarVisualizer vis)
	{
		foreach (var ps in vis.Particles.Values)
			ps.transform.localScale = Vector3.zero;
	}

	#endregion
}
