using UnityEngine;
using UnityEngine.UI;

public class UIRegionStats : MonoBehaviour {

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

    Tile showingTile;

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
    }

    void OnDiable()
    {
        Tile.OnTileFocus -= HandleNewTileFocus;
    }

    void HandleNewTileFocus(Tile tile)
    {
        if (tile == null)
            return;

        showingTile = tile;
        UIVisible = true;
    }

    bool UIVisible
    {
        set
        {
            UIMask.showMaskGraphic = value;
            for (int i=0;i<transform.childCount; i++)
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
            {
                regionTitle.text = showingTile.name;
                population.text = showingTile.population.population.ToString();
                UpdateTaxation();
                nativity.text = showingTile.population.birthRate.ToString();
                taxationDecrease.interactable = showingTile.population.ruler == Population.Affiliation.Player && showingTile.population.taxation > 0;
                taxationIncrease.interactable = showingTile.population.ruler == Population.Affiliation.Player;
                taxationOrderReset.interactable = TaxOrder.HasTaxChangeOrder(showingTile);
            }
        }
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
        var currentTax = showingTile.population.taxation + TaxOrder.TaxChangeOrdered(showingTile);
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
