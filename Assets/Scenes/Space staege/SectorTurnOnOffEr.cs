using System.Collections.Generic;
using UnityEngine;
public class SectorTurnOnOffEr : MonoBehaviour
{
	[Header("Opciones")]
	public float maxDistance = 100f; // distancia máxima a la cámara
	public float updateInterval = 0.1f; // tiempo entre verificaciones
	public StarVisualizer starVisualizer;
	private float timer = 0f;
	public List<GameObject> Sectors;

	void Update()
	{
		if (Sectors == null)
			return;
		if (Sectors.Count == 0)
			return;
		timer += Time.deltaTime;
		if (timer < updateInterval) return;
		timer = 0f;

		Camera cam = Camera.main;
		if (cam == null) return;
		if (starVisualizer == null)
			return;
		if (!starVisualizer.Done)
			return;
		Vector3 camPos = cam.transform.position;
		Vector3 camForward = cam.transform.forward;
		camPos.y = 0;

		foreach (var sector in Sectors)
		{
			var sectorpos = sector.transform.position;
			sectorpos.y = 0;
			Vector3 toSector = sectorpos - camPos;

			// --- distancia en XZ ---
			Vector2 flat = new Vector2(toSector.x, toSector.z);
			float distance = flat.magnitude;
			float angle = Vector3.Angle(camForward, toSector);
			
			// --- producto punto también en XZ ---
			Vector2 camF2 = new Vector2(camForward.x, camForward.z).normalized;
			Vector2 toSector2 = flat.normalized;

			bool inCone = angle <= (cam.fieldOfView / 2);

			bool shouldBeActive = distance <= maxDistance && inCone;

			if (sector.activeSelf != shouldBeActive)
				sector.SetActive(shouldBeActive);
		}

	}
}