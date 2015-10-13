using UnityEngine;
using Boardgame.Data;

namespace Boardgame.Data
{
    public enum PlayerTurn { CivilSociety, Leader, MilitaryOrders, MilitaryActions, Resting };
    public enum PlayerType { Player, RemotePlayer, AI, Neutral }

    [System.Serializable]
    struct GameState
    {
        public int highestParticipantID;
        public int activeParticipant;
    }
}

namespace Boardgame { 

    public class Game : MonoBehaviour
    {

        public delegate void ParticipantState(ref Participant participant);

        public static event ParticipantState OnNewParticipantState;

        [SerializeField]
        Participant[] participants = new Participant[0];

        [SerializeField]
        UI.UIActionPoints _actionPoints;

        [SerializeField]
        Map map;

        [SerializeField]
        Transform playersInEditor;

        [SerializeField]
        GameState gameState;

        public static Transform PlayersInEditor
        {
            get
            {
                return _instance.playersInEditor;
            }
        }


        static string gameStateFile = "gameState.dat";

        public static Map Map
        {
            get
            {
                return _instance.map;
            }
        }

        static Game _instance;

        static string participantsSaveDataFile = "campaignParticipants.dat";

        void Awake()
        {
            if (_instance == null) {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            } else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static int NextParticipantID
        {
            get
            {
                _instance.gameState.highestParticipantID++;
                return _instance.gameState.highestParticipantID;
            }
        }

        public static bool IsCurrentUserID(int ID)
        {
            return ID == _instance.participants[_instance.gameState.activeParticipant].ID;
        }

        void OnEnable()
        {
            if (DataPersistence.HasSavedData(participantsSaveDataFile))
                participants = DataPersistence.LoadData<Participant[]>(participantsSaveDataFile);

            if (DataPersistence.HasSavedData(gameStateFile))
                gameState = DataPersistence.LoadData<GameState>(gameStateFile);
            else
                gameState = new GameState();
               
        }

        void OnDisable()
        {
            DataPersistence.SaveData(participants, participantsSaveDataFile);
            DataPersistence.SaveData(gameState, gameStateFile);
        }

        void Start()
        {
            if (participants.Length == 0)
                AddParticipant(PlayerType.Player);

            if (!HasNeutralParticipant)
                AddParticipant(PlayerType.Neutral);

            Tile.Focus(participants[gameState.activeParticipant].captiol);
            Tile.RemoveSelectLock();

            if (OnNewParticipantState != null)
                OnNewParticipantState(ref participants[gameState.activeParticipant]);

        }

        bool HasNeutralParticipant
        {
            get
            {
                for (int i=0;i<participants.Length;i++)
                {
                    if (participants[i].type == PlayerType.Neutral)
                        return true;
                }
                return false;
            }
        }

        void AddParticipant(PlayerType type)
        {
            var participants = new Participant[this.participants.Length + 1];
            this.participants.CopyTo(participants, 0);
            participants[participants.Length - 1] = new Participant("No name", type, "");

            this.participants = participants;
        }

        public static void Step()
        {
            _instance.updateStep();
            _instance.enactStep();
        }

        void updateStep()
        {
            participants[gameState.activeParticipant].turn = participants[gameState.activeParticipant].turn.Next();

            Debug.Log(string.Format("Player {0} ({1}) will do {2}",
                participants[gameState.activeParticipant].name,
                participants[gameState.activeParticipant].type,
                participants[gameState.activeParticipant].turn));

            if (OnNewParticipantState != null)
                OnNewParticipantState(ref participants[gameState.activeParticipant]);

                if (participants[gameState.activeParticipant].turn == PlayerTurn.Resting)
            {
                gameState.activeParticipant++;
                gameState.activeParticipant %= participants.Length;
                updateStep(); 
            }
        }

        void enactStep()
        {
            var participant = participants[gameState.activeParticipant];
            Debug.Log("Enacting turn: " + participant.turn);
            switch (participant.turn)
            {
                case PlayerTurn.CivilSociety:
                    CivilSociety.Initiative(participant);
                    break;
                case PlayerTurn.Leader:
                    Leader.Initiative(participant.leaderData);
                    break;
                case PlayerTurn.MilitaryOrders:
                    ParticipantController.CollectOrders(participant);
                    break;
                case PlayerTurn.MilitaryActions:
                    StartCoroutine(Order.ExecuteOrders());
                    break;
                default:
                    Debug.LogError(string.Format("Requesting to enact {0} on {1} which is not possible",
                        participant.turn,
                        participant.name));
                    Game.Step();
                    break;
                
            }
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