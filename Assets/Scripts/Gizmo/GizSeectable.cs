using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizSelectable : MonoBehaviour
{
    public bool IsMovable;
    public bool IsScalable;
    public bool IsRotatable;
    public Vector3 StartRot;
    public Vector3 StartPos;
    public Vector3 StartScale;
    private void Awake()
    {
        StartPos = transform.position;
        StartScale = transform.localScale;
        StartRot = transform.rotation.eulerAngles;
    }
}
