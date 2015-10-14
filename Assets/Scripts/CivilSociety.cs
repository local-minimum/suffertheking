using System.Collections.Generic;
using UnityEngine;

namespace Boardgame
{
    public static class CivilSociety
    {
        static int capitolCarryingCapacityFactor = 2;
        static int warCost = 20;
        static int revolutionCost = 40;
        static int warTornCost = 6;
        

        public static void Initiative(Data.Participant participant)
        {
            var domain = Game.Map.Domain(participant.ID);
            Debug.Log(string.Format("Participant {0} ({1}) has {2} regions",
                participant.name, participant.ID, domain.Count));

            for (int i=0, l=domain.Count; i< l;i++)
            {
                SetNewPopulation(domain[i].demographics);
                var tax = Tax(domain[i].demographics);
                domain[i].EnlistPeople(tax);
                SetCivilianState(domain[i].demographics, tax);
                
            }
            Game.Step();
        }

        static void SetNewPopulation(Data.Demographics demographics)
        {

            float overCapacityFactor = 70f;
            int nativityCap = 40;
            int burdenRoll = 20;

            //Add last round's additions to general population
            demographics.population += demographics.nativity;

            //Balance carrying capacity
            var carryingCapacity = demographics.carryingCapacity * (demographics.capitol ? capitolCarryingCapacityFactor : 1);
            var burden = Mathf.RoundToInt(Mathf.Max((float)demographics.population / (float)carryingCapacity, 0.5f) * overCapacityFactor);

            //Deaths
            demographics.deathToll = 0;
            while (burden > 0)
            {

                demographics.deathToll += Dice.Roll(burdenRoll);
                burden -= burdenRoll;
            }
            
            if (demographics.warState == Data.Demographics.StateOfWar.AtWar)
                demographics.deathToll += Dice.Roll(warCost);
            else if (demographics.warState == Data.Demographics.StateOfWar.WarTorn)
                demographics.deathToll += Dice.Roll(warTornCost);

            if (demographics.civilianState == Data.Demographics.StateOfCivils.Revolution)
                demographics.deathToll += Dice.Roll(revolutionCost);

            demographics.population -= demographics.deathToll;

            //Calculate newborns
            if (demographics.population > 1)
                demographics.nativity = Mathf.Max(Mathf.Min(demographics.population, demographics.carryingCapacity) / demographics.birthRate, nativityCap) + Dice.Roll(3);
            else if (demographics.population == 1)
                demographics.nativity = Dice.Roll(2);
            else
                demographics.nativity = 0;

        }

        static int Tax(Data.Demographics demographics)
        {
            var tax = Mathf.Min(demographics.population, demographics.taxation);
            demographics.population -= tax;
            return tax;
        }

        static void SetCivilianState(Data.Demographics demographics, int tax)
        {
            int dieA = 0;
            int dieB = 0;
            int dieC = 0;
            int dieD = 0;
            float happyTaxLevel = 0.01f;
            float acceptableTaxLevel = 0.1f;
            int successRoll = 15;
            int failRoll = 6;
            int critFailRoll = 3;

            if (demographics.affiliationStatus == Data.Demographics.Affiliation.Neutral)
            {
                demographics.civilianState = Data.Demographics.StateOfCivils.AtPeace;
                return;
            }

            switch (demographics.warState)
            {
                case Data.Demographics.StateOfWar.AtWar:
                    dieA = 6;
                    break;
                case Data.Demographics.StateOfWar.WarTorn:
                    dieA = 4;
                    break;
                case Data.Demographics.StateOfWar.AtPeace:
                    dieA = 6;
                    dieB = 6;
                    break;
            }

            switch (demographics.affiliationStatus)
            {
                case Data.Demographics.Affiliation.Claimed:
                    dieD = 10;
                    break;
            }

            var taxFraction = (float)tax / (float)demographics.population;
            if (taxFraction < happyTaxLevel)
                dieC = 12;
            else if (taxFraction < acceptableTaxLevel)
                dieC = 6;

            var roll = Dice.SumRoll(dieA, dieB, dieC, dieD);

            if (roll <= critFailRoll)
                demographics.civilianState = Data.Demographics.StateOfCivils.Revolution;
            else if (roll <= failRoll)
            {
                if (demographics.civilianState == Data.Demographics.StateOfCivils.AtPeace)
                    demographics.civilianState = Data.Demographics.StateOfCivils.Discontent;
                else
                    demographics.civilianState = Data.Demographics.StateOfCivils.Revolution;
            } else if (roll >= successRoll)
            {
                if (demographics.civilianState == Data.Demographics.StateOfCivils.Revolution)
                    demographics.civilianState = Data.Demographics.StateOfCivils.Discontent;
                else
                    demographics.civilianState = Data.Demographics.StateOfCivils.AtPeace;
            }

        }
    }

}
