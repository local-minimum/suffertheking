using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {

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
