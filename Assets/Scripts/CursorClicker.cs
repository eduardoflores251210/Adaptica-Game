using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class CursorClicker : OnScreenControl
{
	public Cursor aCursor;

	[InputControl(layout = "Button")]
	[SerializeField]
	private string m_ControlPath;
	// Start is called before the first frame update
	void Start()
    {
		if (aCursor != null)
		{
			aCursor.ClickA += OnPointerUp;	
		}
    }
	public void OnPointerUp()
	{
		SendValueToControl(0.0f);
		StartCoroutine(nameof(enumerator));
		
	}
	IEnumerator enumerator()
	{
		float timer = 0.01f;
		while (true)
		{
			yield return new WaitForSeconds(timer);
			break;
		}
		OnPointerDown();
	}
	public void OnPointerDown()
	{
		SendValueToControl(1.0f);
	}
	// Update is called once per frame
	void Update()
    {
        
    }
	protected override string controlPathInternal
	{
		get => m_ControlPath;
		set => m_ControlPath = value;
	}
}
