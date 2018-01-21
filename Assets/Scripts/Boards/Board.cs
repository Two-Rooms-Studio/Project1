using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : ScriptableObject{
	protected int rows = 5;
	protected int cols = 5;
	protected List<List<GameTile>> grid = new List<List<GameTile>>(); //data structure for our entire map
	private List<GameTile> Edges = new List<GameTile>(); //map edges used for player vision logic
	protected float xPadding = 0.16f; //spacing we need to place between each tile in the grid
	protected float yPadding = 0.24f; //this SHOULD be consistent throughout all boards....
	protected GameObject tileObject;
	protected Sprite wallSprite;
	protected Sprite floorSprite;
	protected string gridContainerName; //all tiles created by the board will be placed inside a gameobject container that has this name
	protected GameTile playerSpawnPoint;

	//public
	public void init(int p_rows, int p_cols, GameObject p_tileObject)
	{
		//set up consistent gameboard parameters
		rows = p_rows;
		cols = p_cols;
		tileObject = p_tileObject;
	}

	public GameTile GetGridTile(int x, int y)
	{
		return grid[x][y];
	}

	public List<List<GameTile>> GetGrid(){
		return grid;
	}

	public int GetCols()
	{
		return cols;
	}

	public int GetRows()
	{
		return rows;
	}

	public List<GameTile> GetEdges()
	{
		return Edges;
	}

	public GameTile GetPlayerSpawnPoint()
	{
		return playerSpawnPoint;
	}

	public string GetGridContainerName()
	{
		return gridContainerName;
	}

	public List<GameTile> GetAllTilesInRange(ref GameTile tile, int range)
	{
		//Essentially a modified FloodFill that is only allowed to run a set number of times. This will return all tiles within a certain movement range away for example when handed a 
		//range of 1 and the player tile, it will return the tiles in the players cardnial directions (tiles that require one move to reach)
		//This will also mark all tiles within that given range.
		UnMarkAllTiles();
		List<GameTile> allMarkedCells = new List<GameTile>();
		List<GameTile> validNeighbours = new List<GameTile>();
		allMarkedCells.Add(grid[tile.GetX()][tile.GetY()]);
		AddAllNeighbours(ref validNeighbours, tile);
		List<GameTile> nextValidNeighbours = new List<GameTile>();
		range--;
		if (range == 0)
			return validNeighbours;
		else if (range < 0)
			return null; //error
		do {
			nextValidNeighbours.Clear();
			for (int x = 0; x < validNeighbours.Count; x++) {
				validNeighbours[x].SetIsMarked(true);
				allMarkedCells.Add(validNeighbours[x]);
				AddAllNeighbours(ref nextValidNeighbours, validNeighbours[x]);
			}
			validNeighbours = new List<GameTile>(nextValidNeighbours);
			range--;
		} while (nextValidNeighbours.Count != 0 && range != 0);
		return allMarkedCells;
	}

	public void  SetAllTilesToNotVisible()
	{
		//set all tiles on the grid to not visible
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid[x][y].SetIsVisible(false);
			}
		}
	}

	public void UnMarkAllTiles()
	{
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid[x][y].SetIsMarked(false);
			}
		}
	}

	//private
	private void AddAllNeighbours(ref List<GameTile> list, GameTile tile){
		//GetAllTilesInRange helper function, returns a list containing the neighbours surrounding a tile
		grid[tile.GetX()][tile.GetY()].SetIsMarked(true);
		if((tile.GetY() + 1) < rows && !grid[tile.GetX()][tile.GetY()+1].IsMarked()){
			list.Add(grid[tile.GetX()][tile.GetY()+1]);
			grid[tile.GetX()][tile.GetY()+1].SetIsMarked(true);
		}
		if ((tile.GetY() - 1) > 0 && !grid[tile.GetX()][tile.GetY()-1].IsMarked()) {
			list.Add(grid[tile.GetX()][tile.GetY()-1]);
			grid[tile.GetX()][tile.GetY()-1].SetIsMarked(true);
		}
		if ((tile.GetX() + 1) < cols && !grid[tile.GetX()+1][tile.GetY()].IsMarked()) {
			list.Add(grid[tile.GetX()+1][tile.GetY()]);
			grid[tile.GetX()+1][tile.GetY()].SetIsMarked(true);
		}
		if ((tile.GetX() - 1) > 0 && !grid[tile.GetX()-1][tile.GetY()].IsMarked()) {
			list.Add(grid[tile.GetX()-1][tile.GetY()]);
			grid[tile.GetX()-1][tile.GetY()].SetIsMarked(true);
		}
	}

	private void AddValidNeighbours(ref List<GameTile> list, GameTile tile){
		//FloodFill helper function, returns a list containing the neighbours
		//of a tile that should be included in the flood fill
		grid[tile.GetX()][tile.GetY()].SetIsMarked(true);
		if((tile.GetY() + 1) < rows && !grid[tile.GetX()][tile.GetY()+1].IsWall() && !grid[tile.GetX()][tile.GetY()+1].IsMarked()){
			list.Add(grid[tile.GetX()][tile.GetY()+1]);
			grid[tile.GetX()][tile.GetY()+1].SetIsMarked(true);
		}
		if ((tile.GetY() - 1) > 0 && !grid[tile.GetX()][tile.GetY()-1].IsWall() && !grid[tile.GetX()][tile.GetY()-1].IsMarked()) {
			list.Add(grid[tile.GetX()][tile.GetY()-1]);
			grid[tile.GetX()][tile.GetY()-1].SetIsMarked(true);
		}
		if ((tile.GetX() + 1) < cols && !grid[tile.GetX()+1][tile.GetY()].IsWall() && !grid[tile.GetX()+1][tile.GetY()].IsMarked()) {
			list.Add(grid[tile.GetX()+1][tile.GetY()]);
			grid[tile.GetX()+1][tile.GetY()].SetIsMarked(true);
		}
		if ((tile.GetX() - 1) > 0 && !grid[tile.GetX()-1][tile.GetY()].IsWall() && !grid[tile.GetX()-1][tile.GetY()].IsMarked()) {
			list.Add(grid[tile.GetX()-1][tile.GetY()]);
			grid[tile.GetX()-1][tile.GetY()].SetIsMarked(true);
		}
	}

	private void SpawnPlayer()
	{
		Vector2 spawnPoint;
		do {
			spawnPoint = new Vector2((int)Random.Range(0, cols-1), (int)Random.Range(0, rows-1));
		} while (!grid[(int)spawnPoint.x][(int)spawnPoint.y].OpenForPlacement());
		playerSpawnPoint = (grid[(int)spawnPoint.x][(int)spawnPoint.y]);
		grid[(int)spawnPoint.x][(int)spawnPoint.y].SetIsOccupied(true);
	}

	//protected
	protected float CalculatePlayingArea()
	{
		//return a percentage representing the number of open tiles remaining
		//based off of the number of tiles originally created
		int numberOfOpenCells = 0;
		int totalNumberOfCells = rows * cols;
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid[x][y].OpenForPlacement()) {
					numberOfOpenCells++;
				}
			}
		}
		Debug.Log("Total open cells: " + numberOfOpenCells + " total number of cells: " + totalNumberOfCells);
		return (((float)numberOfOpenCells) / ((float)totalNumberOfCells));
	}

	protected void CalculateTileNeighbours()
	{
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

	protected bool checkForUnreachableOpenTile(int x, int y)
	{
	//1 for a open tile that is surrounded by walls on the cardnial directions
	//in otherwords a cave or room made up by one tile
		int count = 0;
		if(grid[x][y].OpenForPlacement()){ 
			if ((y+1) < rows && grid[x][y+1].IsWall()) {
				count++;
			}
			if ((x+1) < cols && grid[x+1][y].IsWall()) {
				count++;
			}
			if ((x-1) >= 0 && grid[x-1][y].IsWall()) {
				count++;
			}
			if ((y-1) >= 0 && grid[x][y-1].IsWall()) {
				count++;
			}
			if (count == 4) {
				return true;
			}
		}
		return false;
	}

	protected int countDestroyed(int x, int y)
	{
		//count the number of destroyed(open air) tiles around a given point in all 8 directions
		int count = 0;
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				int neighbour_x = x + i;
				int neighbour_y = y + j;
				if (!(i == 0 && j == 0)) {
					if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= cols || neighbour_y >= rows) {
						count += 1;
					} else if (grid[neighbour_x][neighbour_y].IsDestroyed()) {
						count += 1;
					}
				}
			}
		}
		return count;
	}

	protected int countFloating(int x, int y)
	{
		//count the number of destroyed(open air) tiles or wall tiles around a given point in all 8 directions
		int count = 0;
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				int neighbour_x = x + i;
				int neighbour_y = y + j;
				if (!(i == 0 && j == 0)) {
					if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= cols || neighbour_y >= rows) {
						count += 1;
					} else if (grid[neighbour_x][neighbour_y].IsDestroyed() || grid[neighbour_x][neighbour_y].IsWall()) {
						count += 1;
					}
				}
			}
		}
		return count;
	}

	protected int countWalls(int x, int y, List<List<GameTile>> grid)
	{
		//count the walls or blanks surrounding a given tile in all 8 directions
		int count = 0;
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				int neighbour_x = x + i;
				int neighbour_y = y + j;
				if (!(i == 0 && j == 0)) {
					if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= cols || neighbour_y >= rows) {
						count += 1;
					} else if (grid[neighbour_x][neighbour_y].IsWall()) {
						count += 1;
					}
				}
			}
		}
		return count;
	}

	protected void DeleteAllMarkedTiles()
	{
		//delete all marked tiles, essentially used to remove all caves except for the largest one
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid[x][y].IsMarked()) {
					grid[x][y].SetIsDestroyed(true);
					grid[x][y].SetIsWall(true);
					grid[x][y].SetIsMarked(false);
					Destroy(grid[x][y].GetObject());
				}
			}
		}
	}

	protected bool EnsureSpawnPointAndExitCanExist()
	{
		//ensure that there is atleast two non-wall tile to be used for a spawnpoint and exit
		int count = 0;
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid[x][y].OpenForPlacement()) {
					count++; //we have atleast one spawn point
				}
				if (count >= 2) {
					return true;
				}
			}
		}
		return false;
	}

	protected void FixEdges(Sprite p_wallSprite)
	{
		//Fix edges so that non-wall tiles never touch a destroyed(empty air) tile by changing open tiles to walls
		for (int x = 0; x < cols; x++) { //turn unblocked edges into walls
			for (int y = 0; y < rows; y++) {
				int DestroyedCount = countDestroyed (x, y);
				if (DestroyedCount >= 1 && !(grid[x][y].IsWall())) {
					grid[x][y].SetIsWall(true);
					grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = p_wallSprite;
				}
			}
		}
	}

	protected int FloodFill(ref GameTile tile, ref List<List<GameTile>> FloodFilledAreas)
	{
		//starting from the provided tile, mark all connected tiles (meaning stop at walls)
		//this essentially produces a cut out of a room that is reachable, i.e. no walls blocking off certian parts
		//it can also return a int count of the number of tiles in the area cut out
		List<GameTile> allMarkedCells = new List<GameTile>();
		List<GameTile> validNeighbours = new List<GameTile>();
		int count = 1;
		grid[tile.GetX()][tile.GetY()].SetIsMarked(true);
		allMarkedCells.Add(grid[tile.GetX()][tile.GetY()]);
		AddValidNeighbours(ref validNeighbours,tile);
		List<GameTile> nextValidNeighbours = new List<GameTile>();
		do {
			nextValidNeighbours.Clear();
			count += validNeighbours.Count;
			for (int x = 0; x < validNeighbours.Count; x++) {
				validNeighbours[x].SetIsMarked(true);
				allMarkedCells.Add(validNeighbours[x]);
				AddValidNeighbours(ref nextValidNeighbours, validNeighbours[x]);
			}
			validNeighbours = new List<GameTile>(nextValidNeighbours);
		} while (nextValidNeighbours.Count != 0);
		FloodFilledAreas.Add(allMarkedCells);
		return count;
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

	protected void RemoveBlockedOpenTiles()
	{
		//remove tiles that are completely surrounded by other walls
		for (int x = 0; x < cols; x++) { 
			for (int y = 0; y < rows; y++) {
				if (checkForUnreachableOpenTile(x, y)) {
					grid[x][y].SetIsDestroyed(true);
					grid[x][y].SetIsWall(true);
					Destroy(grid[x][y].GetObject());
				}
			}
		}
	}

	protected void RemoveFloatingWalls()
	{
		//Remove walls that are floating (surrounded by only walls or destroyed tiles)
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				int FloatingCount = countFloating(x, y);
				if (FloatingCount >= 8 && grid[x][y].IsWall()) {
					grid[x][y].SetIsDestroyed(true);
					grid[x][y].SetIsWall(true);
					Destroy(grid[x][y].GetObject());
				}
			}
		}
	}

	protected void SetAllOriginalSpritesAndColors()
	{
		//used once the board is completely to set all the original sprites so we can change them out during movement and other transitions
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid[x][y].SetOriginalSprite(grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite);
				grid[x][y].SetOriginalColor(grid[x][y].GetObject().GetComponent<SpriteRenderer>().color);
			}
		}
	}

	protected void SetUpEdges()
	{
		//Mark edges (walls that only touch blanks and other walls never floors) for logic use
		int count = 0;
			count = 0;
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if(grid[x][y].IsWall()){ 
					if (((y+1) < rows) && (grid[x][y+1].IsWall() || grid[x][y+1].IsDestroyed())) {
						count++;
					} else if ((y+1) >= rows) {
						count++;
					}
					if (((x+1) < cols) && (grid[x+1][y].IsWall() || grid[x+1][y].IsDestroyed())) {
						count++;
					} else if ((x+1) >= cols) {
						count++;
					}
					if (((x-1) >= 0) && (grid[x-1][y].IsWall() || grid[x-1][y].IsDestroyed())) {
						count++;
					} else if ((x-1) < 0) {
						count ++;
					}
					if (((y-1) >= 0) && (grid[x][y-1].IsWall() || grid[x][y-1].IsDestroyed())) {
						count++;
					} else if ((y-1) < 0) {
						count++;
					}
					if (count == 4) {
						grid[x][y].SetIsEdge(true);
						Edges.Add(grid[x][y]);
					}
					count = 0;
				}
			}
		}
	}

	protected void SmoothMapEdges()
	{
		//Smooth out map edges by removing walls that are surrounded by walls or blank spaces in each of the cardinal directions
		int count = 0;
		bool removed = false;
		do {
			removed = false;
			count = 0;
			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					if(!grid[x][y].IsDestroyed() && grid[x][y].IsWall()){ 
						if (((y+1) < rows) && (grid[x][y+1].IsWall() || grid[x][y+1].IsDestroyed())) {
							count++;
						} else if ((y+1) >= rows) {
							count++;
						}
						if (((x+1) < cols) && (grid[x+1][y].IsWall() || grid[x+1][y].IsDestroyed())) {
							count++;
						} else if ((x+1) >= cols) {
							count++;
						}
						if (((x-1) >= 0) && (grid[x-1][y].IsWall() || grid[x-1][y].IsDestroyed())) {
							count++;
						} else if ((x-1) < 0) {
							count ++;
						}
						if (((y-1) >= 0) && (grid[x][y-1].IsWall() || grid[x][y-1].IsDestroyed())) {
							count++;
						} else if ((y-1) < 0) {
							count++;
						}
						if (count == 4) {
							removed = true;
							grid[x][y].SetIsDestroyed(true);
							grid[x][y].SetIsWall(true);
							Destroy (grid[x][y].GetObject());
						}
						count = 0;
					}
				}
			}
		} while (removed == true);
	}

	protected void SpawnPlayerAndExitPoint()
	{
		SpawnPlayer();
		Vector2 spawnPoint;
		do {
			spawnPoint = new Vector2((int)Random.Range(0, cols-1), (int)Random.Range(0, rows-1));
		} while (!grid[(int)spawnPoint.x][(int)spawnPoint.y].OpenForPlacement());
		GameObject exitPrefab = Resources.Load("Prefabs/ExitPrefab") as GameObject;
		grid[(int)spawnPoint.x][(int)spawnPoint.y].GetObject().GetComponent<SpriteRenderer>().sprite = exitPrefab.GetComponent<SpriteRenderer>().sprite;
		grid[(int)spawnPoint.x][(int)spawnPoint.y].GetObject().AddComponent<ExitTile>();
		grid[(int)spawnPoint.x][(int)spawnPoint.y].SetIsOccupied(true);
		grid[(int)spawnPoint.x][(int)spawnPoint.y].SetOriginalColor(exitPrefab.GetComponent<SpriteRenderer>().color);
		grid[(int)spawnPoint.x][(int)spawnPoint.y].SetOriginalSprite(exitPrefab.GetComponent<SpriteRenderer>().sprite);
	}
}
