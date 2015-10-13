using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Boardgame.Data
{
    [System.Serializable]
    public struct Demographics
    {
        public int population;

        public int taxation;

        [Tooltip("Added population per 1000 inhabitants and turn")]
        public int birthRate;

        public enum Affiliation { Player, Neutral, Enemy }
        public Affiliation ruler;
        public Affiliation loyalty;

        public enum StateOfWar { AtWar, WarTorn, AtPeace };
        public StateOfWar warState;

        public enum StateOfCivils { Revolution, Discontent, AtPeace };
        public StateOfCivils civilianState;

        public enum StateOfRelations { Isolationist, Neutral, Cooperative };
        public StateOfRelations relationState;

    }
}

namespace Boardgame
{
    public class Tile : MonoBehaviour
    {

        public delegate void TileFocus(Tile tile);

        public static event TileFocus OnTileFocus;

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

        bool readyToBecomeSelected = true;
        bool viewing = false;
        bool selected = false;
        bool mouseIsOver = false;

#if UNITY_EDITOR

        static Vector3 gizmoOffset = Vector3.up;
#endif

        void Awake()
        {
            originalColor = GetComponent<Renderer>().material.color;
            Tiles.Add(this);
        }

        void OnEnable()
        {
            OnTileFocus += HandleTileFocus;
        }

        void OnDisable()
        {
            OnTileFocus -= HandleTileFocus;
        }

        void HandleTileFocus(Tile tile)
        {
            readyToBecomeSelected = tile == null;
            viewing = tile == this || (viewing && readyToBecomeSelected);

            if (tile == null)
                selected = false;

            if (!viewing)
            {
                GetComponent<Renderer>().material.color = originalColor;
                selected = false;
            }
            else
            {
                GetComponent<Renderer>().material.color = Color32.Lerp(originalColor, hoverColor, hoverColorCoeff);
            }
        }

        void Update()
        {
            if (mouseIsOver && viewing && Input.GetMouseButtonDown(0))
            {
                if (OnTileFocus != null)
                    OnTileFocus(selected ? null : this);
                selected = !selected;
            }
        }

        void OnMouseEnter()
        {
            mouseIsOver = true;
            if (readyToBecomeSelected && OnTileFocus != null)
                OnTileFocus(this);
        }

        void OnMouseExit()
        {
            mouseIsOver = false;
            if (viewing && !selected && OnTileFocus != null)
                OnTileFocus(null);
        }

        static public void RemoveSelectLock()
        {
            if (OnTileFocus != null)
                OnTileFocus(null);
        }

        static public void Focus(Tile region)
        {
            if (OnTileFocus != null)
                OnTileFocus(region);
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