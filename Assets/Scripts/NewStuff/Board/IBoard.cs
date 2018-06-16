using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;

public interface IBoard
{
    List<List<ITile>> Grid { get; }
    GameObject UnityBoardContainer { get; }
}
