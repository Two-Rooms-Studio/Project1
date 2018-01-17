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
		gameObject.GetComponent<EntityController>().SetMap(map);
		gameObject.GetComponent<EntityController>().PlayerSetup();
	}

	void SetUpBoard()
	{
		map = ScriptableObject.CreateInstance<DungeonBoard>();
		map.init (Settings);
	}

	public void NextLevel()
	{
		Destroy(GameObject.Find(map.GetGridContainerName()));
		Settings.floorLevel += 1;
		Awake();
	}

	void OnApplicationQuit()
	{
		Settings.floorLevel = 1;
	}
}
