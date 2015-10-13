using UnityEngine;
using System.Collections;

namespace Boardgame.Data
{
    [System.Serializable]
    public struct LeaderData
    {
        public int activeObjective;
        public int activeDistraction;
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
