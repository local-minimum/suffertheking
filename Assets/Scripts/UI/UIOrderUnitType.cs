using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Boardgame.UI
{
    public class UIOrderUnitType : MonoBehaviour
    {

        [SerializeField]
        Data.MilitaryUnitType type;

        [SerializeField]
        Text typeLabel;

        [SerializeField]
        Text countText;

        [SerializeField]
        Image icon;

        [SerializeField]
        Button moreUnits;

        [SerializeField]
        Button lessUnits;

        int totalCountAvailable;

        UIDeploymentOrder deploymentOrder;

        void Awake()
        {
            if (deploymentOrder == null)
                deploymentOrder = GetComponentInParent<UIDeploymentOrder>();

        }

        void OnEnable()
        {
            Show();
        }

        public void Show()
        {
            totalCountAvailable = 0;
            var units = Military.AllUnits(type);
            while (units.Count > 0) {
                var unit = units.Dequeue();
                totalCountAvailable += unit.available ? unit.count : 0;
            }
            if (totalCountAvailable > 0)
                UpdateCount(totalCountAvailable);
            else
                Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void UpdateCount(int amount)
        {
            if (deploymentOrder.UpdateOrder(type, amount))
            {
                var newCount = deploymentOrder.GetCount(type);
                lessUnits.interactable = newCount == 0;
                moreUnits.interactable = newCount < totalCountAvailable;
                countText.text = newCount.ToString();
            }
        }
    }
}
