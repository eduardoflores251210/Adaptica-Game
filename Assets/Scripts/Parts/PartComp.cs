using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PartComp :MonoBehaviour
 {
	public string ID;
	public bool isInGame = false;
	public Action UpdateAction;
	private void Update()
	{
		if (isInGame)
		{
			UpdateAction?.Invoke();
		}
	}
}

