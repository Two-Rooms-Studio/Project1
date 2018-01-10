﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoardSettings : ScriptableObject {

	[Header("Dungeon Map Settings")]
	[Tooltip("Number of rows to generate in the maps grid")]
	public int rows = 5;
	[Tooltip("Number of columns to generate in the maps grid")]
	public int cols = 5;
	[Tooltip("Chance for the tile to be a wall during inital generation")]
	public float chanceToStartAlive = 0.4f;
	[Tooltip("The number of a nonwall tiles needed around a wall to make the wall a nonwall tile")]
	public int deathLimit = 3;
	[Tooltip("The number of wall tiles needed around a nonwall tile to make the nonwall tile a wall")]
	public int birthLimit = 4;
	[Tooltip("The number of iterations for calculating if a tile should be a wall or nonwall tile")]
	public int numberOfSimulations = 5;
	[Tooltip("Object to use for the wall of the dungeon")]
	public GameObject wallObject;
	[Tooltip("Object to use for the floor of the dungeon")]
	public GameObject floorObject;
	[Tooltip("Sprite to use for walls the player cannon't see the beginning of")]
	public Sprite innerWallSprite;
	[Tooltip("Sprite to use for teleporters that take the player to unconnected caves")]
	public Sprite teleporterSprite;
	[Tooltip("Wheather to smooth map edges (remove walls surrounded by other walls)")]
	public bool SmoothEdges;

	#if UNITY_EDITOR
	public static class DungeonBoardSettingsMenuItem
	{
		[UnityEditor.MenuItem("Tools/Create/BoardSettings/DungeonBoardSettings")]
		public static void CreateAsset()
		{
			var ex = ScriptableObject.CreateInstance<DungeonBoardSettings> ();
			UnityEditor.AssetDatabase.CreateAsset (ex, UnityEditor.AssetDatabase.GenerateUniqueAssetPath ("Assets/ScriptableObjects/BoardSettings/DungeonBoardSettings/BlankDungeonBoardSettings.asset"));
		}
	}
	#endif
}