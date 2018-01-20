using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : Entity {
	private GameObject playerObject;
	private GameTile playerTile;
	private Sprite playerSprite;
	private Color playerColor;

	public void init (GameTile spawnPoint)
	{
		playerObject = Resources.Load("Prefabs/PlayerPrefab") as GameObject; //TODO:use scriptable object settings instead of loading prefabs
		playerTile = spawnPoint;
		playerSprite = playerObject.GetComponent<SpriteRenderer>().sprite;
		playerColor = playerObject.GetComponent<SpriteRenderer>().color;
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerSprite;
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerColor;
		updateNewPlayerTile(playerTile);
	}

	public GameTile GetPlayerGameTile()
	{
		return playerTile;
	}

	public bool move(char move)
	{
		switch (move) {
			case 'n':
				if (playerTile.GetTileNorth() != null && playerTile.GetTileNorth().IsWalkAble()) {
					updateNewPlayerTile(playerTile.GetTileNorth());
					return true;
				}
				return false;
			case 's':
				if (playerTile.GetTileSouth() != null && playerTile.GetTileSouth().IsWalkAble()) {
					updateNewPlayerTile(playerTile.GetTileSouth());
					return true;
				}
				return false;
			case 'e':
				if (playerTile.GetTileEast() != null && playerTile.GetTileEast().IsWalkAble()) {
					updateNewPlayerTile(playerTile.GetTileEast());
					return true;
				}
				return false;
			case 'w':
				if (playerTile.GetTileWest() != null && playerTile.GetTileWest().IsWalkAble()) {
					updateNewPlayerTile(playerTile.GetTileWest());
					return true;
				}
				return false;
			default:
				Debug.Log("invalid command passed to move function");
				return false;
		}
	}

	public void updateNewPlayerTile(GameTile newPlayerTile)
	{
		playerTile.SetIsOccupied(false);
		playerTile.SetIsWalkAble(true);
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerTile.GetOriginalSprite();
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerTile.GetOriginalColor();
		playerTile = newPlayerTile;
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerSprite;
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerColor;
		playerTile.SetIsOccupied(true);
		playerTile.SetIsWalkAble(false);
		playerTile.SetIsVisible(true);
		vision.UpdateVision(ref playerTile, ref map, 5); //TODO:Make max view (5) publicly changeable
		vision.PostProcessingForPlayerView(ref playerTile, ref map);
	}
}