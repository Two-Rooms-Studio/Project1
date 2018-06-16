using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiles;
using UnityEngine;

namespace Assets.Scripts.NewStuff.Board
{
    public class Board : MonoBehaviour, IBoard
    {
        public List<List<ITile>> Grid { get; set; }
        public GameObject UnityBoardContainer { get; set; }

    }
}
