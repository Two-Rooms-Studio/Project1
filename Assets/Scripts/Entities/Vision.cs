using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision {

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

	public void UpdateVision(ref GameTile visionOriginTile, ref Board map)
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		int maxView = 4;
		List<List<Vector2>> results = new List<List<Vector2>>();
		List<Vector2> result = new List<Vector2>();

		//set all to not visible to start
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				map.GetGrid()[x][y].SetIsVisible(false);
			}
		}

		//cast lines to set create the original vision field
		for (int x = 0; x < cols; x++) {
			//cast lines to all the vertical edges of the map
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[x][0].GetPosition());
			results.Add(result);
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[x][(rows - 1)].GetPosition());
			results.Add(result);
		}
		for (int y = 0; y < rows; y++) {
			//cast lines to all the horizontal edges of the map
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[0][y].GetPosition());
			results.Add(result);
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[(cols-1)][y].GetPosition());
			results.Add(result);
		}

		//Walk down each result(ray) setting all tiles to visible and stopping once we have reached a wall
		for (int x = 0; x < results.Count; x++) {
			for (int y = 0; y < results[x].Count; y++) {
				if (map.GetGrid()[(int)((results[x][y]).x)][(int)((results[x][y]).y)].IsWall()) {
					map.GetGrid()[(int)results[x][y].x][(int)results[x][y].y].SetIsVisible(true);
					break; // if we hit a wall we can't see anything else on this current line so break.
				} else if (y+1 < results[x].Count && !(map.GetGrid()[(int)((results[x][y+1]).x)][(int)((results[x][y+1]).y)].IsWall()) && ((results[x][y].x < results[x][y+1].x || results[x][y].x > results[x][y+1].x) && (results[x][y].y < results[x][y+1].y || results[x][y].y > results[x][y+1].y))) { 
					//since our next tile and current tile are not walls and our x and y are both different at the same time we've assured
					//that there is some type of slope behavior so we have a chance of diagonal vision here and must correct to block that vision
					Vector2 currentPos = results[x][y];
					Vector2 nextPos = results[x][y+1]; 
					if (currentPos.x < nextPos.x) { //diagonal south of origin
						if (currentPos.y < nextPos.y) {//diagonal south east of origin
							if ((map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileEast() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileEast().IsWall()) && (map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileNorth() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileNorth().IsWall())) {
								map.GetGrid()[(int)currentPos.x][(int)currentPos.y].SetIsVisible(true);
								break;
							}
						} else if (currentPos.y > nextPos.y) { //diagonal south west of origin
							if ((map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileEast() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileEast().IsWall()) && (map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileSouth() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileSouth().IsWall())) {
								map.GetGrid()[(int)currentPos.x][(int)currentPos.y].SetIsVisible(true);
								break;
							}
						}
					} else if (currentPos.x > nextPos.x) {//diagonal north of origin
						if (currentPos.y < nextPos.y) {//diagonal north east
							if ((map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileWest() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileWest().IsWall()) && (map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileNorth() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileNorth().IsWall())) {
								map.GetGrid()[(int)currentPos.x][(int)currentPos.y].SetIsVisible(true);
								break;
							}
						} else if (currentPos.y > nextPos.y) {//diagonal north west of origin
							if ((map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileWest() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileWest().IsWall()) && (map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileSouth() == null || map.GetGrid()[(int)currentPos.x][(int)currentPos.y].GetTileSouth().IsWall())) {
								map.GetGrid()[(int)currentPos.x][(int)currentPos.y].SetIsVisible(true);
								break;
							}
						}
					}
				} else {
					map.GetGrid()[(int)results[x][y].x][(int)results[x][y].y].SetIsVisible(true); //floor space, set this to be visible

				}
			}
		}
		maxView++; // because of edge smoothing the triangle effect we actually need to boost the maxView in order to end up with the originally set max view

		//Limit vision to max view by walking through all visible tiles outside of the range limit and setting them to not visible if they are currently visible
		map.GetAllTilesInRange(maxView, visionOriginTile);
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsMarked())
					map.GetGrid()[x][y].SetIsVisible(false);
			}
		}
		//TODO: rewrite smoothing code and functionalize it to clean up this function abit
		//smooth out the diamond effect by removing the points
		if (visionOriginTile.GetX() + maxView < cols && map.GetGrid()[visionOriginTile.GetX() + maxView][visionOriginTile.GetY()].IsVisible()) {
			map.GetGrid()[visionOriginTile.GetX() + maxView][visionOriginTile.GetY()].SetIsVisible(false);
		}
		if (visionOriginTile.GetX() - maxView > 0 && map.GetGrid()[visionOriginTile.GetX() - maxView][visionOriginTile.GetY()].IsVisible()) {
			map.GetGrid()[visionOriginTile.GetX() - maxView][visionOriginTile.GetY()].SetIsVisible(false);
		}
		if (visionOriginTile.GetY() + maxView < rows && map.GetGrid()[visionOriginTile.GetX()][visionOriginTile.GetY() + maxView].IsVisible()) {
			map.GetGrid()[visionOriginTile.GetX()][visionOriginTile.GetY() + maxView].SetIsVisible(false);
		}
		if (visionOriginTile.GetY() - maxView > 0 && map.GetGrid()[visionOriginTile.GetX()][visionOriginTile.GetY() - maxView].IsVisible()) {
			map.GetGrid()[visionOriginTile.GetX()][visionOriginTile.GetY() - maxView].SetIsVisible(false);
		}
		//smooth out the diamond effect even more by removing the top row from the top and bottom
		//top middle
		if (visionOriginTile.GetY() + (maxView - 1) < rows) {
			map.GetGrid()[visionOriginTile.GetX()][visionOriginTile.GetY() + (maxView-1)].SetIsVisible(false);
		}
		//top left
		if ((visionOriginTile.GetY() + (maxView - 1) < rows) && ((visionOriginTile.GetX() - 1) > 0)) {
			map.GetGrid()[visionOriginTile.GetX() - 1][visionOriginTile.GetY() + (maxView - 1)].SetIsVisible(false);
		}
		//top right
		if (visionOriginTile.GetY() + (maxView - 1) < rows && visionOriginTile.GetX() + 1 < cols) {
			map.GetGrid()[visionOriginTile.GetX() + 1][visionOriginTile.GetY() + (maxView - 1)].SetIsVisible(false);
		}
		//bottom middle
		if (visionOriginTile.GetY() - (maxView - 1) > 0) {
			map.GetGrid()[visionOriginTile.GetX()][visionOriginTile.GetY() - (maxView-1)].SetIsVisible(false);
		}
		//bottom left
		if (visionOriginTile.GetY() - (maxView - 1) > 0 && visionOriginTile.GetX() - 1 > 0) {
			map.GetGrid()[visionOriginTile.GetX() - 1][visionOriginTile.GetY() - (maxView - 1)].SetIsVisible(false);
		}
		//bottom right
		if (visionOriginTile.GetY() - (maxView - 1) > 0 && visionOriginTile.GetX() + 1 < cols) {
			map.GetGrid()[visionOriginTile.GetX() + 1][visionOriginTile.GetY() - (maxView - 1)].SetIsVisible(false);
		}
	}

	//player only view processing
	public void PostProcessingForPlayerView(ref GameTile playerTile, ref Board map)
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		List<GameTile> visibleFloorTiles = new List<GameTile>();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y] != null && map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsWall()) {
					visibleFloorTiles.Add(map.GetGrid()[x][y]); //gather a list of all visible floor tiles
				}
			}
		}
		ApplyTileFixesBySection(ref playerTile, ref visibleFloorTiles);
		//Apply fixes to the player's visible tiles dependent on the area a visible tile falls into in regards to player location
		//This is used to fix wall not showing up when logically if we can see the floor we should also be able to see the wall behind it
		//this is a flaw of the raycasting system used to build the original list of visible tiles for the player

		EdgeFixes(ref map);
		UpdateTilesToReflectVisiblityForPlayer(ref playerTile, ref map);
		//Apply corrections to edge walls, making them visible if touching regular wall that the player can see.
	}

	private void UpdateTilesToReflectVisiblityForPlayer(ref GameTile playerTile, ref Board map)
	{
		int rows = map.GetRows();
		int cols = map.GetCols();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (map.GetGrid()[x][y].GetObject() != null && !map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsVisted()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.black;
				}
				if (map.GetGrid()[x][y].GetObject() != null && map.GetGrid()[x][y].IsVisible()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = map.GetGrid()[x][y].GetOriginalColor();
					map.GetGrid()[x][y].SetIsVisited(true);
				}
				if (map.GetGrid()[x][y].GetObject() != null && !map.GetGrid()[x][y].IsVisible() && map.GetGrid()[x][y].IsVisted()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.grey;
				}
			}
		}
	}

	private void ApplyTileFixesBySection(ref GameTile playerTile, ref List<GameTile> visibleTiles)
	{
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
					//on same horizontal line as the player
					if (visibleTiles[x].GetPosition().x > playerTile.GetPosition().x) {
						//TODO: if we are keeping this where it applies the same effect then remove the two inner if statements for both the horizontal and vertical, and then merge horizontal and vertical into one if check as well
						//on same horizontal line but to the east of the player, any visible floor tile in this section should illuminate any wall tiles in cardnial directions
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
					} else if (visibleTiles[x].GetPosition().x < playerTile.GetPosition().x) {
						//on same horizontal line as the player but to the west of the player, any visible floor tile in this section should illuminate any wall tiles  in cardnial directions
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
					}
				} else {
					//on same vertical line as the player
					if (visibleTiles[x].GetPosition().y > playerTile.GetPosition().y) {
						//on same vertical line as the player but north of the player, any visible floor tile in this section should illuminate any wall tiles in cardnial directions
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
					} else if (visibleTiles[x].GetPosition().y < playerTile.GetPosition().y) {
						//on same vertical line as the player but south of the player, any visible floor tile in this section should illuminate any wall tiles in cardnial directions
						if (hasNorthWallTile) northWallTile.SetIsVisible(true);
						if (hasWestWallTile) westWallTile.SetIsVisible(true);
						if (hasEastWallTile) eastWallTile.SetIsVisible(true);
						if (hasSouthWallTile) southWallTile.SetIsVisible(true);
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

	private void EdgeFixes(ref Board map)
	{
		List<GameTile> Edges = map.GetEdges();
		List<GameTile> visibleWallTiles = new List<GameTile>();
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
	//
}
