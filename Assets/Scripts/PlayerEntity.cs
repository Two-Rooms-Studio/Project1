using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : Entity {
	private GameTile playerTile;
	private Sprite playerSprite;
	private Color playerColor;

	public void init (Vector2 spawnPoint, Board map, Sprite p_playerSprite, Color p_playerColor)
	{
		playerSprite = p_playerSprite;
		playerColor = p_playerColor;
		playerTile = map.GetGridTile ((int)spawnPoint.x, (int)spawnPoint.y);
		playerTile.SetIsOccupied(true);
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerSprite;
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerColor;
	}

	public GameTile GetPlayerGameTile(){
		return playerTile;
	}

	public bool move(char move)
	{
		switch (move) {
			case 'n':
				if (playerTile.GetTileNorth() != null && playerTile.GetTileNorth().Open()) {
					updateNewPlayerTile(playerTile.GetTileNorth());
					return true;
				}
				return false;
			case 's':
				if (playerTile.GetTileSouth() != null && playerTile.GetTileSouth().Open()) {
					updateNewPlayerTile(playerTile.GetTileSouth());
					return true;
				}
				return false;
			case 'e':
				if (playerTile.GetTileEast() != null && playerTile.GetTileEast().Open()) {
					updateNewPlayerTile(playerTile.GetTileEast());
					return true;
				}
				return false;
			case 'w':
				if (playerTile.GetTileWest() != null && playerTile.GetTileWest().Open()) {
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
		playerTile.SetIsWalkAble(false);
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerTile.GetOriginalSprite();
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerTile.GetOriginalColor();
		playerTile = newPlayerTile;
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerSprite;
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerColor;
		playerTile.SetIsOccupied(true);
		playerTile.SetIsWalkAble(true);
	}
}
