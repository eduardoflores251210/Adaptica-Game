using ActualUtils;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenuManager : MonoBehaviour
{
	public Canvas Canvas;
	public UIDocument iHistory;
	public UIDocument iFinish; // si sigo copiandole a Apple, con iCosa.
	public InputActionAsset actions;

	[HideInInspector]
	public InputAction PauseAction;

	public bool IsPaused = false;
	public bool IsHistory = false;
	public bool IsFinished = false;

	public CellController Player;
	public event Action OnHistoryExit;

	private bool initedHis = false;
	private bool initedFin = false;
	private bool finishTriggered = false;

	private const float FINISH_PROGRESS_THRESHOLD = 150f;

	void Start()
	{
		if (Canvas != null)
			Canvas.enabled = false;

		if (actions != null)
		{
			var mapa = actions.FindActionMap("PPP", true);
			mapa.Enable();

			PauseAction = mapa.FindAction("PAUSA", true);
			PauseAction.performed += OnPausePerformed;
		}

		if (iHistory != null)
			ResumeHistory();

		if (iFinish != null)
			ResumeFinish();
	}

	private void OnDestroy()
	{
		if (PauseAction != null)
			PauseAction.performed -= OnPausePerformed;
	}

	private void OnPausePerformed(InputAction.CallbackContext ctx)
	{
		TogglePause();
	}

	private void Ect_onClick()
	{
		ToggleHistory();
		OnHistoryExit?.Invoke();
	}

	private void OnFinishOkClicked()
	{
		ToggleFinish();
	}

	public void ToggleHistory()
	{
		if (!IsHistory)
			PauseHistory();
		else
			ResumeHistory();
	}

	public void ToggleFinish()
	{
		if (!IsFinished)
			PauseFinish();
		else
			ResumeFinish();
	}

	public void PauseFinish()
	{
		if (iFinish == null)
			return;

		iFinish.gameObject.SetActive(true);
		iFinish.rootVisualElement.visible = true;

		chechFin();

		Time.timeScale = 0f;
		IsPaused = true;
		IsFinished = true;
	}

	public void ResumeFinish()
	{
		if (iFinish == null)
			return;

		chechFin();
		iFinish.rootVisualElement.visible = false;

		Time.timeScale = 1f;
		IsPaused = false;
		IsFinished = false;
	}

	public void TogglePause()
	{
		if (!IsPaused)
			Pause();
		else
			Resume();
	}

	public void Pause()
	{
		if (IsHistory || IsFinished)
			return;

		if (Canvas != null)
			Canvas.enabled = true;

		Time.timeScale = 0f;
		IsPaused = true;
	}

	public void PauseHistory()
	{
		if (iHistory == null)
			return;

		iHistory.gameObject.SetActive(true);
		iHistory.rootVisualElement.visible = true;

		chechHis();

		Time.timeScale = 0f;
		IsPaused = true;
		IsHistory = true;
	}

	public void PauseNoScreen()
	{
		Time.timeScale = 0f;
		IsPaused = true;
	}

	public void ResumeHistory()
	{
		if (iHistory == null)
			return;

		chechHis();
		iHistory.rootVisualElement.visible = false;

		Time.timeScale = 1f;
		IsPaused = false;
		IsHistory = false;
	}

	public void Resume()
	{
		if (Canvas != null)
			Canvas.enabled = false;

		Time.timeScale = 1f;
		IsPaused = false;
	}

	public void Save()
	{
		if (Player == null)
			throw new GameSavingException("EL JUGADOR ES NULL");

		if (!Saver.HasLoadedAnySave())
			throw new GameSavingException("ESTADO INVALIDO");

		var STD = Saver.CurrentGame.CurentStage;

		if (STD == SerializableTypes.Stages.Microbe)
		{
			// IMPORTANTÍSIMO:
			// conservar el estado Finished actual para no resetear el save
			bool wasFinished = Saver.CurrentGame.CellGameData.Finished;

			Saver.CurrentGame.CellGameData = new()
			{
				DNA_Amount = Player.CurrentEvoPoints,
				MaxDNA_Got = Player.MaxEvoPointsGotStat,
				Gender = Player.CurrentGen,
				PlayerHealth = Player.Health,
				Progress = Player.StageProgress,
				Finished = wasFinished
			};
		}
		else if (STD == SerializableTypes.Stages.Creature)
		{
			throw new GameSavingException("NO IMPLEMENTADO AUN");
		}
		else if (STD == SerializableTypes.Stages.MainMenu)
		{
			throw new GameSavingException("NO SE PUEDE GUARDAR EN EL MENU PRINCIPAL");
		}

		Saver.SaveCurrentGame();
	}

	public void Exit()
	{
		Resume();
		Saver.UnloadCurrentGame(false);
	}

	void chechHis()
	{
		if (initedHis)
			return;

		if (iHistory == null)
			return;

		if (!iHistory.enabled)
			return;

		var roo = iHistory.rootVisualElement;
		if (roo == null)
			return;

		var ect = roo.Q<Button>("exit_history");
		if (ect == null)
		{
			Debug.LogError("NO SE ENCONTRO EL BOTON 'exit_history' EN LA INTERFAZ DE HISTORIA.");
			return;
		}

		ect.clicked -= Ect_onClick;
		ect.clicked += Ect_onClick;

		initedHis = true;
	}

	void chechFin()
	{
		if (initedFin)
			return;

		if (iFinish == null)
			return;

		if (!iFinish.enabled)
			return;

		var roo = iFinish.rootVisualElement;
		if (roo == null)
			return;

		var ect = roo.Q<Button>("OK");
		if (ect == null)
		{
			Debug.LogError("NO SE ENCONTRO EL BOTON DE OK EN LA INTERFAZ DE FINISH, ASEGURATE DE QUE EL NOMBRE DEL BOTON SEA EXACTAMENTE 'OK' Y QUE ESTE DENTRO DEL ROOT DE LA INTERFAZ");
			return;
		}

		ect.clicked -= OnFinishOkClicked;
		ect.clicked += OnFinishOkClicked;

		initedFin = true;
	}

	private void Update()
	{
		if (finishTriggered)
			return;

		if (!Saver.HasLoadedAnySave())
			return;

		if (Player == null)
			return;

		if (Saver.CurrentGame.CurentStage != SerializableTypes.Stages.Microbe)
			return;

		if (Saver.CurrentGame.CellGameData.Finished)
			return;

		if (Player.StageProgress < FINISH_PROGRESS_THRESHOLD)
			return;

		// Trigger de fin de prototipo: SE MANTIENE
		finishTriggered = true;

		//if (Saver.CurrentGame.CellGameData != null) //el tonto de ChatGPT SE OLVIDSO QUE ESO ES Struct Y NO CLass  (idiot gpt)
			Saver.CurrentGame.CellGameData.Finished = true;

		Saver.SaveCurrentGame();

		if (iFinish != null)
		{
			ToggleFinish();
			chechFin();
		}
	}


}
//al finalizar el juego la dopamina se dispara por finalmente haber terminado el prototipo;