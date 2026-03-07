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

		// Si el click empieza sobre el gizmo
		if (clickAction.WasPressedThisFrame())
		{
			if (Physics.Raycast(ray, out hit))
			{
				if (hit.collider.gameObject == this.gameObject)
				{
					isDragging = true;
					dragStartDir = (hit.point - placer.selectedObject.transform.position).normalized;
					initialObjectRot = placer.selectedObject.transform.rotation;
					placer.CantMove = true;

				}

			}
		}

		// Mientras arrastramos
		if (clickAction.IsPressed() && isDragging)
		{
			Plane plane = new Plane(cam.transform.forward, placer.selectedObject.transform.position);
			if (plane.Raycast(ray, out float enter))
			{
				Vector3 hitPoint = ray.GetPoint(enter);
				Vector3 currentDir = (hitPoint - placer.selectedObject.transform.position).normalized;

				// calcular rotación relativa
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
}
