using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoardWaterGeneration : ScriptableObject {
	private DungeonBoard board;
	private Transform container;
	private Sprite waterSprite;
	public void GenerateLiquid(DungeonBoard p_board, ref Transform p_container, Sprite p_waterSprite)
	{
		//generates liquid in internal areas of the map that have been destoryed, basically generates liquid in blank spaces within the map
		//use the board and container handed from the main generation settings
		board = p_board;
		container = p_container;
		waterSprite = p_waterSprite;
		//
		board.UnMarkAllTiles();
		List<List<GameTile>> FloodFilledAreas = new List<List<GameTile>>();
		List<GameTile> FloodFillStartLocations = new List<GameTile>();
		GameTile destroyedTile;
		bool containsMapEdge = false;
		do {
			Vector2 randomPoint = GetNextDestroyedUnMarkedPoint();
			destroyedTile = board.GetGrid()[(int)randomPoint.x][(int)randomPoint.y];
			board.GetGrid()[(int)randomPoint.x][(int)randomPoint.y].SetIsMarked(true);
			board.FloodFillDestroyedTiles(ref destroyedTile, ref FloodFilledAreas, ref containsMapEdge);
			if (!containsMapEdge) {
				FloodFillStartLocations.Add(board.GetGrid()[(int)randomPoint.x][(int)randomPoint.y]);
			}
		} while (HasUnmarkedDestroyedTiles());
		FloodFilledAreas.Clear();
		board.UnMarkAllTiles();
		for (int i = 0; i < FloodFillStartLocations.Count; i++) {
			destroyedTile = FloodFillStartLocations[i];
			destroyedTile.SetIsMarked(true);
			board.FloodFillDestroyedTiles(ref destroyedTile, ref FloodFilledAreas, ref containsMapEdge);
		}
		GenerateGameObjectsForLiquidTiles();
		BreakWallsRandomlyAroundWater(ref FloodFilledAreas);
		board.UnMarkAllTiles();
		FloodFilledAreas.Clear();
	}

	private Vector2 GetNextDestroyedUnMarkedPoint()
	{
		//return a destoryed unmarked point, helper function for Generate Liquid, should only be called when HasUnMarkedDestoryedTiles() has ensured a return
		for (int x = 0; x < board.GetCols(); x++) {
			for (int y = 0; y < board.GetRows(); y++) {
				if ((!board.GetGrid()[x][y].IsMarked()) && (board.GetGrid()[x][y].IsDestroyed())) {
					board.GetGrid()[x][y].SetIsMarked(true);
					return new Vector2 (board.GetGrid()[x][y].GetX(), board.GetGrid()[x][y].GetY());
				}
			}
		}
		Debug.Log("No tile was found MAJOR ERROR AT WATER GENERATION 55");
		return new Vector2(-99.0f, -99.0f); //error value will cause out of bound exception
	}

	private bool HasUnmarkedDestroyedTiles()
	{
		//return true if the board has any destroyed tiles that aren't marked that could be marked
		for (int x = 0; x < board.GetCols(); x++) {
			for (int y = 0; y < board.GetRows(); y++) {
				if (!board.GetGrid()[x][y].IsMarked() && board.GetGrid()[x][y].IsDestroyed()) {
					return true;
				}
			}
		}
		return false;
	}

	private void GenerateGameObjectsForLiquidTiles()
	{
		//Now that we have found the tiles that we want to turn into water, we need to regenerate game objects for them and make them water
		for (int x = 0; x < board.GetCols(); x++) {
			for (int y = 0; y < board.GetRows(); y++) {
				if (board.GetGrid()[x][y].IsMarked()) {
					board.GetGrid()[x][y].SetIsDestroyed(false);
					board.GetGrid()[x][y].SetIsWall(false);
					board.GetGrid()[x][y].SetIsMarked(false);
					board.GetGrid()[x][y].SetIsWalkAble(false);
					board.GetGrid()[x][y].SetIsOccupied(true);
					GameObject instance = Instantiate (board.GetTileObject(), new Vector3 (board.GetGrid()[x][y].GetUnityXPosition(), board.GetGrid()[x][y].GetUnityYPosition(), 0.0f), Quaternion.identity, container);
					instance.name = "(" + x + "," + y + ")";
					instance.GetComponent<SpriteRenderer>().sprite = waterSprite;
					board.GetGrid()[x][y].SetObject(instance);
					//todo:add color settings
				}
			}
		}
	}

	private void BreakWallsRandomlyAroundWater(ref List<List<GameTile>> waterTilesSeperatedByAreas)
	{
		//grab the valid walls around our water areas (walls that are touching floor tiles) and break them (turn them into water tiles)
		int divisor = 4; // What fraction of walls should we ensured are punched out around the water? if divisor = 3 then 1/3 (rounded down) of all walls around a water area will be removed
		float chanceForOtherWallBreaks = 0.10f; // after the ensured walls every other wall has this chance of breaking, for example if set to .45f every other wall would have a 45% chance of breaking
		float chanceForWaterExpansion = 0.75f; // the chance for water to "expand" outwards for the edge of a pool of water
		board.UnMarkAllTiles();
		List<List<GameTile>> wallsAroundWaterTilesSeperatedByAreas = GetWallsSurroundingWaterAreas(ref waterTilesSeperatedByAreas);
		List<GameTile> waterEdgeTiles = new List<GameTile>();

		for (int i = 0; i < wallsAroundWaterTilesSeperatedByAreas.Count; i++) {
			int ensuredWallBreaks = wallsAroundWaterTilesSeperatedByAreas[i].Count / divisor;
			ensuredWallBreaks = (ensuredWallBreaks <= 0) ? 1 : ensuredWallBreaks; //atleast always destory one wall around a water area
			//walk down all of the walls for a given area, selecting a wall at random and breaking it, repeat a number of times equal to our ensuredWallBreaks
			for (int c = 0; c < ensuredWallBreaks; c++) {
				int randomWall = (int)(Random.Range(0.0f, (float)wallsAroundWaterTilesSeperatedByAreas[i].Count - 1));
				BreakWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall], ref waterEdgeTiles);
				if (CheckForIsolatedWaterTile(wallsAroundWaterTilesSeperatedByAreas[i][randomWall])) {
					GameTile tile = FindDividerWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall]);
					BreakWall(tile, ref waterEdgeTiles);
					wallsAroundWaterTilesSeperatedByAreas[i].Remove(board.GetGrid()[tile.GetX()][tile.GetY()]);
				}
				wallsAroundWaterTilesSeperatedByAreas[i].RemoveAt(randomWall);
			}
			int originalSize = wallsAroundWaterTilesSeperatedByAreas[i].Count;
			//walk down all of the walls for a given area, attempting to break each wall, the attempt success rate is based off of our chanceForOtherWallBreaks value
			for (int c = 0; c < originalSize; c++) {
				float breakWallCheck = Random.Range(0.0f, 1.0f);
				if ((breakWallCheck <= chanceForOtherWallBreaks) && (wallsAroundWaterTilesSeperatedByAreas[i].Count > 0)) {
					int randomWall = (int)(Random.Range(0.0f, (float)wallsAroundWaterTilesSeperatedByAreas[i].Count - 1));
					BreakWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall], ref waterEdgeTiles);
					if (CheckForIsolatedWaterTile(wallsAroundWaterTilesSeperatedByAreas[i][randomWall])) {
						GameTile tile = FindDividerWall(wallsAroundWaterTilesSeperatedByAreas[i][randomWall]);
						BreakWall(tile, ref waterEdgeTiles);
						wallsAroundWaterTilesSeperatedByAreas[i].Remove(board.GetGrid()[tile.GetX()][tile.GetY()]);
					}
					wallsAroundWaterTilesSeperatedByAreas[i].RemoveAt(randomWall);
				}
			}
		}
		for (int i = 0; i < waterEdgeTiles.Count; i++) {
			//walk down the edges of all the water areas and attempt to allow the water to expand outward, the attempt sucess rate is based off of our chanceForWaterExpansion value
			float ExpandWaterCheck = Random.Range(0.0f, 1.0f);
			if (ExpandWaterCheck <= chanceForWaterExpansion) {
				List<GameTile> floorTiles = GetFloorTilesAroundTile(waterEdgeTiles[i]);
				if (floorTiles.Count != 0) {
					for (int j = 0; j < floorTiles.Count; j++) {
						AttemptWaterExpansion(floorTiles[j]);
					}
				}
			}
		}
		board.UnMarkAllTiles();
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
		tile.SetIsMarked(true);
		List<GameTile> tileNeighbours = board.GetTileNeighbours(tile);
		foreach(GameTile q in tileNeighbours)
		{
			if (q.IsWall() && !q.IsMarked()) {
				q.SetIsMarked(true);
				allWallsList.Add(q);
			}
			
		}
		foreach(GameTile q in allWallsList) {
			List<GameTile> cardinalNeighbours = board.GetTileCardinalNeighbours(q);
			foreach (GameTile neighbour in cardinalNeighbours) {
				if (neighbour.OpenForPlacement()) {
					validWallsList.Add(q);
					break;
				}
			}
		}
		return validWallsList;
	}

	private void BreakWall(GameTile wall, ref List<GameTile> waterEdgeTiles)
	{
		//sets the wall sprite to water and apply data changes
		wall.SetIsDestroyed(false);
		wall.SetIsWall(false);
		wall.SetIsWalkAble(false);
		wall.SetIsOccupied(true);
		wall.SetSprite(waterSprite);
		waterEdgeTiles.Add(wall);
		//todo:Add color settings
	}

	private bool CheckForIsolatedWaterTile(GameTile tile)
	{
		List<GameTile> neighbours = board.GetTileCardinalNeighbours(tile);
		foreach (GameTile q in neighbours) {
			if (q.GetSprite() == waterSprite)
				return false;
		}
		return true;
	}

	private GameTile FindDividerWall(GameTile tile)
	{
		bool WallRight = (((tile.GetY() + 1) < board.GetRows()) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].IsWall())) ? true : false;
		bool WallLeft = (((tile.GetY() - 1) >= 0) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].IsWall())) ? true : false;
		bool WallNorth = (((tile.GetX() + 1) < board.GetCols()) && (board.GetGrid()[tile.GetX()+1][tile.GetY()].IsWall())) ? true : false;
		bool WallSouth = (((tile.GetX() - 1) >= 0) && (board.GetGrid()[tile.GetX()-1][tile.GetY()].IsWall())) ? true : false;
		if (WallRight) {
			//don't check left as that would be the original tile
			tile = board.GetGrid()[tile.GetX()][tile.GetY() + 1];
			bool WaterRight = (((tile.GetY() + 1) < board.GetRows()) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterNorth = (((tile.GetX() + 1) < board.GetCols()) && (board.GetGrid()[tile.GetX() + 1][tile.GetY()].GetObject() != null) && (board.GetGrid()[tile.GetX() + 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterSouth = (((tile.GetX() - 1) >= 0) && (board.GetGrid()[tile.GetX() - 1][tile.GetY()].GetObject() != null) && (board.GetGrid()[tile.GetX() - 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterRight || WaterNorth || WaterSouth) {
				return tile;
			}
		}
		if (WallLeft) {
			//dont check right as that would be the original tile
			tile = board.GetGrid()[tile.GetX()][tile.GetY() - 1];
			bool WaterLeft = (((tile.GetY() - 1) >= 0) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterNorth = (((tile.GetX() + 1) < board.GetCols()) && (board.GetGrid()[tile.GetX() + 1][tile.GetY()].GetObject() != null) && (board.GetGrid()[tile.GetX() + 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterSouth = (((tile.GetX() - 1) >= 0) && (board.GetGrid()[tile.GetX() - 1][tile.GetY()].GetObject() != null) && (board.GetGrid()[tile.GetX() - 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterLeft || WaterNorth || WaterSouth) {
				return tile;
			}
		}
		if (WallNorth) {
			//dont check south as this would be the original tile
			tile = board.GetGrid()[tile.GetX() + 1][tile.GetY()];
			bool WaterRight = (((tile.GetY() + 1) < board.GetRows()) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterLeft = (((tile.GetY() - 1) >= 0) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterNorth = (((tile.GetX() + 1) < board.GetCols()) && (board.GetGrid()[tile.GetX() + 1][tile.GetY()].GetObject() != null) && (board.GetGrid()[tile.GetX() + 1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterLeft || WaterNorth || WaterRight) {
				return tile;
			}
		}
		if (WallSouth) {
			//dont check north as this would be the original tile
			tile = board.GetGrid()[tile.GetX() - 1][tile.GetY()];
			bool WaterRight = (((tile.GetY() + 1) < board.GetRows()) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].GetObject() != null) && (board.GetGrid()[tile.GetX()][tile.GetY() + 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterLeft = (((tile.GetY() - 1) >= 0) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].GetObject() != null) && (board.GetGrid()[tile.GetX()][tile.GetY() - 1].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			bool WaterSouth = (((tile.GetX() - 1) >= 0) && (board.GetGrid()[tile.GetX()-1][tile.GetY()].GetObject() != null) && (board.GetGrid()[tile.GetX()-1][tile.GetY()].GetObject().GetComponent<SpriteRenderer>().sprite == waterSprite)) ? true : false;
			if (WaterLeft || WaterSouth || WaterRight) {
				return tile;
			}
		}
		Debug.Log("Couldn't find divider wall for water generation error!!");
		Debug.Log("Tile is: " + tile.GetX() + ", " + tile.GetY());
		return tile;
	}

	private List<GameTile> GetFloorTilesAroundTile(GameTile tile)
	{
		//water expansion helper function get the floor tiles around a tile in the cardinal directions
		List<GameTile> neighbours = board.GetTileCardinalNeighbours(tile);
		List<GameTile> validNeighbours = new List<GameTile>();
		foreach (GameTile q in neighbours) {
			if (q.OpenForPlacement())
				validNeighbours.Add(q);
		}
		return validNeighbours;
	}

	private void AttemptWaterExpansion(GameTile floorTile)
	{
		int count = countWaterNeighbours(floorTile);
		if (count != 1) 
			return;
		floorTile.SetIsDestroyed(false);
		floorTile.SetIsWall(false);
		floorTile.SetIsWalkAble(false);
		floorTile.SetIsOccupied(true);
		floorTile.SetSprite(waterSprite);
	}

	private int countWaterNeighbours(GameTile tile)
	{
		//atempt water expansion helper count water tiles in cardinal directions
		int count = 0;
		List<GameTile> neighbours = board.GetTileCardinalNeighbours(tile);
		foreach (GameTile q in neighbours) {
			if (q.GetSprite() == waterSprite) {
				count++;
			}
		}
		return count;
	}
}
