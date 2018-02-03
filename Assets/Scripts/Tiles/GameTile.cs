using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile {
	//corresponds to where the tiles are on the grid script-wise, not in unity
	private int x=0; 
	private int y=0;
	//
	private Vector2 UnityPosition = new Vector2(); // corresponds to the tiles position within Unity, can be used to regenerate a tile that has its object deleted.
	private bool wall = false;
	private bool destroyed = false;
	private bool occupied = false;
	private bool marked = false; //used for floodfilling in order to fixed blocked in map sections
	private bool walkAble = true;
	private bool visible = false;
	private bool edge = false;
	private bool visited = false;
	private bool mapEdge = false; //if the tile is a border tile, i.e. if located at grid[49][x] or grid[x][49] in a 50 by 50 grid

	private Sprite originalSprite;
	private Color originalColor;
	private GameObject tileObject;

	private GameTile tileNorth = null;
	private GameTile tileSouth = null;
	private GameTile tileEast = null;
	private GameTile tileWest = null;

	//constructor
	public GameTile(int p_x, int p_y, Sprite sprite)
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

	public bool IsMapEdge()
	{
		return mapEdge;
	}

	public bool IsDestroyed()
	{
		return destroyed;
	}

	public bool IsWalkAble()
	{
		return walkAble;
	}

	public bool IsVisible()
	{
		return visible;
	}

	public bool IsEdge()
	{
		return edge;
	}

	public bool IsVisted()
	{
		return visited;
	}

	public bool IsWater()
	{
		if (GetObject().GetComponent<WaterTile>() != null) {
			return true;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return new Vector2(this.x, this.y);
	}

	public int GetX()
	{
		return x;
	}

	public int GetY()
	{
		return y;
	}

	public Color GetColor()
	{
		return GetObject().GetComponent<SpriteRenderer>().color;
	}

	public GameObject GetObject()
	{
		return tileObject;
	}

	public Sprite GetOriginalSprite()
	{
		return originalSprite;
	}

	public Color GetOriginalColor()
	{
		return originalColor;
	}

	public Sprite GetSprite()
	{
		return GetObject().GetComponent<SpriteRenderer>().sprite;
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

	public float GetUnityXPosition()
	{
		return UnityPosition.x;
	}

	public float GetUnityYPosition()
	{
		return UnityPosition.y;
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
		if (val) {
			SetIsOccupied(true);
			SetIsWalkAble(false);
		} else {
			SetIsOccupied(false);
			SetIsWalkAble(true);
		}
	}

	public void SetIsOccupied(bool val)
	{
		occupied = val;
	}

	public void SetIsMarked(bool val)
	{
		marked = val;
	}

	public void SetIsWalkAble(bool val)
	{
		walkAble = val;
	}

	public void SetIsVisible(bool val)
	{
		visible = val;
	}

	public void SetIsEdge(bool val)
	{
		edge = val;
	}

	public void SetIsMapEdge(bool val)
	{
		mapEdge = val;
	}

	public void SetIsVisited(bool val)
	{
		visited = val;
	}

	public void SetOriginalSprite(Sprite p_originalSprite)
	{
		originalSprite = p_originalSprite;
	}

	public void SetSprite(Sprite sprite)
	{
		GetObject().GetComponent<SpriteRenderer>().sprite = sprite;
	}
		
	public void SetOriginalColor(Color p_originalColor)
	{
		originalColor = p_originalColor;
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

	public void SetUnityPosition(float x, float y)
	{
		UnityPosition = new Vector2(x, y);
	}
//
	public bool OpenForPlacement()
	{
		//return true if the tile isn't a wall, occupied, or destoryed we can place an object on it
		if ((!IsOccupied() && !IsWall() && !IsDestroyed())) {
			return true;
		}
		return false;
	}
}
