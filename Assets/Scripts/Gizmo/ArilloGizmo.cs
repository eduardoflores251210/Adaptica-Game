using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// AY NO TYPO es AnilloGizmo no ArilloGizmo XD 
/// Componente para el anillo de rotación de un gizmo de transformación
/// </summary>
[RequireComponent(typeof(Collider))]
public class ArilloGizmo : MonoBehaviour
{
    //espacio en blanco sospechoso



    public MultiEje eje;
    private GizmoManager manager;
    private bool arrastrando = false;
    private Vector3 ultimaPosicionPointer;
    public InputActionAsset inputActionsAsset;
    private InputAction dragAction;
    private InputAction pointerPositionAction;

    private void Awake()
    {
        manager = GetComponentInParent<GizmoManager>();
        if (manager == null)
        {
            Debug.LogError("FlechaGizmo requiere que el GizmoManager esté en el padre.");
        }

        // Busca el mapa y las acciones dentro del asset (por nombre)
        var map = inputActionsAsset.FindActionMap("GC", true);
        dragAction = map.FindAction("Drag", true);
        pointerPositionAction = map.FindAction("PPos", true);

        dragAction.started += ctx => ComenzarArrastre();
        dragAction.canceled += ctx => TerminarArrastre();
    }
    private void ComenzarArrastre()
    {

        Vector2 PointerPos = pointerPositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(PointerPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                arrastrando = true;
                Debug.Log($"Gizmo {this.eje} Girando");
                ultimaPosicionPointer = PointerPos;
            }
            else
            {
                arrastrando = false;
            }
        }
        else
        {
            arrastrando = false;
        }
    }

    private void TerminarArrastre()
    {
        arrastrando = false;
    }
    private void OnEnable()
    {
        dragAction.Enable();
        pointerPositionAction.Enable();
    }

    private void OnDisable()
    {
        dragAction.Disable();
        pointerPositionAction.Disable();
    }

    private void Update()
    {
        if (!arrastrando) return;

        Vector3 posActual = pointerPositionAction.ReadValue<Vector2>();
        Vector2 deltaPantalla = (Vector2)(posActual - ultimaPosicionPointer);
        ultimaPosicionPointer = posActual;

        manager.RotarPorEje(eje, deltaPantalla, 30);
    }
    private void OnPointerDown()
    {

        arrastrando = true;
        ultimaPosicionPointer = pointerPositionAction.ReadValue<Vector2>();
    }

    private void OnPointerUp()
    {

        arrastrando = false;
    }


}
