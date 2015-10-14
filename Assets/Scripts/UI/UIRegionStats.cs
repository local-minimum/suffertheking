using UnityEngine;
using UnityEngine.UI;
using Boardgame.Data;

namespace Boardgame.UI
{
    public class UIRegionStats : MonoBehaviour
    {

        [SerializeField]
        Text regionTitle;

        [SerializeField]
        Text population;

        [SerializeField]
        Text taxation;

        [SerializeField]
        Button taxationDecrease;

        [SerializeField]
        Button taxationIncrease;

        [SerializeField]
        Button taxationOrderReset;

        [SerializeField]
        Text nativity;

        [SerializeField]
        Text deathToll;

        Tile showingTile;

        Participant participant;

        [SerializeField]
        Mask UIMask;

        void Start()
        {
            if (UIMask == null)
                UIMask = GetComponent<Mask>();

            UIVisible = showingTile != null;
        }

        void OnEnable()
        {
            Tile.OnTileFocus += HandleNewTileFocus;
            Game.OnNewParticipantState += HandleParticipantState;
        }

        void OnDiable()
        {
            Tile.OnTileFocus -= HandleNewTileFocus;
            Game.OnNewParticipantState -= HandleParticipantState;
        }

        void HandleNewTileFocus(Tile tile)
        {
            if (tile == null)
                return;

            showingTile = tile;
            UIVisible = true;
        }

        void HandleParticipantState(Participant participant)
        {
            this.participant = participant;
            UpdateUIData();
        }


        bool UIVisible
        {
            set
            {
                UIMask.showMaskGraphic = value;
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    var t = child.GetComponentInChildren<Text>();
                    if (t)
                        t.enabled = value;
                    var im = child.GetComponentInChildren<Image>();
                    if (im)
                        im.enabled = value;
                    var b = child.GetComponentInChildren<Button>();
                    if (b)
                        b.enabled = value;
                }
                if (showingTile != null && value)
                    UpdateUIData();
            }
        }

        void UpdateUIData()
        {
            if (showingTile == null)
                return;

            regionTitle.text = showingTile.name;
            population.text = showingTile.demographics.population.ToString();
            UpdateTaxation();

            nativity.text = showingTile.demographics.nativity.ToString();
            deathToll.text = showingTile.demographics.deathToll.ToString();

            taxationDecrease.interactable = Game.IsCurrentUserID(showingTile.demographics.rulerID) && showingTile.demographics.taxation > 0;
            taxationIncrease.interactable = Game.IsCurrentUserID(showingTile.demographics.rulerID);
            taxationOrderReset.interactable = TaxOrder.HasTaxChangeOrder(showingTile);

        }

        public void ChangeTaxation(int value)
        {
            if (!showingTile)
                return;

            if (Game.ActionPoints.CanConsumePoints(TaxOrder.Cost(showingTile)))
            {
                TaxOrder.Tax(showingTile, value);
                UpdateTaxation();
                Tile.RemoveSelectLock();
            }

        }

        void UpdateTaxation()
        {
            var currentTax = showingTile.demographics.taxation + TaxOrder.TaxChangeOrdered(showingTile);
            taxation.text = currentTax.ToString();
            taxationDecrease.interactable = currentTax > 0;
            taxationOrderReset.interactable = TaxOrder.HasTaxChangeOrder(showingTile);

        }

        public void ResetTaxOrder()
        {
            TaxOrder.RemoveTaxOrder(showingTile);
            UpdateTaxation();
            Tile.RemoveSelectLock();
        }
    }
}
