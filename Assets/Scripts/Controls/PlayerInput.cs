using UnityEngine;
using UnityEngine.EventSystems;

namespace Boardgame.Input
{

    public enum InteractionType { Inspect, Select, Path, FinalizePath, Deselect, None };
    public enum InputTypes {Mouse};
    public delegate void InputEvent(Vector3 screenPosition, InteractionType type);

    public class PlayerInput : MonoBehaviour
    {
        public event InputEvent OnPlayerInput;

        protected void Emit(Vector3 screenPosition, InteractionType type)
        {
            if (OnPlayerInput != null)
                OnPlayerInput(screenPosition, type);
        }
    }

    public class MouseInput : PlayerInput
    {
        Tile prevoiusHover;

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            var selectedTile = Tile.SelectLock;
            var hoveredTile = Tile.HoverTile;


            if (UnityEngine.Input.GetMouseButtonDown(0))
            {

                if (selectedTile == null)
                {
                    Emit(InteractionType.Select);
                    prevoiusHover = hoveredTile;
                } 
                else if (hoveredTile == selectedTile)
                    Emit(InteractionType.Deselect);

            } else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                //TODO: should really check path if it is pathing!
                if (selectedTile != null && hoveredTile != selectedTile)
                    Emit(InteractionType.FinalizePath);

            } else if (UnityEngine.Input.GetMouseButton(0))
            {
                if (hoveredTile != prevoiusHover) {
                    prevoiusHover = hoveredTile;
                    Emit(InteractionType.Path);
                }
            } else
            {
                if (selectedTile == null && hoveredTile != null && hoveredTile != prevoiusHover)
                {
                    prevoiusHover = hoveredTile;
                    Emit(InteractionType.Inspect);
                }
            }
        }

        void Emit(InteractionType type)
        {
            Emit(UnityEngine.Input.mousePosition, type);
        }
    }
}