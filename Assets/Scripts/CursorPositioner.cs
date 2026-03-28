using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class CursorPositioner : OnScreenControl
{
	public Cursor aCursor;
	public Canvas CursorCanvas;
	[InputControl(layout = "Vector2")]
	[SerializeField]
	private string m_ControlPath;
	// Start is called before the first frame update
	void Start()
    {

    }


	// Update is called once per frame
	void Update()
	{
		Vector2 ScreenPos = Vector2.zero;

		RectTransform rect = aCursor.transform as RectTransform;

		switch (CursorCanvas.renderMode)
		{
			case RenderMode.ScreenSpaceOverlay:
				ScreenPos = RectTransformUtility.WorldToScreenPoint(
					null,
					rect.position
				);
				break;

			case RenderMode.ScreenSpaceCamera:
				ScreenPos = RectTransformUtility.WorldToScreenPoint(
					CursorCanvas.worldCamera,
					rect.position
				);
				break;

			case RenderMode.WorldSpace:
				ScreenPos = RectTransformUtility.WorldToScreenPoint(
					Camera.main,
					rect.position
				);
				break;
		}

		SendValueToControl(ScreenPos);
	}
	protected override string controlPathInternal
	{
		get => m_ControlPath;
		set => m_ControlPath = value;
	}
}
