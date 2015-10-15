using UnityEngine;
using System.Collections.Generic;

namespace Boardgame
{
    public class Map : MonoBehaviour
    {

        [SerializeField, HideInInspector]
        bool[] map = new bool[0];

        [SerializeField, HideInInspector]
        int mapSize = 0;

        [SerializeField]
        LayerMask layers;

        [SerializeField]
        Camera gameCamera;

        public Material[] playerTintings;

        Dictionary<string, Tile> _provinceCache = new Dictionary<string, Tile>();

        static bool selfConnect = false;

        static Map _instance;

        public static Map instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Map>();

                return _instance;
            }
        }

        public int MapSize
        {
            get
            {
                return instance.mapSize;
            }
        }

        static int MapIndexFromIndices(int a, int b)
        {
            return MapIndexFromIndices(a, b, instance.mapSize);
        }

        static int MapIndexFromIndices(int a, int b, int mapSize)
        {
            return a * mapSize + b;
        }

        public int ConnectionsCount
        {
            get
            {
                int c = 0;

                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = i + 1; j < mapSize; j++)
                    {
                        if (map[MapIndexFromIndices(i, j)])
                            c++;
                    }

                }
                return c;
            }
        }

        public static void Disconnect(Tile to, Tile from)
        {
            SetConnection(to, from, false);
        }


        public static void Connect(Tile to, Tile from)
        {

            SetConnection(to, from, true);
        }

        public static HashSet<Tile> Connections(Tile tile)
        {
            if (tile.tileIndex < 0 || tile.tileIndex >= instance.MapSize)
                instance.UpdateMapAndTiles();

            var connections = new HashSet<Tile>();
            var tiles = Tile.Tiles;
            for (int i = 0, l = tiles.Count; i < l; i++)
            {
                if (instance.map[MapIndexFromIndices(tile.tileIndex, tiles[i].tileIndex)])
                    connections.Add(tiles[i]);
            }

            return connections;
        }

        public static bool Connected(Tile from, Tile to)
        {
            if (from.tileIndex == -1 || to.tileIndex == -1)
                instance.UpdateMapAndTiles();
            return instance.map[MapIndexFromIndices(from.tileIndex, to.tileIndex)];

        }


        static void SetConnection(Tile from, Tile to, bool value)
        {
            if (from.tileIndex == -1 || from.tileIndex == -1)
                instance.UpdateMapAndTiles();

            instance.map[MapIndexFromIndices(from.tileIndex, to.tileIndex)] = value;
            instance.map[MapIndexFromIndices(to.tileIndex, from.tileIndex)] = value;
        }

        void UpdateMapAndTiles()
        {
            var mapSize = UpdateTileIndices();
            ReshapeMapIfNeeded(mapSize);
        }

        int UpdateTileIndices()
        {
            int nextIndex = 0;
            var usedIndices = new HashSet<int>();
            var unIndexedTiles = new List<Tile>();
            var tiles = Tile.Tiles;

            for (int i = 0, l = tiles.Count; i < l; i++)
            {
                if (tiles[i].tileIndex >= 0)
                    usedIndices.Add(tiles[i].tileIndex);
                else
                    unIndexedTiles.Add(tiles[i]);
            }

            for (int i = 0, l = unIndexedTiles.Count; i < l; i++)
            {
                while (usedIndices.Contains(nextIndex))
                    nextIndex++;

                if (nextIndex < MapSize)
                    ClearOldConnectionsInMap(nextIndex);

                unIndexedTiles[i].tileIndex = nextIndex;
                usedIndices.Add(nextIndex);
            }

            while (usedIndices.Contains(nextIndex))
                nextIndex++;

            return nextIndex;
        }

        public void ClearAllConnections()
        {
            for (int i = 0; i < mapSize; i++)
                ClearOldConnectionsInMap(i);
        }

        void ClearOldConnectionsInMap(int index)
        {
            for (int i = 0; i < mapSize; i++)
            {
                map[MapIndexFromIndices(i, index)] = false;
                map[MapIndexFromIndices(index, i)] = false;
            }
            map[MapIndexFromIndices(index, index)] = selfConnect;
        }

        void ReshapeMapIfNeeded(int mapSize)
        {
            if (map.GetLength(0) == mapSize)
                return;

            var newMap = new bool[mapSize * mapSize];
            var copyTo = Mathf.Min(mapSize, map.GetLength(0));

            for (int i = 0; i < copyTo; i++)
            {
                for (int j = 0; j < copyTo; j++)
                {
                    newMap[MapIndexFromIndices(i, j, mapSize)] = map[MapIndexFromIndices(i, j)];
                }
            }

            for (int i = 0; i < mapSize; i++)
                newMap[MapIndexFromIndices(i, i, mapSize)] = selfConnect;

            map = newMap;
            this.mapSize = mapSize;
        }

        void Awake()
        {
            var provinces = GetComponentsInChildren<Tile>();
            for (int i = 0; i < provinces.Length; i++)
                _provinceCache[provinces[i].name] = provinces[i];
        }

        public Tile Province(string name)
        {
            if (_provinceCache.ContainsKey(name))
                return _provinceCache[name];
            return null;
        }

        public Tile Province(Vector3 screenCoordinate)
        {
            var ray = gameCamera.ScreenPointToRay(screenCoordinate);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, layers))
            {
                return hitInfo.collider.GetComponent<Tile>();
            }
            return null;
        }

        public Material TileMaterial(int owner)
        {
            if (owner < playerTintings.Length)
                return playerTintings[owner];
            return null;

        }

        public List<Tile> Domain(int owner)
        {
            var domain = new List<Tile>();
            var land = Tile.Tiles;

            for (int i = 0, l=land.Count; i<l; i++)
            {
                if (land[i].demographics.rulerID == owner)
                    domain.Add(land[i]);
            }
            return domain;
        }

    }

}