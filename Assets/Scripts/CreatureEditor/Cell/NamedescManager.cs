using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NamedescManager : MonoBehaviour
{
    public CompleteCellEditorUiManger phaseManager;
    public UIDocument Document;
    public string Name;         // Persistencia nombre
    public string Description;  // Persistencia descripción
    public bool ISVALID = false;
    private bool initialized = false;
    private TextField Nam; // campo nombre
    private TextField Desc; // campo descripción
    private Button Exit;    // botón salir
    //private Label Warning; // etiqueta de advertencia
    private Button Sig;    // botón siguiente

    void Update()
    {

        if (!initialized)
        {
            if (!Document.isActiveAndEnabled) return;

            var root = Document.rootVisualElement;

            Nam = root.Q<TextField>("Nam");
            Desc = root.Q<TextField>("Desc");
            //Warning = root.Q<Label>("WARNING");
            Sig = root.Q<Button>("Sig");
            Exit = root.Q<Button>("Exit");
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
            Exit.clicked += () => { phaseManager.saver.ExitWitourthSaving(); };

            ValidateInput();

            initialized = true;
        }


    }

    public string ValidateInput()
    {
        string add = "";
        // Ejemplo simple: advertencia si alguno está vacío
        if (string.IsNullOrWhiteSpace(Name))
        {
            add = "Nombre no puede estar vacío.";
            //Warning.text = "Nombre no puede estar vacío.";
            if (Sig == null)
                return add;
            if (Sig.tooltip == null)
                Sig.tooltip = "";
			Sig.tooltip += add;

            ISVALID = false;
        }
        else
        {
            //Warning.text = "";
            Sig.tooltip += "";
            ISVALID = true;
        }
        return add;
    }

    void OnNextClicked()
    {
        // Aquí podrías avanzar a la siguiente fase
        Debug.Log($"Guardar con nombre: {Name} y descripción: {Description}");
        phaseManager.Save();
    }
}
