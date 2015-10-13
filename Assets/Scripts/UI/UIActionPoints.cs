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
                SetUIColors();
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
        }

        void OnEnable()
        {
            Order.OnUndoOrder += HandleUndoOrder;
            Game.OnNewParticipantState += HandleParticipantState;
        }

        void OnDisable()
        {
            Order.OnUndoOrder -= HandleUndoOrder;
            Game.OnNewParticipantState -= HandleParticipantState;
        }

        void HandleUndoOrder(int points)
        {
            currentParticipant.actionPoints += points;
            SetUIColors();
        }

        void HandleParticipantState(Participant participant)
        {
            if (participant.type == PlayerType.Player && participant.turn == Data.PlayerTurn.MilitaryOrders)
            {
                currentParticipant = participant;
                ConsumePoints(-currentParticipant.actionPointsRenewalRate);
                PlayerTurn = true;
                Debug.Log("Action Granted");
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