using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile {
	private float x=0.0f;
	private float y=0.0f;
	private bool wall = false;
	private bool destroyed = false;
	private bool occupied = false;
	private bool marked = false; //used for floodfilling in order to fixed blocked in map sections
	private Sprite originalSprite;
	private GameObject tileObject;
	private GameTile tileNorth = null;
	private GameTile tileSouth = null;
	private GameTile tileEast = null;
	private GameTile tileWest = null;

	//constructor
	public GameTile(float p_x, float p_y, Sprite sprite)
	{
		this.x = p_x;
		this.y = p_y;
		this.originalSprite = sprite;
	}

//getters
	public bool IsWall()
	{
		return wall;
	}

	public bool IsOccupied()
	{
		return occupied;
	}

	public bool IsMarked()
	{
		return marked;
	}

	public bool IsDestroyed()
	{
		return destroyed;
	}

	public Vector2 GetPosition()
	{
		return new Vector2(this.x, this.y);
	}

	public GameObject GetObject()
	{
		return tileObject;
	}

	public Sprite GetOriginalSprite()
	{
		return originalSprite;
	}

	public GameTile GetTileNorth(){
		return tileNorth;
	}

	public GameTile GetTileSouth(){
		return tileSouth;
	}

	public GameTile GetTileEast(){
		return tileEast;
	}

	public GameTile GetTileWest(){
		return tileWest;
	}
//

//setters
	public void SetObject(GameObject tile)
	{
		tileObject = tile;
	}

	public void SetIsDestroyed(bool val)
	{
		destroyed = val;
	}

	public void SetIsWall(bool val)
	{
		wall = val;
	}

	public void SetIsOccupied(bool val)
	{
		occupied = val;
	}

	public void SetIsMarked(bool val)
	{
		marked = val;
	}

	public void SetOriginalSprite(Sprite p_originalSprite)
	{
		originalSprite = p_originalSprite;
	}
		
	public void SetTileNorth(GameTile tile){
		tileNorth = tile;
	}

	public void SetTileSouth(GameTile tile){
		tileSouth = tile;
	}

	public void SetTileEast(GameTile tile){
		tileEast = tile;
	}

	public void SetTileWest(GameTile tile){
		tileWest = tile;
	}
//
	public bool Open()
	{
		//return true if the tile isn't a wall or occupied, in otherwords the player can move onto this tile
		if (!IsOccupied() && !IsWall() && !IsDestroyed()) {
			return true;
		}
		return false;
	}
}
