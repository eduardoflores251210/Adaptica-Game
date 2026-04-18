using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Icon("Assets/Textures/UI/Editor/SaveMicrobeICON.png")]
public class CellSaver : MonoBehaviour
{
	public NamedescManager namedescManager;
	public PartManager partmanager;
	public CompleteCellEditorUiManger phasemanager;
	public SegmentManager segmentmanager;
	

	/// <summary>
	/// Guarda el microbio
	/// </summary>
	public void Save()
	{
		string SavePath = Paths.Cells;
		bool ooo = !System.IO.Directory.Exists(SavePath);
		if (ooo) { System.IO.Directory.CreateDirectory(SavePath); }
		if (partmanager == null ||  phasemanager == null || segmentmanager == null || namedescManager == null)
		{
			return;
		}
		List<PartComp> Fp = new List<PartComp>();
		List<PartComp> Mp = new List<PartComp>();
		bool hasmale;
		if (partmanager.MaleParts != null) 
		{
			if (partmanager.MaleParts.Count == 0)
			{
				hasmale = false;
			}
			else hasmale = true;
		} else { hasmale = false ;}
		foreach (var f in partmanager.FemaleParts) {
			Fp.Add(f.GetComponent<PartComp>());
		} 
		if (hasmale)
		{
			foreach(var m in partmanager.MaleParts)
			{
				Mp.Add(m.GetComponent<PartComp>());
			}
		}
		MicrobeData microbe = SerializeMicrobe(segmentmanager.Segments, Fp, Mp, namedescManager.Name, namedescManager.Description, partmanager.MaleColor, partmanager.FemaleColor, partmanager.IsDimorphic(), phasemanager.Mesh, partmanager.ReproductionType, partmanager.ReproductionMethod);
		microbe.CenterMicrobe();// centramos la celula antes de guardarla
								//err Microbio no celula SIEMPRE me confundo de termino
		string json = JsonUtility.ToJson(microbe, prettyPrint: true);
		string fullPath = System.IO.Path.Combine(SavePath, $"{microbe.Name}.json");
		System.IO.File.WriteAllText(fullPath, json);
        Debug.Log("¡Célula guardada en: " + fullPath + "!");

        bool Load_Stage = false;
        PlanetData planetfat = new();
        string LdStgSndr = "";
        var Mailman = CrossScenePackageSender.Instance;
        if (Mailman != null)
        {
            // Revisar paquetes tipados tipo bool
            if (Mailman.IsThereAnyTypedMailForHim<bool>("CellSaver", out var boolMail))
            {
                foreach (var pkg in boolMail)
                {
                    if (pkg.Tags.Length > 1 && pkg.Tags[1] == "LodStg")
                    {
                        Load_Stage = pkg.Contents;
                        LdStgSndr = pkg.Sender;
                        Mailman.DeleteMyPackage(pkg);
                    }
                }
            }
			
            // Revisar paquetes tipados tipo PlanetData
            if (Mailman.IsThereAnyTypedMailForHim<PlanetData>("CellSaver", out var planetMail))
            {
                planetfat = planetMail[0].Contents;
				Debug.Log(planetMail[0].Contents.ToString());
                Mailman.DeleteMyPackage(planetMail[0]);
            }

        }
		//aqui falla
        if (Load_Stage)
        {
            Mailman.SendTypedPackage(gameObject.name, "Player", microbe, new string[1] { nameof(MicrobeData) });
            if (LdStgSndr != "EnterEdit")
            {
                //bien llegamos a MicrobeSaver ahora vamos a elimianr ese codigo obsoelto para guardar usando Saver
                SavedGame game = new SavedGame(planetfat.id, false, Stages.Microbe, microbe.Name, new(), GetDiet(microbe), 0d);
                Saver.CreateSavefile(microbe.Name, BodyID.FromString(planetfat.id).GetID(), out string NAME);
				Saver.SaveGame(game, NAME);
				Saver.SaveMicrobeRevision(NAME, microbe);
				Saver.LoadGameComplete(NAME);
			}
			else
				StageLoader.LoadCurrentStage();
		}
        else SceneManager.LoadScene(0); // menu principal
    }
	public void ExitWitourthSaving()

	{
		bool Load_Stage = false;
		PlanetData planetfat = new();
		string LdStgSndr = "";
		var Mailman = CrossScenePackageSender.Instance;
		if (Mailman != null)
		{
			// Revisar paquetes tipados tipo bool
			if (Mailman.IsThereAnyTypedMailForHim<bool>("CellSaver", out var boolMail))
			{
				foreach (var pkg in boolMail)
				{
					if (pkg.Tags.Length > 1 && pkg.Tags[1] == "LodStg")
					{
						Load_Stage = pkg.Contents;
						LdStgSndr = pkg.Sender;
						Mailman.DeleteMyPackage(pkg);
					}
				}
			}

			// Revisar paquetes tipados tipo PlanetData
			if (Mailman.IsThereAnyTypedMailForHim<PlanetData>("CellSaver", out var planetMail))
			{
				planetfat = planetMail[0].Contents;
				Mailman.DeleteMyPackage(planetMail[0]);
			}
		}
		if (Load_Stage)
		{
			MicrobeData microbe = Saver.TryToLoadLastMicrobeRevision(Saver.CurrentSaveName, out var data) ? data : null;

			Mailman.SendTypedPackage(gameObject.name, "Player", microbe, new string[1] { nameof(MicrobeData) });
			if (LdStgSndr != "EnterEdit")
			{
				//bien llegamos a MicrobeSaver ahora vamos a elimianr ese codigo obsoelto para guardar usando Saver
				SavedGame game = new SavedGame(planetfat.id, false, Stages.Microbe, microbe.Name, new(), GetDiet(microbe), 0d);
				Saver.CreateSavefile(microbe.Name, BodyID.FromString(planetfat.id).GetID(), out string NAME);
				Saver.SaveGame(game, NAME);
				Saver.SaveMicrobeRevision(NAME, microbe);
				Saver.LoadGameComplete(NAME);
			}
			else
				StageLoader.LoadCurrentStage();
		}
		else
			StageLoader.LoadStage(Stages.MainMenu);
	}
	private Diets GetDiet(MicrobeData microbe)
	{
		Diets diet = Diets.none;
		bool hasHerb = false;
		bool HasOmn = false;
		bool hasCarn = false;
		foreach (var pf in microbe.PartsF)
		{
			var pt = partmanager.Database.GetPartByID(pf.Id);
			if (pt.categories.Contains(PartCategories.Mouths))
			{
				if (pt.tags.Contains("Herb"))
				{
					hasHerb = true;
				}
				if (pt.tags.Contains("Carn"))
				{
					hasCarn = true;
				}
				if (pt.tags.Contains("Omn"))
				{
					HasOmn = true;
				}
			}
		}
		if (hasHerb && !( hasCarn || HasOmn))
		{
			diet = Diets.Herbivore;
		}
		if (hasCarn &&  !( hasHerb || HasOmn))
		{
			diet = Diets.Carnivore;
		}
		if (HasOmn || (hasHerb && hasCarn))
		{
			diet = Diets.Omnivore;
		}
		return diet;
	}

	SegmentData SerializeSegment(Metaball mt)
	{
		SegmentData  segment = new SegmentData();
		segment.radius = mt.Radius;
		segment.transform = (StdUtils.Serializable.Transform)mt.transform;
		return segment;
	}
	SerializedPartData SeriallizePart(PartComp part)
	{
		SerializedPartData data = new(part.ID, (StdUtils.Serializable.Transform)part.transform);
		return data;
	}
	MicrobeData SerializeMicrobe(List<Metaball> Segments, List<PartComp> PartsF, List<PartComp> PartsM, String Name, String Descrition, Color MaleColor, Color FemaleColor,Boolean HasMale, UnityEngine.Mesh mesh,reproductionTypes reproductionTypes,ReproductionMethod reproductionMethod)
	{
		List<SerializedPartData> PartsSM = new List<SerializedPartData>();
		List<SerializedPartData> PartsSF = new List<SerializedPartData>();
		List<SegmentData> Segs = new List<SegmentData>();
		foreach (var part in PartsF)
		{
			PartsSF.Add(SeriallizePart((PartComp)part));
		}
		foreach (var part in PartsM)
		{
			PartsSM.Add(SeriallizePart((PartComp)part));
		}
		foreach (var sg  in Segments)
		{
			Segs.Add(SerializeSegment(sg));
		}
		StdUtils.Serializable.Mesh Smesh = (StdUtils.Serializable.Mesh)mesh;
		return new MicrobeData(Name, Descrition, HasMale, reproductionTypes, reproductionMethod, PartsSF, PartsSM, FemaleColor, MaleColor,Segs,Smesh);
	}
}
public static class Paths
{
	public static string Cells = Path.Join(Application.persistentDataPath, "SavedCells");
	public static string Plants = Path.Join(Application.persistentDataPath, "SavedPlants");
	public static string Creatures = Path.Join(Application.persistentDataPath, "SavedCreatures");
	public static string TribalCreatures = Path.Join(Application.persistentDataPath, "SavedTribes");
	public static string FeudalCreatures = Path.Join(Application.persistentDataPath, "SavedFeudal");
	public static string NationCreatures = Path.Join(Application.persistentDataPath, "SavedCitizen");
	public static string Galaxy = Path.Join(Application.persistentDataPath, "Galaxy");
    public static string GalaxySectors = Path.Join(Galaxy, "Region");
    public static string Planets = Path.Join(Paths.GalaxySectors, "Planets");
    public static string Baricenters = Path.Join(Paths.GalaxySectors, "Baricenters");
    public static string MiscGalaxy = Path.Join(Paths.GalaxySectors, "Misc");
    public static string SaveFiles = Path.Join(Application.persistentDataPath, "Saves"); 
    public static string BackUPCreations = Path.Join(Application.persistentDataPath, "BackUps");
	public static string BackUPCells = Path.Join(BackUPCreations, "Cells");
	public static string BackUPCreatures = Path.Join(BackUPCreations, "Creatures");
	public static string NewGameCache = Path.Join(Application.persistentDataPath, "CACHENS.json");
	public static string Mods = Path.Join(Application.persistentDataPath, "Mods");
	public static string ModParts = Path.Join(Mods, "Parts");

}