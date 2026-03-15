using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NamedescManager : MonoBehaviour
{
    public PhaseManager phaseManager;
    public UIDocument Document;
    public string Name;         // Persistencia nombre
    public string Description;  // Persistencia descripción

    private bool initialized = false;
    private TextField Nam; // campo nombre
    private TextField Desc; // campo descripción
    private Label Warning; // etiqueta de advertencia
    private Button Sig;    // botón siguiente

    void Update()
    {
        if (phaseManager.CurrentPhase == EditPhases.NamingAndSaving)
        {
            if (!initialized)
            {
                if (!Document.isActiveAndEnabled) return;

                var root = Document.rootVisualElement;

                Nam = root.Q<TextField>("Nam");
                Desc = root.Q<TextField>("Desc");
                Warning = root.Q<Label>("WARNING");
                Sig = root.Q<Button>("Sig");

                // Restaurar texto guardado
                Nam.value = Name;
                Desc.value = Description;

                // Suscribirse a cambios para actualizar persistencia
                Nam.RegisterValueChangedCallback(evt =>
                {
                    Name = evt.newValue;
                    ValidateInput();
                });

                Desc.RegisterValueChangedCallback(evt =>
                {
                    Description = evt.newValue;
                    ValidateInput();
                });

                Sig.clicked += OnNextClicked;

                ValidateInput();

                initialized = true;
            }
        }
        else
        {
            initialized = false;
        }
    }

    void ValidateInput()
    {
        // Ejemplo simple: advertencia si alguno está vacío
        if (string.IsNullOrWhiteSpace(Name))
        {
            Warning.text = "Nombre no puede estar vacío.";
            Sig.SetEnabled(false);
        }
        else
        {
            Warning.text = "";
            Sig.SetEnabled(true);
        }
    }

    void OnNextClicked()
    {
        // Aquí podrías avanzar a la siguiente fase
        Debug.Log($"Siguiente con nombre: {Name} y descripción: {Description}");
    }
}
