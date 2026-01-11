using SerializableTypes.Biology;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SegmentManager : MonoBehaviour
{
    public PhaseManager PhaseManager;
    public UIDocument iDoc;
    public GameObject MetaballPrefab;
    public Metaball Selected;
    public List<Metaball> Segments;
    public GizmoManager GizmoManager;
    public CameraOrbitController cameraStuff;
    private Button Add;
    private Button Remove;
    private Slider Rad;
    bool HasTOloadCreture(out MicrobeData DataToload)
    {
        DataToload = null;
        if (CrossScenePackageSender.Instance != null)
        {
            var Mailman = CrossScenePackageSender.Instance;
            if (Mailman.IsThereAnyTypedMailForHim<MicrobeData>("MC.SegmentManager", out var F ))
            {
                var Paquete = F[0];
                if (Paquete.Contents != null)
                {
                    DataToload = Paquete.Contents;
                    return true;
                }
            }
        }
        return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        iDoc = PhaseManager.iDocuments[0];
        var root = iDoc.rootVisualElement;
        Add = root.Q<Button>("Crr");
        Remove = root.Q<Button>("Brr");
        Rad = root.Q<Slider>("rad");
        Add.clicked += AddSegment;
        Remove.clicked += RemoveSegment;
        if (HasTOloadCreture(out var GGG))
        {
            MetaballManager.Instance.DestroyAllMetaballs();
            foreach (var G in GGG.Segments)
            {
				var segGO = new GameObject();
                var seg = segGO.AddComponent<Metaball>();
                seg.AddComponent<GizSelectable>();
                Segments.Add(seg);
            }
            PhaseManager.saver.partmanager.LoadMicrobe(GGG);
        }

    }

    private void RemoveSegment()
    {
        Segments.Remove(Selected);
        Destroy(Selected.gameObject);
    }

    private void AddSegment()
    {
        var f= Instantiate(MetaballPrefab);
        Segments.Add(f.GetComponent<Metaball>());
    }
    Metaball OldSelected;
    // Update is called once per frame
    void Update()
    {
        if (PhaseManager.CurrentPhase == EditPhases.BodyEdit)
        {
            if (GizmoManager.objetoActivo == null)
            {
                Selected = null;
            }
            else
            if (GizmoManager.objetoActivo.gameObject.TryGetComponent<Metaball>(out Metaball metaball))
            {
                Selected = metaball;
                if (OldSelected != Selected) 
                Rad.value = Selected.Radius;
                else Selected.Radius = Rad.value;
            }else
            {
                Selected = null;
            }
        }
        foreach (Metaball metab in Segments)
        {
            if (metab.TryGetComponent<GizSelectable>(out var ar))
            {
                ar.IsMovable = (PhaseManager.CurrentPhase == EditPhases.BodyEdit);
            }
        }
        OldSelected = Selected;
    }
}
