using UnityEngine;
using System.Collections;


namespace Boardgame.UI
{
    [RequireComponent(typeof(MeshRenderer))]
    public class UIPathVisualizer : MonoBehaviour
    {

        Tile[] currentPath;
        //LineRenderer lRenderer;

        void OnEnable()
        {
            Pather.OnPathChange += HandleNewPath;
        }

        void OnDisable()
        {
            Pather.OnPathChange -= HandleNewPath;
        }

        private void HandleNewPath(Tile[] path, PathAction action)
        {
            currentPath = path;    
        }

        /*
        void Start()
        {
            lRenderer = GetComponent<LineRenderer>();
            lRenderer.useWorldSpace = true;
        }

        void Update()
        {
            if (currentPath == null)
            {
                lRenderer.enabled = false;
                return;
            }
            lRenderer.enabled = true;
            lRenderer.SetVertexCount(currentPath.Length);
            for (int i = 0; i < currentPath.Length; i++)
                lRenderer.SetPosition(i, currentPath[i].PathPoint);
        }
        */
    }
}