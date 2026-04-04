using SerializableTypes;
using System;
using Random = UnityEngine.Random; //para no confundir
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SerializableTypes.Space; 
using UnityEngine.UI;
using System.IO;

public class NewGameMaker : MonoBehaviour
{
	public GalaxyGenerator GalaxyGenerator;
	public bool AllowStars = true;
	public bool AllowRouguePlanetsMoons = false;

	private void Start()
	{
		GalaxyGenerator.OnGalaxyGenerated += GeneratePlanetCache;
		if (File.Exists(Paths.NewGameCache))
			File.Delete(Paths.NewGameCache);
		GetCache();
	}

	private void GeneratePlanetCache()
	{
		if (!(AllowStars && AllowRouguePlanetsMoons))
		{
			Debug.Log("¿??????????????????????????????????????????");
			AllowStars = AllowRouguePlanetsMoons = true;//si no permites nada permites todo
		}
		if (GalaxyData.TryToLoadGalaxy(out var es))
		{
			// Filtrar planetas Terra y Jungle
			List<PlanetData> f = es.LookForTypePlanet(PlanetTypes.Terra);
			List<PlanetData> g = es.LookForTypePlanet(PlanetTypes.Jungle);
			List<PlanetData> candidates = f.Concat(g).ToList();
			PlanetCacheFile cacheFile = new PlanetCacheFile();
			int i = 0;
			foreach (var p in candidates.OrderBy(x => Random.value))
			{
				// Intentar encontrar un planeta válido según reglas

				if (!AllowStars && p.ParentID.StartsWith("E"))
					continue; // ignorar planetas con estrella

				if (!AllowRouguePlanetsMoons && p.ParentID.StartsWith("P"))
					continue; // ignorar planetas solitarios
				if (p.IsSaveFile) { continue; }
				var chosenPlanet = p;
				if (Random.value < 0.5)
				{
					if (cacheFile.PlanetIds == null)
						cacheFile.PlanetIds = new();
					cacheFile.PlanetIds.Add(p.id);
					if (cacheFile.ParrentTypes == null)
						cacheFile.ParrentTypes = new();
					cacheFile.ParrentTypes.Add( (p.type).ToString());
					i++;
				}
				if (i >= 6)
				break;

			}
			string path = Paths.NewGameCache;
			string JSON = JsonUtility.ToJson(cacheFile);
			File.WriteAllText(path, JSON);
		}

	}
	public PlanetCacheFile GetCache()
	{
		if (!File.Exists(Paths.NewGameCache))
			GeneratePlanetCache();

		string path = Paths.NewGameCache;
		string JSON = File.ReadAllText(path);
		var Cachefile = JsonUtility.FromJson<PlanetCacheFile>(JSON);
		if (Cachefile.PlanetIds == null)
		{
			GeneratePlanetCache();
			JSON = File.ReadAllText(path);
			Cachefile = JsonUtility.FromJson<PlanetCacheFile>(JSON);
		}
		return Cachefile;
	}
	public void OverWriteCache(PlanetCacheFile cacheFile)
	{
		string path = Paths.NewGameCache;
		string JSON = JsonUtility.ToJson(cacheFile);
		File.WriteAllText(path, JSON);
	}
	// esto es publico por razones UGUI
	public void NewCell() //MICROBIO PERO POR MALA MEMORIA SE LLAMA CELL COMO CELULA
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(1); //carga el editor de micrboio
	}
	public void NewPlant()
	{
		LoadWithLoadingScreen.LoadScene(5, Stages.Creature); //carga el editor de plantas
	}
	//razones UGUI
	public void NewGame()
	{
		if (!(AllowStars && AllowRouguePlanetsMoons))
		{
			Debug.Log("¿?????????????????????????????????????????? si no permites nada permites todo"); //el script esta confundido ante tus cuestionables decisiones, pero no se atreve a decirlo en voz alta
			AllowStars = AllowRouguePlanetsMoons = true;//si no permites nada permites todo
		}
		var ins = CrossScenePackageSender.Instance;

		if (GalaxyData.TryToLoadGalaxy(out var es))
		{
			// Filtrar planetas Terra y Jungle
			List<PlanetData> candidates = new()
				;
			var cache = GetCache();
			foreach (var item in cache.PlanetIds)
			{
				candidates.Add(es.LoadPlanet(BodyID.FromString(item).GetID())	);
			}
			PlanetData chosenPlanet = null;
			// Intentar encontrar un planeta válido según reglas
			foreach (var p in candidates.OrderBy(x => Random.value)) // barajar aleatoriamente
			{
				if (!AllowStars && p.ParentID.StartsWith("E"))
					continue; // ignorar planetas con estrella

				if (!AllowRouguePlanetsMoons && p.ParentID.StartsWith("P"))
					continue; // ignorar planetas solitarios
				if (p.IsSaveFile)
					continue;
				chosenPlanet = p;
				break;
			}

			if (chosenPlanet == null)
			{
				// No se encontró ningún planeta válido
				ShowBSOD("No se encontró ningún planeta válido", 0x01);
				return;
			}

			string planetID = chosenPlanet.id;
			PlanetData New = chosenPlanet;
			New.IsSaveFile = true;
			es.UpdatePlanet(BodyID.FromString(planetID).GetID(), planetData => { planetData = New; });
			if (chosenPlanet.ParentID.StartsWith("E"))
			{
				StarData star = es.LookForStar(BodyID.FromString(chosenPlanet.ParentID).GetID());

				GC.Collect();

				ins.SendTypedPackage<String>(gameObject.name, "Star", planetID, new string[1] { nameof(String) });
				ins.SendTypedPackage<StarData>(gameObject.name, "Star", star, new string[1] { nameof(StarData) });
				ins.SendTypedPackage<bool>(gameObject.name, "Star", false, new string[1] { nameof(Boolean) });
			}
			else if (chosenPlanet.ParentID.StartsWith("P"))
			{
				PlanetData Planet = es.LookForPlanet(BodyID.FromString(chosenPlanet.ParentID).GetID());

				GC.Collect();

				ins.SendTypedPackage<String>(gameObject.name, "Star", planetID, new string[1] { nameof(String) });
				ins.SendTypedPackage(gameObject.name, "Star", Planet, new string[1] { nameof(PlanetData) });
				ins.SendTypedPackage<bool>(gameObject.name, "Star", true, new string[1] { nameof(Boolean) });
			}
			int pos = cache.PlanetIds.IndexOf(planetID);
			if (pos != -1)
			{
				cache.ParrentTypes.RemoveAt(pos);
				cache.PlanetIds.Remove(planetID);
			}
			OverWriteCache(cache);
			LoadWithLoadingScreen.LoadScene(3, Stages.Microbe); //aun no puedes iniciar en otras etapas por que solo es funcional Microbio.
		}
	}

	private void ShowBSOD(string message, int errorCode)
	{
		GameObject A = new("BSOD");
		var C = A.AddComponent<Canvas>();
		C.renderMode = RenderMode.ScreenSpaceOverlay;

		GameObject ImageGO = new("IMG");
		Image IMG = ImageGO.AddComponent<Image>();

		Texture2D AA = new Texture2D(8, 8, TextureFormat.RGBA32, false);

		for (int x = 0; x < 8; x++)
			for (int y = 0; y < 8; y++)
				AA.SetPixel(x, y, Color.blue);

		AA.Apply();

		IMG.sprite = Sprite.Create(AA, new Rect(0, 0, 8, 8), Vector2.zero);
		IMG.color = Color.white;

		IMG.rectTransform.SetParent(A.transform, false);
		IMG.rectTransform.anchorMin = Vector2.zero;
		IMG.rectTransform.anchorMax = Vector2.one;
		IMG.rectTransform.offsetMin = Vector2.zero;
		IMG.rectTransform.offsetMax = Vector2.zero;
		
		GameObject TextGO = new("ERROR_TXT");
		var TEXT = TextGO.AddComponent<TMPro.TextMeshProUGUI>();
		TEXT.text = $"ERROR\n {message}\n ERROR CODE {errorCode.ToString("X")}";//mensaje de error con el mensaje y el codigo de error en hexadecimal
		TEXT.fontSize = 120;
		TEXT.color = Color.white;
		TEXT.alignment = TMPro.TextAlignmentOptions.Center;

		TEXT.rectTransform.SetParent(C.transform, false);
		TEXT.rectTransform.anchorMin = Vector2.zero;
		TEXT.rectTransform.anchorMax = Vector2.one;
		TEXT.rectTransform.offsetMin = Vector2.zero;
		TEXT.rectTransform.offsetMax = Vector2.zero;
	}

}
/// <summary>
/// Cache de planetas para nuevas partidas
/// Son bastante utiles para evitar que el usuario
/// espere mucho para encontrar un planeta valido
/// </summary>
[Serializable]
public struct PlanetCacheFile
{
	public List<string> PlanetIds;
	public List<string> ParrentTypes;
}


















































//2.71 ,3.14, 1.61, 42.00, 10.0.26200.5622