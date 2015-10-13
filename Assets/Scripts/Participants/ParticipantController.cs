using UnityEngine;
using System.Collections.Generic;

namespace Boardgame.Data
{
    [System.Serializable]
    public class Participant
    {
        private int _id;
        public int ID
        {
            get
            {
                return _id;
            }
        }

        public string name;
        public PlayerTurn turn;
        public PlayerType type;
        public string capitolName;
        public LeaderData leaderData;
        public int actionPoints;
        public int actionPointsRenewalRate;

        public Tile captiol
        {
            get
            {
                return Game.Map.Province(capitolName);
            }
        }

        public Participant(string name, PlayerType type, string capitolName)
        {
            this.type = type;
            turn = PlayerTurn.Resting;
            this.capitolName = capitolName;
            this.name = name;
            leaderData = new LeaderData();
            _id = Game.NextParticipantID;
            actionPoints = 0;
            actionPointsRenewalRate = 4;
        }

        static ParticipantController InstantiateController(PlayerType type)
        {
            var GO = new GameObject();
            ParticipantController retVal;

            switch (type)
            {
                case PlayerType.Player:
                    retVal = GO.AddComponent<LocalPlayer>();
                    break;
                default:
                    retVal = GO.AddComponent<ParticipantController>();
                    break;
            }

            return retVal;
        }
    }

    public static class Extentions
    {

        public static PlayerTurn Next(this PlayerTurn e)
        {
            switch (e)
            {
                case PlayerTurn.CivilSociety:
                    return PlayerTurn.Leader;
                case PlayerTurn.Leader:
                    return PlayerTurn.MilitaryOrders;
                case PlayerTurn.MilitaryOrders:
                    return PlayerTurn.MilitaryActions;
                case PlayerTurn.MilitaryActions:
                    return PlayerTurn.Resting;
                case PlayerTurn.Resting:
                    return PlayerTurn.CivilSociety;
                default:
                    return PlayerTurn.Resting;

            }
        }

    }
}

namespace Boardgame
{
    public class ParticipantController : MonoBehaviour
    {

        static Dictionary<int, ParticipantController> controllers = new Dictionary<int, ParticipantController>();

        public static void CollectOrders(Data.Participant participant)
        {
            if (!controllers.ContainsKey(participant.ID))
                addControllerByType(participant);

            controllers[participant.ID].CollectOrders();
        }

        static void addControllerByType(Data.Participant participant)
        {
            var GO = new GameObject();
            GO.name = participant.name;
            GO.transform.SetParent(Game.PlayersInEditor);
            ParticipantController controller;
            if (participant.type == Data.PlayerType.Player)
                controller = GO.AddComponent<LocalPlayer>();
            else
                controller = GO.AddComponent<ParticipantController>();
            controllers.Add(participant.ID, controller);
        }

        virtual public void CollectOrders()
        {
            Debug.Log("No orders");
            Game.Step();
        }

    }
}
