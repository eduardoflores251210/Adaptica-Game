using ActualUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Historia 
public class TimelineUI : MonoBehaviour
{
	public CellController Player;
	public string Elementy_name = string.Empty;
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		List<float > list = new List<float>();
		foreach (var item in Saver.CurrentGame.Actions)
		{
			list.Add((item.Path) switch
			{
				SerializableTypes.HistoryPaths.Friendly => 1f,
				SerializableTypes.HistoryPaths.Neutral => 0f,
				SerializableTypes.HistoryPaths.Agressive => -1f,
				_ => 0f

			}
			);
			
		}
		//eje Y 
		//x = idx *5+1
	}
}
