using UnityEngine;
using Boardgame.Data;
using System.Collections.Generic;
using System.Linq;

namespace Boardgame.Data
{
    public enum PlayerTurn { CivilSociety, Leader, MilitaryConstruction, MilitaryOrders, MilitaryActions, Resting };
    public enum PlayerType { Player, RemotePlayer, AI, Neutral, Active}

    [System.Serializable]
    public class GameState
    {
        public int activeParticipant = 0;
    }
}

namespace Boardgame { 

    public class Game : MonoBehaviour
    {

        public delegate void ParticipantState(Participant participant);

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
                return instance.playersInEditor;
            }
        }


        static string gameStateFile = "gameState.dat";

        public static Map Map
        {
            get
            {
                return instance.map;
            }
        }

        static Game _instance;
        static Game instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<Game>();
                    if (_instance == null)
                    {
                        var GO = new GameObject();
                        GO.name = "Game Manager";
                        _instance = GO.AddComponent<Game>();
                    }

                }
                return _instance;
            }
        }
        static string participantsSaveDataFile = "campaignParticipants.dat";

        Pather pather;
        public static Pather Pather
        {
            get
            {
                if (instance.pather == null)
                    instance.pather = instance.gameObject.AddComponent<Pather>();

                return instance.pather;
            }
        }

        void Awake()
        {
            if (_instance == null) {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            } else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }            
        }        

        public static bool IsCurrentUserID(int ID)
        {
            return ID == activeUserID;
        }

        public static int activeUserID
        {
            get
            {
                return instance.participants[instance.gameState.activeParticipant].ID;
            }
        }

        public static Participant GetParticipant(int id)
        {
            for (int i=0, l=instance.participants.Length; i< l; i++)
            {
                if (instance.participants[i].ID == id)
                    return instance.participants[i];
            }
            return null;
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
            SetupParticipantsNeededByMap();

            var tile = participants[gameState.activeParticipant].captiol;
            if (tile)
                tile.InteractWith(Input.InteractionType.Inspect);

            if (OnNewParticipantState != null)
                OnNewParticipantState(participants[gameState.activeParticipant]);

        }

        void SetupParticipantsNeededByMap()
        {
            var neededParticipants = Map.GetRequiredParticipants();
            SetActiveParticipants(neededParticipants.Keys.ToList());
            foreach (var kvp in neededParticipants)
            {
                if (!HasMatchingParticipant(kvp.Value, kvp.Key))
                {

                    //TODO: Here may need to add player type if multiplayer when supporting it
                    AddParticipant(kvp.Value != PlayerType.Active ? kvp.Value : PlayerType.AI, kvp.Key);

                }

                var participant = GetParticipant(kvp.Key);
                ParticipantController.SetupController(participant);
                Debug.Log(string.Format("{0} (ID {1}) setup as {2} ({3})",
                    participant.name, participant.ID, participant.type, kvp.Value));
            }
        }

        bool HasMatchingParticipant(PlayerType type, int id)
        {
            for (int i = 0; i < participants.Length; i++)
            {
                if (participants[i].ID == id)
                {
                    if (participants[i].type != type && !(type == PlayerType.Active && (type == PlayerType.AI || type == PlayerType.Player || type == PlayerType.RemotePlayer)))
                    {
                        Debug.Log("Changing type for participant" + participants[i]);
                        participants[i].type = type;
                    }
                    return true;
                }
            }
            return false;

        }

        void SetActiveParticipants(List<int> activeID)
        {
            for (int i=0;i<participants.Length;i++)
                participants[i].active = activeID.Contains(participants[i].ID);

        }

        void AddParticipant(PlayerType type, int id)
        {
            var participants = new Participant[this.participants.Length + 1];
            this.participants.CopyTo(participants, 0);
            participants[participants.Length - 1] = new Participant("No name", type, "", id);

            this.participants = participants;
        }

        public static void Step()
        {
            instance.updateStep();
            instance.enactStep();
        }

        void updateStep()
        {
            participants[gameState.activeParticipant].turn = participants[gameState.activeParticipant].turn.Next();

            Debug.Log(string.Format("Player {0} ({1}) will do {2}",
                participants[gameState.activeParticipant].name,
                participants[gameState.activeParticipant].type,
                participants[gameState.activeParticipant].turn));

            if (OnNewParticipantState != null)
                OnNewParticipantState(participants[gameState.activeParticipant]);

            if (participants[gameState.activeParticipant].turn == PlayerTurn.Resting)
            {
                regenActionPoints();
                gameState.activeParticipant++;
                gameState.activeParticipant %= participants.Length;
                updateStep(); 
            }
        }

        void regenActionPoints()
        {
            var p = participants[gameState.activeParticipant];
            p.actionPoints = Mathf.Min(p.actionPoints + p.actionPointsRenewalRate, p.actionPointsMax);
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
                case PlayerTurn.MilitaryConstruction:
                    Military.BuildUnits(participant);
                    break;
                case PlayerTurn.MilitaryOrders:
                    ParticipantController.CollectOrders(participant);
                    break;
                case PlayerTurn.MilitaryActions:
                    StartCoroutine(OrderLog.ExecuteOrders());
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
                return instance._actionPoints;
            }
        }


    }

}