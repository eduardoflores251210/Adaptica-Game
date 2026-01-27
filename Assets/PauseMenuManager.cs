using ActualUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PauseMenuManager : MonoBehaviour
{
    public Canvas Canvas;
    public InputActionAsset actions;
    [HideInInspector]
    public InputAction PauseAction;
    public bool IsPaused = false;
    public CellController Player;
    // Start is called before the first frame update
    void Start()
    {
        Canvas.enabled = false;
        var mapa = actions.FindActionMap("PPP",true);
        mapa.Enable();
        PauseAction = mapa.FindAction("PAUSA",true);
        PauseAction.performed += delegate
        {
            if (!IsPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        };
    }

    public void Pause()
    {
		Canvas.enabled = true;
		Time.timeScale = 0f;
		IsPaused = true;
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
            Saver.CurrentGame.CellGameData = new
                ()
            { DNA_Amount = Player.CurrentEvoPoints, MaxDNA_Got = Player.MaxEvoPointsGotStat, Gender = Player.CurrentGen, PlayerHealth = Player.Health, Progress = Player.StageProgress };

            Saver.SaveCurrentGame();
        }else
        {
            var EX = new GameSavingException("ESTADO INVALIDO");

			throw EX;
        }
    }
    public void Exit()
    {
        Saver.UnloadCurrentGame(false);
    }
}

