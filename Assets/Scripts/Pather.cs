﻿using UnityEngine;
using Boardgame.Input;
using System.Collections.Generic;

namespace Boardgame
{

    public enum PathAction { Initalized, Extended, Finalized, Cleared};

    public class Pather : MonoBehaviour
    {
        Queue<Tile> path = new Queue<Tile>();
        Tile lastItem;

        public delegate void PathHasChanged(Tile[] path, PathAction action);
        public static event PathHasChanged OnPathChange;

        void OnEnable()
        {
            Tile.OnInteraction += OnNewFocus;

        }

        void OnDisable()
        {
            Tile.OnInteraction -= OnNewFocus;
        }

        void OnNewFocus(Tile tile, InteractionType type)
        {
            if (type == InteractionType.Select)
                RegisterPathStart(tile);
            else if (type == InteractionType.Deselect)
                path.Clear();
            else if (type == InteractionType.Path)
            {
                if (!ExtendPath(tile))
                    return;
            }

            EmitEvent(type == InteractionType.FinalizePath);

        }

        bool ExtendPath(Tile nextTile)
        {
            if (Map.Connected(lastItem, nextTile))
            {
                path.Enqueue(nextTile);
                lastItem = nextTile;
                return true;
            }
            return false;
        }

        void RegisterPathStart(Tile tile)
        {
            if (!Military.HasAnyAvailableUnit(tile, Game.activeUserID))
                path.Clear();
            else if (path.Count == 0 || path.Peek() != tile)
            {
                path.Clear();
                path.Enqueue(tile);
                lastItem = tile;
            }

        }

        public void ClearPath()
        {
            path.Clear();
            EmitEvent(false);
        }

        int pathSteps
        {
            get
            {
                return path.Count - 1;
            }
        }

        public bool Pathing
        {
            get
            {
                return pathSteps >= 0;
            }
        }

        void EmitEvent(bool finalized)
        {
            PathAction action;

            if (pathSteps == 0)
                action = PathAction.Cleared;
            else if (pathSteps == 1)
                action = PathAction.Initalized;
            else if (finalized)
                action = PathAction.Finalized;
            else
                action = PathAction.Extended;

            if (OnPathChange != null)
                OnPathChange(path.ToArray(), action);
        }
    }
}