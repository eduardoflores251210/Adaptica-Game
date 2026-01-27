using UnityEngine;
using UnityEngine.UIElements;

public class CellHudSync : MonoBehaviour
{
	public UIDocument document;
	public CellController Player;
	public PauseMenuManager PauseManager;
	ProgressBar ProgressBar95;//Jeje referencia a progressbar95
	float MaxPoints = 260f;

	void Start()
	{
		ProgressBar95 = document.rootVisualElement.Q<ProgressBar>();
		var a = document.rootVisualElement.Q<Button>("Settings");
		a.clicked += delegate
		{
			if (PauseManager != null)
			{
				PauseManager.TogglePause();
			}
		};
	}
	void Update()
	{
		if (document == null)document = GetComponent<UIDocument>(); //solucion propuesta por Unity
		
		if (Player == null)Player =  GetComponent<CellController>(); //unity esta haciendo drama por que dice que  "Unity objects should not use coalescing assignment."


		if (ProgressBar95 == null) ProgressBar95 = document.rootVisualElement.Q<ProgressBar>();

		//comprobación paranoica
		if (ProgressBar95 == null||document == null || Player == null)
		{
			Debug.LogError("💥✨ SYSTEM FAILURE: A VALUABLE REFERENCE HAS TRAGICALLY PERISHED ✨💥");
			return;
		}
		float progress = Player.StageProgress;
		float normalized = (progress / MaxPoints) * 100f;
		normalized = Mathf.Clamp(normalized, 0f, 100f);
		ProgressBar95.value = normalized;
	}
}












































// e^iPi=-1
//Caos del gato rf okdffhdosiudfgrfgygsdfdfgsgdf huj trhijuyg nmdyyyyykyyyywert ui89owqer9o ^rdrrreddtgtfhtghytghtg grf