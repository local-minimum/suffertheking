using UnityEngine;
using System.Collections;

public enum Turn { Boss, Orders, Actions };
public enum PlayerType { Player, RemotePlayer, AI }

public struct Participant
{
    public Turn turn;
    public PlayerType type;
}

public class Game : MonoBehaviour {

    [SerializeField]
    Participant[] participants;

    [SerializeField]
    UIActionPoints _actionPoints;

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

    public static UIActionPoints ActionPoints {
        get
        {
            return _instance._actionPoints;
        }
    }
}
