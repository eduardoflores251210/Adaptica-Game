using UnityEngine;
using System.Collections.Generic;
using ActualUtils;

public class PlantStemGenerator : MonoBehaviour
{
	[Header("Stem Settings")]
	public int segmentCount = 10;
	public Vector3 origin = Vector3.zero;
	[Tooltip("Distancia fija entre segmentos, independiente del radio")]
	public float segmentSpacing = 0.7f;

	[Header("Radius Settings")]
	public float startRadius = 10f;
	public float endRadius = 1f;

	private int lastSegmentCount = -1;
	private Vector3 lastOrigin;
	private Vector2 lastRadius;

	private List<GameObject> spawnedBalls = new List<GameObject>();
	private List<Metaball> spawnedBallsComp = new List<Metaball>();
	public float OldSegmentSpacing = 0f;
	private void Start()
	{
		PlayerManager.RegisterEditor(this, SerializableTypes.Editors.Plant);
	}

	private void Update()
	{
		Vector2 currentRadius = new Vector2(startRadius, endRadius);
		if (segmentCount < 0)
		{
			segmentCount = 1;
		}
		// Regenerar solo si cambia segmentCount o origin
		if (segmentCount != lastSegmentCount || origin != lastOrigin|| segmentSpacing != OldSegmentSpacing)
		{
			GenerateStem();
			lastSegmentCount = segmentCount;
			lastOrigin = origin;
		}
		// Actualizar radios si cambian
		else if (currentRadius != lastRadius)
		{
			UpdateRadii();
		}

		lastRadius = currentRadius;
		OldSegmentSpacing = segmentSpacing;
	}

	private void GenerateStem()
	{
		// Limpiar metaballs anteriores
		foreach (var ball in spawnedBalls)
			if (ball != null) Destroy(ball);

		spawnedBalls.Clear();
		spawnedBallsComp.Clear();

		if (segmentCount <= 0) return;

		for (int i = 0; i < segmentCount; i++)
		{
			float t = (segmentCount == 1) ? 0 : (float)i / (segmentCount - 1);
			float radius = Mathf.Lerp(startRadius, endRadius, t);

			// Separación fija entre segmentos, no depende del radio
			Vector3 pos = origin + new Vector3(0, i * segmentSpacing, 0);

			GameObject ballGO = new GameObject($"Metaball_{i}");
			ballGO.transform.position = pos;
			if (i == 0)
			{
				ballGO.AddComponent<SphereCollider>();
			}

			Metaball m = ballGO.AddComponent<Metaball>();
			m.Radius = radius;

			spawnedBalls.Add(ballGO);
			spawnedBallsComp.Add(m);
		}

		//Debug.Log($"🌱 Tallos generados: {segmentCount}");
	}

	private void UpdateRadii()
	{
		if (spawnedBallsComp.Count == 0) return;

		for (int i = 0; i < spawnedBallsComp.Count; i++)
		{
			float t = (spawnedBallsComp.Count == 1) ? 0 : (float)i / (spawnedBallsComp.Count - 1);
			float radius = Mathf.Lerp(startRadius, endRadius, t);

			var metaball = spawnedBallsComp[i];
			if (metaball != null)
			{
				metaball.Radius = radius;
			}
		}

		//Debug.Log("🔧 Radios actualizados sin regenerar tallos.");
	}
	private void OnDestroy()
	{
		if (PlayerManager.Player == this)
		PlayerManager.UnregisterEditor();
	}
}
