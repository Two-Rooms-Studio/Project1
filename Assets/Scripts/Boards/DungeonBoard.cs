using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoard : Board {
	//setup vars
	private float chanceToStartAlive = 0.4f;
	private float minimumPercentageOfOpenTiles = 0.1f;
	private int deathLimit = 3;
	private int birthLimit = 4;
	private int numberOfSimulations = 5;
	private Sprite innerWallSprite;
	private Sprite teleporterSprite;
	private bool runEdgeSmoothing = false;
	private bool allowDisconnectedCaves = true;

	//other
	private Transform container;

	//public
	public void init (DungeonBoardSettings Settings)
	{
		base.init(Settings.rows, Settings.cols, Settings.tileObject);
		chanceToStartAlive = Settings.chanceToStartAlive;
		minimumPercentageOfOpenTiles = Settings.minimumPercentageOfOpenTiles;
		deathLimit = Settings.deathLimit;
		birthLimit = Settings.birthLimit;
		numberOfSimulations = Settings.numberOfSimulations;
		innerWallSprite = Settings.innerWallSprite;
		wallSprite = Settings.wallSprite;
		floorSprite = Settings.floorSprite;
		teleporterSprite = Settings.teleporterSprite;
		runEdgeSmoothing = Settings.runEdgeSmoothing;
		allowDisconnectedCaves = Settings.allowDisconnectedCaves;
		xPadding = 0.16f;
		yPadding = 0.24f;
		gridContainerName = "DungeonGrid";
		initMap();
		MapSimulation();
		MapCleanUp();
		if (!EnsureSpawnPointExsits()) {
			FixSpawnPoint(tileObject);
		}
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
				instance.name = "(" + x + "," + y + ")";
				ySpace += yPadding;
				instance.GetComponent<SpriteRenderer>().sprite = floorSprite;
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
			if (runEdgeSmoothing) {
				SmoothMapEdges ();
			}
		}
		ChangeInnerWallSprites();
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
		List<List<GameTile>> FloodFilledAreas = new List<List<GameTile>>();
		Vector2 randomPoint;
		bool atLeastOneOpen;
		int randomIndexX, randomIndexY;
		randomPoint = GetRandomOpenUnMarkedPoint();
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].OpenForPlacement() || grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		GameTile randomTile = grid[(int)randomPoint.x][(int)randomPoint.y]; 
		randomTile.SetIsMarked(true);
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
		List<List<GameTile>> FloodFilledAreas = new List<List<GameTile>>();
		//Finds disconnected caves and removes them from the map
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
		float percentageOfOpenTiles = CalculatePlayingArea();
		Debug.Log("Percent of Open Tiles: " + (percentageOfOpenTiles * 100));
		if (percentageOfOpenTiles < minimumPercentageOfOpenTiles) {
			FloodFilledAreas.Clear();
			DeleteEntireMap();
			initMap();
			MapSimulation();
			return;
		}
		FloodFilledAreas.Clear();
		return;
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