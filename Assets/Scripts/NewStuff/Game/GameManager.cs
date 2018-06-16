using Assets.Scripts.NewStuff.Board;
using Assets.Scripts.NewStuff.Board.BoardSettings;
using System.Collections;
using System.Collections.Generic;
using TilePrefabs;
using Tiles;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public IBoard CurrentBoard;
    public GameObject CurrentBoardContainer;
    public List<IBoard> GameBoards = new List<IBoard>();
    [Header("Board Settings")]
    public DemoBoardSettings DemoBoardSettings;
    public List<TilePrefab> tilePrefabs = new List<TilePrefab>();

    void Awake() {
        GameBoards.Add(new DemoBoard(DemoBoardSettings));
    }
    // Use this for initialization
    void Start () {
        CurrentBoard = GameBoards[0];
        CurrentBoard.CreateBoard();
        float x = 0.0f, y = 0.0f, xPadding = 0.16f, yPadding = 0.24f;
        foreach (var tileRows in CurrentBoard.Grid)
        {
            foreach (var tile in tileRows)
            {
                tile.UnityObject = Instantiate(tilePrefabs[0].Prefab, new Vector3(x, y), Quaternion.identity, CurrentBoardContainer.transform);
                x += xPadding;
            }
            x = 0.0f;
            y += yPadding;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
