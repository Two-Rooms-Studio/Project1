using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BoardController : MonoBehaviour {
	[SerializeField]

	[Header("Global Settings")]
	[Tooltip("Number of rows to generate in the maps grid")]
	public int rows = 5;
	[Tooltip("Number of columns to generate in the maps grid")]
	public int cols = 5;
	[Tooltip("The starting sprite of the player character")]
	public Sprite playerSprite;
	[Tooltip("The starting color of the sprite of the player character")]
	public Color playerColor;

	[Header("Dungeon Map Settings")]
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

	private DungeonBoard map;
	private Transform boardHolder;
	private PlayerEntity player;

	void Awake()
	{
		SetUpBoard();
		SpawnPlayer();
		gameObject.GetComponent<EntityController>().SetPlayer(player);
		gameObject.GetComponent<EntityController>().enabled = true;
	}

	void SetUpBoard()
	{
		map = ScriptableObject.CreateInstance<DungeonBoard>();
		map.init (rows, cols, floorObject, wallObject, innerWallSprite, teleporterSprite, chanceToStartAlive, deathLimit, birthLimit, numberOfSimulations);
	}

	void SpawnPlayer()
	{
		Vector2 spawnPoint;
		do {
		spawnPoint = new Vector2((int)Random.Range(0, cols-1), (int)Random.Range(0, rows-1));
		} while (!map.GetGridTile((int)spawnPoint.x, (int)spawnPoint.y).Open());
		player = ScriptableObject.CreateInstance<PlayerEntity>();
		player.init(spawnPoint, map, playerSprite, playerColor);
	}
}
