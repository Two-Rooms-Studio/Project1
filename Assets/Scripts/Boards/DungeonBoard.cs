using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoard : Board {
	private int minNumberOfRows = 4;
	private int minNumberOfCols = 4;
	private int numberOfMapsGenerated = 0; //keep track of how many times we try to generate a map with minimumPercentageOfOpenTiles
	private int maxMapsToGenerate = 15; // if we can't generate a map with minimumPercentageOfOpenTiles before reaching this number, then just generate a any map
	DungeonBoardSettings mapSettings;
	private Transform container;

	//public
	public void init (DungeonBoardSettings Settings)
	{
		//if the settings call for rows or cols under the minimum amount then scale the value up to the minimum
		Settings.rows = (Settings.rows < minNumberOfRows) ? minNumberOfRows : Settings.rows;
		Settings.cols = (Settings.cols < minNumberOfCols) ? minNumberOfCols : Settings.cols;
		Settings.minimumPercentageOfOpenTiles = (Settings.rows == minNumberOfRows && Settings.cols == minNumberOfCols) ? 0.25f : Settings.minimumPercentageOfOpenTiles;
		//

		//basic board info setup
		base.init(Settings.rows, Settings.cols, Settings.tileObject, "DungeonGrid");
		mapSettings = Settings;
		//

		//map generation
		initMap(); //create the static grid that we use to play the game of life on
		MapSimulation(); //play the game of life on the grid a number of times equal to mapSettings.numberOfSimulations
		//

		//map cleanup
		if (!EnsureSpawnPointAndExitCanExist()) { //ensure that we can create a spawn and exit, if not restart map generation
			RespawnMap();
		}
		RemoveBlockedOpenTiles(); // if any floor tiles are completely blocked in, remove them from the game
		FixEdges(mapSettings.wallSprite); //if any floor tiles are touching the edge of the gird change them to walls
		RemoveFloatingWalls(); //remove any walls that are just freefloating next to destoryed tiles
		//

		//water generation
		DungeonBoardWaterGeneration waterGeneration = ScriptableObject.CreateInstance<DungeonBoardWaterGeneration>();
		waterGeneration.GenerateLiquid(this, ref container, mapSettings.waterSprite); //generate water in natural holes in the map
		//

		//fix caves
		if (mapSettings.allowDisconnectedCaves) { //if we allow caves to spawn disconnected then place teleporters to connect them
			ConnectDisconnectedCaves();
		} else { //otherwise remove the disconnected caves and make sure we retain a certain percentage of play space
			RemoveDisconnectedCaves();
			CheckForMinimumMapSize();
		}
		if (mapSettings.runEdgeSmoothing) { //if we enable smoothing out map edges
			SmoothMapEdges ();
		}
		SetDestroyed(); //Set the destroyed status for all tiles
		ChangeInnerWallSprites(); //set walls that the player cannot see the "bottom" side of to a different sprite
		//

		//Final data setup
		SetAllOriginalSpritesAndColors(); //set the sprite and color values for each tile for use in later logic
		SpawnPlayerAndExitPoint(); //spawn the player and the exit point for the map
		SetUpEdges(); //setup all tiles with edge information
		CalculateTileNeighbours(); //setup all tiles with neighbour information
		//
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
				GameTile tile = new GameTile(x, y, mapSettings.floorSprite);
				GameObject instance = Instantiate (tileObject, new Vector3 (xSpace, ySpace, 0.0f), Quaternion.identity, container);
				tile.SetUnityPosition(xSpace, ySpace);
				instance.name = "(" + x + "," + y + ")";
				ySpace += yPadding;
				instance.GetComponent<SpriteRenderer>().sprite = mapSettings.floorSprite;
				if (x == 0 || x == cols - 1 || y == 0 || y == rows - 1) {
					tile.SetIsMapEdge(true);
				}
				if (randNum < mapSettings.chanceToStartAlive) {
					//set the tile to be a wall tile since it passed random test
					//at this point its pointless to destroy and reinstantiate a wall object but in the future we may have to do that
					tile.SetIsWall(true);
					instance.GetComponent<SpriteRenderer>().sprite = mapSettings.wallSprite;
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
		for (int i = 0; i < mapSettings.numberOfSimulations; i++) {
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
					if (wallsAroundPoint < mapSettings.deathLimit) {
						grid[x][y].SetIsWall(false);
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = mapSettings.floorSprite;
					} 
				} else {
					if (wallsAroundPoint > mapSettings.birthLimit) {
						grid[x][y].SetIsWall(true);
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = mapSettings.wallSprite;
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
		init(mapSettings);
		if(!EnsureSpawnPointAndExitCanExist())
		{
			RespawnMap();
		}
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

	private void PlaceTeleporter(ref GameTile tile)
	{
		//place a teleporter on tile
		tile.SetIsOccupied(true);
		tile.GetObject().GetComponent<SpriteRenderer>().sprite = mapSettings.teleporterSprite;
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
		FixEdges(mapSettings.wallSprite);
		FloodFilledAreas.Clear();
	}

	private Vector2 GetRandomOpenUnMarkedPoint()
	{
		//return a random tile that is open and not marked
		Vector2 randomPoint;
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].OpenForPlacement() || grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		return randomPoint;
	}

	private Vector2 GetRandomOpenMarkedPoint()
	{
		//return a random tile that is open and marked
		Vector2 randomPoint;
		do {
			randomPoint = new Vector2((int)Random.Range(0, cols - 1), (int)Random.Range(0, rows - 1));
		} while (!grid[(int)randomPoint.x][(int)randomPoint.y].OpenForPlacement() && !grid[(int)randomPoint.x][(int)randomPoint.y].IsMarked());
		return randomPoint;
	}

	private void CheckForMinimumMapSize()
	{
		//check that our map has atleast a certain minimum of playable space if not regenerate the map
		float percentageOfOpenTiles = CalculatePlayingArea();
		if (percentageOfOpenTiles < mapSettings.minimumPercentageOfOpenTiles) {
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
				FixEdges(mapSettings.wallSprite);
				RemoveFloatingWalls();
				DungeonBoardWaterGeneration waterGeneration = ScriptableObject.CreateInstance<DungeonBoardWaterGeneration>();
				waterGeneration.GenerateLiquid(this, ref container, mapSettings.waterSprite);
				RemoveDisconnectedCaves();
				CheckForMinimumMapSize();
				return;
			} else {
				Debug.Log("Map generation failed too many times based off of minimum percentage of open tiles allowed, therefore a map was generated without a minimum number of open tiles, consider the settings your using!");
				mapSettings.minimumPercentageOfOpenTiles = 0.0f;
				numberOfMapsGenerated = 0;
				DeleteEntireMap();
				initMap();
				if (!EnsureSpawnPointAndExitCanExist()) {
					RespawnMap();
				}
				//rerun the already called MapCleanUp functions
				RemoveBlockedOpenTiles();
				FixEdges(mapSettings.wallSprite);
				RemoveFloatingWalls();
				DungeonBoardWaterGeneration waterGeneration = ScriptableObject.CreateInstance<DungeonBoardWaterGeneration>();
				waterGeneration.GenerateLiquid(this, ref container, mapSettings.waterSprite);
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
						grid[x][y].GetObject().GetComponent<SpriteRenderer>().sprite = mapSettings.innerWallSprite;
					}
				}
			}
		}
	}
}