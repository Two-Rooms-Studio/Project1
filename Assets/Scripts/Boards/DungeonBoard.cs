﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoard : Board {
	//setup vars
	private int minNumberOfRows = 4;
	private int minNumberOfCols = 4;
	private float chanceToStartAlive = 0.4f;
	private int deathLimit = 3;
	private int birthLimit = 4;
	private int numberOfSimulations = 5;
	private int numberOfMapsGenerated = 0; //keep track of how many times we try to generate a map with minimumPercentageOfOpenTiles
	private int maxMapsToGenerate = 5; // if we can't generate a map with minimumPercentageOfOpenTiles before reaching this number, then just generate a any map
	private float minimumPercentageOfOpenTiles = 0.1f;
	private Sprite innerWallSprite;
	private Sprite teleporterSprite;
	private Sprite waterSprite;
	private bool runEdgeSmoothing = false;
	private bool allowDisconnectedCaves = true;

	//other
	private Transform container;

	//public
	public void init (DungeonBoardSettings Settings)
	{
		//if the settings call for rows or cols under the minimum amount then scale the value up to the minimum
		Settings.rows = (Settings.rows < minNumberOfRows) ? minNumberOfRows : Settings.rows;
		Settings.cols = (Settings.cols < minNumberOfCols) ? minNumberOfCols : Settings.cols;
		Settings.minimumPercentageOfOpenTiles = (Settings.rows == minNumberOfRows && Settings.cols == minNumberOfCols) ? 0.25f : Settings.minimumPercentageOfOpenTiles;

		//TODO:Just make Settings a part of the DungeonBoard and then hand off what is needed from the settings object instead of doing all this setup
		//Configure based off of setting values
		base.init(Settings.rows, Settings.cols, Settings.tileObject);
		chanceToStartAlive = Settings.chanceToStartAlive;
		minimumPercentageOfOpenTiles = Settings.minimumPercentageOfOpenTiles;
		deathLimit = Settings.deathLimit;
		birthLimit = Settings.birthLimit;
		numberOfSimulations = Settings.numberOfSimulations;
		innerWallSprite = Settings.innerWallSprite;
		wallSprite = Settings.wallSprite;
		floorSprite = Settings.floorSprite;
		waterSprite = Settings.waterSprite;
		teleporterSprite = Settings.teleporterSprite;
		runEdgeSmoothing = Settings.runEdgeSmoothing;
		allowDisconnectedCaves = Settings.allowDisconnectedCaves;
		xPadding = Settings.xPadding;
		yPadding = Settings.yPadding;
		//
		gridContainerName = "DungeonGrid";
		initMap();
		if (!EnsureSpawnPointAndExitCanExist()) {
			RespawnMap();
		}
		MapCleanUp();
		SetAllOriginalSpritesAndColors();
		SpawnPlayerAndExitPoint();
		SetUpEdges(); //setup all tiles with edge information
		CalculateTileNeighbours(); //setup all tiles with neighbour information
	}

	//privates
	private void initMap()
	{
		//init the map grid
		container = new GameObject (gridContainerName).transform;
		List<GameTile> row = new List<GameTile>();
		float ySpace = 0.0f, xSpace = 0.0f;
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				float randNum = Random.Range(.0f, 1.0f);
				GameTile tile = new GameTile(x, y, floorSprite);
				GameObject instance = Instantiate (tileObject, new Vector3 (xSpace, ySpace, 0.0f), Quaternion.identity, container);
				tile.SetUnityPosition(xSpace, ySpace);
				instance.name = "(" + x + "," + y + ")";
				ySpace += yPadding;
				instance.GetComponent<SpriteRenderer>().sprite = floorSprite;
				if (x == 0 || x == cols - 1 || y == 0 || y == rows - 1) {
					tile.SetIsMapEdge(true);
				}
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
			ySpace = 0.0f;
			xSpace += xPadding;
		}
		MapSimulation();
		return;
	}

	private void MapSimulation()
	{
		for (int i = 0; i < numberOfSimulations; i++) {
			SimulationStep();
		}
	}

	private void SimulationStep()
	{
		//run through the grid making tiles walls or nonwalls dependent on the rules of the game of life
		List<List<GameTile>> oldmap = new List<List<GameTile>>(grid);
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				int wallsAroundPoint = countWalls (x, y, oldmap);
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

	private void RespawnMap()
	{
		//Constantly rerolls the map until a valid map is generated
		DeleteEntireMap();
		initMap();
		if(!EnsureSpawnPointAndExitCanExist())
		{
			RespawnMap();
		}
	}

//Map Cleanup functions
	private void MapCleanUp()
	{
		//Cleanup various factors left over after generation
		RemoveBlockedOpenTiles();
		FixEdges(wallSprite);
		RemoveFloatingWalls();
		if (allowDisconnectedCaves) {
			ConnectDisconnectedCaves();
			if (runEdgeSmoothing) {
				SmoothMapEdges ();
			}
		} else {
			RemoveDisconnectedCaves();
			CheckForMinimumMapSize();
			if (runEdgeSmoothing) {
				SmoothMapEdges ();
			}
		}
		SetDestroyed();
		GenerateLiquid();
		ChangeInnerWallSprites();
	}

	private void CheckForMinimumMapSize()
	{
		float percentageOfOpenTiles = CalculatePlayingArea();
		if (percentageOfOpenTiles < minimumPercentageOfOpenTiles) {
			if (numberOfMapsGenerated < maxMapsToGenerate) {
				numberOfMapsGenerated++;
				// if this reaches MaxMapsToGenerate then we have tried to generate too many maps with the minimumPercentageOfOpenTiles,
				//at this point to prevent a crash we need to ignore minimumPercentageOfOpenTiles
				DeleteEntireMap();
				initMap();
				if (!EnsureSpawnPointAndExitCanExist()) {
					RespawnMap();
				}
				//rerun the already called MapCleanUp functions
				RemoveBlockedOpenTiles();
				FixEdges(wallSprite);
				RemoveFloatingWalls();
				RemoveDisconnectedCaves();
				CheckForMinimumMapSize();
				return;
			} else {
				Debug.Log("Map generation failed too many times based off of minimum percentage of open tiles allowed, therefore a map was generated without a minimum number of open tiles, consider the settings your using!");
				minimumPercentageOfOpenTiles = 0.0f;
				numberOfMapsGenerated = 0;
				DeleteEntireMap();
				initMap();
				if (!EnsureSpawnPointAndExitCanExist()) {
					RespawnMap();
				}
				//rerun the already called MapCleanUp functions
				RemoveBlockedOpenTiles();
				FixEdges(wallSprite);
				RemoveFloatingWalls();
				RemoveDisconnectedCaves();
				return;
			}
		}
	}

	private void ChangeInnerWallSprites()
	{
		//changes walls the player can not see the outside of too a different sprite
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if((y-1) >= 0 && grid[x][y].IsWall()) {
					if ((!grid[x][y-1].IsDestroyed() && grid[x][y-1].IsWall())) {
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = innerWallSprite;
					}
				}
			}
		}
	}

	private void ConnectDisconnectedCaves()
	{
		//connect all disconnected caves with "teleporters" in order to ensure the player can reach all caves
		UnMarkAllTiles();
		List<List<GameTile>> FloodFilledAreas = new List<List<GameTile>>();
		Vector2 randomPoint;
		bool atLeastOneOpen;
		int randomIndexX, randomIndexY;
		randomPoint = GetRandomOpenUnMarkedPoint();
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].OpenForPlacement() || grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		GameTile randomTile = grid[(int)randomPoint.x][(int)randomPoint.y]; 
		FloodFill(ref randomTile, ref FloodFilledAreas);
		if (HasUnmarkedTiles()) {//if we have unreachable caves then we need to make teleporters to access them
			do {
				randomPoint = GetRandomOpenUnMarkedPoint();
				randomTile = grid[(int)randomPoint.x][(int)randomPoint.y]; //switch randomTile to the new unmarked tile
				randomTile.SetIsMarked(true);
				FloodFill(ref randomTile, ref FloodFilledAreas);
				PlaceTeleporter(ref randomTile);
				atLeastOneOpen = false;
				do {
					randomIndexX = (int)(Random.Range(0.0f, (float)FloodFilledAreas.Count - 2)); //place a corresponding exit point randomly in an area already flood filled
					for(int x = 0; x < FloodFilledAreas[randomIndexX].Count; x++)
					{
						if(FloodFilledAreas[randomIndexX][x].OpenForPlacement()){
							atLeastOneOpen = true;
						}
					}
				} while (!atLeastOneOpen);
				do {
					randomIndexY = (int)(Random.Range(0.0f, (float)FloodFilledAreas[randomIndexX].Count - 1));
				} while (!FloodFilledAreas[randomIndexX][randomIndexY].OpenForPlacement());
				GameTile previouslyMarkedTileToUseAsExit = FloodFilledAreas[randomIndexX][randomIndexY];
				randomTile.GetObject().AddComponent<TeleporterTile>().exitPointObject = previouslyMarkedTileToUseAsExit.GetObject();
				randomTile.GetObject().GetComponent<TeleporterTile>().exitPoint = previouslyMarkedTileToUseAsExit;
				GameTile previous_tile = randomTile;
				randomTile = previouslyMarkedTileToUseAsExit;
				randomTile.GetObject().AddComponent<TeleporterTile>().exitPointObject = previous_tile.GetObject();
				randomTile.GetObject().GetComponent<TeleporterTile>().exitPoint = previous_tile;
				PlaceTeleporter(ref randomTile); 
			} while (HasUnmarkedTiles());
		}
		FloodFilledAreas.Clear();
	}

	private void DeleteEntireMap()
	{
		//Deletes the entire map in order to generate a new one
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				Destroy(grid[x][y].GetObject());
			}
		}
		grid.Clear();
		Destroy(container.gameObject);
	}

	private Vector2 GetRandomOpenUnMarkedPoint()
	{
		Vector2 randomPoint;
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].OpenForPlacement() || grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		return randomPoint;
	}

	private Vector2 GetRandomOpenMarkedPoint()
	{
		Vector2 randomPoint;
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].OpenForPlacement() && !grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		return randomPoint;
	}

	private void PlaceTeleporter(ref GameTile tile)
	{
		tile.SetIsOccupied(true);
		tile.GetObject().GetComponent<SpriteRenderer>().sprite = teleporterSprite;
		tile.GetObject().GetComponent<SpriteRenderer>().color = Color.cyan;
		tile.SetIsWalkAble(true);
	}

	private void RemoveDisconnectedCaves()
	{
		//Finds disconnected caves and removes them from the map
		UnMarkAllTiles();
		List<List<GameTile>> FloodFilledAreas = new List<List<GameTile>>();
		Vector2 randomPoint;
		GameTile startingTile;
		int size = 0, largest_size = 0, largest_index = 0, count = -1;
		List<GameTile> FloodFillStartLocations = new List<GameTile>();
		do {
			randomPoint = GetRandomOpenUnMarkedPoint();
			startingTile = grid[(int)randomPoint.x][(int)randomPoint.y];
			startingTile.SetIsMarked(true);
			size = FloodFill(ref startingTile, ref FloodFilledAreas);
			count++;
			if(size > largest_size){
				largest_size = size;
				largest_index = count;
			}
			FloodFillStartLocations.Add(grid[(int)randomPoint.x][(int)randomPoint.y]);
		}	while (HasUnmarkedTiles());
		FloodFillStartLocations.RemoveAt(largest_index);//remove the largest cave system from what we are going to remove
		UnMarkAllTiles();
		for (int i = 0; i < FloodFillStartLocations.Count; i++) {
			startingTile = FloodFillStartLocations[i];
			startingTile.SetIsMarked(true);
			FloodFill(ref startingTile, ref FloodFilledAreas);
		}
		DeleteAllMarkedTiles();
		RemoveFloatingWalls();
		FixEdges(wallSprite);
		FloodFilledAreas.Clear();
	}

	private void SetDestroyed()
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

	private void GenerateLiquid()
	{
		//generates liquid in internal areas of the map that have been destoryed, basically generates liquid in blank spaces within the map
		UnMarkAllTiles();
		List<List<GameTile>> FloodFilledAreas = new List<List<GameTile>>();
		List<GameTile> FloodFillStartLocations = new List<GameTile>();
		GameTile destroyedTile;
		bool containsMapEdge = false;
		do {
			Vector2 randomPoint = GetNextDestroyedUnMarkedPoint();
			destroyedTile = grid[(int)randomPoint.x][(int)randomPoint.y];
			grid[(int)randomPoint.x][(int)randomPoint.y].SetIsMarked(true);
			FloodFillDestroyedTiles(ref destroyedTile, ref FloodFilledAreas, ref containsMapEdge);
			if (!containsMapEdge) {
				FloodFillStartLocations.Add(grid[(int)randomPoint.x][(int)randomPoint.y]);
			}
		} while (HasUnmarkedDestroyedTiles());
		UnMarkAllTiles();
		for (int i = 0; i < FloodFillStartLocations.Count; i++) {
			destroyedTile = FloodFillStartLocations[i];
			destroyedTile.SetIsMarked(true);
			FloodFillDestroyedTiles(ref destroyedTile, ref FloodFilledAreas, ref containsMapEdge);
		}
		GenerateGameObjectsForLiquidTiles();
		FloodFilledAreas.Clear();
	}

	private Vector2 GetNextDestroyedUnMarkedPoint()
	{
		//return a destoryed unmarked point, helper function for Generate Liquid, should only be called when HasUnMarkedDestoryedTiles() has ensured a return
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if ((!grid[x][y].IsMarked()) && (grid[x][y].IsDestroyed())) {
					grid[x][y].SetIsMarked(true);
					return new Vector2 (grid[x][y].GetX(), grid[x][y].GetY());
				}
			}
		}
		Debug.Log("No tile was found MAJOR ERROR AT DUNGEON BOARD 196");
		return new Vector2(-99.0f, -99.0f); //error value will cause out of bound exception
	}

	private bool HasUnmarkedDestroyedTiles()
	{
		//return true if the board has any destroyed tiles that aren't marked that could be marked
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (!grid[x][y].IsMarked() && grid[x][y].IsDestroyed()) {
					return true;
				}
			}
		}
		return false;
	}

	private void GenerateGameObjectsForLiquidTiles()
	{
		//Now that we have found the tiles that we want to turn into water, we need to regenerate game objects for them and make them water
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid[x][y].IsMarked()) {
					grid[x][y].SetIsDestroyed(false);
					grid[x][y].SetIsWall(false);
					grid[x][y].SetIsMarked(false);
					grid[x][y].SetIsWalkAble(false);
					grid[x][y].SetIsOccupied(true);
					GameObject instance = Instantiate (tileObject, new Vector3 (grid[x][y].GetUnityXPosition(), grid[x][y].GetUnityYPosition(), 0.0f), Quaternion.identity, container);
					instance.name = "(" + x + "," + y + ")";
					instance.GetComponent<SpriteRenderer>().sprite = waterSprite;
					grid[x][y].SetObject(instance);
				}
			}
		}
	}
}