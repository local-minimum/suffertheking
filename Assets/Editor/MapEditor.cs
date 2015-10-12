using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Remove all connections"))
        {
            (target as Map).ClearAllConnections();
            SceneView.RepaintAll();
        }

        EditorGUILayout.HelpBox(string.Format("Nodes with connections (GO): {0}\nNodes: {1}", 
            ((Map)target).ConnectionsCount, 
            ((Map)target).MapSize
            ), MessageType.Info);
    }
}
