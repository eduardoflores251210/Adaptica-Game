using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompleteCellEditorUiManger : MonoBehaviour
{
	public UIDocument iDocument; 
	public CurrentCategory currentCat;


	public MetaballManager metaball;
	public CellSaver saver;
	public Mesh Mesh;
	private TabView PartsAndBody;
	private TabView PaintAndBehabiour;
	public void Start()
	{
		var root = iDocument.rootVisualElement;
		PartsAndBody = root.Q<TabView>("BP");
		PaintAndBehabiour = root.Q<TabView>("PB");

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
			var aCat = CurrentCategory.None;
			if (PartsAndBody.activeTab.name == "BOD")
			{
				aCat |= CurrentCategory.Body;
			}
			if (PartsAndBody.activeTab.name == "Part")
			{
				aCat |= CurrentCategory.Parts;
			}
			Debug.Log(PartsAndBody.tabIndex.ToString());
			NewCat |= aCat;
		}
		if (PaintAndBehabiour != null)
		{
			var bCat = CurrentCategory.None;
			if (PaintAndBehabiour.activeTab.name == "pin")
			{
				bCat |= CurrentCategory.Paint;
			}
			if (PaintAndBehabiour.activeTab.name == "Beh")
			{
				bCat |= CurrentCategory.Behabiour;
			}
			NewCat |= bCat;
		}
		currentCat= NewCat;
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