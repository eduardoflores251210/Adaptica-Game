using ActualUtils;
using SerializableTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// aun no esta compelto como el
/// editor de plantas pero sera como el CompleteCellEditorUiManger 
/// pero vegetal
/// </summary>
public class PLANTUIMANAGER : MonoBehaviour
{
	public UIDocument iDocument;
    public PlantStemGenerator stemGenerator;
    public DynamicUIButtonCreator buttonCreator;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitToMenu()
    {
        LoadWithLoadingScreen.LoadScene(0, Stages.MainMenu);
    }
}
