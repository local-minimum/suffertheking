using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Boardgame.UI
{
    public class UIUnitStats : MonoBehaviour
    {

        [SerializeField]
        Text activeText;

        [SerializeField]
        Text busyText;

        [SerializeField]
        Text buildingText;

        [SerializeField]
        Data.MilitaryUnitType unitType;

 
        void OnEnable()
        {
            Tile.OnInteraction += HandleNewTileInspection;
            Game.OnNewParticipantState += HandleNewTurnPhase;
        }

        void OnDisable()
        {
            Tile.OnInteraction -= HandleNewTileInspection;
            Game.OnNewParticipantState -= HandleNewTurnPhase;
        }

        private void HandleNewTurnPhase(Data.Participant participant)
        {
            if (participant.turn == Data.PlayerTurn.MilitaryOrders)
                HandleNewTileInspection(Tile.SelectLock ? Tile.SelectLock : Tile.HoverTile, Input.InteractionType.Inspect);
        }

        private void HandleNewTileInspection(Tile tile, Input.InteractionType type)
        {
            if (!(type == Input.InteractionType.Select || type == Input.InteractionType.Inspect))
                return;

            int actives = 0;
            int busies = 0;
            int building = 0;
            var units = Military.AllUnits(tile, Game.activeUserID, unitType);

            Debug.Log(string.Format("Summing up units of type {0} for player {1} ({2})", unitType, Game.activeUserID, units.Count));

            while (units.Count > 0)
            {
                var unit = units.Dequeue();
                if (unit.underConstruction)
                    building += unit.count;
                else if (unit.available)
                    actives += unit.count;
                else
                    busies += unit.count;
            }

            UpdateText(actives, busies, building);
        }

        public void UpdateText(int actives, int busies, int building)
        {
            activeText.text = actives.ToString();
            busyText.text = busies.ToString();
            buildingText.text = building.ToString();
        }
    }
}
