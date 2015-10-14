﻿using UnityEngine;
using System.Collections;

namespace Boardgame.Data
{

    public enum MilitaryUnitType { Fodder, Tech, Infantry, Bomber, MissileBattery, Tanks}

    [System.Serializable]
    public class MilitaryUnit 
    {
        public MilitaryUnitType type;

        public int count;

        [HideInInspector]
        public int commander = -1;

        public int[] attack;
        public int[] defence;

        public int constrtuctionTurns;
        public int constructionProgress;

        bool deployed = false;
        public string location;

        public bool available {
            get
            {
                return !deployed && !underConstruction;
            }
        }

        public bool underConstruction
        {
            get
            {
                return constructionProgress < constrtuctionTurns;
            }
        }

        public MilitaryUnit split(int removalCount)
        {
            if (!available || removalCount >= count)
                return null;

            count -= removalCount;

            return template(removalCount);
        }

        public MilitaryUnit join(MilitaryUnit other)
        {
            if (Joinable(this, other))
            {
                other.count += this.count;
                this.count = 0;
            }
            return other;
        }

        static bool Joinable(MilitaryUnit a, MilitaryUnit b)
        {
            return a.type == b.type && a.available && b.available && a.location == b.location;
        }

        public MilitaryUnit template(int count)
        {
            var newUnit = new MilitaryUnit();
            System.Array.Copy(attack, newUnit.attack, attack.Length);
            System.Array.Copy(defence, newUnit.defence, defence.Length);

            newUnit.constrtuctionTurns = constrtuctionTurns;
            newUnit.constructionProgress = constructionProgress;
            newUnit.deployed = false;

            newUnit.location = location;
            return newUnit;
        }
    }

}
