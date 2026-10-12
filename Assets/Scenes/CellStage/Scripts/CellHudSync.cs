using UnityEngine;
using UnityEngine.UIElements;

public class CellHudSync : MonoBehaviour
{
	public UIDocument document;
	public CellController Player;
	public PauseMenuManager PauseManager;
	ProgressBar ProgressBar95;//Jeje referencia a progressbar95
	ProgressBar BarOS; // jeje referencia a barOS de Progressbar95
	Button DNA_DISPLAY; // no hay referencia a videojuego en este XD
	public float MaxPoints = 150f; //ah con razon es muy tardado temrinar el juego. listo ahora es 150 puntos :)

	void Start()
	{
		ProgressBar95 = document.rootVisualElement.Q<ProgressBar>("StageProgress");
		BarOS = document.rootVisualElement.Q<ProgressBar>("Health");
		var a = document.rootVisualElement.Q<Button>("Settings");
		var b = document.rootVisualElement.Q<Button>("Evolution");
		DNA_DISPLAY = document.rootVisualElement.Q<Button>("DNA_Text");//si ni se porque es un boton pero bueno XD
		a.clicked += delegate
		{
			if (PauseManager != null)
			{
				PauseManager.TogglePause();
			}
		};
		b.clicked += delegate
		{
			if (PauseManager != null)
			{
				PauseManager.ToggleHistory();
			}
		};
	}
	void Update()
	{
		if (document == null)document = GetComponent<UIDocument>(); //solucion propuesta por Unity
		
		if (Player == null)Player =  GetComponent<CellController>(); //unity esta haciendo drama por que dice que  "Unity objects should not use coalescing assignment."
		if (DNA_DISPLAY == null) DNA_DISPLAY = document.rootVisualElement.Q<Button>("DNA_Text");

		if (ProgressBar95 == null) ProgressBar95 = document.rootVisualElement.Q<ProgressBar>("StageProgress");
		if (BarOS == null) BarOS = document.rootVisualElement.Q<ProgressBar>("Health");

		//comprobación paranoica
		if (ProgressBar95 == null||document == null || Player == null)
		{
			Debug.LogError("💥✨ SYSTEM FAILURE: A VALUABLE REFERENCE HAS TRAGICALLY PERISHED ✨💥");//creo que el script se cree mettaton de undertale.
			return;
		}
		//comprobación paranoica 2
		if (BarOS == null || document == null || Player == null)
		{
			Debug.LogError("💥✨ SYSTEM FAILURE: A VALUABLE REFERENCE HAS TRAGICALLY PERISHED ✨💥");//creo que el script se cree mettaton de undertale.
			return;
		}
		//comprobación paranoica 3
		if (DNA_DISPLAY == null || document == null || Player == null)
		{
			Debug.LogError("💥✨ SYSTEM FAILURE: A VALUABLE REFERENCE HAS TRAGICALLY PERISHED ✨💥");//creo que el script se cree mettaton de undertale.
			return;
		}
		float progress = Player.StageProgress;
		float normalized = (progress / MaxPoints) * 100f;
		normalized = Mathf.Clamp(normalized, 0f, 100f);
		ProgressBar95.value = normalized;
		BarOS.lowValue = 0f;
		BarOS.highValue = Player.MaxHealth;
		BarOS.value = Player.Health;
		DNA_DISPLAY.text = $"Đ{Player.CurrentEvoPoints}"; //si Đ porque parece un simbolo de moneda XD
	}
}












































// e^iPi=-1
//Caos del gato rf okdffhdosiudfgrfgygsdfdfgsgdf huj trhijuyg nmdyyyyykyyyywert ui89owqer9o ^rdrrreddtgtfhtghytghtg grf