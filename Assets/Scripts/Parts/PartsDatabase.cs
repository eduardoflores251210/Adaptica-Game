using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "PartDatabase", menuName = "Database/Parts DB")]
public class PartsDatabase : ScriptableObject
{
    public List<BasePart> allParts;

    public BasePart GetPartByID(string id)
    {
        return allParts.Find(p => p.partID == id);
    }

    public List<T> GetPartsOfType<T>() where T : BasePart
    {
        return allParts.OfType<T>().ToList();
    }

	public BasePart GetPartByName(string name)
	{
		return allParts.Find(p=>p.name == name);
	}
    public void AddPart(BasePart part)
    {
        if (!allParts.Contains(part))
        {
            allParts.Add(part);
        }
	}

	internal void RemovePart(string ID)
	{
		if (allParts.Any(p => p.partID== ID))
        {
            allParts.Remove(allParts.Find(p => p.partID == ID));
		}
	}
}
