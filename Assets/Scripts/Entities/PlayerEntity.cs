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
		//TODO: Iterate through the map once to check for section critera, then apply the appropriate fixes within that iteration instead of building lists
		//TODO: Method away the null checks for the love of all that is holy
		int rows = map.GetRows();
		int cols = map.GetCols();
		List<GameTile> visibleFloorTiles = new List<GameTile>();
		List<GameTile> playerVerticalLineTiles;
		List<GameTile> playerHorizontalLineTiles;
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y] != null && map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsWall()) {
					visibleFloorTiles.Add(map.GetGrid()[x][y]); //gather a list of all visible floor tiles
				}
			}
		}
		ApplyTileFixesBySection(ref visibleFloorTiles, out playerVerticalLineTiles, out playerHorizontalLineTiles);
		//Apply fixes to the player's visible tiles dependent on the area a visible tile falls into in regards to player location
		//This is used to fix wall not showing up when logically if we can see the floor we should also be able to see the wall behind it
		//this is a flaw of the raycasting system used to build the original list of visible tiles for the player

		//EdgeFixes(ref playerVerticalLineTiles, ref playerHorizontalLineTiles);
	}

	private void ApplyTileFixesBySection(ref List<GameTile> visibleTiles, out List<GameTile> playerVerticalLineTiles, out List<GameTile> playerHorizontalLineTiles)
	{
		playerVerticalLineTiles = new List<GameTile>();
		playerHorizontalLineTiles = new List<GameTile>();
		//Looks at all current visible floor tiles and applys fixes to the player's vision dependent on which "area" the tiles are around the player
		for (int x = 0; x < visibleTiles.Count; x++) {
			bool hasNorthWallTile = (visibleTiles[x].GetTileNorth() != null && visibleTiles[x].GetTileNorth().GetObject() != null && visibleTiles[x].GetTileNorth().IsWall());
			bool hasSouthWallTile = (visibleTiles[x].GetTileSouth() != null && visibleTiles[x].GetTileSouth().GetObject() != null && visibleTiles[x].GetTileSouth().IsWall());
			bool hasEastWallTile = (visibleTiles[x].GetTileEast() != null && visibleTiles[x].GetTileEast().GetObject() != null && visibleTiles[x].GetTileEast().IsWall());
			bool hasWestWallTile = (visibleTiles[x].GetTileWest() != null && visibleTiles[x].GetTileWest().GetObject() != null && visibleTiles[x].GetTileWest().IsWall());
			GameTile northWallTile = (hasNorthWallTile) ? visibleTiles[x].GetTileNorth() : null;
			GameTile southWallTile = (hasSouthWallTile) ? visibleTiles[x].GetTileSouth() : null;
			GameTile eastWallTile = (hasEastWallTile) ? visibleTiles[x].GetTileEast() : null;
			GameTile westWallTile = (hasWestWallTile) ? visibleTiles[x].GetTileWest() : null;

			if (visibleTiles[x].GetPosition().y == playerTile.GetPosition().y || visibleTiles[x].GetPosition().x == playerTile.GetPosition().x) {

				//these tiles lie directly on the same vertical or horizontal line the player is currently on
				if (visibleTiles[x].GetPosition().y == playerTile.GetPosition().y) {
					playerHorizontalLineTiles.Add(visibleTiles[x]);
					//on same horizontal line as the player
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//on same horizontal line but to the east of the player, any visible floor tile in this section should illuminate any wall tiles to the north, west, or south
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
					} else if (visibleTiles[x].GetPosition().x < playerTile.GetPosition().x) {
						//on same horizontal line as the player but to the west of the player, any visible floor tile in this section should illuminate any wall tiles to the north, east, or south
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
					}
				} else {
					//on same vertical line as the player
					playerVerticalLineTiles.Add(visibleTiles[x]);
					if (visibleTiles[x].GetPosition().y > playerTile.GetPosition().y) {
						//on same vertical line as the player but north of the player, any visible floor tile in this section should illuminate any wall tiles to the south, west, or east
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
					} else if (visibleTiles[x].GetPosition().y < playerTile.GetPosition().y) {
						//on same vertical line as the player but south of the player, any visible floor tile in this section should illuminate any wall tiles to north, west, or east
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
					}
				}
			
			} else {
				
				//these tiles do not like directly on the same vertical or horizontal line as the player
				if (visibleTiles[x].GetPosition().y > playerTile.GetPosition().y) {
					//North sections
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//north east
						//above the player's horizontal line and to the east of the player;s vertical line, any visible floor tile in this section should illuminate any wall tiles to the north or east
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
					} else {
						//north west
						//above the player's horizontal line and to the west of the player's vertical line, any visible floor tile in this section should illuminate any wall tiles to the north or west
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
					}
				} else {
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//south east
						//below the player's horizontal line and to the east of the player's vertical line, any visible floor tile in this section sould illuminate any wall tiles to the south or east
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
					} else {
						//south west
						//below the player's horizontal line and to the west of the player's vertical line, any visible floor tile in this section should illuminate any wall tiles to the south or west 
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
					}
				}
			}
		}
	}

	private void EdgeFixes(ref List<GameTile> playerVerticalLineTiles, ref List<GameTile> playerHorizontalLineTiles)
	{
		List<GameTile> visibleWallTiles = new List<GameTile>();
		bool InPlayerLineTiles = false;
		int rows = map.GetRows();
		int cols = map.GetCols();
		//Any edge tile connected to a visible wall should be visible, otherwise we'll have weird pop in edge walls e.e
		//we don't want to apply these fixes to any tile that is currently on the same vertical or horizontal line as the player
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
						for (int z = 0; z < playerVerticalLineTiles.Count; z++) {
							InPlayerLineTiles = false;
							if (playerVerticalLineTiles[z] == visibleWallTiles[x]) {
								InPlayerLineTiles = true;
								break;
							}
						}
						if (!InPlayerLineTiles) visibleWallTiles[x].GetTileNorth().SetIsVisible(true);
					}
				}
			}
			if (visibleWallTiles[x].GetTileSouth() != null) {
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileSouth() == Edges[i]) {
						for (int z = 0; z < playerVerticalLineTiles.Count; z++) {
							InPlayerLineTiles = false;
							if (playerVerticalLineTiles[z] == visibleWallTiles[x]) {
								InPlayerLineTiles = true;
								break;
							}
						}
						if (!InPlayerLineTiles) visibleWallTiles[x].GetTileSouth().SetIsVisible(true);
					}
				}
			}

			if (visibleWallTiles[x].GetTileEast() != null) {
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileEast() == Edges[i]) {
						for (int z = 0; z < playerHorizontalLineTiles.Count; z++) {
							InPlayerLineTiles = false;
							if (playerHorizontalLineTiles[z] == visibleWallTiles[x]) {
								InPlayerLineTiles = true;
								break;
							}
						}
						if (!InPlayerLineTiles)  visibleWallTiles[x].GetTileEast().SetIsVisible(true);
					}
				}
			}
			if (visibleWallTiles[x].GetTileWest() != null) {
				InPlayerLineTiles = false;
				for (int i = 0; i < Edges.Count; i++) {
					if (visibleWallTiles[x].GetTileWest() == Edges[i]) {
						for (int z = 0; z < playerHorizontalLineTiles.Count; z++) {
							InPlayerLineTiles = false;
							if (playerHorizontalLineTiles[z] == visibleWallTiles[x]) {
								InPlayerLineTiles = true;
								break;
							}
						}
						if (!InPlayerLineTiles) visibleWallTiles[x].GetTileWest().SetIsVisible(true);
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
		//TODO: Document, and fully understand it myself.....
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
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.black;
				}
				if (map.GetGrid()[x][y].GetObject() != null && map.GetGrid()[x][y].IsVisible()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.white;
				}
			}
		}
	}
}