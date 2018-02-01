using System.Collections;
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
		FloodFilledAreas.Clear();
		UnMarkAllTiles();
		for (int i = 0; i < FloodFillStartLocations.Count; i++) {
			destroyedTile = FloodFillStartLocations[i];
			destroyedTile.SetIsMarked(true);
			FloodFillDestroyedTiles(ref destroyedTile, ref FloodFilledAreas, ref containsMapEdge);
		}
		GenerateGameObjectsForLiquidTiles();
		BreakWallsRandomlyAroundWater(ref FloodFilledAreas);
		UnMarkAllTiles();
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
					//todo:add color settings
					grid[x][y].SetObject(instance);
				}
			}
		}
	}

	private void BreakWallsRandomlyAroundWater(ref List<List<GameTile>> waterTilesSeperatedByAreas)
	{
		//grab the valid walls around our water areas (walls that are touching floor tiles) and break them (turn them into water tiles)
		int divisor = 3; // What fraction of walls should we ensured are punched out around the water? if divisor = 3 then 1/3 (rounded down) of all walls around a water area will be removed
		float chanceForOtherWallBreaks = 0.35f; // after the ensured walls every other wall has this chance of breaking, for example if set to .45f every other wall would have a 45% chance of breaking
		float chanceForWaterExpansion = 0.35f; // the chance for water to "expand" outwards for the edge of a pool of water
		UnMarkAllTiles();
		List<List<GameTile>> wallsAroundWaterTilesSeperatedByAreas = GetWallsSurroundingWaterAreas(ref waterTilesSeperatedByAreas);
		for (int i = 0; i < wallsAroundWaterTilesSeperatedByAreas.Count; i++) {
			int ensuredWallBreaks = wallsAroundWaterTilesSeperatedByAreas[i].Count / divisor;
			ensuredWallBreaks = (ensuredWallBreaks <= 0) ? 1 : ensuredWallBreaks; //atleast always destory one wall around a water area
			//walk down all of the walls for a given area, selecting a wall at random and breaking it, repeat a number of times equal to our ensuredWallBreaks
			for (int c = 0; c < ensuredWallBreaks; c++) {
				int randomWall = (int)(Random.Range(0.0f, (float)wallsAroundWaterTilesSeperatedByAreas[i].Count - 1));
				BreakWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall]);
				if (CheckForIsolatedWaterTile(wallsAroundWaterTilesSeperatedByAreas[i][randomWall])) {
					FindAndBreakDividerWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall]);
				}
				wallsAroundWaterTilesSeperatedByAreas[i].RemoveAt(randomWall);
			}
			int originalSize = wallsAroundWaterTilesSeperatedByAreas[i].Count;
			//walk down all of the walls for a given area, attempting to break each wall, the attempt success rate is based off of our chanceForOtherWallBreaks value
			for (int c = 0; c < originalSize; c++) {
				float breakWallCheck = Random.Range(0.0f, 100.0f);
				if ((breakWallCheck <= chanceForOtherWallBreaks) && (wallsAroundWaterTilesSeperatedByAreas[i].Count > 0)) {
					int randomWall = (int)(Random.Range(0.0f, (float)wallsAroundWaterTilesSeperatedByAreas[i].Count - 1));
					BreakWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall]);
					if (CheckForIsolatedWaterTile(wallsAroundWaterTilesSeperatedByAreas[i][randomWall])) {
						FindAndBreakDividerWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall]);
					}
					wallsAroundWaterTilesSeperatedByAreas[i].RemoveAt(randomWall);
				}
			}
		}
		UnMarkAllTiles();
//		//walk down all of the water tiles for a given area, attempting to cause the water to expand outward, the attempt sucess rate is based off of our chanceForWaterExpansion value
//		for(int i = 0; i < waterTilesSeperatedByAreas.Count; i++) {
//			int originalSize = waterTilesSeperatedByAreas[i].Count;
//			for (int j = 0; j < originalSize; j++) {
//				float ExpandWaterCheck = Random.Range(0.0f, 100.0f);
//				if ((ExpandWaterCheck <= chanceForWaterExpansion) && (waterTilesSeperatedByAreas[i].Count > 0)) {
//					AttemptToExpandWater(waterTilesSeperatedByAreas[i][j], ref waterTilesSeperatedByAreas);
//				}
//			}
//		}
//		UnMarkAllTiles();
	}

	private List<List<GameTile>> GetWallsSurroundingWaterAreas(ref List<List<GameTile>> waterTilesSeperatedByAreas)
	{
		//return the walls around a area of water that are touching a valid floor tile.
		List<GameTile> wallsAroundWaterArea = new List<GameTile>();
		List<List<GameTile>> wallsAroundWaterTilesSeperatedByAreas = new List<List<GameTile>>();
		for (int i = 0; i < waterTilesSeperatedByAreas.Count; i++) {
			for (int j = 0; j < waterTilesSeperatedByAreas[i].Count; j++) {
				wallsAroundWaterArea = (ReturnFloorTouchingWallsAroundTile(waterTilesSeperatedByAreas[i][j]));
				if (wallsAroundWaterArea.Count > 0) {
					wallsAroundWaterTilesSeperatedByAreas.Add(wallsAroundWaterArea);
				}
			}
		}
		return wallsAroundWaterTilesSeperatedByAreas;
	}

	private List<GameTile> ReturnFloorTouchingWallsAroundTile(GameTile tile)
	{
		//retruns walls around a given tile, will only add tiles that aren't marked and are valid neighbour's of floor tiles
		List<GameTile> allWallsList = new List<GameTile>();
		List<GameTile> validWallsList = new List<GameTile>();
		grid[tile.GetX()][tile.GetY()].SetIsMarked(true);
		if((tile.GetY() + 1) < rows && grid[tile.GetX()][tile.GetY()+1].IsWall() && !grid[tile.GetX()][tile.GetY()+1].IsMarked()){
			grid[tile.GetX()][tile.GetY()+1].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()][tile.GetY()+1]);
		}
		if ((tile.GetY() - 1) >= 0 && grid[tile.GetX()][tile.GetY()-1].IsWall() && !grid[tile.GetX()][tile.GetY()-1].IsMarked()) {
			grid[tile.GetX()][tile.GetY()-1].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()][tile.GetY()-1]);
		}
		if ((tile.GetX() + 1) < cols && grid[tile.GetX()+1][tile.GetY()].IsWall() && !grid[tile.GetX()+1][tile.GetY()].IsMarked()) {
			grid[tile.GetX()+1][tile.GetY()].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()+1][tile.GetY()]);
		}
		if ((tile.GetX() - 1) >= 0 && grid[tile.GetX()-1][tile.GetY()].IsWall() && !grid[tile.GetX()-1][tile.GetY()].IsMarked()) {
			grid[tile.GetX()-1][tile.GetY()].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()-1][tile.GetY()]);
		}
		if ((tile.GetX() + 1) < cols && tile.GetY() + 1 < rows && grid[tile.GetX() + 1][tile.GetY() + 1].IsWall() && !grid[tile.GetX() + 1][tile.GetY() + 1].IsMarked()) {
			grid[tile.GetX()+1][tile.GetY()+1].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()+1][tile.GetY()+1]);
		}
		if ((tile.GetX() - 1) >= 0 && tile.GetY() - 1 >= 0 && grid[tile.GetX() - 1][tile.GetY() - 1].IsWall() && !grid[tile.GetX() - 1][tile.GetY() - 1].IsMarked()) {
			grid[tile.GetX()-1][tile.GetY()-1].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()-1][tile.GetY()-1]);
		}
		if ((tile.GetX() + 1) < cols && tile.GetY() - 1 >= 0 && grid[tile.GetX() + 1][tile.GetY() - 1].IsWall() && !grid[tile.GetX() + 1][tile.GetY() - 1].IsMarked()) {
			grid[tile.GetX()+1][tile.GetY()-1].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()+1][tile.GetY()-1]);
		}
		if ((tile.GetX() - 1) >= 0 && tile.GetY() + 1 < rows && grid[tile.GetX() - 1][tile.GetY() + 1].IsWall() && !grid[tile.GetX() - 1][tile.GetY() + 1].IsMarked()) {
			grid[tile.GetX()-1][tile.GetY()+1].SetIsMarked(true);
			allWallsList.Add(grid[tile.GetX()-1][tile.GetY()+1]);
		}
		for (int i = 0; i < allWallsList.Count; i++) {
			int count = 0;
			if((allWallsList[i].GetY() + 1) < rows && grid[allWallsList[i].GetX()][allWallsList[i].GetY() + 1].OpenForPlacement()){
				count++;
			}
			if ((allWallsList[i].GetY() - 1) >= 0 && grid[allWallsList[i].GetX()][allWallsList[i].GetY()-1].OpenForPlacement()) {
				count++;
			}
			if ((allWallsList[i].GetX() + 1) < cols && grid[allWallsList[i].GetX()+1][allWallsList[i].GetY()].OpenForPlacement()) {
				count++;
			}
			if ((allWallsList[i].GetX() - 1) >= 0 && grid[allWallsList[i].GetX()-1][allWallsList[i].GetY()].OpenForPlacement()) {
				count++;
			}
			if (count > 0) {
				validWallsList.Add(allWallsList[i]);
			}
		}
		return validWallsList;
	}

	private void BreakWall(GameTile wall)
	{
		//sets the wall sprite to water and apply data changes
		grid[wall.GetX()][wall.GetY()].SetIsDestroyed(false);
		grid[wall.GetX()][wall.GetY()].SetIsWall(false);
		grid[wall.GetX()][wall.GetY()].SetIsWalkAble(false);
		grid[wall.GetX()][wall.GetY()].SetIsOccupied(true);
		grid[wall.GetX()][wall.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite = waterSprite;
		//todo:Add color settings
	}

	private bool CheckForIsolatedWaterTile(GameTile tile)
	{
		bool WaterRight = (((tile.GetY() + 1) < rows) && (grid[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
		bool WaterLeft = (((tile.GetY() - 1) >= 0) && (grid[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
		bool WaterNorth = (((tile.GetX() + 1) < cols) && (grid[tile.GetX()+1][tile.GetY()].GetObject() != null) && (grid[tile.GetX()+1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
		bool WaterSouth = (((tile.GetX() - 1) >= 0) && (grid[tile.GetX()-1][tile.GetY()].GetObject() != null) && (grid[tile.GetX()-1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
		if (!WaterRight && !WaterLeft && !WaterNorth && !WaterSouth) {
			return true;
		}
		return false;
	}

	private void FindAndBreakDividerWall(GameTile tile)
	{
		bool WallRight = (((tile.GetY() + 1) < rows) && (grid[tile.GetX()][tile.GetY() + 1].IsWall())) ? true : false;
		bool WallLeft = (((tile.GetY() - 1) >= 0) && (grid[tile.GetX()][tile.GetY() - 1].IsWall())) ? true : false;
		bool WallNorth = (((tile.GetX() + 1) < cols) && (grid[tile.GetX()+1][tile.GetY()].IsWall())) ? true : false;
		bool WallSouth = (((tile.GetX() - 1) >= 0) && (grid[tile.GetX()-1][tile.GetY()].IsWall())) ? true : false;
		if (WallRight) {
			//don't check left as that would be the original tile
			tile = grid[tile.GetX()][tile.GetY() + 1];
			bool WaterRight = (((tile.GetY() + 1) < rows) && (grid[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterNorth = (((tile.GetX() + 1) < cols) && (grid[tile.GetX() + 1][tile.GetY()].GetObject() != null) && (grid[tile.GetX() + 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterSouth = (((tile.GetX() - 1) >= 0) && (grid[tile.GetX() - 1][tile.GetY()].GetObject() != null) && (grid[tile.GetX() - 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterRight || WaterNorth || WaterSouth) {
				BreakWall(tile);
				return;
			}
		}
		if (WallLeft) {
			//dont check right as that would be the original tile
			tile = grid[tile.GetX()][tile.GetY() - 1];
			bool WaterLeft = (((tile.GetY() - 1) >= 0) && (grid[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterNorth = (((tile.GetX() + 1) < cols) && (grid[tile.GetX() + 1][tile.GetY()].GetObject() != null) && (grid[tile.GetX() + 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterSouth = (((tile.GetX() - 1) >= 0) && (grid[tile.GetX() - 1][tile.GetY()].GetObject() != null) && (grid[tile.GetX() - 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterLeft || WaterNorth || WaterSouth) {
				BreakWall(tile);
				return;
			}
		}
		if (WallNorth) {
			//dont check south as this would be the original tile
			tile = grid[tile.GetX() + 1][tile.GetY()];
			bool WaterRight = (((tile.GetY() + 1) < rows) && (grid[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterLeft = (((tile.GetY() - 1) >= 0) && (grid[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterNorth = (((tile.GetX() + 1) < cols) && (grid[tile.GetX() + 1][tile.GetY()].GetObject() != null) && (grid[tile.GetX() + 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterLeft || WaterNorth || WaterRight) {
				BreakWall(tile);
				return;
			}
		}
		if (WallSouth) {
			//dont check north as this would be the original tile
			tile = grid[tile.GetX() - 1][tile.GetY()];
			bool WaterRight = (((tile.GetY() + 1) < rows) && (grid[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterLeft = (((tile.GetY() - 1) >= 0) && (grid[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (grid[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterSouth = (((tile.GetX() - 1) >= 0) && (grid[tile.GetX()-1][tile.GetY()].GetObject() != null) && (grid[tile.GetX()-1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterLeft || WaterSouth || WaterRight) {
				BreakWall(tile);
				return;
			}
		}
		Debug.Log("Couldn't find divider wall for water generation error!!");
		Debug.Log("Tile is: " + tile.GetX() + ", " + tile.GetY());
		return;
	}
}