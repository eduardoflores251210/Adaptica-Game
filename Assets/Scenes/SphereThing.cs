using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereThing : MonoBehaviour
{
	public ObjectPlacer placer; // referencia al ObjectPlacer
	public Camera cam;
	public float rotationSpeed = 100f;

	private bool isDragging = false;
	private Vector3 dragStartDir;
	private Quaternion initialObjectRot;
	private Vector3 sphereCenter;
	private float sphereRadius;
	private Vector3 dragStartPoint;

	private InputAction cursorAction;
	private InputAction clickAction;

	void Start()
	{
		var map = placer.inputActions.FindActionMap("GC", true);
		cursorAction = map.FindAction("PPos", true);
		clickAction = map.FindAction("Click", true);
	}

	void Update()
	{
		if (placer.selectedObject == null) return;

		Vector2 cursorPos = cursorAction.ReadValue<Vector2>();
		Ray ray = cam.ScreenPointToRay(cursorPos);
		RaycastHit hit;

		// Si el click empieza sobre el gizmo (este GameObject)
		if (clickAction.WasPressedThisFrame())
		{
			if (Physics.Raycast(ray, out hit))
			{
				if (hit.collider != null && hit.collider.gameObject == this.gameObject)
				{
					isDragging = true;
					sphereCenter = this.transform.position;
					dragStartPoint = hit.point;
					sphereRadius = (hit.point - sphereCenter).magnitude;
					// dirección desde el centro del objeto hacia el punto pulsado del gizmo
					dragStartDir = (dragStartPoint - placer.selectedObject.transform.position).normalized;
					initialObjectRot = placer.selectedObject.transform.rotation;
					placer.CantMove = true;
				}
			}
		}

		// Mientras arrastramos
		if (clickAction.IsPressed() && isDragging)
		{
			Vector3 currentDir;
			// Intentar intersecar rayo con la esfera del gizmo (centro = posicion del gizmo)
			if (TryRaySphereIntersection(ray, sphereCenter, sphereRadius, out Vector3 sphereHit))
			{
				currentDir = (sphereHit - placer.selectedObject.transform.position).normalized;
			}
			else
			{
				// Fallback: proyectar sobre un plano pasando por el centro del gizmo, perpendicular a la cámara
				Plane plane = new Plane(cam.transform.forward, sphereCenter);
				if (plane.Raycast(ray, out float enter))
				{
					Vector3 hitPoint = ray.GetPoint(enter);
					currentDir = (hitPoint - placer.selectedObject.transform.position).normalized;
				}
				else
				{
					// último recurso: dirección desde objeto hacia la cámara
					currentDir = (cam.transform.position - placer.selectedObject.transform.position).normalized;
				}
			}

			// Evitar ruido muy pequeño
			if (Vector3.Angle(dragStartDir, currentDir) > 0.05f)
			{
				Quaternion deltaRot = Quaternion.FromToRotation(dragStartDir, currentDir);
				placer.selectedObject.transform.rotation = deltaRot * initialObjectRot;
			}
		}

		// Al soltar click
		if (clickAction.WasReleasedThisFrame() && isDragging)
		{
			isDragging = false;
			placer.CantMove = false;
		}
	}

	private bool TryRaySphereIntersection(Ray ray, Vector3 center, float radius, out Vector3 point)
	{
		point = Vector3.zero;
		Vector3 oc = ray.origin - center;
		float a = Vector3.Dot(ray.direction, ray.direction);
		float b = 2f * Vector3.Dot(oc, ray.direction);
		float c = Vector3.Dot(oc, oc) - radius * radius;
		float discriminant = b * b - 4f * a * c;
		if (discriminant < 0f) return false;

		float sqrt = Mathf.Sqrt(discriminant);
		float t1 = (-b - sqrt) / (2f * a);
		float t2 = (-b + sqrt) / (2f * a);

		// Preferir la intersección más cercana adelante del rayo
		float t = float.MaxValue;
		if (t1 >= 0f) t = Mathf.Min(t, t1);
		if (t2 >= 0f) t = Mathf.Min(t, t2);

		// Si ambas son negativas (origen dentro de la esfera), usar la mayor (salida)
		if (t == float.MaxValue)
		{
			if (t1 > t2) t = t1; else t = t2;
			if (t < 0f) return false;
		}

		point = ray.GetPoint(t);
		return true;
	}
}
