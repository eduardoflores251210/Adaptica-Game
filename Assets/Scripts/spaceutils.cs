//advertencia hay MUCHO comentario XD y NO hay que tocar mucho de este archivo por que es el corazon del juego
//un momento de silencio por el pobre SHA512 que se queda sin su gloria de nombrar galaxias y ahora solo nombra guardados XD
using ActualUtils;
using FixedMath;
using SerializableTypes;
using SerializableTypes.Biology;
using SerializableTypes.Space;
using StandartUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh; 
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;


public static class SpaceUtils
{

	/// <summary>
	/// 🪐 NOMBRADORES GALÁCTICOS
	/// </summary>
	public static class Naming
	{
		public static System.Random Random = new();
		public static string GenerateGalaxyName()
		{
			string[] Catalogues = {
				"GCA",
				"EGC",
				"SGC",
				"GCGC",
				"VGE",
				"G",
				"S5GC"
			};
			// significados de Los catalogos:
			//Galactic Catalogue A,      //NGC
			//Extra Galactic Catalogue, //inspíracion LEDA y ESO a la vez
			//SGC Super Galactic Catalogue, //isnpiracion PGC
			//Global Clusters and Galaxies Catalogue,    //inspiracion CGCG
			//VGE = Virgo Galactic Extention //inspiracion  VCC fusionado con NGC 
			//Guadalupe             //insperacion Messier. si, MESSIER pero es una señora que amaba ver cometas pero solo encontró galaxias XD
			//SHA512 Galactic Catalogue	//inspiracion NINGUNO, originalidad 100% XD 
			string Catalogo = Catalogues[Random.Range(0, Catalogues.Length)];
			int number1 = Random.Range(100, 9999);
			int number2 = Random.Range(10, 999);
			int number3 = Random.Range(1, 125);
			int number4 = Random.Range(1, 9999);

			string ShaIn = "" + Random.Range(int.MinValue, int.MaxValue);
			string SHAOUT = "";

			using SHA512 SHA512 = SHA512.Create();
			{
				byte[] hashBytes = SHA512.ComputeHash(Encoding.UTF8.GetBytes(ShaIn));

				// Convertir a hexadecimal solo Los primeros  128
				StringBuilder sb = new StringBuilder();
				int i = 1;
				foreach (byte b in hashBytes)
				{
					sb.Append(b.ToString("x2"));
					i++;
					if (i >= 128)
						break;
				}
				SHAOUT = sb.ToString();
			}
			switch (Catalogo)
			{
				case "VGE":

					return $"{Catalogo} {number3}-{number4}";
				case "G":
					return $"{Catalogo} {number3}";
				case "GCGC":
					return $"{Catalogo} {number3}:{number1}";
				case "SGC":
					return $"{Catalogo} {number4}";
				case "S5GC":
					return "S5GC " + SHAOUT ;
				default:
					return $"{Catalogo} {number1}-{number2}";
					
			}

			//funfact: NGC es el catalogo de galaxias mas famoso y usado en la vida real, pero no lo uso por que es muy obvio XD
			//2 VGE se iba a llamar VGA pero VGA es un cable 
			//3 S5GC era algo random que se me ocurio despues de tener la decimo cuarta crisis creativa del año 2026
		}

		public static string GenerateStarName_NASAStyle()
		{
			int catalogNumber = Random.Range(10000, 999999);
			List<string> Catalogues = new List<string>()
			{
				"SC",//Clasico 1 aka Star Catalog
				"SL",//Clasico 2 //Star List // oh wow Comentario en un comentario X3
				"Krumpler", //si Keppler pero inspirado en krampus... si el de la navidad XD
				"HUP", //Hipparcos pero ahora es Hupparcus XD
				"FHD", //jaja FHD en vez de HD // que significa Full HD  pregintaras? pues Flores-Hernandez-Diaz catalogo de estrellas       si es un nombre largo XD
				"DESS" //ups referencia implicita accidental a deltarune (dess la hermana mayor de Noelle la que esta desaparecida) auque originamente esto era referencia a TESS. pero aqui DESS significa Deep Extra Stellar Survey no December Holiday (aka la Hermana de Noelle XD)
			};
			//favor de ignorar el infodump de DESS pls, no quiero cambiar el nombre por que ya lo use en varios lados XD
			string catalogPrefix = Catalogues[Random.Range(0,Catalogues.Count)];
			return $"{catalogPrefix} {catalogNumber}";
		}

		public static string GeneratePlanetName_NASAStyle(string systemName, int idx)
		{
			//Debug.Log(idx.ToString());
			char suffix = (char)('b' + idx); // b, c, d, etc.
			return $"{systemName}{suffix}";
		}
		
		public static string GenerateMoonName_NASAStyle(string PlanetName, int idx)
		{
			//Debug.Log(idx.ToString());

			return $"{PlanetName} {idx.ToRoman()}";
		}

	}
	public static void AddTooltipManipulators(UIDocument uiDocument)
	{
		if (uiDocument == null || uiDocument.rootVisualElement == null)
			return;

		System.Action<VisualElement> walk = null;
		walk = (ve) =>
		{
			if (!string.IsNullOrEmpty(ve.tooltip))
				ve.AddManipulator(new ToolTipManipulator());

			foreach (var child in ve.Children())
				walk(child);
		};

		walk(uiDocument.rootVisualElement);
	}

	public static class UnitConversion
	{
		public static float LightYearToParsec(float value) 
		{
			return value * 3.26156f;

		}
		public static float ParsecToLightYear(float value)
		{
			return (value / 3.26156f);
		}
		public static float LightYearToAU(float value)
		{
			return value * 63241.1f;
		}
		public static float AuToLightYear(float value)
		{
			return ((value / 63241.1f));

		}
		/// <summary>
		/// convierte kilometros a unidades astronomicas 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float KmToAU(float value)
		{
			return value / 149597870.7f; 
		}
		/// <summary>
		/// convierte Unidades astronomicas a Kilometros
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float AuToKm(float value)
		{
			return value * 149597870.7f;
		}
		/// <summary>
		/// convierte kilometros a metros
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static float KmToM(float value)
		{
			return value * 1000;
		}

		public static float MeterToKm(float value)
		{
			return value / 1000;
		}

	}
	//easter egg recursivo originalmentre pense que era un coinflip pero uego me di cuenta que era recursivo XD
	static void a(bool f)
	{
		if (f) 
		a(Random.Range(0,2)==0);
	}
	/// <summary>
	/// Convierte un numero entero a numero romano
	/// </summary>
	/// <param name="number"> el numero</param>
	/// <returns></returns>
	public static string ToRoman(this int number)
	{
		if (number > 3999) return "0"; // límites clásicos del sistema romano
		if (number == 0)
			return "O";
		bool isNEg = false;
		if (number< 0 )
			isNEg = true;
		var romanNumerals = new[]
		{
		new { Value = 1000, Symbol = "M" },
		new { Value = 900, Symbol = "CM" },
		new { Value = 500, Symbol = "D" },
		new { Value = 400, Symbol = "CD" },
		new { Value = 100, Symbol = "C" },
		new { Value = 90, Symbol = "XC" },
		new { Value = 50, Symbol = "L" },
		new { Value = 40, Symbol = "XL" },
		new { Value = 10, Symbol = "X" },
		new { Value = 9, Symbol = "IX" },
		new { Value = 5, Symbol = "V" },
		new { Value = 4, Symbol = "IV" },
		new { Value = 1, Symbol = "I" }
	};

		string result = "";
		foreach (var item in romanNumerals)
		{
			while (number >= item.Value)
			{
				result += item.Symbol;
				number -= item.Value;
			}
		}
		if (isNEg)
			result = '-' + result;

		return result;
	}
	/// <summary>
	/// Instancia un sistema solar... ¡AHORA CON POSICIONES BASADAS EN ÍNDICE! 
	/// </summary>
	public static class SystemObjectsBuilder
	{
		public static Material BaseMat;
		public static Material BholMat;
		public static Material BaseGasMaterial;
		public static Material OpaceMat;
		public static Dictionary<StarTypes, Material> Mats = new Dictionary<StarTypes, Material>();
		public static GalaxyData galaxyData;
		public static InstantiatedSystemData systemData;
		public static UnityEngine.Mesh CacheSphere;

		private static bool Inited;

		public static void InitStuf()
		{
			foreach (StarTypes st in Enum.GetValues(typeof(StarTypes)))
			{
				Material material = new Material(BaseMat);
				material.SetFloat("_Temp_K", StarData.Temperatures[st]);
				Mats[st] = material;
				if (st == StarTypes.X) Mats[st] = BholMat;
				else if (st == StarTypes.EN) Mats[st] = OpaceMat;
			}
			Inited = true;
		}

		static void GiveSphere(GameObject @object, Material mat)
		{
			@object.AddComponent<MeshFilter>().mesh = CacheSphere;
			@object.AddComponent<MeshRenderer>().material = mat;
			@object.AddComponent<MeshCollider>();
		}

		public static GameObject InstantiateStar(StarData starData)
		{
			GameObject starGO = new GameObject(starData.Name);
			GiveSphere(starGO, Mats[starData.type]);
			starGO.transform.localScale = Vector3.one*2.5f; 
            if (starData == null)
                throw new NullReferenceException("STARDATA NULL");
			systemData.IDS.Stars.Add(BodyID.FromString(starData.id));
			systemData.Datas.Stars.Add(starData);
			systemData.ObjAndIDS.Add(starGO, starData.id);
			return starGO;
		}

		public static GameObject InstantiateBaricenter(BaricenterData baricenterData)
		{
			var gol = new GameObject(baricenterData.Name);
			systemData.IDS.baricenters.Add(BodyID.FromString(baricenterData.id));
			systemData.Datas.baricenters.Add(baricenterData);
			systemData.ObjAndIDS.Add(gol, baricenterData.id);
			return gol;
		}

		public static GameObject InstantiatePlanet(PlanetData planetData)
		{
			GameObject planetGO = new GameObject(planetData.Name);
			Material Mat = OpaceMat;

			if (planetData.type == PlanetTypes.BasicGas || planetData.type == PlanetTypes.IceGas)
			{
				if (planetData.GasColors != null && planetData.GasColors.Count >= 5 && BaseGasMaterial != null)
				{
					Mat = new Material(BaseGasMaterial);
					Mat.SetColor("_PoloNorte", planetData.GasColors[0]);
					Mat.SetColor("_Arriba", planetData.GasColors[1]);
					Mat.SetColor("_Ecuador", planetData.GasColors[2]);
					Mat.SetColor("_Abajo", planetData.GasColors[3]);
					Mat.SetColor("_PoloSur", planetData.GasColors[4]);
				}
			}

			GiveSphere(planetGO, Mat);
			planetGO.transform.localScale = Vector3.one * planetData.radius;

			systemData.IDS.planets.Add(BodyID.FromString(planetData.id));
			systemData.Datas.planets.Add(planetData);
			systemData.ObjAndIDS.Add(planetGO, planetData.id);
			return planetGO;
		}

		public static void InstantiateBody(string StartID, UnityEngine.Transform parent, int index = 0)
		{
			if (StartID[0] == 'S') throw new ArgumentException("NO SECTORES");

			if (galaxyData == null && !GalaxyData.TryToLoadGalaxy(out galaxyData))
				throw new Exception("ERROR AL CARGAR GALAXIA");

			BodyID bodyID = BodyID.FromString(StartID);
			if (!TryToLoadABody(bodyID, out var body, out var celestialBodyType))
				throw new Exception("ERROR CARGANDO");

			GameObject gameObject = celestialBodyType switch
			{
				CelestialBodyType.Planet => InstantiatePlanet((PlanetData)body),
				CelestialBodyType.Star => InstantiateStar((StarData)body),
				CelestialBodyType.Baricenter => InstantiateBaricenter((BaricenterData)body),
				CelestialBodyType.Nova => throw new NotImplementedException(),
				CelestialBodyType.Nebula => throw new NotImplementedException(),
				_ => null
			};

			if (gameObject != null)
			{
				gameObject.transform.SetParent(parent);
				// POSICIONAMIENTO POR ÍNDICE: Separación de 5 unidades por nivel
				float dist = (index + 1) * 5f;
				gameObject.transform.localPosition = new Vector3(dist, 0, 0);
			}

			if (body.Children != null && body.Children.Count > 0)
			{
				for (int i = 0; i < body.Children.Count; i++)
				{
					InstantiateBody(body.Children[i], gameObject.transform, i);
				}
			}
		}

		public static void InstantiateSystem(string ParentId)
		{
			if (!Inited) InitStuf();

			systemData = new InstantiatedSystemData
			{
				Datas = new GalObjCollection() { Stars = new(), baricenters = new(), nebulas = new(), novas = new(), planets = new()},
				IDS = new GalObjCollectionID() { Stars = new(), baricenters = new(), nebulas = new(), novas = new(), planets = new()},
				ObjAndIDS = new Dictionary<GameObject, string>()
			};

			InstantiateBody(ParentId, null);
		}

		static bool TryToLoadABody(BodyID bodyID, out CelestialBody body, out CelestialBodyType d)
		{
			try
			{
				d = bodyID.GetCelestialBodyType();
				body = d switch
				{
					CelestialBodyType.Planet => galaxyData.LoadPlanet(bodyID.GetID()),
					CelestialBodyType.Star => galaxyData.LookForStar(bodyID.GetID()),
					CelestialBodyType.Baricenter => galaxyData.LookForBaricenter(bodyID.GetID()),
					CelestialBodyType.Nova => galaxyData.LookForNova(bodyID.GetID()),
					CelestialBodyType.Nebula => galaxyData.LookForNebula(bodyID.GetID()),
					_ => throw new Exception("TIPO DESCONOCIDO"),
				};
				return true;
			}
			catch { body = null; d = CelestialBodyType.None; return false; }
		}

		[Serializable]
		public struct InstantiatedSystemData
		{
			public GalObjCollection Datas;
			public GalObjCollectionID IDS;
			public Dictionary<GameObject, string> ObjAndIDS;
		}
	}
}
