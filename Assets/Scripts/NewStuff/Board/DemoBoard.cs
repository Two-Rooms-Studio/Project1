﻿using Assets.Scripts.NewStuff.Board.BoardSettings;
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
        public DemoBoard(DemoBoardSettings Settings)
        {
            Grid = new List<List<ITile>>();
            for (int x = 0; x < Settings.Rows; x++)
            {
                for (int y = 0; y < Settings.Cols; y++)
                {
                    
                }
            }
        }
    }
}