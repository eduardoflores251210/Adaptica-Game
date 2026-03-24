using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CompleteCellEditorUiManger : MonoBehaviour
{
	public UIDocument iDocument; 
	public CurrentCategory currentCat;
	public bool PanelBEnabled = true;
	public bool PanelAEnabled = true;

	public MetaballManager metaball;
	public CellSaver saver;
	public Mesh Mesh;
	private TabView PartsAndBody;
	private TabView PaintAndBehabiour;
	private Button Paint;
	private Button Body;
	Foldout FoldA;
	Foldout FoldB;
	public bool MoveNOW = false;
	public void Start()
	{
		var root = iDocument.rootVisualElement;
		PartsAndBody = root.Q<TabView>("BP");
		PaintAndBehabiour = root.Q<TabView>("PB");
		Paint = root.Q<Button>("PTBH");
		Body = root.Q<Button>("BDPT");
		FoldA = root.Q<Foldout>("FOLDA");
		FoldB = root.Q<Foldout>("FOLDB");
		if (Paint == null)
		{
			Debug.Log("button PTHB missing ");
		}else
		{
			Paint.clicked += delegate 
			{ 
				PanelBEnabled = !PanelBEnabled; 
				if (PanelBEnabled)
				{
					PanelAEnabled = !PanelBEnabled;
					MoveNOW = true;
				}
			};
		}
		if (Body  == null)
		{
			Debug.Log("button BDPT missing");
		}
		else
		{
			Body.clicked += delegate 
			{
				PanelAEnabled = !PanelAEnabled;
				if (PanelAEnabled)
				{
					PanelBEnabled = !PanelAEnabled;
					MoveNOW = true;
				}
			};
		}


	}
	private void Save()
	{
		saver.Save();
	}
	private void Update()
	{
		CurrentCategory NewCat = CurrentCategory.None;
		if (PartsAndBody != null)
		{
			if (!PanelAEnabled)
			{
				;  //NO-OP
			}
			else
			{
				var aCat = CurrentCategory.None;
				if (PartsAndBody.activeTab.name == "BOD")
				{
					aCat |= CurrentCategory.Body;
					metaball.enabled = true;
				}
				if (PartsAndBody.activeTab.name == "Part")
				{
					aCat |= CurrentCategory.Parts;
					metaball.enabled = false;
				}
				Debug.Log("pb "+ PartsAndBody.activeTab.name.ToString());
				NewCat |= aCat;
			}
		}
		if (PaintAndBehabiour != null)
		{
			if (!PanelBEnabled)
			{	;	}
			else
			{
				var bCat = CurrentCategory.None;
				if (PaintAndBehabiour.activeTab.name == "pin")
				{
					bCat |= CurrentCategory.Paint;
					metaball.enabled = false;

				}
				if (PaintAndBehabiour.activeTab.name == "Beh")
				{
					bCat |= CurrentCategory.Behabiour;
					metaball.enabled = false;

				}
				NewCat |= bCat;
				Debug.Log( "ph " + PaintAndBehabiour.activeTab.name.ToString());

			}
		}
		currentCat= NewCat;


		if (MoveNOW)
		{
			float percentA = PanelAEnabled ? 0f : 100f;  // se va a la derecha
			float percentB = PanelBEnabled ? 0f : 100f;  // se va a la izquierda

			FoldA.style.left = Length.Percent(percentA);
			FoldB.style.left = Length.Percent(-percentB);
			MoveNOW = false;	
		}
	}


}
[Flags]
public enum CurrentCategory
{
	None = 0,
	Body = 1,
	Parts =2,
	Paint  = 4,
	Behabiour =8, 
	Naming     =16
}