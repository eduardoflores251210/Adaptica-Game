using StandartUtilities;
using UnityEngine;
using System.Collections.Generic;

public class GizmoManager : MonoBehaviour
{
    [Header("Objeto a mover directamente")]
    public Transform objetoActivo;
    public GameObject InfoUI;
    public Transform Cursor; //cursor en caso de Gamepad
    private Camera camara;
    private List<GameObject> Children = new();

    private void Awake() //13
    {
        camara = Camera.main;
        for (int i = 0; i < transform.childCount ; i++)
        {
            Children.Add(transform.GetChild(i).gameObject);
        }
    }
    void Update() 
    {
        if (InfoUI == null) { } else
        { InfoUI.SetActive(objetoActivo != null); }
        transform.position = objetoActivo? objetoActivo.position: Vector3.zero;
      
            foreach (var child in Children)
            {
                if (child.TryGetComponent<MeshRenderer>(out var r))
                { 
                    if (child.TryGetComponent<FlechaGizmo>(out _))
                {
                    if (objetoActivo  == null) {r.enabled = false;} else
                    if (objetoActivo.TryGetComponent<GizSelectable>(out var gizSelectable))
                    {
                        r.enabled = gizSelectable.IsMovable; // esta línea decide si se muestran o no
                    }
                }
                    else
                {
                    //r.enabled = true; // otros hijos no flechas o Anillos siempre visibles
                }
                    if (child.TryGetComponent<ArilloGizmo>(out _))
                {
                    if (objetoActivo  == null) {r.enabled = false;} else
                    if (objetoActivo.TryGetComponent<GizSelectable>(out var gizSelectable))
                    {
                        r.enabled = gizSelectable.IsRotatable; // esta línea decide si se muestran o no
                    }
                }
                    else
                    {
                    //r.enabled = true; // otros hijos no flechas o Anillos siempre visibles
                    }
                    

                }
                if (child.TryGetComponent<Collider>(out var c))
                {

                if (child.TryGetComponent<FlechaGizmo>(out _))
                {
                    if (objetoActivo == null) { c.enabled = false; }
                    else
                    if (objetoActivo.TryGetComponent<GizSelectable>(out var gizSelectable))
                    {
                        c.enabled = gizSelectable.IsMovable; // esta línea decide si se muestran o no
                    }
                }
                else
                {
                    //c.enabled = true; // otros hijos no flechas o Anillos siempre visibles
                }
                if (child.TryGetComponent<ArilloGizmo>(out _))
                {
                    if (objetoActivo == null) { c.enabled = false; }
                    else
                    if (objetoActivo.TryGetComponent<GizSelectable>(out var gizSelectable))
                    {
                        c.enabled = gizSelectable.IsRotatable; // esta línea decide si se muestran o no
                    }
                }
                else
                {
                    //c.enabled = true; // otros hijos no flechas o Anillos siempre visibles
                }
            }
            }
        
    }

    // Llamado por las flechas, movimiento proyectado
    public void RotarPorEje(MultiEje eje, Vector2 deltaPantalla,float mult=1)
    {
        if (objetoActivo == null || camara == null) return;
        if (objetoActivo.TryGetComponent<GizSelectable>(out var gizMovable))
        {
            if (!gizMovable.IsMovable)
            {
                return;
            }
        }
        // Convertir delta de pantalla a delta de mundo
        Vector3 desde = camara.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 1f));
        Vector3 hacia = camara.ScreenToWorldPoint(new Vector3(Screen.width / 2 + deltaPantalla.x, Screen.height / 2 + deltaPantalla.y, 1f));
        Vector3 deltaMundo = hacia - desde;

        Vector3 direccion = eje switch
        {
            MultiEje.X => Vector3.right,
            MultiEje.Y => Vector3.up,
            MultiEje.Z => Vector3.forward,
            MultiEje.XY => new Vector3(1, 1, 0),
            MultiEje.YZ => new Vector3(0, 1, 1),
            MultiEje.XZ => new Vector3(1, 0, 1),
            MultiEje.XYZ => new Vector3(1, 1, 1),
            _ => Vector3.zero
        };

        float magnitud = Vector3.Dot(deltaMundo, direccion.normalized);
        var Mov = (direccion.normalized * magnitud);
        if (eje == MultiEje.X)
        {
            Mov.y = 0f;
            Mov.z = 0f;
        }
        else if (eje == MultiEje.Y)
        { 
            Mov.x = 0f;
            Mov.z = 0f;
        }
        else if (eje == MultiEje.Z)
        { 
            Mov.x = 0f;
            Mov.y = 0f; 
        }else if (eje == MultiEje.XY)
        {
            Mov.z = 0f;
        }
        else if (eje == MultiEje.YZ)
        {
            Mov.x = 0f;
        }else if (eje == MultiEje.XZ)
        {
            Mov.y = 0f;
        }
        objetoActivo.Rotate( Mov * mult,Space.World);
    }
    public void MoverPorEje(MultiEje eje, Vector2 deltaPantalla,float mult=1)
    {
        if (objetoActivo == null || camara == null) return;
        if (objetoActivo.TryGetComponent<GizSelectable>(out var gizMovable))
        {
            if (!gizMovable.IsMovable)
            {
                return;
            }
        }
        // Convertir delta de pantalla a delta de mundo
        Vector3 desde = camara.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 1f));
        Vector3 hacia = camara.ScreenToWorldPoint(new Vector3(Screen.width / 2 + deltaPantalla.x, Screen.height / 2 + deltaPantalla.y, 1f));
        Vector3 deltaMundo = hacia - desde;

        Vector3 direccion = eje switch
        {
            MultiEje.X => Vector3.right,
            MultiEje.Y => Vector3.up,
            MultiEje.Z => Vector3.forward,
            MultiEje.XY => new Vector3(1, 1, 0),
            MultiEje.YZ => new Vector3(0, 1, 1),
            MultiEje.XZ => new Vector3(1, 0, 1),
            MultiEje.XYZ => new Vector3(1, 1, 1),
            _ => Vector3.zero
        };

        float magnitud = Vector3.Dot(deltaMundo, direccion.normalized);
        var Mov = (direccion.normalized * magnitud);
        if (eje == MultiEje.X)
        {
            Mov.y = 0f;
            Mov.z = 0f;
        }
        else if (eje == MultiEje.Y)
        {
            Mov.x = 0f;
            Mov.z = 0f;
        }
        else if (eje == MultiEje.Z)
        {
            Mov.x = 0f;
            Mov.y = 0f;
        }
        else if (eje == MultiEje.XY)
        {
            Mov.z = 0f;
        }
        else if (eje == MultiEje.YZ)
        {
            Mov.x = 0f;
        }
        else if (eje == MultiEje.XZ)
        {
            Mov.y = 0f;
        }
        objetoActivo.position += Mov * mult;
    }
}
