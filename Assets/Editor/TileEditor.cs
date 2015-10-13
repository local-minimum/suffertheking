using UnityEditor;
using UnityEngine;
using Boardgame;

[CustomEditor(typeof(Tile), true), CanEditMultipleObjects]
public class TileEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (targets.Length == 2)
        {

            bool clicked = false;
            if (Map.Connected(targets[0] as Tile, targets[1] as Tile))
            {
                if (GUILayout.Button("Disconnect"))
                {
                    Map.Disconnect(targets[0] as Tile, targets[1] as Tile);
                    clicked = true;
                }
            }
            else if (GUILayout.Button("Connect"))
            {
                Map.Connect(targets[0] as Tile, targets[1] as Tile);
                clicked = true;
            }

            if (clicked)
            {
                SceneView.RepaintAll();
            }
        }
    }
}
