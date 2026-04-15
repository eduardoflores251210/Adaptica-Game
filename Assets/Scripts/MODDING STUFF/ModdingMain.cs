using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Modding
{
	public class ModdingMain : MonoBehaviour
	{
		public PartsDatabase VegetalParts;
		public PartsDatabase AnimalParts;
		public PartsDatabase VehicleParts;

		private void Start()
		{
			if (!Directory.Exists(Paths.ModParts))
				return; //no lo creamos porque no es necesario, el juego puede funcionar sin mods, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como crear la carpeta de mods o cosas asi, simplemente la creo manualmente y ya
			if (PartsContainer.customParts == null)
			{
				PartsContainer.customParts = new List<CustomPart>();
				PartsContainer.loaded = false;
			}
			if (!PartsContainer.loaded)
			{
				List<string> FilesInPartsDir = Directory.GetFiles(Paths.ModParts).ToList()
				;


				foreach (string file in FilesInPartsDir)
				{
					if (Path.GetExtension(file) != ".apart") //apart es Json disfrazado, es un formato de archivo personalizado que solo se usa para las partes personalizadas, esto es para evitar confusiones con otros tipos de archivos y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como manejar otros tipos de archivos o cosas asi, simplemente manejo archivos json y ya
						continue; //solo queremos archivos apart, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como manejar otros tipos de archivos o cosas asi, simplemente manejo archivos json y ya
					string json = File.ReadAllText(file);
					CustomPart customPart = JsonUtility.FromJson<CustomPart>(json);
					try
					{
						if (customPart.Type == PartTypes.Animal)
						{
							CustomAnimalPart animalPart = JsonUtility.FromJson<CustomAnimalPart>(json);
							customPart = animalPart;
							var part = animalPart.ConvertToPart();
							DontDestroyOnLoad(part.prefab); //esto es para que el prefab de la parte personalizada no se destruya al cargar una nueva escena o cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que el prefab de la parte personalizada no se destruya al cargar una nueva escena o cosas asi, simplemente le digo que no se destruya y ya
							AnimalParts.AddPart(part);
							part.prefab.SetActive(false); //esto es para que el prefab de la parte personalizada no aparezca en el mundo del juego ni nada, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que el prefab de la parte personalizada no aparezca en el mundo del juego o cosas asi, simplemente lo desactivo y ya
						}
						else if (customPart.Type == PartTypes.Vehicle)
						{
							CustomVehiclePart vehiclePart = JsonUtility.FromJson<CustomVehiclePart>(json);
							customPart = vehiclePart;
							var part = vehiclePart.ConvertToPart();
							DontDestroyOnLoad(part.prefab);
							VehicleParts.AddPart(part);
							part.prefab.SetActive(false); //esto es para que el prefab de la parte personalizada no aparezca en el mundo del juego ni nada, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que el prefab de la parte personalizada no aparezca en el mundo del juego o cosas asi, simplemente lo desactivo y ya
						}
						else if (customPart.Type == PartTypes.Vegetal)
						{
							CustomVegetalPart vegetalPart = JsonUtility.FromJson<CustomVegetalPart>(json);
							customPart = vegetalPart;
							var part = vegetalPart.ConvertToPart(); //esto es para que el prefab de la parte personalizada no aparezca en el mundo del juego ni nada, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que el prefab de la parte personalizada no aparezca en el mundo del juego o cosas asi, simplemente lo pongo en una posicion muy lejana y ya
							DontDestroyOnLoad(part.prefab);
							VegetalParts.AddPart(part);
							part.prefab.SetActive(false); //esto es para que el prefab de la parte personalizada no aparezca en el mundo del juego ni nada, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que el prefab de la parte personalizada no aparezca en el mundo del juego o cosas asi, simplemente lo desactivo y ya
						}
					}
					catch (Exception e)
					{
						Debug.LogError($"Error al cargar la parte personalizada {customPart.InternalName}: {e}");
						continue; //si hay un error al cargar la parte personalizada, simplemente lo ignoramos y seguimos cargando las demas partes personalizadas, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como manejar los errores al cargar las partes personalizadas o cosas asi, simplemente los ignoro y sigo cargando las demas partes personalizadas y ya
					}
					PartsContainer.customParts.Add(customPart);

					//apart significa Adaptica part como dije es solo json con sombrero, esto es para evitar confusiones con otros tipos de archivos y cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como manejar otros tipos de archivos o cosas asi, simplemente manejo archivos json y ya
				}
				DontDestroyOnLoad(this.gameObject); //esto es para que el objeto de modding no se destruya al cargar una nueva escena o cosas asi, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como hacer que el objeto de modding no se destruya al cargar una nueva escena o cosas asi, simplemente le digo que no se destruya y ya
				PartsContainer.loaded = true;
			}
			else
				Destroy(this);
		}


		//asumimos que si se destruye el objeto de modding es porque el juego se esta cerrando o cosas asi, asi que no es necesario hacer nada en el metodo de OnDestroy, ademas de que es mas facil de organizar asi, ademas de que es mas facil de implementar asi, ya que no tengo que preocuparme por como manejar la destruccion del objeto de modding o cosas asi, simplemente no hago nada y ya
		private void OnDestroy()
		{
			foreach (var part in PartsContainer.customParts)
			{
				if (part.Type == PartTypes.Animal)
				{
					AnimalParts.RemovePart(part.InternalName);
				}
				else if (part.Type == PartTypes.Vehicle)
				{
					VehicleParts.RemovePart(part.InternalName);
				}
				else if (part.Type == PartTypes.Vegetal)
				{
					VegetalParts.RemovePart(part.InternalName);
				}
			}
		}
	}

	
}