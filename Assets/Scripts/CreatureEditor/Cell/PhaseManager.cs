using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

public class PhaseManager : MonoBehaviour
{
    public EditPhases CurrentPhase;
    public List<UIDocument> iDocuments; //uy suena a apple
    public MetaballManager metaball;
    public CellSaver saver;
    public Mesh Mesh;
    private Button Sig;
    private Button Atr;
    // Start is called before the first frame update
    void Start()
    {
        ShowCurrentCanvas();
    }

    void ShowCurrentCanvas()
    {
        var i = 0;
        foreach (UIDocument document in iDocuments)
        {
            if (i == (int)CurrentPhase) {
                document.gameObject.SetActive(true);
            } else
            {
                document.gameObject.SetActive(false);
            }
            i++;
        }
        enabledPhaseBehabiour();
    }
    private void Update()
    {
        if (CurrentPhase == EditPhases.BodyEdit)
        {
            Mesh = metaball.mesh;
        }
    }

    private void enabledPhaseBehabiour()
    {
        VisualElement root = CurrentPhase switch
        {
            EditPhases.BodyEdit => iDocuments[0].rootVisualElement, //una interfaz que parece de win 95
            EditPhases.PartEditAndBodyColouring => iDocuments[1].rootVisualElement, //mas win 95
            EditPhases.NamingAndSaving => iDocuments[2].rootVisualElement, //aun no existe
            _ => iDocuments[0].rootVisualElement, //???????????????????????
        };
        Sig = root.Q<Button>("Sig");
        if (CurrentPhase != EditPhases.NamingAndSaving)
            Sig.clicked += ChangePhasePlus;
        else Sig.clicked += Save;
        if (CurrentPhase != EditPhases.BodyEdit)
        {
            Atr = root.Q<Button>("Atrs");
            Atr.clicked += ChangePhaseMinus;
        }else
        {
            Mesh = metaball.mesh;
        }
    }

    private void ChangePhaseMinus()
    {
        CurrentPhase--;
        metaball.enabled = (CurrentPhase == EditPhases.BodyEdit);
        ShowCurrentCanvas();
    }

    private void ChangePhasePlus()
    {
        CurrentPhase++;
        metaball.enabled = (CurrentPhase == EditPhases.BodyEdit);
        ShowCurrentCanvas();
    }
    private void Save()
    {
        saver.Save();
    }
}
public enum EditPhases
{
    BodyEdit,
    PartEditAndBodyColouring,
    NamingAndSaving
}

