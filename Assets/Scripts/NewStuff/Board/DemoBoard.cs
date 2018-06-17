using Assets.Scripts.NewStuff.Board.BoardSettings;
using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
namespace Assets.Scripts.NewStuff.Board
{
    public class DemoBoard : IBoard
    {
        public List<List<ITile>> Grid { get; private set; }
        private DemoBoardSettings Settings {get; set;}
        public DemoBoard(DemoBoardSettings Settings)
        {
            Grid = new List<List<ITile>>();
            this.Settings = Settings;
        }

        public void CreateBoard()
        {
            for (uint x = 0; x < Settings.Rows; x++)
            {
                List<ITile> row = new List<ITile>();
                for (uint y = 0; y < Settings.Cols; y++)
                {
                    Floor floor = new Floor(x, y, Settings.FloorSprite);
                    row.Add(floor);
                }
                Grid.Add(row);
            }
        }

        public void SpawnPlayer()
        {
            throw new NotImplementedException();
        }
    }
}
