using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class BasePart : ScriptableObject
{
	public string partID;
	public string displayName;
	public string description;
	public Sprite icon;
	public GameObject prefab;
	public PartCategories[] categories;
	public float size;
	public float healthBoost;
	public List<string> tags;
	public StatList Stats;
}
public enum PartCategories
{
	#region Bio
	Cell,
	Eyes,
	Cats,// esto es broma
	Mouths,
	Defense,
	Limbs,
	HandsAndFeet,
	BioWings,
	Decoration,
	#endregion
	#region Vehicle
	VehWings,
	Tires,
	Weapons,
	WindowsAndLights,
	Looks,
	Chasis,
	#endregion
	#region Vegetal
	Leaves,
	Sporangium,
	Flowers,
	Fruits,
	Branchs,
	Roots
	#endregion
}


[Serializable]
public struct Stat
{
	public string key;
	public string value;

	public Stat(string key, string value)
	{
		this.key = key;
		this.value = value;
	}
}

[Serializable]
public class StatList
{
	[SerializeField]
	public List<Stat> stats = new List<Stat>();

	// Permite acceder tipo diccionario: statList["vida"]
	public string this[string key]
	{
		get
		{
			for (int i = 0; i < stats.Count; i++)
			{
				if (stats[i].key == key)
					return stats[i].value;
			}
			throw new Exception($"No se encontró la clave '{key}'");
		}
		set
		{
			for (int i = 0; i < stats.Count; i++)
			{
				if (stats[i].key == key)
				{
					stats[i] = new Stat(key, value);
					return;
				}
			}
			// Si no existe, se agrega
			stats.Add(new Stat(key, value));
		}
	}

	public bool ContainsKey(string key)
	{
		foreach (var stat in stats)
			if (stat.key == key)
				return true;
		return false;
	}

	public bool ContainsValue(string value)
	{
		foreach (var stat in stats)
			if (stat.value == value)
				return true;
		return false;
	}

	public void Remove(string key)
	{
		stats.RemoveAll(s => s.key == key);
	}

	public string GetValue(string key)
	{
		return this[key];
	}

	public void SetValue(string key, string value)
	{
		this[key] = value;
	}

	public List<string> Keys()
	{
		var list = new List<string>();
		foreach (var stat in stats)
			list.Add(stat.key);
		return list;
	}

	public List<string> Values()
	{
		var list = new List<string>();
		foreach (var stat in stats)
			list.Add(stat.value);
		return list;
	}
}
