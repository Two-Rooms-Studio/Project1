using Assets.Scripts.NewStuff.Board.BoardSettings;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;

public interface IBoard
{
    List<List<ITile>> Grid { get; }
    IBoardSettings Settings { get; }
    GameObject UnityBoardContainer { get; }
}
