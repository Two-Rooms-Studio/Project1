using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : Entity {
	private GameTile playerTile;
	private Sprite playerSprite;
	private Color playerColor;
	private List<GameTile> Edges = new List<GameTile>(); //map edges used for player vision logic

	public void init (Vector2 spawnPoint, Sprite p_playerSprite, Color p_playerColor)
	{
		playerSprite = p_playerSprite;
		playerColor = p_playerColor;
		playerTile = map.GetGridTile((int)spawnPoint.x, (int)spawnPoint.y);
		playerTile.SetIsOccupied(true);
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerSprite;
		playerTile.GetObject().GetComponent<SpriteRenderer>().color = playerColor;
		playerTile.SetIsVisible(true);
		SetUpEdges();
		updateNewPlayerTile(playerTile);
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
		playerTile.SetIsVisible(true);
		TestSetAllToNotVisible();
		UpdateVision();
		TestChangeFloorColor();
		playerTile.GetObject().GetComponent<SpriteRenderer>().sprite = playerSprite;
	}

	private void SetUpEdges()
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if(map.GetGrid()[x][y].IsEdge()){
					Edges.Add(map.GetGrid()[x][y]);
				}
			}
		}
	}

	private void TestSetAllToNotVisible()
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				map.GetGrid()[x][y].SetIsVisible(false);
				if(map.GetGrid()[x][y].GetObject() != null) map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.grey;
			}
		}
	}

	private void UpdateVision()
	{
		//TODO: Figure out some type of class system or something to isolate this vision code away from the actual playerEntity
		//TODO: Generalize this code in order to let enemies use everything later on, this goes along with setting the vision code up in its own isolated area
		int rows = map.GetRows();
		int cols = map.GetCols();
		List<List<Vector2>> results = new List<List<Vector2>>();
		List<Vector2> result = new List<Vector2>();

		for (int x = 0; x < cols; x++) {
			//cast lines to all the vertical edges of the map
			result = BresenhamLine(playerTile.GetPosition(), map.GetGrid()[x][0].GetPosition());
			results.Add(result);
			result = BresenhamLine(playerTile.GetPosition(), map.GetGrid()[x][(rows-1)].GetPosition());
			results.Add(result);
		}

		for (int y = 0; y < rows; y++) {
			//cast lines to all the horizontal edges of the map
			result = BresenhamLine(playerTile.GetPosition(), map.GetGrid()[0][y].GetPosition());
			results.Add(result);
			result = BresenhamLine(playerTile.GetPosition(), map.GetGrid()[(cols-1)][y].GetPosition());
			results.Add(result);
		}
			
		for (int x = 0; x < results.Count; x++) {
			for (int y = 0; y < results[x].Count; y++) {
				if (map.GetGrid()[(int)((results[x][y]).x)][(int)((results[x][y]).y)].IsWall()) {
					map.GetGrid()[(int)results[x][y].x][(int)results[x][y].y].SetIsVisible(true);
					break; // if we hit a wall we can't see anything else on this current line so break.
				} else {
					map.GetGrid()[(int)results[x][y].x][(int)results[x][y].y].SetIsVisible(true); //floor space, set this to be visible
				}
			}
		}
		PostProcessing();
	}

	private void PostProcessing()
	{
		//TODO: Label the GameTiles for their sections instead of building list, iterate through the map once to apply lables, then iterate through once more to apply the appropriate fixes by checking their labels
		//TODO: Method away the null checks for the love of all that is holy
		int rows = map.GetRows();
		int cols = map.GetCols();
		List<GameTile> visibleFloorTiles = new List<GameTile>();
		List<GameTile> visibleWallTiles = new List<GameTile>();
		List<GameTile> northEastTiles = new List<GameTile>();
		List<GameTile> northWestTiles = new List<GameTile>();
		List<GameTile> southEastTiles = new List<GameTile>();
		List<GameTile> southWestTiles = new List<GameTile>();
		List<GameTile> northVerticalTiles = new List<GameTile>();
		List<GameTile> southVerticalTiles = new List<GameTile>();
		List<GameTile> eastHorizontalTiles = new List<GameTile>();
		List<GameTile> westHorizontalTiles = new List<GameTile>();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y] != null && map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsWall()) {
					visibleFloorTiles.Add(map.GetGrid()[x][y]); //gather a list of all visible floor tiles
				}
			}
		}
		//place tiles into seperate sections(NW,NE,SE,SW)
		SetUpTileSections(ref visibleFloorTiles, ref northEastTiles, ref northWestTiles, ref southEastTiles, ref southWestTiles, ref northVerticalTiles, ref southVerticalTiles, ref eastHorizontalTiles, ref westHorizontalTiles);
		//Apply needed corrections to tiles that fall to the northeast or northwest of the player
		NorthSectionFixes(ref northEastTiles, ref northWestTiles);
		//Apply needed corrections to tiles that fall to the southeast or southwest of the player
		SouthSectionFixes(ref southEastTiles, ref southWestTiles);
		//Apply needed corrections to tiles that fall on the same line as the player
		VerticalLineFixes(ref northVerticalTiles, ref southVerticalTiles);
		//Apply needed corrections to tiles that fall on the same horizontal line as the player
		HorizontalLineFixes(ref eastHorizontalTiles, ref westHorizontalTiles);
		//Apply needed corrections to "edge" walls (walls that only touch empty spaces or other walls)
		EdgeFixes(ref visibleWallTiles);
	}

	private void SetUpTileSections(ref List<GameTile> visibleTiles, ref List<GameTile> northEastTiles, ref List<GameTile> northWestTiles, ref List<GameTile> southEastTiles, ref List<GameTile> southWestTiles, ref List<GameTile> northVerticalTiles, ref List<GameTile> southVerticalTiles, ref List<GameTile> eastHorizontalTiles, ref List<GameTile> westHorizontalTiles)
	{
		for (int x = 0; x < visibleTiles.Count; x++) {
			if (visibleTiles[x].GetPosition().y == playerTile.GetPosition().y || visibleTiles[x].GetPosition().x == playerTile.GetPosition().x) {
				//these tiles lie directly on the same vertical or horizontal line the player is currently on, and therefore can belong to multiple tile sections
				if (visibleTiles[x].GetPosition().y == playerTile.GetPosition().y) {
					//on same horizontal line as the player
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//on same horizontal line but to the east of the player, belongs to eastHorizontalTiles
						eastHorizontalTiles.Add(visibleTiles[x]);
					} else if (visibleTiles[x].GetPosition().x < playerTile.GetPosition().x) {
						//on same horizontal line as the player but to the west of the player, belongs to westHorizontalTiles
						westHorizontalTiles.Add(visibleTiles[x]);
					}
				} else {
					//on same vertical line as the player
					if (visibleTiles[x].GetPosition().y > playerTile.GetPosition().y) {
						//on same vertical line as the player but north of the player, can belongs to northVerticalTiles
						northVerticalTiles.Add(visibleTiles[x]);
					} else if (visibleTiles[x].GetPosition().y < playerTile.GetPosition().y) {
						//on same vertical line as the player but south of the player, belongs to southVerticalTiles
						southVerticalTiles.Add(visibleTiles[x]);
					}
				}
			} else {
				if (visibleTiles[x].GetPosition().y > playerTile.GetPosition().y) {
					//North sections
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//north east
						northEastTiles.Add(visibleTiles[x]);
					} else {
						//north west
						northWestTiles.Add(visibleTiles[x]);
					}
				} else {
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//south east = green
						southEastTiles.Add(visibleTiles[x]);
					} else {
						//south west
						southWestTiles.Add(visibleTiles[x]);
					}
				}
			}
		}
	}

	private void NorthSectionFixes(ref List<GameTile> northEastTiles, ref List<GameTile> northWestTiles)
	{
		//Any visible floor in the north east section should also have a visible north wall tile and visible east wall tile:
		for(int x = 0; x < northEastTiles.Count; x++){
			if (northEastTiles[x].GetTileNorth() != null && northEastTiles[x].GetTileNorth().GetObject() != null && northEastTiles[x].GetTileNorth().IsWall()) {
				northEastTiles[x].GetTileNorth().SetIsVisible(true);
			}
			if (northEastTiles[x].GetTileEast() != null && northEastTiles[x].GetTileEast().GetObject() != null && northEastTiles[x].GetTileEast().IsWall()) {
				northEastTiles[x].GetTileEast().SetIsVisible(true);
			}
		}
		//Any visible floor in the north west section should also have a visible north wall tile and visible west wall tile:
		for(int x = 0; x < northWestTiles.Count; x++){
			if (northWestTiles[x].GetTileNorth() != null && northWestTiles[x].GetTileNorth().GetObject() != null && northWestTiles[x].GetTileNorth().IsWall()) {
				northWestTiles[x].GetTileNorth().SetIsVisible(true);
			}
			if (northWestTiles[x].GetTileWest() != null && northWestTiles[x].GetTileWest().GetObject() != null && northWestTiles[x].GetTileWest().IsWall()) {
				northWestTiles[x].GetTileWest().SetIsVisible(true);
			}
		}
	}

	private void SouthSectionFixes(ref List<GameTile> southEastTiles, ref List<GameTile> southWestTiles)
	{
		//Any visible floor in the south east section should also have a visible south wall tile and visible east wall tile
		for(int x = 0; x < southEastTiles.Count; x++){
			if (southEastTiles[x].GetTileSouth() != null && southEastTiles[x].GetTileSouth().GetObject() != null && southEastTiles[x].GetTileSouth().IsWall()) {
				southEastTiles[x].GetTileSouth().SetIsVisible(true);
			}
			if (southEastTiles[x].GetTileEast() != null && southEastTiles[x].GetTileEast().GetObject() != null && southEastTiles[x].GetTileEast().IsWall()) {
				southEastTiles[x].GetTileEast().SetIsVisible(true);
			}
		}
		//Any visible floor in the south west section should also have a visible south wall tile and visible west wall tile
		for(int x = 0; x < southWestTiles.Count; x++){
			if (southWestTiles[x].GetTileSouth() != null && southWestTiles[x].GetTileSouth().GetObject() != null && southWestTiles[x].GetTileSouth().IsWall()) {
				southWestTiles[x].GetTileSouth().SetIsVisible(true);
			}
			if (southWestTiles[x].GetTileWest() != null && southWestTiles[x].GetTileWest().GetObject() != null && southWestTiles[x].GetTileWest().IsWall()) {
				southWestTiles[x].GetTileWest().SetIsVisible(true);
			}
		}
	}

	private void VerticalLineFixes(ref List<GameTile> northVerticalTiles, ref List<GameTile> southVerticalTiles)
	{
		//Any visible floor north of the player and on the same vertical line should also have a visible south, east, or west wall tile
		for(int x = 0; x < northVerticalTiles.Count; x++){
			if (northVerticalTiles[x].GetTileSouth() != null && northVerticalTiles[x].GetTileSouth().GetObject() != null && northVerticalTiles[x].GetTileSouth().IsWall()) {
				northVerticalTiles[x].GetTileSouth().SetIsVisible(true);
			}
			if (northVerticalTiles[x].GetTileWest() != null && northVerticalTiles[x].GetTileWest().GetObject() != null && northVerticalTiles[x].GetTileWest().IsWall()) {
				northVerticalTiles[x].GetTileWest().SetIsVisible(true);
			}
			if (northVerticalTiles[x].GetTileEast() != null && northVerticalTiles[x].GetTileEast().GetObject() != null && northVerticalTiles[x].GetTileEast().IsWall()) {
				northVerticalTiles[x].GetTileEast().SetIsVisible(true);
			}
		}
		//Any visible floor south of the player and on the same vertical line should also have a visible north, east or west wall tile
		for(int x = 0; x < southVerticalTiles.Count; x++){
			if (southVerticalTiles[x].GetTileNorth() != null && southVerticalTiles[x].GetTileNorth().GetObject() != null && southVerticalTiles[x].GetTileNorth().IsWall()) {
				southVerticalTiles[x].GetTileNorth().SetIsVisible(true);
			}
			if (southVerticalTiles[x].GetTileWest() != null && southVerticalTiles[x].GetTileWest().GetObject() != null && southVerticalTiles[x].GetTileWest().IsWall()) {
				southVerticalTiles[x].GetTileWest().SetIsVisible(true);
			}
			if (southVerticalTiles[x].GetTileEast() != null && southVerticalTiles[x].GetTileEast().GetObject() != null && southVerticalTiles[x].GetTileEast().IsWall()) {
				southVerticalTiles[x].GetTileEast().SetIsVisible(true);
			}
		}
	}

	private void HorizontalLineFixes(ref List<GameTile> eastHorizontalTiles, ref List<GameTile> westHorizontalTiles)
	{
		//Any visible floor east of the player and on the same horizontal line should also have a visible north, south, or west wall tile
		for(int x = 0; x < eastHorizontalTiles.Count; x++){
			if (eastHorizontalTiles[x].GetTileNorth() != null && eastHorizontalTiles[x].GetTileNorth().GetObject() != null && eastHorizontalTiles[x].GetTileNorth().IsWall()) {
				eastHorizontalTiles[x].GetTileNorth().SetIsVisible(true);
			}
			if (eastHorizontalTiles[x].GetTileWest() != null && eastHorizontalTiles[x].GetTileWest().GetObject() != null && eastHorizontalTiles[x].GetTileWest().IsWall()) {
				eastHorizontalTiles[x].GetTileWest().SetIsVisible(true);
			}
			if (eastHorizontalTiles[x].GetTileSouth() != null && eastHorizontalTiles[x].GetTileSouth().GetObject() != null && eastHorizontalTiles[x].GetTileSouth().IsWall()) {
				eastHorizontalTiles[x].GetTileSouth().SetIsVisible(true);
			}
		}
		//Any visible floor west of the player and on the same horizontal line should also have a visible north, south, or east wall tile
		for(int x = 0; x < westHorizontalTiles.Count; x++){
			if (westHorizontalTiles[x].GetTileNorth() != null && westHorizontalTiles[x].GetTileNorth().GetObject() != null && westHorizontalTiles[x].GetTileNorth().IsWall()) {
				westHorizontalTiles[x].GetTileNorth().SetIsVisible(true);
			}
			if (westHorizontalTiles[x].GetTileEast() != null && westHorizontalTiles[x].GetTileEast().GetObject() != null && westHorizontalTiles[x].GetTileEast().IsWall()) {
				westHorizontalTiles[x].GetTileEast().SetIsVisible(true);
			}
			if (westHorizontalTiles[x].GetTileSouth() != null && westHorizontalTiles[x].GetTileSouth().GetObject() != null && westHorizontalTiles[x].GetTileSouth().IsWall()) {
				westHorizontalTiles[x].GetTileSouth().SetIsVisible(true);
			}
		}
	}

	private void EdgeFixes(ref List<GameTile> visibleWallTiles)
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		//Any edge tile connected to a visible wall should be visible, otherwise we'll have weird pop in edge walls e.e
		//gather a list of all visible wall tiles
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y] != null && map.GetGrid()[x][y].IsVisible() && map.GetGrid()[x][y].IsWall()) {
					visibleWallTiles.Add(map.GetGrid()[x][y]);
				}
			}
		}
		for(int x = 0; x < visibleWallTiles.Count; x++) {
			if (visibleWallTiles[x].GetTileNorth() != null) {
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileNorth() == Edges[i]) {
						visibleWallTiles[x].GetTileNorth().SetIsVisible(true);
					}
				}
			}
			if (visibleWallTiles[x].GetTileSouth() != null) {
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileSouth() == Edges[i]) {
						visibleWallTiles[x].GetTileSouth().SetIsVisible(true);
					}
				}
			}
			if (visibleWallTiles[x].GetTileEast() != null) {
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileEast() == Edges[i]) {
						visibleWallTiles[x].GetTileEast().SetIsVisible(true);
					}
				}
			}
			if (visibleWallTiles[x].GetTileWest() != null) {
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileWest() == Edges[i]) {
						visibleWallTiles[x].GetTileWest().SetIsVisible(true);
					}
				}
			}
		}
	}

	// Returns the list of points from p0 to p1 
	private List<Vector2> BresenhamLine(Vector2 p0, Vector2 p1) {
		return BresenhamLine((int)p0.x, (int)p0.y, (int)p1.x, (int)p1.y);
	}

	public List<Vector2> BresenhamLine(int x,int y,int x2, int y2) {
		//source: https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
		List<Vector2> results = new List<Vector2>();
		int w = x2 - x ;
		int h = y2 - y ;
		int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0 ;
		if (w<0) dx1 = -1 ; else if (w>0) dx1 = 1 ;
		if (h<0) dy1 = -1 ; else if (h>0) dy1 = 1 ;
		if (w<0) dx2 = -1 ; else if (w>0) dx2 = 1 ;
		int longest = (int)Mathf.Abs(w);
		int shortest = (int)Mathf.Abs(h);
		if (!(longest>shortest)) {
			longest = (int)Mathf.Abs(h);
			shortest = (int)Mathf.Abs(w);
			if (h<0) dy2 = -1 ; else if (h>0) dy2 = 1 ;
			dx2 = 0 ;            
		}
		int numerator = longest >> 1 ;
		for (int i=0;i<=longest;i++) {
			results.Add(new Vector2((float)x,(float)y));
			numerator += shortest ;
			if (!(numerator<longest)) {
				numerator -= longest ;
				x += dx1 ;
				y += dy1 ;
			} else {
				x += dx2 ;
				y += dy2 ;
			}
		}
		return results;
	}

	private void TestChangeFloorColor() 
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y].GetObject() != null && !map.GetGrid()[x][y].IsVisible()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.grey;
				}
				if (map.GetGrid()[x][y].GetObject() != null && map.GetGrid()[x][y].IsVisible()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.white;
				}
			}
		}
	}
}