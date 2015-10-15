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
        [SerializeField]
        string MouseButtonKeyPref = "Mouse.Button";

        int mouseButton;

        void Start()
        {
            mouseButton = PlayerPrefs.GetInt(MouseButtonKeyPref, 0);
        }

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            var selectedTile = Tile.SelectLock;

            if (UnityEngine.Input.GetMouseButtonDown(mouseButton))
            {

                if (selectedTile == null)
                {
                    Emit(InteractionType.Select);
                } 
                else if (Tile.HoverTile == selectedTile)
                    Emit(InteractionType.Deselect);

            } else if (UnityEngine.Input.GetMouseButtonUp(mouseButton))
            {
                if (Game.Pather.Pathing)
                    Emit(InteractionType.FinalizePath);

            } else if (UnityEngine.Input.GetMouseButton(mouseButton))
            {
                Emit(InteractionType.Path);
            } else
            {
                if (selectedTile == null)
                {
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