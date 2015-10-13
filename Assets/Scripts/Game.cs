using UnityEngine;
using Boardgame.Logic;

public enum PlayerTurn { CivilSociety, Leader, MilitaryOrders, MilitaryActions, Resting};
public enum PlayerType { Player, RemotePlayer, AI}

namespace Boardgame.Logic
{
    [System.Serializable]
    public struct Participant
    {
        public PlayerTurn turn;
        public PlayerType type;
        public Player controler;

        public Participant(PlayerType type)
        {
            this.type = type;
            turn = PlayerTurn.Resting;
            controler = InstantiateController(type);
            
        }

        static Player InstantiateController(PlayerType type)
        {
            var GO = new GameObject();
            Player retVal;

            switch (type)
            {
                case PlayerType.Player:
                    retVal = GO.AddComponent<Player>();
                    break;
                default:
                    retVal = null;
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

namespace Boardgame { 

    public class Game : MonoBehaviour
    {

        [SerializeField]
        Logic.Participant[] participants;

        [SerializeField]
        UI.UIActionPoints _actionPoints;

        [SerializeField]
        Map map;

        int activeParticipant = 0;

        static Game _instance;

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            if (participants.Length == 0)
                participants = new Logic.Participant[] { new Logic.Participant(PlayerType.Player) };

            Tile.Focus(participants[activeParticipant].controler.capitol);
            Tile.RemoveSelectLock();
        }

        public static void Step()
        {
            _instance.updateStep();
            _instance.enactStep();
        }

        void updateStep()
        {
            participants[activeParticipant].turn = participants[activeParticipant].turn.Next();
            Debug.Log(string.Format("Player {0} ({1}) is {2}", participants[activeParticipant].controler.name, participants[activeParticipant].type, participants[activeParticipant].turn));
            if (participants[activeParticipant].turn == PlayerTurn.Resting)
            {
                activeParticipant++;
                activeParticipant %= participants.Length;
                updateStep(); 
            }
        }

        void enactStep()
        {

        }

        public static UI.UIActionPoints ActionPoints
        {
            get
            {
                return _instance._actionPoints;
            }
        }


    }

}