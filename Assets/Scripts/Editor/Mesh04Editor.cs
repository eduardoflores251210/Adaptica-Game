using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Mesh04))]
public class Mesh04Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Mesh04 scr = (Mesh04)target;
        scr.DrawLines();
    }
}
