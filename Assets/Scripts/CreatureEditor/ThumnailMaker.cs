using SerializableTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumnailMaker : MonoBehaviour
{
    public static ThumnailMaker i => Instance as ThumnailMaker; //ms copilot lo sujerio
    public Dictionary<string, RenderTexture> UUIDsAndRedrTextures;
    private static ThumnailMaker _ins;
    public Camera MyCamMMMMMMMMMMMMMMMMMMMMMMmera;
    public static ThumnailMaker Instance
    {
        get
        {
            if (_ins == null)
                InitInstance();

            return _ins;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (_ins == null)
            _ins = this;
        if (_ins != null && _ins != this)
        {
            Destroy(gameObject);
            Destroy(MyCamMMMMMMMMMMMMMMMMMMMMMMmera.gameObject);
        }
        else if (_ins == this)
        {
            UUIDsAndRedrTextures = new Dictionary<string, RenderTexture>();

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(MyCamMMMMMMMMMMMMMMMMMMMMMMmera.gameObject);
        }
    }

    static void InitInstance()
    {
        if (_ins != null)
            return;
        GameObject game = new GameObject("THUMNAIL MAKER" + Guid.NewGuid());
        var tg = game.AddComponent<ThumnailMaker>();
        GameObject Cam = new GameObject("Camera" + Guid.NewGuid());
        var camm = Cam.AddComponent<Camera>();
        tg.MyCamMMMMMMMMMMMMMMMMMMMMMMmera = camm;
        camm.targetDisplay = 8;
        camm.enabled = false;

    }
    public void MakeThumnail(GameState gameState, GameObject gameObject, out string UUID)
    {
        MyCamMMMMMMMMMMMMMMMMMMMMMMmera.enabled = true;

        PositionCamera(gameState);
        GameObject obja = Instantiate(gameObject);
        var carggg = obja.GetComponentInChildren<CellController>();
        if (carggg != null)
        {
            Destroy(carggg);
        }
        UUID = "NotImplementedException";
        obja.transform.position = MyCamMMMMMMMMMMMMMMMMMMMMMMmera.transform.position + new Vector3(50, -50, 50);
        MyCamMMMMMMMMMMMMMMMMMMMMMMmera.transform.LookAt(obja.transform);
        RenderTexture renderTexture = new RenderTexture(256, 256, 24);
        MyCamMMMMMMMMMMMMMMMMMMMMMMmera.targetTexture = renderTexture;
        UUID = Guid.NewGuid().ToString();
        UUIDsAndRedrTextures.Add(UUID, renderTexture);
        MyCamMMMMMMMMMMMMMMMMMMMMMMmera.Render();
        MyCamMMMMMMMMMMMMMMMMMMMMMMmera.enabled = false;
    }

    void PositionCamera(GameState gameState)
    {
        switch (gameState.CurrentStage)
        {
            case Stages.Microbe:
                MyCamMMMMMMMMMMMMMMMMMMMMMMmera.transform.position = new Vector3(0, -100, 0);// al ser 2D en p1ano XZ solo nececitamos bajar la camara a posiciones Y negativas
                MyCamMMMMMMMMMMMMMMMMMMMMMMmera.farClipPlane = 200;
                break;
            case Stages.Creature:
            case Stages.tribal:
            case Stages.City:
            case Stages.Civilization:
                throw new NotImplementedException("ETAPA NO IMPLEMENTADA");
            case Stages.Space:
                if (gameState.IsInGameOrEditor)
                {
                    MyCamMMMMMMMMMMMMMMMMMMMMMMmera.transform.position = new Vector3(0, 100, 0);
                    MyCamMMMMMMMMMMMMMMMMMMMMMMmera.farClipPlane = 200;
                }
                else
                {
                    MyCamMMMMMMMMMMMMMMMMMMMMMMmera.transform.position = new Vector3(0, -100, 0);
                    MyCamMMMMMMMMMMMMMMMMMMMMMMmera.farClipPlane = 200;
                }
                break;
            case Stages.MainMenu:
                MyCamMMMMMMMMMMMMMMMMMMMMMMmera.transform.position = new Vector3(0, 1935, 0);
                MyCamMMMMMMMMMMMMMMMMMMMMMMmera.farClipPlane = 200;

                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        pass();
    }
    void pass() { return;}
    public struct GameState
    {
        public Stages CurrentStage;
        public bool IsInGameOrEditor;
        public bool IsInSystemOrMap; //para el estadio del espacios
    }
    

}
