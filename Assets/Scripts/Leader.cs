using UnityEngine;
using System.Collections;

namespace Boardgame.Data
{
    [System.Serializable]
    public class LeaderData
    {
        public int activeObjective = -1;
        public int activeDistraction = -1;
    }
}

namespace Boardgame
{   
    public static class Leader 
    {

        public static void Initiative(Data.LeaderData data)
        {
            Debug.Log("Leaderless society, leader always passive");
            Game.Step();
        }
    }
}
