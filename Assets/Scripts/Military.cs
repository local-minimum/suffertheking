using UnityEngine;
using System.Collections.Generic;
using Boardgame.Data;

namespace Boardgame {
    public class Military : MonoBehaviour {

        static Military _instance;

        [SerializeField]
        Dictionary<MilitaryUnitType, MilitaryUnit> unitTypes = new Dictionary<MilitaryUnitType, MilitaryUnit>();

        [SerializeField]
        List<MilitaryUnit> armies = new List<MilitaryUnit>();

        static Military instance
        {
            get
            {
                if (_instance != null)
                {
                    var GO = new GameObject();
                    GO.AddComponent<Military>();
                }
                return _instance;
            }
        }

        void Awake() {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

            } else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        static MilitaryUnit Allocate(Participant participant, Tile region, MilitaryUnitType unitType, int quantity)
        {
            var armies = instance.armies;
            for (int i=0, l=armies.Count; i< l; i++)
            {

            }

            return null;
        }

        static void Construct(Participant participant, Tile region, MilitaryUnitType type, int quantity)
        {

            var newUnit = instance.unitTypes[type].template(quantity);
            newUnit.commander = participant.ID;
            newUnit.location = region.name;

            instance.armies.Add(newUnit);

        }
    }
}