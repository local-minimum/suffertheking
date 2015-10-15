using UnityEngine;
using System.Collections.Generic;
using Boardgame.Data;

namespace Boardgame {
    public class Military : MonoBehaviour {

        static Military _instance;

        [SerializeField]
        MilitaryUnit[] unitTypes = new MilitaryUnit[] { };

        [SerializeField, HideInInspector]
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

                if (IsMatchingAndAvailable(armies[i], participant, region, unitType))
                {
                    var unit = armies[i];
                    if (unit.count <= quantity)
                        return unit;

                    var newUnit = unit.split(quantity);
                    instance.armies.Add(newUnit);
                    return newUnit;
                }
            }

            return null;
        }

        public static bool HasAnyAvailableUnit(Tile region, int participantID)
        {
            var armies = instance.armies;
            for (int i=0, l=armies.Count; i<l; i++)
            {
                if (armies[i].commander == participantID && armies[i].location == region.name && armies[i].available && armies[i].count > 0)
                    return true;
            }
            return false;
        }

        static bool IsMatchingAndAvailable(MilitaryUnit unit, Participant participant, Tile region, MilitaryUnitType unitType)
        {
            return unit.commander == participant.ID && unit.type == unitType && unit.location == region.name && unit.available;
        }

        static public bool Construct(Participant participant, Tile region, MilitaryUnitType type, int quantity)
        {

            for (int i = 0; i < instance.unitTypes.Length; i++)
            {
                if (instance.unitTypes[i].type == type)
                {
                    var newUnit = instance.unitTypes[i].template(quantity);
                    newUnit.commander = participant.ID;
                    newUnit.location = region.name;

                    instance.armies.Add(newUnit);
                    return true;
                }
            }
            Debug.LogWarning("No model found for type " + type);
            return false;
        }

        public static void BuildUnits(Participant participant)
        {
            var allArmies = instance.armies;
            for (int i = 0, l = allArmies.Count; i<l; i++)
            {
                if (allArmies[i].underConstruction && allArmies[i].commander == participant.ID)
                    allArmies[i].constructionProgress++;
            }
            Game.Step();
        }
    }
}