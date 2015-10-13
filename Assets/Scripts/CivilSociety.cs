using System.Collections.Generic;
using UnityEngine;

namespace Boardgame
{
    public static class CivilSociety
    {
        public static void Initiative(Data.Participant participant)
        {
            var domain = Game.Map.Domain(participant.ID);
            for (int i=0, l=domain.Count; i< l;i++)
            {
                domain[i].demographics.population += domain[i].demographics.population  * domain[i].demographics.birthRate / 1000;
                var tax = Mathf.Min(domain[i].demographics.population, domain[i].demographics.taxation);
                domain[i].demographics.population -= tax;

                //Todo: Give tax to military
            }
            Game.Step();
        }
    }

}
