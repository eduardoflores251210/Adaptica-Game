using ActualUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenuManager : MonoBehaviour
{
	public Canvas Canvas;
	public UIDocument iHistory;
	public InputActionAsset actions;
	[HideInInspector]
	public InputAction PauseAction;
	public bool IsPaused = false;
	public bool IsHistory = false;
	public CellController Player;
	public event Action OnHistoryExit;
	bool initedHis = false;
	// Start is called before the first frame update
	void Start()
	{
		Canvas.enabled = false;
		var mapa = actions.FindActionMap("PPP", true);
		mapa.Enable();
		PauseAction = mapa.FindAction("PAUSA", true);
		PauseAction.performed += delegate
		{
			TogglePause();
		};

		if (iHistory != null )
		{
			ResumeHistory();
		}
	}

	private void Ect_onClick()
	{
		ToggleHistory();
		OnHistoryExit?.Invoke();
		
	}

	public void ToggleHistory()
	{
		if (!IsHistory)
		{
			PauseHistory();
		}
		else
		{
			ResumeHistory();
		}
	}
	public void TogglePause()
	{

		if (!IsPaused)
		{
			Pause();
		}
		else
		{
			Resume();
		}

	}

	public void Pause()
	{
		if (IsHistory)
			return;
		Canvas.enabled = true;
		Time.timeScale = 0f;
		IsPaused = true;
	}
	public void PauseHistory()
	{

		iHistory.rootVisualElement.visible = 
		(true);
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
		chechHis();
		iHistory.rootVisualElement.visible = false;
		Time.timeScale = 1f;
		IsPaused = false;
		IsHistory = false;
	}
	public void Resume()
	{
		Canvas.enabled = false;
		Time.timeScale = 1f;
		IsPaused = false;
	}
	public void Save()
	{
		if (Player == null)
			throw new GameSavingException("EL JUGADOR ES NULL");
		if (Saver.HasLoadedAnySave())
		{
			//mejor pongo una variable para acceder facilmente al estado actual
			var STD = Saver.CurrentGame.CurentStage;
			if (STD == SerializableTypes.Stages.Microbe)

			{
				Saver.CurrentGame.CellGameData = new
				()
				{ DNA_Amount = Player.CurrentEvoPoints, MaxDNA_Got = Player.MaxEvoPointsGotStat, Gender = Player.CurrentGen, PlayerHealth = Player.Health, Progress = Player.StageProgress };
			}
			//else if (STD == SerializableTypes.Stages.Creature)
			//{
			//
			//}
			//aun no implementado asi que por eso son comentarios
			else if (STD == SerializableTypes.Stages.Creature)
			{
				throw new GameSavingException("NO IMPLEMENTADO AUN");
			}
			else if (STD == SerializableTypes.Stages.MainMenu)
			{
				throw new GameSavingException("????????????????\nNO SE PUEDE GUARDAR EN EL MENU PRINCIPAL");// el juego se confunde por que quieres guardar en el menu principal
			}
			Saver.SaveCurrentGame();
		}else
		{
			var EX = new GameSavingException("ESTADO INVALIDO");

			throw EX;
		}
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
		if (iHistory != null)
		{
			if (!iHistory.enabled)
				return;
			var roo = iHistory.rootVisualElement;
			if (roo == null)
			{
				initedHis = false;
				return;
			}
			var ect = roo.Q<Button>("exit_history");
			
			ect.clicked += Ect_onClick;
			initedHis = true;
		}
	}
	private void Update()
	{
		if (iHistory != null)
		{
			chechHis();
		}
	}
}

