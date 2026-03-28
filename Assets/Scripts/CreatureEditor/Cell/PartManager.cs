using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PartManager : MonoBehaviour
{
	public CompleteCellEditorUiManger PhaseManager;
	public List<GameObject> MaleParts = new();
	public GameObject MalePartsHolder;
	public List<GameObject> FemaleParts = new();
	public GameObject FemalePartsHolder;
	public PartsDatabase Database;
	public GéneroBiológico ActiveGen = GéneroBiológico.Female;
	public ReproductionMethod ReproductionMethod;
	public reproductionTypes ReproductionType;
	public UIDocument iDoc;
	public Material MaleMat;
	public Material FemaleMat;
	public MeshRenderer CreatureRenderer;
	public GizmoManager gizmoManager;
	public ColorPreview Preview;
	public Color MaleColor;
	public Color FemaleColor;
	public Material BodyEditMat;

	public string PartTabName;
	public string WaponsTabName;
	public string MouyhsTabName;
	public string EyesTabName;

	public Sprite MaleSprite;
	public Sprite FemaleSprite;

	private EnumField RepTypeEnum;
	private EnumField RepMethodEnum;
	private RadioButtonGroup radio;
	private Button Next;
	private Button AppPaint;

	private Toggle DIM;
	private Button GenToggleButton;

	public bool inited = false;
	public bool runtimeButtonsBuilt = false;

	int MaleMouthCount = 0;
	int FemaleMouthCount = 0;


	// Contenedores de pestañas
	private VisualElement partTabContainer;
	private VisualElement weaponsTabContainer;
	private VisualElement mouthsTabContainer;
	private VisualElement eyesTabContainer;

	void Start()
	{
		PlayerManager.RegisterEditor(this, Editors.Microbe);
	}
	public void LoadMicrobe(MicrobeData microbe)
	{
		ActiveGen = GéneroBiológico.Female;
		RepMethodEnum.value = microbe.RepMeth;
		MaleColor = microbe.MaleColor;
		FemaleColor = microbe.FemaleColor;

		foreach (var PF in microbe.PartsF)
			addPart(PF.Id);

		if (microbe.PartsM != null)
		{
			ActiveGen = GéneroBiológico.Male;
			foreach (var PF in microbe.PartsM)
				addPart(PF.Id);

			ActiveGen = GéneroBiológico.Female;
		}
	}

	void Update()
	{
		if (!inited)
		{
			TryInitUI();
			return;
		}
		if ((PhaseManager.currentCat & CurrentCategory.Body) == CurrentCategory.Body)
		{
			CreatureRenderer.material = BodyEditMat;
			if (inited)
			ShowWarning(Next);
			return;
		}



		ShowWarning(Next);

		if (ActiveGen == GéneroBiológico.Female || ActiveGen == GéneroBiológico.None)
			CreatureRenderer.material = FemaleMat;
		else
			CreatureRenderer.material = MaleMat;
	}

	private void TryInitUI()
	{
		if (!iDoc || !iDoc.isActiveAndEnabled)
			return;

		var root = iDoc.rootVisualElement;

		RepMethodEnum = root.Q<EnumField>("MTD");
		RepTypeEnum = root.Q<EnumField>("ASE");
		DIM = root.Q<Toggle>("DIM");
		Next = root.Q<Button>("Sig");
		AppPaint = root.Q<Button>("ApplyPaint");
		radio = root.Q<RadioButtonGroup>("SlctT");
		GenToggleButton = root.Q<Button>("SRX");

		partTabContainer = root.Q<VisualElement>(PartTabName);
		weaponsTabContainer = partTabContainer?.Q<VisualElement>(WaponsTabName);
		mouthsTabContainer = partTabContainer?.Q<VisualElement>(MouyhsTabName);
		eyesTabContainer = partTabContainer?.Q<VisualElement>(EyesTabName);

		BuildRuntimePartButtons();

		Preview.SetColor(ActiveGen == GéneroBiológico.Female ? FemaleColor : MaleColor);

		GenToggleButton.clicked += ToggleGender;
		AppPaint.clicked += AppPaint_clicked;

		RepTypeEnum.RegisterValueChangedCallback(a =>
		{
			ReproductionType = (reproductionTypes)a.newValue;
			ShowWarning(Next);
			UpdateGenderButtonState();
		});

		RepMethodEnum.RegisterValueChangedCallback(a =>
		{
			ReproductionMethod = (ReproductionMethod)a.newValue;
			ShowWarning(Next);
		});

		DIM.RegisterValueChangedCallback(_ =>
		{
			ShowWarning(Next);
			UpdateGenderButtonState();
		});



		UpdateGenderButtonSprite();
		UpdateGenderButtonState();
		UpdateHolderActiveState();
		ShowWarning(Next);

		try
		{
			SpaceUtils.AddTooltipManipulators(iDoc);
		}
		catch
		{
			if (Application.isEditor)
				Debug.Log("Error añadiendo tooltips");
		}

		inited = true;
	}

	private void BuildRuntimePartButtons()
	{
		if (runtimeButtonsBuilt || Database == null || Database.allParts == null)
			return;

		foreach (var part in Database.allParts)
		{
			if (part == null)
				continue;

			// Si quieres mostrar SOLO partes de criatura, deja esto.
			if (part.categories == null || !part.categories.Contains(PartCategories.Cell))
				continue;

			var container = GetContainerForPart(part);
			if (container == null)
				continue;

			var button = new Button(() => addPart(part.partID))
			{
				text = part.partID,
				tooltip = part.description
			};

			if (part.icon != null)
				button.style.backgroundImage = Background.FromSprite(part.icon);

			button.style.width = 64;
			button.style.height = 64;
			button.style.marginLeft = 4;
			button.style.marginRight = 4;
			button.style.marginTop = 4;
			button.style.marginBottom = 4;
			button.style.unityTextAlign = TextAnchor.LowerCenter;
			button.style.whiteSpace = WhiteSpace.Normal;

			container.Add(button);
		}

		runtimeButtonsBuilt = true;
	}

	private VisualElement GetContainerForPart(BasePart part)
	{
		if (part.categories == null)
			return partTabContainer;

		if (part.categories.Contains(PartCategories.Eyes) && eyesTabContainer != null)
			return eyesTabContainer;

		if (part.categories.Contains(PartCategories.Mouths) && mouthsTabContainer != null)
			return mouthsTabContainer;

		if (part.categories.Contains(PartCategories.Weapons) && weaponsTabContainer != null)
			return weaponsTabContainer;

		return partTabContainer;
	}

	private void AppPaint_clicked()
	{
		if (ActiveGen == GéneroBiológico.Female)
		{
			FemaleColor = Preview.GetColor(0.5f);
			FemaleMat.color = FemaleColor;
		}
		else
		{
			MaleColor = Preview.GetColor(0.5f);
			MaleMat.color = MaleColor;
		}
	}

	private void ToggleGender()
	{
		if (ActiveGen == GéneroBiológico.Female || ActiveGen == GéneroBiológico.None)
		{
			ActiveGen = GéneroBiológico.Male;
			Preview.SetColor(MaleColor);
		}
		else
		{
			ActiveGen = GéneroBiológico.Female;
			Preview.SetColor(FemaleColor);
		}

		UpdateHolderActiveState();
		UpdateGenderButtonSprite();
	}

	void UpdateGenderButtonSprite()
	{
		Sprite sprite = ActiveGen == GéneroBiológico.Female ? FemaleSprite : MaleSprite;
		GenToggleButton.style.backgroundImage = Background.FromSprite(sprite);
	}

	void UpdateGenderButtonState()
	{
		bool disableGenToggle = DIM.value && (reproductionTypes)RepTypeEnum.value == reproductionTypes.SingleCell;
		GenToggleButton.SetEnabled(!disableGenToggle);
	}

	void addPart(string id = "-1")
	{
		var PartData = Database.GetPartByID(id);
		if (PartData == null || PartData.prefab == null)
			return;

		var Part = Instantiate(PartData.prefab);
		var gz = Part.AddComponent<SnapOffset>();

		if (PartData is BiologicalPart biol)
		{
			if (biol.function == BiologicalPartFunction.Mouth)
			{
				switch (ActiveGen)
				{
					case GéneroBiológico.Male:
						MaleMouthCount++;
						break;
					default:
						FemaleMouthCount++;
						break;
				}
			}
		}
		


		if (ActiveGen == GéneroBiológico.Female || ActiveGen == GéneroBiológico.None)
		{
			Part.transform.parent = FemalePartsHolder.transform;
			FemaleParts.Add(Part);
		}
		else if (ActiveGen == GéneroBiológico.Male)
		{
			Part.transform.parent = MalePartsHolder.transform;
			MaleParts.Add(Part);
		}
	}

	void UpdateHolderActiveState()
	{
		MalePartsHolder.SetActive(ActiveGen == GéneroBiológico.Male);
		FemalePartsHolder.SetActive(ActiveGen == GéneroBiológico.Female || ActiveGen == GéneroBiológico.None);
	}

	public bool IsDimorphic()
	{
		if (MaleParts == null || MaleParts.Count == 0)
			return false;
		if (FemaleParts == null || FemaleParts.Count == 0)
			return false;
		return true;
	}

	void ShowWarning(Button label)
	{
		label.SetEnabled(true);

		ReproductionMethod method = (ReproductionMethod)RepMethodEnum.value;
		reproductionTypes RepTyp = (reproductionTypes)RepTypeEnum.value;

		bool A = (!SerializableTypes.Biology.SerializedPartData.IsValidMethodTypePair(RepTyp, method));
		bool B = (!(IsDimorphic() && RepTyp != reproductionTypes.SingleCell) && DIM.value);
		bool C = DIM.value && (RepTyp == reproductionTypes.SingleCell);
		bool D = (FemaleMouthCount <= 0 || (MaleMouthCount <= 0 && DIM.value));

		if (A && !B)
			label.tooltip = $"Metodo de reproduccion invalido {method} para {RepTyp}";
		else if (A && B && (!C))
			label.tooltip = $"Metodo de reproduccion invalido {method} para {RepTyp} Y falta un Genero";
		else if (!A && B & (!C))
			label.tooltip = "falta un Genero";
		else if (C)
			label.tooltip = "No puede haber dimorfismo si se reproduce de manera sin pareja";
		else if (D)
			label.tooltip = "le falta boca a un genero";
		else
			label.tooltip = "";

		PhaseManager.saver.namedescManager.ValidateInput();
		label.SetEnabled(label.tooltip == "");


		GenToggleButton.SetEnabled(!(C || !DIM.value));
		
	}

	public void OnDestroy()
	{
		if (this == PlayerManager.Player)
			PlayerManager.UnregisterEditor();
	}
}