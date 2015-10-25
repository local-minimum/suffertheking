using UnityEngine;
using System.Collections.Generic;
using Boardgame.Data;

namespace Boardgame {
    public class Military : MonoBehaviour {

        static Military _instance;

        [SerializeField]
        MilitaryUnit[] unitTypes = new MilitaryUnit[] { };

        [SerializeField]
        List<MilitaryUnit> armies = new List<MilitaryUnit>();

        static Military instance
        {
            get
            {
                if (_instance == null)
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

        public static MilitaryUnit Allocate(Participant participant, Tile region, MilitaryUnitType unitType, int quantity)
        {
            var armies = instance.armies;
            for (int i=0, l=armies.Count; i< l; i++)
            {

                if (IsMatchingAndAvailable(armies[i], participant, region, unitType, quantity))
                {
                    var unit = armies[i];
                    if (unit.count == quantity)
                    {
                        unit.Deploy();
                        return unit;
                    }

                    var newUnit = unit.split(quantity);
                    newUnit.Deploy();
                    instance.armies.Add(newUnit);
                    return newUnit;
                }
            }

            return null;
        }

        public static void AllocateInto(ref MilitaryUnit targetUnit, Participant participant, Tile region, MilitaryUnitType unitType, int quantity)
        {
            var additions = Allocate(participant, region, unitType, quantity);
            if (additions != null)
            {
                Debug.Log("Got addition of " + additions.count + " to add to the current " + targetUnit);
                additions.Free();
                targetUnit.Free();
                additions.join(targetUnit);
                targetUnit.Deploy();
                instance.armies.Remove(additions);
            }
            else
                Debug.Log("Couldn't allocate into as no unit got allocated");
        }

        public static bool FreeFrom(ref MilitaryUnit unit, int amount)
        {
            if (unit.count == amount)
            {
                return Free(unit);

            } else if (unit.count > amount)
            {
                unit.Free();
                var freedPartialUnit = unit.split(amount);
                unit.Deploy();
                if (freedPartialUnit == null)
                {
                    Debug.LogWarning("Couldn't split unit");
                    return false;
                }

                if (!JoinWithSimilar(freedPartialUnit))
                    instance.armies.Add(freedPartialUnit);
                return true;
            }
            return false;
        }

        public static bool Free(MilitaryUnit unit)
        {
            unit.Free();
            JoinWithSimilar(unit);
            return true;
        }

        static bool JoinWithSimilar(MilitaryUnit unit)
        {
            var similarUnits = AllUnits(Game.Map.GetProvince(unit.location), unit.commander, unit.type);
            while (similarUnits.Count > 0)
            {
                var otherUnit = similarUnits.Dequeue();
                if (otherUnit != unit && otherUnit.available)
                {
                    Debug.Log("Will join " + unit + " with " + otherUnit);
                    unit.join(otherUnit);
                    if (!instance.armies.Remove(unit))
                        Debug.Log("I don't recognize " + unit + " among the enlisted, this is OK when freeing partial units");
                    return true;
                }
            }

            return false;

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

        public static Queue<MilitaryUnit> AllUnits(MilitaryUnitType type)
        {
            if (Tile.SelectLock == null)
                return new Queue<MilitaryUnit>();
            else
                return AllUnits(Tile.SelectLock, Game.activeUserID, type);
        }

        public static Queue<MilitaryUnit> AllUnits(Tile region, int participantID, MilitaryUnitType type)
        {
            var units = new Queue<MilitaryUnit>();
            if (region == null)
                return units;

            var allArmies = instance.armies;
            for (int i = 0, l = allArmies.Count; i < l; i++)
            {
                if (allArmies[i].type == type && allArmies[i].commander == participantID && allArmies[i].location == region.name)
                    units.Enqueue(allArmies[i]);
            }
            return units;
        }

        static bool IsMatchingAndAvailable(MilitaryUnit unit, Participant participant, Tile region, MilitaryUnitType unitType, int count)
        {
            return IsMatchingAndAvailable(unit, participant, region, unitType) && unit.count >= count;
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