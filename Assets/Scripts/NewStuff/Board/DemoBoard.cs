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
        new IBoardSettings Settings { get { return Settings as DemoBoardSettings; } set { Settings = value as DemoBoardSettings; } }
        public DemoBoard(DemoBoardSettings Settings)
        {
            Grid = new List<List<ITile>>();
            this.Settings = Settings;
        }
        public void CrateBoard()
        {
            for (int x = 0; x < Settings.Rows; x++)
            {
                for (int y = 0; y < Settings.Cols; y++)
                {
                    Grid[x][y] = new Floor(x, y);
                    Floor.UnityObject.Trasnform.position = new Vector3(x, y, 0.0f);
                    Floor.UnityObject.Transform.parent = UnityBoardContainer;
                }
            }
        }
    }
}
