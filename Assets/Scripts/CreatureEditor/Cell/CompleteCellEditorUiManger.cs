using UnityEngine;
using UnityEngine.UIElements;

public class CompleteCellEditorUiManger : MonoBehaviour
{
	[SerializeField]
	public UIDocument iDocumentA; 
	public UIDocument iDocumentB;
	public currentCateory currentCat;
}
public enum currentCateory
{
	Body, Parts,
	Paint, Reproduction
}