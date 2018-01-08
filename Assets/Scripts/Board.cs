using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : ScriptableObject{
	protected int rows = 5;
	protected int cols = 5;
	protected List<List<GameTile>> grid = new List<List<GameTile>>(); //data structure for our entire map
	protected Sprite floorSprite;
	protected Sprite wallSprite;
	protected float xPadding = 0.0f; //spacing we need to place between each tile in the grid
	protected float yPadding = 0.0f; //this SHOULD be consistent throughout all boards....
	protected GameObject floorObject;
	protected GameObject wallObject;

	//public
	public void init(int p_rows, int p_cols, GameObject p_floorObject, GameObject p_wallObject)
	{
		//set up consistent gameboard parameters
		rows = p_rows;
		cols = p_cols;
		floorObject = p_floorObject;
		wallObject = p_wallObject;
		floorSprite = floorObject.GetComponent<SpriteRenderer> ().sprite;
		wallSprite = wallObject.GetComponent<SpriteRenderer> ().sprite;
	}

	public GameTile GetGridTile(int x, int y)
	{
		return grid[x][y];
	}

	public List<List<GameTile>> GetGrid(){
		return grid;
	}
		
	//protected
	protected void CalculateTileNeighbours(){
		//set up the neighbour information for each tile on the map
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (y + 1 >= rows) {
					grid [x] [y].SetTileNorth (null);
				} else {
					grid[x][y].SetTileNorth(grid[x][y + 1]);
				}
				if (y - 1 < 0) {
					grid [x] [y].SetTileSouth (null);
				} else {
					grid[x][y].SetTileSouth(grid[x][y - 1]);
				}
				if (x + 1 >= cols) {
					grid[x][y].SetTileEast(null);
				} else {
					grid[x][y].SetTileEast(grid[x + 1][y]);
				}
				if (x - 1 < 0) {
					grid[x][y].SetTileWest(null);
				} else {
					grid[x][y].SetTileWest(grid[x - 1][y]);
				}
			}
		}
	}

	protected bool HasUnmarkedTiles()
	{
		//return true if the board has any tiles that aren't marked that could be marked
		//for floodfill correction of blocked off rooms
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (!grid[x][y].IsMarked() && !grid[x][y].IsWall()) {
					return true;
				}
			}
		}
		return false;
	}

	protected void SetAllOriginalSprites()
	{
		//used once the board is completely to set all the original sprites so we can change them out during movement and other transitions
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid[x][y].SetOriginalSprite(grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite);
			}
		}
	}

	protected bool EnsureSpawnPointExsits()
	{
		//ensure that there is atleast one non-wall tile to be used for a spawnpoint
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (!grid[x][y].IsWall() && !grid[x][y].IsDestroyed()) {
					return true; //we have atleast one spawn point
				}
			}
		}
		return false;
	}
}
