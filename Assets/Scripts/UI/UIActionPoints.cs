using UnityEngine;
using UnityEngine.UI;
using Boardgame.Data;

namespace Boardgame.UI
{
    public class UIActionPoints : MonoBehaviour
    {

        [SerializeField]
        Image[] points;

        Participant currentParticipant;

        bool _playerTurn = false;

        [SerializeField]
        Color32 remainingPointColor;

        [SerializeField]
        Color32 usedPointColor;

        [SerializeField]
        Button redeemButton;

        [SerializeField]
        Button endTurnButton;

        bool PlayerTurn
        {
            set
            {
                redeemButton.interactable = value;
                endTurnButton.interactable = value;
                _playerTurn = value;
            }

            get {
                return _playerTurn;
            }
        }

        void Awake()
        {
            PlayerTurn = false;
        }

        void Start()
        {
            SetUIColors();
        }

        void OnEnable()
        {
            Order.OnUndoOrder += HandleUndoOrder;
        }

        void OnDisable()
        {
            Order.OnUndoOrder -= HandleUndoOrder;
        }

        void HandleUndoOrder(int points)
        {
            currentParticipant.actionPoints += points;
            SetUIColors();
        }

        void HandlePlayerOrders(ref Participant participant)
        {
            if (participant.type == PlayerType.Player)
            {
                currentParticipant = participant;
                ConsumePoints(-currentParticipant.actionPointsRenewalRate);
                PlayerTurn = true;
                SetUIColors();
            }

        }

        void SetUIColors()
        {
            for (int i = 0; i < points.Length; i++)
                points[i].color = PlayerTurn && i <  currentParticipant.actionPoints ? remainingPointColor : usedPointColor;
        }

        public bool ConsumePoints(int points)
        {
            if (points < currentParticipant.actionPoints)
            {
                currentParticipant.actionPoints -= points;
                currentParticipant.actionPoints = Mathf.Min(currentParticipant.actionPoints, this.points.Length);
                SetUIColors();
                return true;
            }
            return false;
        }

        public bool CanConsumePoints(int points)
        {
            return points <= currentParticipant.actionPoints;
        }

        public void EndTurn()
        {
            PlayerTurn = false;
            Game.Step();
        }

        public void ResetAllOrders()
        {
            Order.ClearAllOrders();
        }
    }
}