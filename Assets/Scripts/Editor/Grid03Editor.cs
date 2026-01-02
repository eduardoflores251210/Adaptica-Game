using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Mesh03))]
public class Grid03Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Mesh03 scr = (Mesh03)target;
        scr.OutlineGrid();        
        scr.UpdateGridDisplay();
        scr.SetCurrentCell();
    }
}
