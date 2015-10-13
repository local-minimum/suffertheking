using UnityEngine;
using UnityEngine.UI;
using Boardgame.Logic;

namespace Boardgame.UI
{
    public class UIActionPoints : MonoBehaviour
    {

        [SerializeField]
        Image[] points;

        [SerializeField]
        int renewalRate = 4;

        int currentPoints = 0;

        [SerializeField]
        Color32 remainingPointColor;

        [SerializeField]
        Color32 usedPointColor;

        void HandlePlayerTurnStart()
        {
            ConsumePoints(-renewalRate);
        }

        void Start()
        {
            ConsumePoints(-renewalRate);
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
            currentPoints += points;
            SetUIColors();
        }

        void SetUIColors()
        {
            for (int i = 0; i < points.Length; i++)
                points[i].color = i < currentPoints ? remainingPointColor : usedPointColor;
        }

        public bool ConsumePoints(int points)
        {
            if (points < currentPoints)
            {
                currentPoints -= points;
                currentPoints = Mathf.Min(currentPoints, this.points.Length);
                SetUIColors();
                return true;
            }
            return false;
        }

        public bool CanConsumePoints(int points)
        {
            return points <= currentPoints;
        }

        public void EndTurn()
        {
            Debug.Log("End turn");
        }

        public void ResetAllOrders()
        {
            Order.Clear();
        }
    }
}