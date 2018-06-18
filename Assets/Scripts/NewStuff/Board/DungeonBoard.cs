using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiles;

namespace Assets.Scripts.NewStuff.Board
{
    public class DungeonBoard : IBoard
    {
        public List<List<ITile>> Grid { get; private set; }
        private DungeonBoardSettings Settings { get; set; }
        public DungeonBoard(DungeonBoardSettings Settings)
        {
            Grid = new List<List<ITile>>();
            this.Settings = Settings;
        }

        public void CreateBoard()
        {
            SetupInitialBoard();
            for (uint i = 0; i < Settings.numberOfSimulations; i++)
            {
                GameOfLifeSimulation();
            }
            if (!SpawnPointAndExitCanExist())
            {
                RespawnMap();
            }
            RemoveBlockedOpenTiles();
            FixEdges();
            RemoveFloatingWalls();

        }

        public void SpawnPlayer()
        {

        }

        /// <summary>
        /// Generate the initial map, which is a random grid of walls and floors
        /// </summary>
        private void SetupInitialBoard()
        {
            for (uint x = 0; x < Settings.Rows; x++)
            {
                List<ITile> row = new List<ITile>();
                for (uint y = 0; y < Settings.Cols; y++)
                {
                    float randNum = Random.Range(.0f, 1.0f);
                    ITile tile;
                    if (randNum < Settings.ChanceToStartAlive)
                    {
                        tile = new Wall(x, y, DefaultWallSprite);
                    }
                    else
                    {
                        tile = new Floor(x, y, DefaultFloorSprite);
                    }
                    row.Add(tile);
                }
                Grid.Add(row);
            }
        }

        /// <summary>
        /// Run through the grid a set number of time chaning walls to floors 
        /// or vice versa depending on if a neighbor cells are alive or dead
        /// </summary>
        private void GameOfLifeSimulation()
        {
            List<List<ITile>> oldgrid = new List<List<ITile>>(Grid);
            for (uint x = 0; x < Settings.Rows; x++)
            {
                for (uint y = 0; y < Settings.Cols; y++)
                {
                    int wallsAroundPoint = Tile.CountWalls(oldgrid[(int)x][(int)y]);
                    if (oldgrid[(int)x][(int)y] is Wall)
                    {
                        if (wallsAroundPoint < Settings.DeathLimit)
                        {
                            Grid[(int)x][(int)y] = new Floor(x, y, DefaultFloorSprite);
                        }
                    }
                    else
                    {
                        if (wallsAroundPoint > Settings.BirthLimit)
                        {
                            Grid[(int)x][(int)y] = new Wall(x, y, DefaultWallSprite);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ensure that there are atleast two non-wall tiles that can be used for a
        /// spawnpoint and an exit
        /// </summary>
        private bool SpawnPointAndExitCanExist()
        {
            int count = 0;
            for(uint x = 0; x < Settings.Rows; x++)
            {
                for(uint y = 0; y < Settings.Cols; y++)
                {
                    if (Grid[x][y] is Floor)
                    {
                        count++;
                    }
                    if (count >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the map and restarts the creation process then validates the new map
        /// can have a spawn point and exit
        /// </summary>
        private bool RespawnMap()
        {
            Grid.Clear();
            CreateBoard();
            if (!SpawnPointAndExitCanExist())
            {
                RespawnMap();
            }
        }

    }
}
