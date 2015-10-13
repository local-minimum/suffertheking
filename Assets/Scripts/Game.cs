using UnityEngine;
using System.Collections;

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

        [SerializeField]
        Player player;

        static Game _instance;

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            Tile.Focus(player.capitol);
            Tile.RemoveSelectLock();
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