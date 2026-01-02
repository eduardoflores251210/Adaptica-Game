using StandartUtilities;// Antes creo que hacia una comprovacion de par aqui
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class FlechaGizmo : MonoBehaviour
{
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
                Debug.Log($"Gizmo {this.eje} arrastrando");
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

        manager.MoverPorEje(eje, deltaPantalla,5);
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
