﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoardGrassGeneration : ScriptableObject {
	private DungeonBoard board;
	private Transform container;
	private Sprite[] grassSprites;
	public void GenerateGrass(DungeonBoard p_board, ref Transform p_container, Sprite[] p_grassSprites)
	{
		//driver for grass generation
		//use the board and container handed from the main generation settings
		board = p_board;
		container = p_container;
		grassSprites = p_grassSprites;
		float chanceDecreasePerStep = 0.2f;
		//
		List<GameTile> validLivingGrassTiles = GetTilesWithinRangeOfWater();
		if (validLivingGrassTiles.Count == 0) { //if there is no valid places to spawn living grass than end generation
			Debug.Log("No valid living grass tiles possible");
			return;
		}
		List<GameTile> roots = GetLivingGrassRootTiles(validLivingGrassTiles);
		int checkForGrassSpawn = 100;
		for(int i = 0; i < 5; i++)
		{
			roots = SpawnGrassAroundRootTiles(roots, checkForGrassSpawn);
			checkForGrassSpawn -= 20;
		}

	}

	private List<GameTile> GetTilesWithinRangeOfWater()
	{
		//return a list of floor tiles that are within 3 tiles of a waters edge, this means these tiles are valid for spawning live grass
		int validRangeFromWater = 3; //TODO: public facing
		board.UnMarkAllTiles();
		List<GameTile> waterTiles = new List<GameTile>();
		List<GameTile> edgeWaterTiles = new List<GameTile>();
		List<GameTile> validFloorTiles = new List<GameTile>();
		for (int x = 0; x < board.GetCols(); x++) {
			for (int y = 0; y < board.GetRows(); y++) {
				if (board.GetGrid()[x][y].GetObject() != null && board.GetGrid()[x][y].GetObject().GetComponent<WaterTile>() != null) {
					waterTiles.Add(board.GetGrid()[x][y]);
				}
			}
		}
		foreach (GameTile tile in waterTiles) {
			List<GameTile> possibleFloorTiles = board.GetTileCardinalNeighbours(tile);
			foreach(GameTile possibleFloortile in possibleFloorTiles)
			{
				if (possibleFloortile.OpenForPlacement()) {
					edgeWaterTiles.Add(possibleFloortile);
				}
			}
		}
		foreach (GameTile tile in edgeWaterTiles) {
			List<GameTile> possibleValidFloorTiles = board.GetAllTilesInRange(tile, validRangeFromWater);
			foreach (GameTile possibleValidFloorTile in possibleValidFloorTiles) {
				if (possibleValidFloorTile.OpenForPlacement()) {
					validFloorTiles.Add(possibleValidFloorTile);
				}
			}
		}
		board.UnMarkAllTiles();
		return validFloorTiles;
	}

	private List<GameTile> GetLivingGrassRootTiles(List<GameTile> validLivingGrassTiles)
	{
		//return a list of "root" tiles that will serve as the starting location to "grow" grass from
		//these roots are picked from tiles within a range of water that pass a dice check
		int rootCount = 0;
		int checkForRootSpawn = 1; // 1 percent chance for root to spawn
		List<GameTile> roots = new List<GameTile>();
		foreach (GameTile possibleRoot in validLivingGrassTiles) { //we cant use this, its going to favor left WAY WAY to much we need to grab a tile randomly in list
			int testForRootSpawn = Random.Range(1, 101);
			if (testForRootSpawn <= checkForRootSpawn && possibleRoot.OpenForPlacement()) {
				Debug.Log(testForRootSpawn);
				roots.Add(possibleRoot);
				possibleRoot.SetColor(Color.red); //test code to show root node
				rootCount++;
			}
		}
		return roots;
	}

	private List<GameTile> SpawnGrassAroundRootTiles(List<GameTile> roots, float checkForGrassSpawn)
	{
		//Changes the textures of tiles around the root node to create patches of grass
		List<GameTile> newGrassTiles = new List<GameTile>();
		foreach (GameTile root in roots) {
			List<GameTile> rootNeighbours = board.GetTileNeighbours(root);
			foreach (GameTile possibleGrassTiles in rootNeighbours) {
				float testForGrassSpawn = Random.Range(1, 101);
				if (testForGrassSpawn <= checkForGrassSpawn && possibleGrassTiles.OpenForPlacement()) {
					newGrassTiles.Add(possibleGrassTiles);
				}
			}
		}
		ChangeToGrassSprite(newGrassTiles);
		return newGrassTiles;
	}

	private void ChangeToGrassSprite(List<GameTile> newGrassTiles)
	{
		foreach (GameTile tile in newGrassTiles) {
			if (tile.GetColor() != Color.red) //test code to show root nodes
				tile.SetColor(Color.green);
		}
	}
}
