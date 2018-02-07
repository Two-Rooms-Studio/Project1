using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : ScriptableObject{
	protected int rows = 5;
	protected int cols = 5;
	protected List<List<GameTile>> grid = new List<List<GameTile>>(); //data structure for our entire map
	protected List<GameTile> Edges = new List<GameTile>(); //map edges used for player vision logic
	protected float xPadding = 0.16f; //spacing we need to place between each tile in the grid
	protected float yPadding = 0.24f; //this SHOULD be consistent throughout all boards....
	protected GameObject tileObject;
	protected string gridContainerName; //all tiles created by the board will be placed inside a gameobject container that has this name
	protected GameTile playerSpawnPoint;

	//public
	public void init(int p_rows, int p_cols, GameObject p_tileObject, string p_gridContainerName)
	{
		//set up consistent gameboard parameters
		rows = p_rows;
		cols = p_cols;
		tileObject = p_tileObject;
		gridContainerName = p_gridContainerName;
	}

	public GameObject GetTileObject()
	{
		return tileObject;
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

	public List<GameTile> GetAllTilesInRange(GameTile tile, int range)
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

	public GameTile GetTile(GameTile tile)
	{
		return grid[tile.GetX()][tile.GetY()];
	}

	public GameTile GetTileWest(GameTile tile)
	{
		GameTile tileLeft = new GameTile(tile.GetX() - 1, tile.GetY(), tile.GetOriginalSprite());
		if (InBounds(tileLeft))
			return GetTile(tileLeft);
		return null;
	}

	public GameTile GetTileEast(GameTile tile)
	{
		GameTile tileRight = new GameTile(tile.GetX() + 1, tile.GetY(), tile.GetOriginalSprite());
		if (InBounds(tileRight))
			return GetTile(tileRight);
		return null;
	}

	public GameTile GetTileNorth(GameTile tile)
	{
		GameTile tileNorth = new GameTile(tile.GetX(), tile.GetY() + 1, tile.GetOriginalSprite());
		if (InBounds(tileNorth))
			return GetTile(tileNorth);
		return null;
	}

	public GameTile GetTileSouth(GameTile tile)
	{
		GameTile tileSouth = new GameTile(tile.GetX(), tile.GetY() - 1, tile.GetOriginalSprite());
		if (InBounds(tileSouth))
			return GetTile(tileSouth);
		return null;
	}

	public GameTile GetLeftTopCorner(GameTile tile)
	{
		GameTile tileLeftTopCorner = new GameTile(tile.GetX() - 1, tile.GetY() + 1, tile.GetOriginalSprite());
		if (InBounds(tileLeftTopCorner))
			return GetTile(tileLeftTopCorner);
		return null;
	}

	public GameTile GetRightTopCorner(GameTile tile)
	{
		GameTile tileRightTopCorner = new GameTile(tile.GetX() + 1, tile.GetY() + 1, tile.GetOriginalSprite());
		if (InBounds(tileRightTopCorner))
			return GetTile(tileRightTopCorner);
		return null;
	}

	public GameTile GetLeftBotCorner(GameTile tile)
	{
		GameTile tileLeftBotCorner = new GameTile(tile.GetX() - 1, tile.GetY() - 1, tile.GetOriginalSprite());
		if (InBounds(tileLeftBotCorner))
			return GetTile(tileLeftBotCorner);
		return null;
	}

	public GameTile GetRightBotCorner(GameTile tile)
	{
		GameTile tileRightBotCorner = new GameTile(tile.GetX() + 1, tile.GetY() - 1, tile.GetOriginalSprite());
		if (InBounds(tileRightBotCorner))
			return GetTile(tileRightBotCorner);
		return null;
	}

	public List<GameTile> GetTileNeighbours(GameTile tile)
	{
		List<GameTile> validTileNeighbours = new List<GameTile>();
		List<GameTile> allTileNeighbours = new List<GameTile>();
		allTileNeighbours.Add(GetTileWest(tile));
		allTileNeighbours.Add(GetTileEast(tile));
		allTileNeighbours.Add(GetTileNorth(tile));
		allTileNeighbours.Add(GetTileSouth(tile));
		allTileNeighbours.Add(GetLeftTopCorner(tile));
		allTileNeighbours.Add(GetRightTopCorner(tile));
		allTileNeighbours.Add(GetLeftBotCorner(tile));
		allTileNeighbours.Add(GetRightBotCorner(tile));
		foreach (GameTile q in allTileNeighbours) {
			if (q != null && q.GetObject() != null) {
				validTileNeighbours.Add(q);
			}
		}
		return validTileNeighbours;
	}

	public List<GameTile> GetTileCardinalNeighbours(GameTile tile)
	{
		List<GameTile> validTileNeighbours = new List<GameTile>();
		List<GameTile> allTileNeighbours = new List<GameTile>();
		allTileNeighbours.Add(GetTileWest(tile));
		allTileNeighbours.Add(GetTileEast(tile));
		allTileNeighbours.Add(GetTileNorth(tile));
		allTileNeighbours.Add(GetTileSouth(tile));
		foreach (GameTile q in allTileNeighbours) {
			if (q != null && q.GetObject() != null) {
				validTileNeighbours.Add(q);
			}
		}
		return validTileNeighbours;
	}

	public bool InBounds(GameTile tile)
	{
		if((tile.GetX() < cols) && (tile.GetX() >= 0) && (tile.GetY() < rows) && (tile.GetY() >= 0))
			return true;
		return false;
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

	public void SetDestroyed()
	{
		//Iterate through the grid, if an grid tile does not have an object in unity then update it to be a Destroyed tile
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid[x][y].GetObject() == null) {
					grid[x][y].SetIsDestroyed(true);
				}
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
		List<GameTile> tileNeighbours = GetTileCardinalNeighbours(tile);
		foreach (GameTile neighbour in tileNeighbours) {
			if (!neighbour.IsMarked()) {
				neighbour.SetIsMarked(true);
				list.Add(neighbour);
			}
		}
	}

	private void AddDestroyedNeighbours(ref List<GameTile> list, GameTile tile){
		//FloodFillDestroyedTiles helper function, returns a list containing the destroyed neighbour tiles
		//of a tile that should be included in the flood fill
		List<GameTile> tileNeighbours = GetTileCardinalNeighbours(tile);
		foreach (GameTile neighbour in tileNeighbours) {
			if (!neighbour.IsMarked() && neighbour.IsDestroyed()) {
				neighbour.SetIsMarked(true);
				list.Add(neighbour);
			}
		}
	}

	private void AddValidNeighbours(ref List<GameTile> list, GameTile tile){
		//FloodFill helper function, returns a list containing the neighbours
		//of a tile that should be included in the flood fill
		List<GameTile> tileNeighbours = GetTileCardinalNeighbours(tile);
		foreach (GameTile neighbour in tileNeighbours) {
			if (neighbour.OpenForPlacement() && !neighbour.IsMarked()) {
				neighbour.SetIsMarked(true);
				list.Add(neighbour);
			}
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
		return (((float)numberOfOpenCells) / ((float)totalNumberOfCells));
	}

	protected void CalculateTileNeighbours()
	{
		//set up the neighbour information for each tile on the map
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid[x][y].SetTileNorth(GetTileNorth(grid[x][y]));
				grid[x][y].SetTileEast(GetTileEast(grid[x][y]));
				grid[x][y].SetTileWest(GetTileWest(grid[x][y]));
				grid[x][y].SetTileSouth(GetTileSouth(grid[x][y]));
			}
		}
	}

	protected bool checkForUnreachableOpenTile(int x, int y)
	{
	//1 for a open tile that is surrounded by walls on the cardnial directions
	//in otherwords a cave or room made up by one tile
		int count = 0;
		if(grid[x][y].OpenForPlacement()){
			List<GameTile> tileNeighbours = GetTileCardinalNeighbours(grid[x][y]);
			foreach (GameTile neighbour in tileNeighbours) {
				if (neighbour.IsWall())
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
					grid[x][y].SetIsDestroyed(false);
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

	public int FloodFillDestroyedTiles(ref GameTile tile, ref List<List<GameTile>> FloodFilledAreas, ref bool containsMapEdge)
	{
		//starting from the provided tile, mark all connected tiles (meaning stop at walls)
		//this essentially produces a cut out of a room that is reachable, i.e. no walls blocking off certian parts
		//it can also return a int count of the number of tiles in the area cut out
		List<GameTile> allMarkedCells = new List<GameTile>();
		List<GameTile> validNeighbours = new List<GameTile>();
		containsMapEdge = false;
		int count = 1;
		grid[tile.GetX()][tile.GetY()].SetIsMarked(true);
		allMarkedCells.Add(grid[tile.GetX()][tile.GetY()]);
		if (grid[tile.GetX()][tile.GetY()].IsMapEdge()) {
			containsMapEdge = true;
		}
		AddDestroyedNeighbours(ref validNeighbours,tile);
		List<GameTile> nextValidNeighbours = new List<GameTile>();
		do {
			nextValidNeighbours.Clear();
			count += validNeighbours.Count;
			for (int x = 0; x < validNeighbours.Count; x++) {
				validNeighbours[x].SetIsMarked(true);
				allMarkedCells.Add(validNeighbours[x]);
				if(validNeighbours[x].IsMapEdge())
				{
					containsMapEdge = true;
				}
				AddDestroyedNeighbours(ref nextValidNeighbours, validNeighbours[x]);
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
				if (!grid[x][y].IsMarked() && grid[x][y].OpenForPlacement()) {
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
				grid[x][y].SetOriginalSprite(grid[x][y].GetSprite());
				grid[x][y].SetOriginalColor(grid[x][y].GetColor());
			}
		}
	}

	protected void SetUpEdges()
	{
		//Mark edges (walls that only touch blanks and other walls never floors) for logic use
		int count = 0;
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
