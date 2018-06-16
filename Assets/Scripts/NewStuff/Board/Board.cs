using Assets.Scripts.NewStuff.Board.BoardSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiles;
using UnityEngine;

namespace Assets.Scripts.NewStuff.Board
{
    public abstract class Board : MonoBehaviour, IBoard
    {
        public List<List<ITile>> Grid { get; set; }
        public GameObject UnityBoardContainer { get; set; }
        public abstract void CreateBoard();
        public abstract void SpawnPlayer();
    }
}
