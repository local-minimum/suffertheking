using UnityEngine;
using Boardgame.Data;

public enum PlayerTurn { CivilSociety, Leader, MilitaryOrders, MilitaryActions, Resting};
public enum PlayerType { Player, RemotePlayer, AI, Neutral}


namespace Boardgame { 

    public class Game : MonoBehaviour
    {

        [SerializeField]
        Participant[] participants = new Participant[0];

        [SerializeField]
        UI.UIActionPoints _actionPoints;

        [SerializeField]
        Map map;

        [SerializeField]
        Transform playersInEditor;

        public static Transform PlayersInEditor
        {
            get
            {
                return _instance.playersInEditor;
            }
        }

        static int _highestParticipantID = 0;

        static string highestParticipantIDFile = "highestParticipantID";

        public static Map Map
        {
            get
            {
                return _instance.map;
            }
        }

        int activeParticipant = 0;

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
                _highestParticipantID++;
                return _highestParticipantID;
            }
        }

        void OnEnable()
        {
            if (DataPersistence.HasSavedData(participantsSaveDataFile))
                participants = DataPersistence.LoadData<Participant[]>(participantsSaveDataFile);

            if (DataPersistence.HasSavedData(highestParticipantIDFile))
                _highestParticipantID = DataPersistence.LoadData<int>(highestParticipantIDFile);
               
        }

        void OnDisable()
        {
            DataPersistence.SaveData(participants, participantsSaveDataFile);
            DataPersistence.SaveData(_highestParticipantID, highestParticipantIDFile);
        }



        void Start()
        {
            if (participants.Length == 0)
                AddParticipant(PlayerType.Player);

            if (!HasNeutralParticipant)
                AddParticipant(PlayerType.Neutral);

            Tile.Focus(participants[activeParticipant].captiol);
            Tile.RemoveSelectLock();
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
            var participant = participants[activeParticipant];
            participants[activeParticipant].turn = participant.turn.Next();

            Debug.Log(string.Format("Player {0} ({1}) is {2}", 
                participant.name, 
                participant.type, 
                participant.turn));

            if (participant.turn == PlayerTurn.Resting)
            {
                activeParticipant++;
                activeParticipant %= participants.Length;
                updateStep(); 
            }
        }

        void enactStep()
        {
            var participant = participants[activeParticipant];
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