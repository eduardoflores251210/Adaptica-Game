using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigPaws : MonoBehaviour
{
    public AbstractGridCell AbstractsGridCell;
    // Start is called before the first frame update
    void Start()
    {
		AbstractGridPoint.OnPointValueChange += AbstractGridPoint_OnPointValueChange; ;

	}

	// Update is called once per frame
	void Update()
    {
		AbstractGridPoint.triggerEb();
	}

	private void AbstractGridPoint_OnPointValueChange(ref AbstractGridPoint gp)
	{
		Debug.Log(" AAAAAAAAAAAAAAAAAA");
	}
}
