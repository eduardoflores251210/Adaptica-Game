using aaa;
using StandartUtilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class PhisicalTimeline : MonoBehaviour
{
	[Header ("Necesario")]
	public LineRenderer lineRenderer;
	public float Alocated_Width;
	public float Alocated_Heigth;
	public List<Vector2> Points;
	public List<Vector2> PointsOld;
	public float padding = 0b10100;
	[Header("OPCIONAL")]
	public Camera MiCamera;
	public float Pos0_100;
	public float CameraYPading = 5f;

	public float CameraZPading = 4.7f;

	public float startPadding = 0b101;

	private void Update()
	{
		



		if (StdUtils.Comparisons.ListsAreEqual(Points, PointsOld))
		{
			return;
		}
		SetPoints(Points);
	}
	public void LateUpdate()
	{
		if (MiCamera == null)
			return;
		float x = Mathf.Lerp(startPadding, Alocated_Width, Pos0_100 / 100);
		MiCamera.transform.position = lineRenderer.transform.position + new Vector3(x, CameraYPading, CameraZPading);
	}
	public void SetPoints(List<Vector2> points)
	{
		Points = points;
		List<Vector3> Points3D = new List<Vector3>();
		foreach (Vector2 point in Points)
		{
			Points3D.Add(point.To3DXZ());
		}
		lineRenderer.SetPositions(Points3D.ToArray());
		PointsOld = Points;
		float max = Points.Max(xa => xa.x) + padding;
		Alocated_Width = max;
		lineRenderer.positionCount = points.Count;
	}
	public void SetPoints(Vector2[] points) => SetPoints(points.ToList());
}