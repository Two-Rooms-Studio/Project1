using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoard : Board {
	private float chanceToStartAlive = 0.4f;
	private int deathLimit = 3;
	private int birthLimit = 4;
	private int numberOfSimulations = 5;
	private Sprite innerWallSprite;
	private Sprite teleporterSprite;

	//public
	public void init (int p_rows, int p_cols, GameObject floorObject, GameObject wallObject, Sprite p_innerWallSprite, Sprite p_teleporterSprite, float p_chanceToStartAlive, int p_deathLimit, int p_birthLimit, int p_numberofSimulations)
	{
		//setup the dungeon board
		base.init (p_rows, p_cols, floorObject, wallObject);
		chanceToStartAlive = p_chanceToStartAlive;
		deathLimit = p_deathLimit;
		birthLimit = p_birthLimit;
		numberOfSimulations = p_numberofSimulations;
		innerWallSprite = p_innerWallSprite;
		teleporterSprite = p_teleporterSprite;
		initMap();
		for (int i = 0; i < numberOfSimulations; i++) {
			SimulationStep();
		}
		MapCleanUp ();
		if (!EnsureSpawnPointExsits()) { // we need to make a spawn point one did not generate
			FixSpawnPoint(floorObject);
			CalculateTileNeighbours();
		}
		SetAllOriginalSprites();
		CalculateTileNeighbours();
	}

	//privates
	private void initMap()
	{
		//init the map grid
		Transform boardHolder = new GameObject ("DungeonGrid").transform;
		List<GameTile> row = new List<GameTile>();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				float randNum = Random.Range(.0f, 1.0f);
				GameTile tile = new GameTile((float)x, (float)y, floorSprite);
				GameObject instance = Instantiate (floorObject, new Vector3 ((float)xPadding, (float)yPadding, 0.0f), Quaternion.identity, boardHolder);
				instance.name = "(" + x + "," + y + ")";
				yPadding += 0.24f;
				if (randNum < chanceToStartAlive) {
					//set the tile to be a wall tile since it passed random test
					//at this point its pointless to destroy and reinstantiate a wall object but in the future we may have to do that
					tile.SetIsWall(true);
					instance.GetComponent<SpriteRenderer>().sprite = wallSprite;
				}
				tile.SetObject(instance);
				row.Add(tile);
			}
			grid.Add(row);
			row = new List<GameTile>();
			yPadding = 0.0f;
			xPadding += 0.16f;
		}
		return;
	}

	private void SimulationStep()
	{
		//run through the grid making tiles walls or nonwalls dependent on the rules of the game of life
		List<List<GameTile>> oldmap = new List<List<GameTile>>(grid);
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				int wallsAroundPoint = countWalls (oldmap, x, y);
				if (oldmap[x][y].IsWall()) {
					if (wallsAroundPoint < deathLimit) {
						grid[x][y].SetIsWall(false);
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = floorSprite;
					} else {
						grid[x][y].SetIsWall(true);
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = wallSprite;
					}
				} else {
					if (wallsAroundPoint > birthLimit) {
						grid[x][y].SetIsWall(true);
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = wallSprite;
					} else {
						grid[x][y].SetIsWall(false);
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = floorSprite;
					}
				}
			}
		}
		return;
	}

	private int countWalls(List<List<GameTile>> grid, int x, int y)
	{
		//count the walls surrounding a given tile
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

//Map Cleanup functions
	private void MapCleanUp()
	{
		//Cleanup various factors left over after generation
		RemoveExtraWalls();
		FixEdges();
		RemoveFloatingWalls();
		CalculateTileNeighbours();
		SmoothMapEdges();
		ChangeInnerWallSprites();
		CalculateTileNeighbours();
		EnsureCaveConnection();
	}

	private void RemoveExtraWalls()
	{
		//remove walls that are completely surrounded by other walls
		for (int x = 0; x < cols; x++) { 
			for (int y = 0; y < rows; y++) {
				int wallCount = countWalls(grid, x, y);
				if (wallCount >= 8) {
					grid[x][y].SetIsDestroyed(true);
					grid[x][y].SetIsWall(true);
					Destroy(grid[x][y].GetObject());
				}
			}
		}
	}
		
	private void FixEdges()
	{
		//Fix edges so that non-wall tiles never touch a destroyed(empty air) tile by changing open tiles to walls
		for (int x = 0; x < cols; x++) { //turn unblocked edges into walls
			for (int y = 0; y < rows; y++) {
				int DestroyedCount = countDestroyed (x, y);
				if (DestroyedCount >= 1 && !(grid[x][y].IsWall())) {
					grid[x][y].SetIsWall(true);
					grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = wallSprite;
				}
			}
		}
	}

	private int countDestroyed(int x, int y)
	{
		//count the number of destroyed(open air) tiles around a given point
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
		
	private void RemoveFloatingWalls()
	{
		//Remove walls that are floating (surrounded by only walls or destroyed tiles)
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				int FloatingCount = countFloating (x, y);
				if (FloatingCount >= 8 && grid[x][y].IsWall()) {
					grid[x][y].SetIsDestroyed(true);
					grid[x][y].SetIsWall(true);
					Destroy(grid[x][y].GetObject());
				}
			}
		}
	}

	private int countFloating(int x, int y)
	{
		//count the number of destroyed or wall tiles around a given point
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
		
	private void SmoothMapEdges()
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
						if (grid[x][y].GetTileNorth() == null || grid[x][y].GetTileNorth().IsWall() || grid[x][y].GetTileNorth ().IsWall()) {
							count++;
						}
						if (grid[x][y].GetTileEast() == null || grid[x][y].GetTileEast().IsWall() || grid[x][y].GetTileEast().IsDestroyed()) {
							count++;
						}
						if (grid[x][y].GetTileWest() == null || grid[x][y].GetTileWest ().IsWall() || grid[x][y].GetTileWest().IsDestroyed()) {
							count++;
						}
						if (grid[x][y].GetTileSouth() == null || grid[x][y].GetTileSouth().IsWall() || grid[x][y].GetTileSouth().IsDestroyed()) {
							count++;
						}
						if (count == 4) {
							removed = true;
							grid[x][y].SetIsDestroyed(true);
							grid[x][y].SetIsWall(true);
							Destroy (grid[x][y].GetObject());
							CalculateTileNeighbours();
						}
						count = 0;
					}
				}
			}
		} while (removed == true);
	}

	private void ChangeInnerWallSprites()
	{
		//changes walls the player can not see the outside of to a different sprite
		CalculateTileNeighbours();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if(grid[x][y].GetTileSouth() != null && grid[x][y].IsWall()) {
					if ((!grid[x][y].GetTileSouth().IsDestroyed() && grid[x][y].GetTileSouth().IsWall())) {
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = innerWallSprite;
					}
				}
			}
		}
	}

	private void EnsureCaveConnection()
	{
		//connect all disconnected caves with "teleporters" in order to ensure the player can reach all caves
		Vector2 randomPoint;
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].Open() || grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		GameTile randomTile = grid[(int)randomPoint.x][(int)randomPoint.y]; 
		randomTile.SetIsMarked(true);
		FloodFill(ref randomTile);
		if (HasUnmarkedTiles()) {//if we have unreachable caves then we need to make teleporters to access them
			randomTile.SetIsOccupied(true);
			randomTile.GetObject().GetComponent<SpriteRenderer>().sprite = teleporterSprite;
			randomTile.GetObject().GetComponent<SpriteRenderer>().color = Color.cyan;
			do {
				do {
					randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
				} while (!grid[(int)randomPoint.x][(int)randomPoint.y].Open() || grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
				randomTile = grid[(int)randomPoint.x][(int)randomPoint.y]; //switch randomTile to the new unmarked tile
				randomTile.SetIsMarked(true);
				FloodFill(ref randomTile);
				randomTile.SetIsOccupied(true);
				randomTile.GetObject().GetComponent<SpriteRenderer>().sprite = teleporterSprite;
				randomTile.GetObject().GetComponent<SpriteRenderer>().color = Color.cyan;
			} while (HasUnmarkedTiles());
		}

	}

	private void FloodFill(ref GameTile tile)
	{
		//starting from the provided tile, mark all connected tiles (meaning stop at walls)
		//this essentially produces a cut out of a room that is reachable, i.e. no walls blocking off certian parts
		List<GameTile> validNeighbours = new List<GameTile>();
		tile.SetIsMarked(true);
		AddValidNeighbours(ref validNeighbours, tile);
		List<GameTile> nextValidNeighbours = new List<GameTile>();
		do {
			nextValidNeighbours.Clear();
			for (int x = 0; x < validNeighbours.Count; x++) {
				validNeighbours[x].SetIsMarked(true);
				AddValidNeighbours(ref nextValidNeighbours, validNeighbours[x]);
			}
			validNeighbours = new List<GameTile>(nextValidNeighbours);
		} while (nextValidNeighbours.Count != 0);
		return;
	}

	private void AddValidNeighbours(ref List<GameTile> list, GameTile tile){
		tile.SetIsMarked(true);//
		if(tile.GetTileNorth() != null && !tile.GetTileNorth().IsWall() && !tile.GetTileNorth().IsMarked()){
			list.Add(tile.GetTileNorth());
			tile.GetTileNorth().SetIsMarked(true);
		}
		if (tile.GetTileSouth() != null && !tile.GetTileSouth().IsWall() && !tile.GetTileSouth().IsMarked()) {
			list.Add(tile.GetTileSouth());
			tile.GetTileSouth().SetIsMarked(true);
		}
		if (tile.GetTileEast() != null && !tile.GetTileEast().IsWall() && !tile.GetTileEast().IsMarked()) {
			list.Add(tile.GetTileEast());
			tile.GetTileEast().SetIsMarked(true);
		}
		if (tile.GetTileWest() != null && !tile.GetTileWest().IsWall() && !tile.GetTileWest().IsMarked()) {
			list.Add(tile.GetTileWest());
			tile.GetTileWest().SetIsMarked(true);
		}
	}
//
	private void FixSpawnPoint(GameObject tileObject)
	{
		//ran if the generated map has absolutely no place to place a spawn point 
		//obviously the hope is maps generated in this way will be rejected and regenerated but this prevents crashing for now
		Transform boardHolder = new GameObject("DungeonGrid").transform;
		Debug.Log("Had to fix spawn point");
		Vector2 randPoint = new Vector2((int)Random.Range(0, cols-1), (int)Random.Range(0, rows-1));
		grid[(int)randPoint.x][(int)randPoint.y].SetIsWall(false);
		if (grid [(int)randPoint.x][(int)randPoint.y].IsDestroyed()) {
			GameObject instance = Instantiate (tileObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, boardHolder);
			grid [(int)randPoint.x][(int)randPoint.y].SetObject (instance);				
			grid[(int)randPoint.x][(int)randPoint.y].SetIsDestroyed(false);
		}
		grid [(int)randPoint.x][(int)randPoint.y].GetObject().GetComponent<SpriteRenderer>().sprite = floorSprite;
	}
}