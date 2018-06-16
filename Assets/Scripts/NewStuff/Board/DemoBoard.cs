using Assets.Scripts.NewStuff.Board.BoardSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiles;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Assets.Scripts.NewStuff.Board
{
    public class DemoBoard : Board, IBoard
    {
        private DemoBoardSettings Settings {get; set;}
        public DemoBoard(DemoBoardSettings Settings)
        {
            Grid = new List<List<ITile>>();
            this.Settings = Settings;
        }

        public override void CreateBoard()
        {
            for (uint x = 0; x < Settings.Rows; x++)
            {
                for (uint y = 0; y < Settings.Cols; y++)
                {
                    Floor floor = new Floor(x, y, Settings.FloorSprite);
                    Grid[(int)x][(int)y] = floor;
                    floor.UnityObject.transform.position = new Vector3(x, y);
                    floor.UnityObject.transform.parent = UnityBoardContainer.transform;
                }
            }
        }

        public override void SpawnPlayer()
        {
            throw new NotImplementedException();
        }
    }
}
