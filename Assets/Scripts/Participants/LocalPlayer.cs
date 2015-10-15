using UnityEngine;
using System.Collections;

namespace Boardgame
{
    public class LocalPlayer : ParticipantController
    {
        Input.PlayerInput input;

        [SerializeField]
        string inputPreference = "LocalPlayerInput";

        void OnEnable()
        {
            int inputType = PlayerPrefs.GetInt(inputPreference, 0);
            switch ((Input.InputTypes) inputType)
            {
                case Input.InputTypes.Mouse:
                    input = gameObject.AddComponent<Input.MouseInput>();
                    break;
                default:
                    input = gameObject.AddComponent<Input.MouseInput>();
                    break;

            }
            input.OnPlayerInput += HandleInput;
        }

        void OnDisable()
        {
            input.OnPlayerInput -= HandleInput;
        }

        void HandleInput(Vector3 inputScreenPos, Input.InteractionType type)
        {
            var tile = Game.Map.Province(inputScreenPos);

            if (tile == null)
                return;

            bool activeUser = Game.IsCurrentUserID(myData.ID);

            if (!activeUser && (type == Input.InteractionType.Path || type == Input.InteractionType.FinalizePath))
            {
                Debug.Log("No pathing when not your turn");
                return;
            }

            if (tile == Tile.HoverTile && type == Input.InteractionType.Inspect)
                return;

            tile.InteractWith(type);

        }

        public override void CollectOrders()
        {
            //Do nothing, just let player to UI things.
        }
    }
}