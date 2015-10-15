using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Boardgame.Input;

namespace Boardgame.Data
{
    [System.Serializable]
    public class Demographics
    {
        public int population;

        public int taxation;

        [Tooltip("1 birth per X citizens guaranteed")]
        public int birthRate = 300;

        [HideInInspector]
        public int nativity = 0;

        [HideInInspector]
        public int deathToll = 0;

        public int carryingCapacity = 1000;

        public enum Affiliation { Neutral, Contested, Claimed }
        public Affiliation affiliationStatus;

        public int rulerID;
        public int rootsID;

        public enum StateOfWar { AtWar, WarTorn, AtPeace };
        public StateOfWar warState;

        public enum StateOfCivils { Revolution, Discontent, AtPeace };
        public StateOfCivils civilianState;

        public enum StateOfRelations { Isolationist, Neutral, Cooperative };
        public StateOfRelations relationState;

        public bool capitol;

    }
}

namespace Boardgame
{

    public class Tile : MonoBehaviour
    {

        public delegate void Interaction(Tile tile, InteractionType type);

        public static event Interaction OnInteraction;

        static Color32 hoverColor = Color.white;

        static float hoverColorCoeff = 0.2f;

        Color32 originalColor;

        [HideInInspector]
        public int tileIndex = -1;

        public Data.Demographics demographics;

#if UNITY_EDITOR
        public static List<Tile> Tiles
        {
            get
            {
                return GameObject.FindObjectsOfType<Tile>().ToList();
            }
        }
#else
    public static List<Tile> Tiles = new List<Tile>();
#endif

       
        static Tile selectLock;
        public static Tile SelectLock
        {
            get
            {
                return selectLock;
            }
        }

        static Tile hoverTile;
        public static Tile HoverTile
        {
            get
            {
                return hoverTile;
            }
        }

        InteractionType interactionStatus = InteractionType.None;

#if UNITY_EDITOR

        static Vector3 gizmoOffset = Vector3.up;
#endif

        void Awake()
        {
            Tiles.Add(this);
        }

        void Start()
        {
            var rend = GetComponent<Renderer>();
            rend.material = Game.Map.TileMaterial(demographics.rulerID);
            originalColor = rend.material.color;

        }

        void OnEnable()
        {
            OnInteraction += HandleTileFocus;
        }

        void OnDisable()
        {
            OnInteraction -= HandleTileFocus;
        }

        void HandleTileFocus(Tile tile, InteractionType type)
        {
            //Debug.Log((tile == null ? "None" : tile.name) + ": " + type);

            if (tile == this)
            {
                interactionStatus = type;
                if (type == InteractionType.Select)
                    selectLock = this;
                else if (type == InteractionType.Deselect)
                    selectLock = null;
            }
            else
                interactionStatus = InteractionType.None;

            if (interactionStatus == InteractionType.None)
                GetComponent<Renderer>().material.color = originalColor;
            else
                GetComponent<Renderer>().material.color = Color32.Lerp(originalColor, hoverColor, hoverColorCoeff);
        }
        
        void OnMouseEnter()
        {
            hoverTile = this;
        }

        void OnMouseExit()
        {
            hoverTile = null;
        }

        static public void RemoveSelectLock()
        {
            if (OnInteraction != null)
                OnInteraction(null, InteractionType.Deselect);
        }

        static public void Focus(Tile region)
        {
            if (OnInteraction != null)
                OnInteraction(region, region == null ? InteractionType.None : InteractionType.Inspect);
        }

        public void InteractWith(InteractionType type)
        {

        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            foreach (var tile in Map.Connections(this))
            {
                Gizmos.DrawLine(transform.position + gizmoOffset, tile.transform.position + gizmoOffset);
            }

        }
    }
}