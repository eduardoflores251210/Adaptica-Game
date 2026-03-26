using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using StandartUtilities.Extentions;

public class ObjectPlacer : MonoBehaviour
{
	[Header("Setup")]
	public Camera cam;
	public LayerMask raycastMask;
	public bool CantMove = false;

	[Header("Input")]
	public InputActionAsset inputActions;
	private InputActionMap map;
	private InputAction cursorAction;
	private InputAction ClickAction;

	[Header("State")]
	public GameObject selectedObject = null;
	public GameObject Creature; // target de snap
	public bool isMoving = false;
	public List<GameObject> objectList = new List<GameObject>();

	// click timing
	public float pressTime = 0f;
	private const float longClickThreshold = 0.3f;

	// internals
	private GameObject pressStartedOver = null;
	[SerializeField]
	private bool pressCanCount = false;
	private bool listVisible = false;
	private bool lastCantMove = false;

	private enum State { Idle, Selected, Dragging }
	private State state = State.Idle;

	// ------------------------------------------------------------------
	void OnEnable()
	{
		map = inputActions.FindActionMap("GC", true);
		map.Enable();

		cursorAction = map.FindAction("PPos", true);
		ClickAction = map.FindAction("Click", true);

		cursorAction.Enable();
		ClickAction.Enable();

		StartCoroutine(SanityCheckCoroutine());
	}

	void OnDisable()
	{
		CancelCurrentPress();
		cursorAction?.Disable();
		ClickAction?.Disable();
		if (map != null) map.Disable();
		StopAllCoroutines();
	}

	void OnApplicationFocus(bool hasFocus) { if (!hasFocus) CancelCurrentPress(); }
	void OnApplicationPause(bool paused) { if (paused) CancelCurrentPress(); }

	[SerializeField]
	bool AP = false;
	// ------------------------------------------------------------------
	void Update()
	{
		
		EnsureActionsEnabled();
		AP = ClickAction.IsPressed();

		// Si CantMove cambió, limpia estados
		if (CantMove != lastCantMove)
		{
			lastCantMove = CantMove;
			if (CantMove) CancelCurrentPress();
		}

		if (CantMove)
		{
			// fallback
			if (!ClickAction.IsPressed()) pressTime = 0f;
			return;
		}

		// Un solo raycast por frame
		Vector2 cursorPos = cursorAction.ReadValue<Vector2>();
		Ray ray = cam.ScreenPointToRay(cursorPos);
		RaycastHit hit;
		bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask);
		GameObject hitGO = hitSomething ? hit.collider.gameObject : null;

		// limpiezas rápidas si algún GameObject fue destruido
		if (pressStartedOver != null && pressStartedOver == null) { pressStartedOver = null; pressCanCount = false; } // Unity null check
		if (selectedObject != null && selectedObject == null) { selectedObject = null; isMoving = false; state = State.Idle; }

		// ignora inicios de press sobre UI
		bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		if (pointerOverUI)
		{
			
		}

		// Cuando empieza la pulsación: marcas dónde empezó
		if (ClickAction.WasPressedThisFrame())
		{
			pressTime = 0f;
			pressStartedOver = pointerOverUI ? null : hitGO;
			pressCanCount = EvaluatePressRelevance(pressStartedOver);
		}

		// Si mantienes presionado, solo cuentas si la pulsación inicial es relevante
		if (ClickAction.IsPressed())
		{
			if (pressCanCount)
			{
				pressTime += Time.deltaTime;
			}

			// Si alcanzamos long click y aún no estamos arrastrando -> intentar iniciar drag
			if (!isMoving && pressCanCount && pressTime >= longClickThreshold)
			{
				TryStartDrag(hitGO, ray);
			}

			// Si estamos arrastrando -> actualizar posición SIEMPRE (snap o float)
			if (isMoving && selectedObject != null)
			{
				UpdateDragPosition(ray, hitSomething, hit);
			}
		}

		// Al soltar click
		if (ClickAction.WasReleasedThisFrame())
		{
			if (isMoving && selectedObject != null)
			{
				EndDrag();
			}
			else
			{
				// click corto: si pressCanCount era false igual permitimos seleccionar lo que esté bajo cursor (comportamiento razonable)
				if (hitGO != null && hitGO.TryGetComponent<SnapOffset>(out var so))
				{
					// seleccionar y toggle lista
					selectedObject = hitGO;
					state = State.Selected;
					ToggleList();
				}
			}

			// reset
			pressTime = 0f;
			pressStartedOver = null;
			pressCanCount = false;
		}

		// fallback: si por alguna razón el botón no está presionado y pressTime quedó > 0 (WasReleased missed), limpiamos
		if (!ClickAction.IsPressed() && pressTime != 0f)
		{
			pressTime = 0f;
			pressStartedOver = null;
			pressCanCount = false;
		}

		// saneamiento final
		SanitizeLayersAndState();
	}

	// ------------------------------------------------------------------
	private bool EvaluatePressRelevance(GameObject startGO)
	{
		if (startGO == null) return false;

		// Si no hay selectedObject: solo cuenta si el press empezó sobre algo arrastrable (SnapOffset)
		if (selectedObject == null)
		{
			return startGO.TryGetComponent<SnapOffset>(out _);
		}

		// Si hay selectedObject: cuenta solo si el press empezó sobre el mismo objeto, un hijo suyo o su gizmo UI3DElement
		if (startGO == selectedObject) return true;
		if (selectedObject != null && startGO.transform.IsChildOf(selectedObject.transform)) return true;
		if (startGO.TryGetComponent<UI3DElement>(out _)) return true;

		return false;
	}

	// ------------------------------------------------------------------
	private void TryStartDrag(GameObject currentHitGO, Ray ray)
	{
		// si no tienes seleccion y presionaste sobre algo arrastrable -> seleccionar y drag
		if (selectedObject == null)
		{
			if (pressStartedOver != null && pressStartedOver.TryGetComponent<SnapOffset>(out var so))
			{
				selectedObject = pressStartedOver;
			}
			else
			{
				return; // no hay nada válido con lo que empezar
			}
		}

		// que la pulsación inicial fuera sobre el selectedObject / gizmo / hijo
		if (pressStartedOver != null && !IsPressOverSelectedOrGizmo(pressStartedOver))
		{
			return;
		}

		// ver que selectedObject sigue válido/activo
		if (selectedObject == null) return;
		if (!selectedObject.activeInHierarchy) return;

		// iniciar drag
		isMoving = true;
		state = State.Dragging;
		SetSelectedLayerIgnoringRaycast(true);
		SetCreatureIgnoreRaycast(false);
		OffList();
	}

	private bool IsPressOverSelectedOrGizmo(GameObject go)
	{
		if (go == null) return false;
		if (go == selectedObject) return true;
		if (selectedObject != null && go.transform.IsChildOf(selectedObject.transform)) return true;
		if (go.TryGetComponent<UI3DElement>(out _)) return true;
		return false;
	}

	// ------------------------------------------------------------------
	private void UpdateDragPosition(Ray ray, bool hitSomething, RaycastHit hit)
	{
		if (selectedObject == null) return;

		if (hitSomething)
		{
			SnapAble snapTarget = hit.collider.GetComponent<SnapAble>();
			if (snapTarget != null)
			{
				SnapOffset offset = selectedObject.GetComponent<SnapOffset>();
				Vector3 finalPos = hit.point;
				Quaternion finalRot = Quaternion.LookRotation(hit.normal);
				if (offset != null)
				{
					finalPos += offset.PosOffset;
					finalRot *= Quaternion.Euler(offset.RotOffset);
				}
				selectedObject.transform.SetPositionAndRotation(finalPos, finalRot);
				return;
			}
		}

		// si no hay snap o no golpea nada, siempre flotamos con el cursor
		MoveObjectInXY(ray);
	}

	private void EndDrag()
	{
		SetSelectedLayerIgnoringRaycast(false);
		isMoving = false;
		state = selectedObject != null ? State.Selected : State.Idle;
		SetCreatureIgnoreRaycast(true);
	}

	// ------------------------------------------------------------------
	private void SanitizeLayersAndState()
	{
		// sin selectedObject no hay que hacer nada con capas
		if (selectedObject == null)
		{
			state = State.Idle;
			return;
		}

		// Caso: ambos en la misma capa -> forzar selected detectable, creature ignore
		if (selectedObject.layer == Creature.layer)
		{
			SetSelectedLayerIgnoringRaycast(false);
			SetCreatureIgnoreRaycast(true);
		}

		// Si selected está en ignore pero no se está moviendo -> corregir
		if (selectedObject.layer == 2 && !isMoving)
		{
			SetSelectedLayerIgnoringRaycast(false);
			SetCreatureIgnoreRaycast(true);
		}

		// Si isMoving pero el click ya no está presionado -> finalizar drag seguro
		if (isMoving && !ClickAction.IsPressed())
		{
			EndDrag();
		}

		if (!isMoving && selectedObject != null) state = State.Selected;
	}

	// ------------------------------------------------------------------
	private void SetSelectedLayerIgnoringRaycast(bool ignore)
	{
		if (selectedObject == null) return;
		selectedObject.layer = ignore ? 2 : 0;
	}

	private void SetCreatureIgnoreRaycast(bool ignore)
	{
		if (Creature == null) return;
		Creature.layer = ignore ? 2 : 0;
	}

	// ------------------------------------------------------------------
	// ToggleList ahora usa listVisible para evitar toggles innecesarios
	void ToggleList()
	{
		listVisible = !listVisible;
		for (int i = 0; i < objectList.Count; i++)
		{
			var obj = objectList[i];
			bool shouldBeActive = listVisible;
			if (obj.activeInHierarchy != shouldBeActive)
			{
				obj.SetActive(shouldBeActive);
				obj.transform.parent = shouldBeActive ? selectedObject?.transform : null;
				if (obj.TryGetComponent<UI3DElement>(out UI3DElement i3DElement))
				{
					obj.transform.SetLocalPositionAndRotation(
						shouldBeActive ? i3DElement.offsetPos : Vector3.zero,
						shouldBeActive ? Quaternion.Euler(i3DElement.offsetRot) : Quaternion.identity
					);
					obj.transform.localScale = shouldBeActive ? i3DElement.offsetSiz.Divide3d( obj.transform.parent.localScale) : Vector3.one;
				}
			}
		}
	}

	// Apaga la lista sin hacer toggles
	void OffList()
	{
		if (!listVisible) return;
		listVisible = false;
		foreach (var obj in objectList)
		{
			if (obj != null && obj.activeInHierarchy)
			{
				obj.SetActive(false);
				obj.transform.parent = null;
				if (obj.TryGetComponent<UI3DElement>(out UI3DElement i3DElement))
				{
					obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
					obj.transform.localScale = Vector3.one;
				}
			}
		}
	}

	// ------------------------------------------------------------------
	void MoveObjectInXY(Ray ray)
	{
		if (selectedObject == null) return;
		float distance = Mathf.Abs(cam.transform.position.z - selectedObject.transform.position.z);
		Vector3 point = ray.GetPoint(distance);
		selectedObject.transform.position = new Vector3(point.x, point.y, selectedObject.transform.position.z);
	}

	// ------------------------------------------------------------------
	// Sanity-check coroutine (watchdog)
	private IEnumerator SanityCheckCoroutine()
	{
		var wait = new WaitForSeconds(0.1f);
		while (true)
		{
			yield return wait;

			// Si CantMove está activo, asegúrate de resetear pressTime e isMoving
			if (CantMove)
			{
				if (pressTime != 0f) pressTime = 0f;
				if (isMoving)
				{
					EndDrag();
				}
			}

			// Evita que pressTime siga aumentando si no hay click presionado
			if (!ClickAction.IsPressed() && pressTime != 0f)
			{
				pressTime = 0f;
				pressStartedOver = null;
				pressCanCount = false;
			}

			// Corregir capas incoherentes
			if (selectedObject != null && Creature != null)
			{
				if (!isMoving && selectedObject.layer == 2)
					SetSelectedLayerIgnoringRaycast(false);
				if (!isMoving && Creature.layer != 2)
					SetCreatureIgnoreRaycast(true);
			}

			// Si selectedObject se destruyó por fuera mientras arrastrabas
			if (isMoving && (selectedObject == null))
			{
				EndDrag();
			}
		}
	}

	// ------------------------------------------------------------------
	// utilities
	private void CancelCurrentPress()
	{
		pressTime = 0f;
		pressStartedOver = null;
		pressCanCount = false;
		if (isMoving) EndDrag();
	}

	private void EnsureActionsEnabled()
	{
		if (map != null && !map.enabled) map.Enable();
		if (ClickAction != null && !ClickAction.enabled) ClickAction.Enable();
		if (cursorAction != null && !cursorAction.enabled) cursorAction.Enable();
	}
}
