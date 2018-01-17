using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBoardSettings : ScriptableObject {
	[Header("Dungeon Map Settings")]
	[Tooltip("How deep the player currently is in the dungeon (starts on floor 1)")]
	public int floorLevel = 1;
	[Tooltip("Number of rows to generate in the maps grid")]
	public int rows = 5;
	[Tooltip("Number of columns to generate in the maps grid")]
	public int cols = 5;
	[Tooltip("Minimum Percentage of Tiles required to be open if we disallow disjointed caves")]
	[Range(0.0f , 0.5f)]
	public float minimumPercentageOfOpenTiles = 0.3f;
	[Tooltip("Chance for the tile to be a wall during inital generation")]
	public float chanceToStartAlive = 0.4f;
	[Tooltip("The number of a nonwall tiles needed around a wall to make the wall a nonwall tile")]
	public int deathLimit = 3;
	[Tooltip("The number of wall tiles needed around a nonwall tile to make the nonwall tile a wall")]
	public int birthLimit = 4;
	[Tooltip("The number of iterations for calculating if a tile should be a wall or nonwall tile")]
	public int numberOfSimulations = 5;
	[Tooltip("Object to use for all tiles of the dungeon")]
	public GameObject tileObject;
	[Tooltip("Object to use for the floor of the dungeon")]
	public Sprite floorSprite;
	[Tooltip("Sprite to use for walls in dungeon")]
	public Sprite wallSprite;
	[Tooltip("Sprite to use for walls the player cannon't see the beginning of")]
	public Sprite innerWallSprite;
	[Tooltip("Sprite to use for teleporters that take the player to unconnected caves")]
	public Sprite teleporterSprite;
	[Tooltip("Wheather to smooth map edges (remove walls surrounded by other walls)")]
	public bool runEdgeSmoothing;
	[Tooltip("Wheather to allow disconnected caves with teleporter connections")]
	public bool allowDisconnectedCaves;

	#if UNITY_EDITOR
	public static class DungeonBoardSettingsMenuItem
	{
		[UnityEditor.MenuItem("Tools/Create/BoardSettings/DungeonBoardSettings")]
		public static void CreateAsset()
		{
			string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
			string assetPath = path + "/BlankDungeonBoardSettings.asset";
			DungeonBoardSettings item = ScriptableObject.CreateInstance<DungeonBoardSettings> ();
			UnityEditor.ProjectWindowUtil.CreateAsset(item, assetPath);
		}
	}
	#endif
}