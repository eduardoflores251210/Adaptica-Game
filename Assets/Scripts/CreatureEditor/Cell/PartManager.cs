using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PartManager : MonoBehaviour
{
	public PhaseManager PhaseManager;
	public List<GameObject> MaleParts = new();
	public GameObject MalePartsHolder;
	public List<GameObject> FemaleParts = new();
	public GameObject FemalePartsHolder;
	public PartsDatabase Database;
	public GéneroBiológico ActiveGen = GéneroBiológico.Female;
	public UIDocument iDoc;
	public Material MaleMat;
	public Material FemaleMat;
	public MeshRenderer CreatureRenderer;
	public GizmoManager gizmoManager;
	public ColorPreview Preview;
	public Color MaleColor;
	public Color FemaleColor;
	public Material BodyEditMat;
	private Button Addeye;
	private Button MH;
	private Button MC;
	private Button MO;
	private Button GenToggleButton; // Botón para cambiar Genero
	public Sprite MaleSprite;       // Sprite para Genero masculino
	public Sprite FemaleSprite;     // Sprite para Genero femenino
	private EnumField RepTypeEnum;
	private EnumField RepMethodEnum;
	private RadioButtonGroup radio;
	private Button Next;
	private Button AppPaint;
	private Label Lbl;
	private Toggle DIM;
	bool inited = false;

	// Variables para persistencia de datos UI
	private GéneroBiológico savedActiveGendr;
	public reproductionTypes savedRepType;
	public ReproductionMethod savedRepMethod;
	public int savedtool;
	private bool savedDIM;
	private Color savedMaleColor;
	private Color savedFemaleColor;

	void Start()
	{
		Debug.LogWarning("ADVERTENCIA ESTO NO ESTA COMPLETO AUN FALTA EL CODIGO PARA MANEJAR BIEN LAS PROXIMAS PARTES AÑADIDAS");
		// Inicializamos los valores guardados con los actuales
		savedActiveGendr = ActiveGen;
		savedRepType = reproductionTypes.SingleCell; // o algún valor por defecto válido
		savedRepMethod = ReproductionMethod.Mitosis; 
		savedDIM = false;
		savedMaleColor = MaleColor;
		savedFemaleColor = FemaleColor;
		PlayerManager.RegisterEditor(this, Editors.Microbe);
	}
	public void LoadMicrobe( MicrobeData microbe)
	{
		savedActiveGendr = GéneroBiológico.Female;
		savedRepMethod = microbe.RepMeth;
		savedFemaleColor= microbe.FemaleColor;
		savedMaleColor= microbe.MaleColor;
		ActiveGen = GéneroBiológico.Female;
		foreach (var PF in microbe.PartsF)
		{
			addPart(PF.Id);
		}
		if (microbe.PartsM != null)
		{
			ActiveGen = GéneroBiológico.Male;
			foreach(var PF in microbe.PartsM)
				{ addPart(PF.Id); }
			ActiveGen = GéneroBiológico.Female;
		}
	}

	void Update()
	{
		if (PhaseManager.CurrentPhase == EditPhases.BodyEdit)
		{
			CreatureRenderer.material = BodyEditMat;
		}
		if (PhaseManager.CurrentPhase == EditPhases.PartEditAndBodyColouring)
		{
			if (!inited)
			{
				if (!iDoc.isActiveAndEnabled)
				{
					return;
				}

				var root = iDoc.rootVisualElement;

				Addeye = root.Q<Button>("eye");
				MC = root.Q<Button>("MouthC");
				MH = root.Q<Button>("MouthH");
				MO = root.Q<Button>("MouthO");
				GenToggleButton = root.Q<Button>("SRX"); // nombre del botón en UI
				RepMethodEnum = root.Q<EnumField>("MTD");
				RepTypeEnum = root.Q<EnumField>("ASE");
				Lbl = root.Q<Label>("WARNING");
				DIM = root.Q<Toggle>("DIM");
				Next = root.Q<Button>("Sig");
				AppPaint = root.Q<Button>("ApplyPaint");
				var EyePart = Database.GetPartByID("-1");
				Addeye.tooltip = EyePart.description;
				Debug.Log(Addeye.tooltip);
				Addeye.style.backgroundImage  = Background.FromSprite(EyePart.icon);
				radio = root.Q<RadioButtonGroup>("SlctT");
				
				// Restaurar valores guardados
				ActiveGen = savedActiveGendr;
				RepTypeEnum.value = savedRepType;
				RepMethodEnum.value = savedRepMethod;
				DIM.value = savedDIM;
				MaleColor = savedMaleColor;
				FemaleColor = savedFemaleColor;
				radio.value = savedtool;

				Preview.SetColor(ActiveGen == GéneroBiológico.Female ? FemaleColor : MaleColor);

				// Suscribirse a eventos después de restaurar valores para evitar disparar eventos al setear
				Addeye.clicked += Addeye_clicked;
				MC.clicked += MC_clicked;
				MH.clicked += MH_clicked;
				MO.clicked += MO_clicked;
				GenToggleButton.clicked += ToggleGender;
				RepTypeEnum.RegisterValueChangedCallback(evt =>
				{
					savedRepType = (reproductionTypes)evt.newValue;
					ShowWarning(Lbl);
				});
				RepMethodEnum.RegisterValueChangedCallback(evt =>
				{
					savedRepMethod = (ReproductionMethod)evt.newValue;
					ShowWarning(Lbl);
				});
				DIM.RegisterValueChangedCallback(evt =>
				{
					savedDIM = evt.newValue;
					ShowWarning(Lbl);
					UpdateGenderButtonState();
				});
				radio.RegisterValueChangedCallback(evt =>
				{
					savedtool = evt.newValue;
					if (gizmoManager.objetoActivo != null)
					{
						var obj = gizmoManager.objetoActivo.gameObject;
						if (obj.TryGetComponent<PartComp>(out _))
						{
							if (obj.TryGetComponent<GizSelectable>(out var gg))
							{
								switch (evt.newValue)
								{
									case 0:
										gg.IsMovable = true; 
										gg.IsRotatable = false;
										gg.IsScalable = false;
										break;
									case 1:
										gg.IsRotatable = true;
										gg.IsMovable = false;
										gg.IsScalable = false;
										break;
									case 2:
										gg.IsScalable = true;
										gg.IsMovable = false;
										gg.IsRotatable = false;
										break;
								}
							}
						}
					}
				});
				AppPaint.clicked += AppPaint_clicked;

				UpdateGenderButtonSprite();
				UpdateHolderActiveState();
				ShowWarning(Lbl);

				inited = true;
			}
			else
			{
				ShowWarning(Lbl);
				if (ActiveGen == GéneroBiológico.Female || ActiveGen == GéneroBiológico.None)
				{
					CreatureRenderer.material = FemaleMat;
				}
				else
				{
					CreatureRenderer.material = MaleMat;
				}
			}

		}
		else
		{
			inited = false;
		}

	}

	private void MO_clicked()
	{
		addPart("2");
	}

	private void MH_clicked()
	{
		addPart("1");
	}

	private void MC_clicked()
	{
		addPart("0");
	}

	private void AppPaint_clicked()
	{
		if (ActiveGen == GéneroBiológico.Female)
		{
			FemaleColor = Preview.GetColor(0.5f);
			FemaleMat.color = FemaleColor;
			savedFemaleColor = FemaleColor;
		}
		else
		{
			MaleColor = Preview.GetColor(0.5f);
			MaleMat.color = MaleColor;
			savedMaleColor = MaleColor;
		}
	}

	private void Addeye_clicked()
	{
		addPart();
	}

	private void ToggleGender()
	{
		if (ActiveGen == GéneroBiológico.Female || ActiveGen == GéneroBiológico.None)
		{
			ActiveGen = GéneroBiológico.Male;
			Preview.SetColor(savedMaleColor);
		}
		else
		{
			ActiveGen = GéneroBiológico.Female;
			Preview.SetColor(savedFemaleColor);
		}

		savedActiveGendr = ActiveGen;
		UpdateHolderActiveState();
		UpdateGenderButtonSprite();
	}

	void UpdateGenderButtonSprite()
	{
		Sprite sprite = null;
		if (ActiveGen == GéneroBiológico.Female)
		{
			sprite = FemaleSprite;
		}
		else
		{
			sprite = MaleSprite;
		}
		GenToggleButton.style.backgroundImage = Background.FromSprite(sprite);
	}

	void UpdateGenderButtonState()
	{
		// Activar o desactivar botón según condiciones
		bool disableGenToggle = savedDIM && savedRepType == reproductionTypes.SingleCell;
		GenToggleButton.SetEnabled(!disableGenToggle);
	}

	void addPart(string id = "-1")
	{
		var Part = Instantiate(Database.GetPartByID(id).prefab);
		var gz = Part.AddComponent<GizSelectable>();
		gz.IsMovable = true;
		gz.IsRotatable = true;
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
		if ((MaleParts == null || MaleParts.Count == 0))
			return false;
		if (FemaleParts == null || FemaleParts.Count == 0)
			return false;
		return true;
	}

	void ShowWarning(Label label)
	{
		label.SetEnabled(true);
		ReproductionMethod method = (ReproductionMethod)RepMethodEnum.value;
		reproductionTypes RepTyp = (reproductionTypes)RepTypeEnum.value;
		bool A = (!SerializableTypes.Biology.SerializedPartData.IsValidMethodTypePair(RepTyp, method));
		bool B = (!(IsDimorphic() && RepTyp != reproductionTypes.SingleCell) && DIM.value);
		bool C = DIM.value && (RepTyp == reproductionTypes.SingleCell);
		if (A && !B)
			label.text = $"Metodo de reproduccion invalido {method} para {RepTyp}";
		else if (A && B && (!C))
			label.text = $"Metodo de reproduccion invalido {method} para {RepTyp} Y falta un Genero";
		else if (!A && B & (!C))
			label.text = "falta un Genero";
		else if (C)
			label.text = "No puede haber dimorfismo si se reproduce de manera sin pareja";
		else
			label.text = "";
		if (label.text == "")
			label.SetEnabled( false);
		GenToggleButton.SetEnabled(!(C || !DIM.value));
		Next.SetEnabled(label.text == "");
	}
	public void OnDestroy()
	{
		if (this == PlayerManager.Player)
			PlayerManager.UnregisterEditor();
	}
}
