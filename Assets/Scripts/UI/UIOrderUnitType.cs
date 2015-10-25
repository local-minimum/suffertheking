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

        public void Show()
        {
            totalCountAvailable = deploymentOrder.GetCount(type); ;
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


        public void Increase()
        {
            UpdateCount(1);
        }

        public void Decrease()
        {
            UpdateCount(-1);
        }

        void UpdateCount(int amount)
        {
            if (!deploymentOrder.UpdateOrder(type, amount))
                Debug.LogWarning("Update refused!");

            var newCount = deploymentOrder.GetCount(type);
            lessUnits.interactable = newCount > 0;
            moreUnits.interactable = newCount < totalCountAvailable;
            countText.text = newCount.ToString();

        }
    }
}
