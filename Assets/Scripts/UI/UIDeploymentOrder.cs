using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Boardgame.UI {
    public class UIDeploymentOrder : MonoBehaviour {

        [SerializeField]
        Transform contents;

        [SerializeField]
        Text fromRegion;

        [SerializeField]
        Text toRegion;

        [SerializeField]
        Toggle synchMove;

        [SerializeField]
        string defaultPrefSynchronized = "Game.DeploymentOrders.DefaultMoveSynchronized";

        Tile[] path;

        static UIDeploymentOrder instance;

        Data.DeploymentOrder order;

        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(this);
        }

        void OnEnable()
        {
            Pather.OnPathChange += HandleNewPath;
            Game.OnNewParticipantState += HandleNewTurnPhase;
        }

        void OnDisable()
        {
            Pather.OnPathChange -= HandleNewPath;
            Game.OnNewParticipantState -= HandleNewTurnPhase;
        }

        private void HandleNewTurnPhase(Data.Participant participant)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void HandleNewPath(Tile[] path, PathAction action)
        {
            Debug.Log("New path action " + action);
            if (action == PathAction.Finalized)
            {
                this.path = path;
                Show();
            }
        }

        void Start()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public bool UpdateOrder(Data.MilitaryUnitType type, int change)
        {
            
            return false;
        }

        public int GetCount(Data.MilitaryUnitType type)
        {
            return 0;
        }

        public void Show()
        {
            order = Data.DeploymentOrder.Create(path, PlayerPrefs.GetInt(defaultPrefSynchronized, 0) == 1);
            fromRegion.text = path[0].name;
            toRegion.text = path[path.Length - 1].name;
            synchMove.isOn = order.synchronizedMovement;
            transform.GetChild(0).gameObject.SetActive(true);
            for (int i=0, l=contents.childCount; i<l; i++)
            {
                var child = contents.GetChild(i);
                child.gameObject.SetActive(true);
                var unitDeployment = child.GetComponent<UIOrderUnitType>();
                unitDeployment.Show();
            }      
        }

        public void SignOrder()
        {
            order.SetOrdered();
            Tile.SelectLock.InteractWith(Input.InteractionType.Deselect);

        }

        public void RegretOrder()
        {
            order.undo();
            order = null;
            Tile.SelectLock.InteractWith(Input.InteractionType.Deselect);
        }
    }
}