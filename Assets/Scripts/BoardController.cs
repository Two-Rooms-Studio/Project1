using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {
	[SerializeField]

	[Header("Global Settings")]
	[Tooltip("The starting sprite of the player character")]
	public Sprite playerSprite;
	[Tooltip("The starting color of the sprite of the player character")]
	public Color playerColor;

	[Header("Dungeon Map Settings")]
	[Tooltip("Scriptable Object which holds all settings for generating Dungeon Boards")]
	public DungeonBoardSettings Settings;

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
		map.init (Settings);
	}

	void SpawnPlayer()
	{
		Vector2 spawnPoint;
		do {
		spawnPoint = new Vector2((int)Random.Range(0, Settings.cols-1), (int)Random.Range(0, Settings.rows-1));
		} while (!map.GetGridTile((int)spawnPoint.x, (int)spawnPoint.y).Open());
		player = ScriptableObject.CreateInstance<PlayerEntity>();
		player.init(spawnPoint, map, playerSprite, playerColor);
	}
}
