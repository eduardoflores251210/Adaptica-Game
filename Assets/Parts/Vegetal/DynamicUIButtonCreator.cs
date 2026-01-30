using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// se deveria llamar PlantEditorMenuPopulator
/// pues añade botones dinamicamente a un UI Document que permiten añadir partes vegetales
/// como esporangios, hojas, tallos, raices, etc
/// </summary>
public class DynamicUIButtonCreator : MonoBehaviour
{
	public PartsDatabase PlantDatabase;
	public VisualTreeAsset buttonTemplate; // opcional, si quieres un template de botón
	public UIDocument uiDocument;
	public PlantStemGenerator PlantStemGenerator;
	public MinMaxSlider radii;
	public SliderInt	 segments;
	public Slider espaciado;
	public string UIDocumentRootName;
	public List<string> buttonNames;
	private void Start()
	{
		CreateUIDocumentWithButtons();
	}
	// Esta función crea el UI Document y añade los botones
	public void CreateUIDocumentWithButtons()
	{
		VisualElement root = null;
		// Crear un GameObject con UIDocument si no existe
		if (uiDocument == null)
		{
			GameObject uiGO = new GameObject("DynamicUI");
			uiDocument = uiGO.AddComponent<UIDocument>();
			// Crear un root VisualElement
			root = new VisualElement();
			root.style.flexDirection = FlexDirection.Column;
			root.style.alignItems = Align.Center;
			root.style.justifyContent = Justify.FlexStart;
			root.style.flexGrow = 1;
			root.style.paddingTop = 10;
			uiDocument.rootVisualElement.Add(root);
		}
		else
		{
			root = uiDocument.rootVisualElement;
			var q = root.Q(UIDocumentRootName);
			radii = root.Q<MinMaxSlider>("Radios");
			segments = root.Q<SliderInt>("seg");
			espaciado = root.Q<Slider>("esp");

			root = q;
		}
		
		


		// Crear botones
		foreach (var a in PlantDatabase.allParts)
		{
			string name = a.displayName;
			string iNam = a.name;
			Button btn = new Button();
			btn.text = name;
			btn.style.marginBottom = 5;

			btn.style.flexShrink = 1;
			btn.style.maxHeight = new StyleLength(new Length(50, LengthUnit.Percent));
			btn.style.minHeight = new StyleLength(new Length(5, LengthUnit.Percent));
			btn.style.maxWidth = new StyleLength(new Length(50, LengthUnit.Percent));
			btn.style.minWidth = new StyleLength(new Length(5, LengthUnit.Percent));
			btn.style.width = new StyleLength (StyleKeyword.Auto);
			btn.style.height = new StyleLength (StyleKeyword.Auto);
			btn.tooltip = $"añade la parte {name}";
			root.Add(btn);
			//string IInam = iNam;
			btn.clicked += delegate (){ 
				Vector3 Z = new(0, 0, Camera.main.nearClipPlane * 50); 
				GameObject OBJ =Instantiate(PlantDatabase.GetPartByName(iNam).prefab,Camera.main.ScreenToWorldPoint(((Vector3)Pointer.current.position.ReadValue()) + Z),Quaternion.identity);
				OBJ.AddComponent<SnapOffset>();
			}; //name es el internal name y Displayname es el nombre a mostrar
		}

		Debug.Log("UI Document creado con " + PlantDatabase.allParts.Count + " botones.");
	}
	private void Update()
	{
		if (PlantStemGenerator ==  null)
		{
			PlantStemGenerator = FindAnyObjectByType<PlantStemGenerator>();
		}
		if (PlantStemGenerator == null)
			return;
		PlantStemGenerator.startRadius = radii.maxValue;
		PlantStemGenerator.endRadius = radii.minValue;
		PlantStemGenerator.segmentCount = segments.value;
		PlantStemGenerator.segmentSpacing = espaciado.value;
	}

}
