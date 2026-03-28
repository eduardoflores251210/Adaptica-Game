using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartRizeser : MonoBehaviour
{
	public Cursor cursor;
	public CameraOrbitController CameraCtrl;
	// Start is called before the first frame update
	void Start()
	{
		
	}
	PartComp lastPart;

	// Update is called once per frame
	void Update()
	{
		var gol = cursor.GetObjInCursor();
		if (gol == null )
			return;


		CameraCtrl.EnableZoom = !(gol.TryGetComponent<PartComp>(out lastPart));


	}
	private void LateUpdate()
	{
		if (lastPart == null) return;
		var scale = lastPart.transform.localScale;	
		lastPart.transform.localScale = scale +( Vector3.one * CameraCtrl.ZoomOutput);
	}
}
