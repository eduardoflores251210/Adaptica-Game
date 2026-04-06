using StandartUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
	[Serializable]
	public class CustomPart
	{
		public string InternalName; //debe ser [MODNAME]:[PART] ej "Create:Cogwhell" (si es el mismo sistema de minecraft por eso use de ejemplo el create mod) esto es para evitar colisiones de nombres entre mods y cosas asi, ademas de que es mas facil de organizar asi, el nombre interno no se muestra en el juego ni nada, es solo para identificacion interna del mod y cosas asi, el nombre que se muestra en el juego es displayName
		public PartTypes Type;
		public string displayName;
		public String icon; //creo que esto deveria ser una imagen BASE64 o algo asi para evitar problemas de serializacion con las texturas de Unity y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como serializar las texturas de Unity o cosas asi, simplemente uso una imagen en base64 y ya
		public StdUtils.Serializable.Mesh Mesh; //malla serializable de StandartUtilities, esto es para evitar problemas de serializacion con las mallas de Unity y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como serializar las mallas de Unity o cosas asi, simplemente uso la malla serializable de StandartUtilities y ya
		public string[] categories;
		public float size;
		public float healthBoost;
		public List<string> tags;
		public StatList Stats;
		public Color color; //esto es para darle un color a la parte personalizada, esto es para que el juego pueda mostrar la parte personalizada con el color que el modder quiera, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como darle un color a la parte personalizada o cosas asi, simplemente le doy un color a la parte personalizada y ya
		public BasePart ConvertToPart()
		{
			//esto se implementara en las versiones de otro tipo
			return null;
		}

		GameObject createPrefab()
		{
			//esto se implementara en las versiones de otro tipo, esto es para crear el prefab de la parte a partir de la malla y las texturas y cosas asi, esto es para evitar problemas de serializacion con los prefabs de Unity y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como serializar los prefabs de Unity o cosas asi, simplemente creo el prefab a partir de la malla y las texturas y cosas asi y ya
			return null;	
		}
	}
	[Serializable]
	public class CustomAnimalPart : CustomPart
	{
		public float attackPower;
		public float movementBoost;
		public BiologicalPartFunction function;
		GameObject createPrefab()
		{
			GameObject partPrefab = new GameObject(displayName);
			partPrefab.AddComponent<MeshFilter>().mesh = (Mesh)Mesh; //se ve raro pero basicamente convierte mi malla serializable de StandartUtilities a una malla de Unity y se la asigna al MeshFilter del prefab, esto es para evitar problemas de serializacion con las mallas de Unity y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como serializar las mallas de Unity o cosas asi, simplemente uso la malla serializable de StandartUtilities y ya
			Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
				mat.color = color;
			partPrefab.AddComponent<MeshRenderer>().material = mat; //uso URP
			partPrefab.AddComponent<MeshCollider>(); //esto es para que la parte personalizada pueda tener colision y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como darle colision a la parte personalizada o cosas asi, simplemente le doy un MeshCollider y ya
			var comp = partPrefab.AddComponent<PartComp>(); //esto es para que la parte personalizada pueda ser reconocida como una parte por el juego y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que la parte personalizada sea reconocida como una parte por el juego o cosas asi, simplemente le doy un PartComp y ya
			comp.ID = InternalName;
			comp.isInGame = false; //generalmente las partes se instancian en el editor de criaturas asi QUE no queremos que intbenten hacer cosas de parte normal como regeneracion de vida o cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que la parte personalizada se comporte como una parte normal o cosas asi, simplemente le digo que no esta en el juego y ya
			
			return partPrefab;
		}
		public new BiologicalPart ConvertToPart()
		{
			BiologicalPart partData = ScriptableObject.CreateInstance<BiologicalPart>();
			partData.displayName = partData.name = displayName;
			partData.partID = InternalName;
			byte[] imageBytes = Convert.FromBase64String(icon);
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(imageBytes);
			var IIICON = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			partData.icon = IIICON;
			var prefab = createPrefab();
			partData.prefab = prefab;
			var categoriesa = BasePart.ParseCategories(categories);
			partData.categories = categoriesa.ToArray();
			partData.size = size;
			partData.healthBoost = healthBoost;
			partData.tags = tags;
			partData.attackPower = attackPower;
			partData.movementBoost = movementBoost;
			partData.function = function;
			return partData;
		}
	}
	[Serializable]
	public class CustomVehiclePart : CustomPart
	{
		public float powerConsumption;
		public float armor;
		public float speedBoost;
		public VehicleFunction function; //si el nombre no es consistente con su version Scripotable pero es consistente con sus hermanos personalizabdos 
		GameObject createPrefab()
		{
			GameObject partPrefab = new GameObject(displayName);
			partPrefab.AddComponent<MeshFilter>().mesh = (Mesh)Mesh; //se ve raro pero basicamente convierte mi malla serializable de StandartUtilities a una malla de Unity y se la asigna al MeshFilter del prefab, esto es para evitar problemas de serializacion con las mallas de Unity y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como serializar las mallas de Unity o cosas asi, simplemente uso la malla serializable de StandartUtilities y ya
			Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
			mat.color = color;
			partPrefab.AddComponent<MeshRenderer>().material = mat; //uso URP
			partPrefab.AddComponent<MeshCollider>(); //esto es para que la parte personalizada pueda tener colision y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como darle colision a la parte personalizada o cosas asi, simplemente le doy un MeshCollider y ya
			var comp = partPrefab.AddComponent<PartComp>(); //esto es para que la parte personalizada pueda ser reconocida como una parte por el juego y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que la parte personalizada sea reconocida como una parte por el juego o cosas asi, simplemente le doy un PartComp y ya
			comp.ID = InternalName;
			comp.isInGame = false; //generalmente las partes se instancian en el editor de criaturas asi QUE no queremos que intbenten hacer cosas de parte normal como regeneracion de vida o cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que la parte personalizada se comporte como una parte normal o cosas asi, simplemente le digo que no esta en el juego y ya

			return partPrefab;
		}
		public new VehiclePart ConvertToPart()
		{
			VehiclePart partData = ScriptableObject.CreateInstance<VehiclePart>();
			partData.displayName = partData.name = displayName;
			byte[] imageBytes = Convert.FromBase64String(icon);
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(imageBytes);
			var IIICON = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			partData.icon = IIICON;
			partData.partID = InternalName;
			partData.prefab = createPrefab();
			var categoriesa = BasePart.ParseCategories(categories);
			partData.categories = categoriesa.ToArray();
			partData.size = size;
			partData.healthBoost = healthBoost;
			partData.tags = tags;
			partData.powerConsumption = powerConsumption;
			partData.armor = armor;
			partData.speedBoost = speedBoost;
			partData.Func = function;
			return partData;
		}
	}
	[Serializable]
	public class CustomVegetalPart : CustomPart
	{
		public float SunLightPower;
		public float WaterAbsortionBoost;
		public VegetalPartFunction function;
		public float AtackPower;
		GameObject createPrefab()
		{
			GameObject partPrefab = new GameObject(displayName);
			partPrefab.AddComponent<MeshFilter>().mesh = (Mesh)Mesh; //se ve raro pero basicamente convierte mi malla serializable de StandartUtilities a una malla de Unity y se la asigna al MeshFilter del prefab, esto es para evitar problemas de serializacion con las mallas de Unity y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como serializar las mallas de Unity o cosas asi, simplemente uso la malla serializable de StandartUtilities y ya
			Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
			mat.color = color;
			partPrefab.AddComponent<MeshRenderer>().material = mat; //uso URP
			partPrefab.AddComponent<MeshCollider>(); //esto es para que la parte personalizada pueda tener colision y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como darle colision a la parte personalizada o cosas asi, simplemente le doy un MeshCollider y ya
			var comp = partPrefab.AddComponent<PartComp>(); //esto es para que la parte personalizada pueda ser reconocida como una parte por el juego y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que la parte personalizada sea reconocida como una parte por el juego o cosas asi, simplemente le doy un PartComp y ya
			comp.ID = InternalName;
			comp.isInGame = false; //generalmente las partes se instancian en el editor de criaturas asi QUE no queremos que intbenten hacer cosas de parte normal como regeneracion de vida o cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que la parte personalizada se comporte como una parte normal o cosas asi, simplemente le digo que no esta en el juego y ya

			return partPrefab;
		}
		public new VegetalPart ConvertToPart()
		{
			VegetalPart partData = ScriptableObject.CreateInstance<VegetalPart>();
			partData.displayName = partData.name = displayName;
			byte[] imageBytes = Convert.FromBase64String(icon);
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(imageBytes);
			var IIICON = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			partData.icon = IIICON;
			partData.partID = InternalName;
			partData.prefab = createPrefab();
			var categoriesa = BasePart.ParseCategories(categories);
			partData.categories = categoriesa.ToArray();
			partData.size = size;
			partData.healthBoost = healthBoost;
			partData.tags = tags;
			partData.SunLightPower = SunLightPower;
			partData.WaterAbsortionBoost = WaterAbsortionBoost;
			partData.function = function;
			partData.AtackPower = AtackPower;
			return partData;
		}
	}

	//para mantenr en memoria las partes personalizadas que se han creado, esto es para que el juego pueda acceder a ellas y usarlas en el juego, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como guardar las partes personalizadas en el disco o cosas asi, simplemente las guardo en esta lista y ya
	public static class PartsContainer
	{
		public static List<CustomPart> customParts = new List<CustomPart>();
	}
}