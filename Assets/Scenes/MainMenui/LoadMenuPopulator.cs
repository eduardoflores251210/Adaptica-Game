using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;
using SerializableTypes.Space;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadMenuPopulator : MonoBehaviour
{
    
    private string SavesFolderPath = "";
    public string ContainerName;
    public VisualTreeAsset buttonTemplate; // Plantilla de botón en UI Toolkit
    public UIDocument document;
    private VisualElement ButtonContaniner;
    private VisualElement Welement;
    public string ElementoDeVentana = "WindowRoot";
    public Button CloseButton;
    public string CloseName = "Close";
    public Button OpenButton;
    public string OpenName;


    private void OnEnable()
    {
        SavesFolderPath = Paths.SaveFiles; //eso esta mal ahora es subcarpetas por archivo de guardado

		// Obtener referencia al root VisualElement del UI Document
		var root = document.rootVisualElement;
        ButtonContaniner = root.Q(ContainerName);
        Welement = root.Q<VisualElement>(ElementoDeVentana);
       
        CloseButton = root.Q<Button>(CloseName);
        OpenButton = root.Q<Button>(OpenName);
        CloseButton.clicked += () => toggleWindow();
        if (OpenButton != null)
            OpenButton.clicked += () => toggleWindow();
        LoadButtons();
        toggleWindow(); //inicia activa pero hay que ocultarla al inicio mejor
    }
    public void toggleWindow()
    {
        if (Welement.style.display == DisplayStyle.None)
            Welement.style.display = DisplayStyle.Flex; // o Grid, según tu UXML
        else
            Welement.style.display = DisplayStyle.None;
    }

    public static bool validateSave(string Json)
    {
        return validateSave(Json, out _);
    }
    public static bool validateSave(string Json, out SavedGame game)
    {
        game = null;
        if (string.IsNullOrEmpty(Json))
        {
            return false;
        }
        SavedGame savefile = JsonUtility.FromJson<SavedGame>(Json);
        if (savefile != null)
        {
            game = savefile;
            if (savefile.CreatureName == null)
            {
                return false ;
            } if (savefile.isCPUEmpire)
            {
                return false;
            }
            return true; 
        }else
            return false;
    }
    bool IsValidNameForRepair(string FileName)
    {

        if  (!string.IsNullOrEmpty(FileName))
        {
            string NoExtFilNam = FileName.Replace(".json", "");
            string NoPrefFilNam = NoExtFilNam.Replace("Game", "");
            if (int.TryParse(NoPrefFilNam, out var id))
            {
                return true;
            }
            else return false;
        }    else return false;
    }
    bool TryRepair(string contents, string FilePath)
    {
        try
        {
            if (!validateSave(contents, out var game))
            {
                if (game == null)
                {
                    string FileName = Path.GetFileName(FilePath);
                    if (IsValidNameForRepair(FileName))
                    {

                        string NoExtFilNam = FileName.Replace(".json", "");
                        string NoPrefFilNam = NoExtFilNam.Replace("Game", "");
                        ulong id = ulong.Parse(NoPrefFilNam);
                        string[] cells = Directory.GetFiles(Paths.Cells);
                        if (cells.Length > 0)
                        {
                            string rnd = cells[Random.Range(0, cells.Length)];
                            string Cell = Path.GetFileNameWithoutExtension(rnd);
                            game = new SavedGame(id, false, Stages.Microbe, Cell, new List<HistoryActions>(), Diets.Omnivore, 0d);
                            var newContent = JsonUtility.ToJson(game);
                            File.WriteAllText(FilePath, newContent);
                            return true;
                        }
                        else return false;

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (game.CreatureName == null)
                    {
                        string[] cells = Directory.GetFiles(Paths.Cells);
                        if (cells.Length > 0)
                        {
                            string rnd = cells[Random.Range(0, cells.Length)];
                            string Cell = Path.GetFileNameWithoutExtension(rnd);
                            game.CreatureName = Cell;
                            var newContent = JsonUtility.ToJson(game);
                            File.WriteAllText(FilePath, newContent);
                            return true;
                        }
                        else return false;
                    }
                    else if (game.isCPUEmpire)
                    {
                        return false; // No queremos imperios de CPU en el menu de guardar causaria caos
                    }
                    else return false; //no se que pasa;

                }
            }
            else return true; //el juego esta bien
        }
        catch (System.Exception) { return false; }
    }

    private void LoadButtons()
    {
        if (!Directory.Exists(SavesFolderPath))
        {
            Debug.LogWarning($"La carpeta {SavesFolderPath} no existe.");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(SavesFolderPath, "*.json"); //logica inutil por que no es compatible con el nuevo sistema de guardado por carpetas
		int buttonCount = 0;
        var ACTUALSAVES = Saver.ListSavefiles();

		foreach (var fil in ACTUALSAVES)
        {
            string fil2 = Path.Combine(SavesFolderPath, fil);
			string file = Path.Combine(fil2, "Save.json"); //ahora cada guardado es una carpeta con varios archivos dentro
			Debug.Log(file);
            string content = File.ReadAllText(file).Trim();
            if (TryRepair(content,file))
            {
                
                // Instanciar el botón desde la plantilla
                var button = buttonTemplate.CloneTree().Q<Button>();
                button.text = Path.GetFileNameWithoutExtension(file);

                // Ejemplo de acción al presionar el botón
                button.clicked += () =>
				{
					Saver.LoadGameComplete(fil);
				};

                // Añadir al contenedor
                ButtonContaniner.Add(button);

                buttonCount++;
            }
        }

        Debug.Log($"{buttonCount} botones creados desde JSONs no vacíos.");
    }
}
