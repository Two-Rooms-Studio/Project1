using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision {

	//publics
	public void UpdateVision(ref GameTile visionOriginTile, ref Board map, int maxView)
	{
		/*
		 * Called by any given entity to update what they are able to see at the start of their turn 
		*/

		List<List<Vector2>> results = new List<List<Vector2>>();

		map.SetAllTilesToNotVisible(); 
		//to start set all tiles to not visible
		results = CastVisionRaysToEdges(ref visionOriginTile, ref map); 
		//return a list containing all of the tiles passed over while casting vision rays to all the edges of the map
		SetUpVisibility(ref visionOriginTile, ref map, ref results); 
		//walk down each ray (within results) setting all floor tiles to visible stopping once we have reached a wall
		maxView++; //because of edge smoothing the triangle effect we actually need to boost the maxView in order to end up with the originally set max view
		LimitVisibilityToMaxView(ref visionOriginTile, ref map, maxView);
		//Limit vision to the max (diamond shaped at this point) range possible by walking through all visible tiles outside of the range and setting them to not visible
		CreateCircularVisibility(ref visionOriginTile, ref map, maxView); 
		//currently our algorithm for limiting visibility has created a diamond shape vision field, this turns the field into a circular field by trimming off some of the outtermost visible tiles
	}
		
	public void PostProcessingForPlayerView(ref GameTile playerTile, ref Board map)
	{
		/*
		 * The player entity runs UpdateVision once per turn just like every other entity in the game, but these functions apply specific graphical fixes to tweak the player's vision in ways we don't necessarly want applied to other entities.
		 * Mainly these effects are purely for beauty reasons and won't effect the actual important part of vision (floor tiles) at all therefore applying them to other entities would be wasteful.
		*/

		List<GameTile> visibleFloorTiles = new List<GameTile>();

		//gather a list of all visible floor tiles
		for (int x = 0; x < map.GetCols(); x++) {
			for (int y = 0; y < map.GetRows(); y++) {
				if (map.GetGrid()[x][y] != null && map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsWall()) {
					visibleFloorTiles.Add(map.GetGrid()[x][y]);
				}
			}
		}

		ApplyTileFixesBySection(ref playerTile, ref visibleFloorTiles);
		/*
		 * Apply fixes to the player's visible tiles dependent on the area a visible tile falls into in regards to player location
		 * This is used to fix wall not showing up when logically if we can see the floor we should also be able to see the wall behind it
		 * this is a flaw of the raycasting system used to build the original list of visible tiles for the player
		*/

		EdgeFixes(ref map);
		//Apply corrections to edge walls, making them visible if touching regular wall that the player can see.
		UpdateTilesToReflectVisiblityForPlayer(ref playerTile, ref map);
		//Sets the colors of tiles based off of the player's visibility, it is what essentially creates the graphical representation of the player's visibility on screen
	}
	//

	//privates
	private void ApplyTileFixesBySection(ref GameTile playerTile, ref List<GameTile> visibleTiles)
	{
		//TODO:Cleanup
		/*
		 * Looks at all current visible floor tiles and applys fixes to the player's vision dependent on which "area" the tiles are around the player
		*/

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
		
	private List<Vector2> BresenhamLine(Vector2 p0, Vector2 p1) 
	{
		/*
		 * Casts a ray from p0 to p1, and then returns a List of Vector2s which essentially represents all tiles that the ray has passed over before ending
		*/

		return BresenhamLine((int)p0.x, (int)p0.y, (int)p1.x, (int)p1.y);
	}

	private List<Vector2> BresenhamLine(int x,int y,int x2, int y2)
	{
		//TODO: Document
		//source: https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
		/*
		 * Casts a ray from (x,y) to (x2,y2) and then returns a List of Vector2s which essentially represents all tiles that the ray has passed over before ending
		*/

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

	private List<List<Vector2>> CastVisionRaysToEdges(ref GameTile visionOriginTile, ref Board map)
	{
		/*
		 * Cast lines to all the edges of the given map using Bresenham's Line algorithm to create the initial visibility for an entity
		*/

		List<Vector2> result = new List<Vector2>();
		List<List<Vector2>> results = new List<List<Vector2>>();

		//cast lines to all the vertical edges of the map
		for (int x = 0; x < map.GetCols(); x++) {
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[x][0].GetPosition());
			results.Add(result);
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[x][(map.GetRows() - 1)].GetPosition());
			results.Add(result);
		}
		//cast lines to all the horizontal edges of the map
		for (int y = 0; y < map.GetRows(); y++) {
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[0][y].GetPosition());
			results.Add(result);
			result = BresenhamLine(visionOriginTile.GetPosition(), map.GetGrid()[(map.GetCols() - 1)][y].GetPosition());
			results.Add(result);
		}
		return results;
	}

	private void CreateCircularVisibility(ref GameTile visionOriginTile, ref Board map, int maxView)
	{
		/* 
		 * Smooths out the diamond field of vision created by LimitVisibilityToMaxView() in order to give the entity a circular field of vision 
		*/

		//The X and Y point of the entities current tile
		int originXPoint = visionOriginTile.GetX();
		int originYPoint = visionOriginTile.GetY();

		//Each of these corresponds to a point (or edge if you prefer) of the diamond vision field
		int topPoint = visionOriginTile.GetX() + maxView;
		int rightPoint = visionOriginTile.GetY() + maxView;
		int botPoint = visionOriginTile.GetX() - maxView;
		int leftPoint = visionOriginTile.GetY() - maxView;

		//These bools are true if the corresponding points (or edges if you prefer) of the diamond are actually on the map grid
		bool hasTopPoint = topPoint < map.GetCols();
		bool hasBottomPoint = botPoint - maxView > 0; 
		bool hasRightPoint = rightPoint < map.GetRows(); 
		bool hasLeftPoint = leftPoint - maxView > 0; 

		//These bools are true if the corresponding points on the second to last top and bottom row of the diamond are actually on the map grid
		//For example these bools would correspond to points found on:
		//      *
		//    * * * <- this row
		//   * * * *
		//  * * * * *  
		//   * * * * 
		//    * * * <- and this row
		//      * 
		bool hasTopLeft = originYPoint + (maxView - 1) < map.GetRows() && originXPoint - 1 > 0;
		bool hasTopMiddle = originYPoint + (maxView - 1) < map.GetRows();
		bool hasTopRight = originYPoint + (maxView - 1) < map.GetRows() && originXPoint + 1 < map.GetCols();
		bool hasBotLeft = originYPoint - (maxView - 1) > 0 && originXPoint - 1 > 0;
		bool hasBotMid = originYPoint - (maxView - 1) > 0;
		bool hasBotRight = originYPoint - (maxView - 1) > 0 && originXPoint + 1 < map.GetCols();

		//remove the 4 extreme points (or edges) of the diamond
		if (hasTopPoint) map.GetGrid()[topPoint][originYPoint].SetIsVisible(false);
		if (hasBottomPoint) map.GetGrid()[botPoint][originYPoint].SetIsVisible(false);
		if (hasRightPoint)  map.GetGrid()[originXPoint][rightPoint].SetIsVisible(false);
		if (hasLeftPoint) map.GetGrid()[originXPoint][leftPoint].SetIsVisible(false);

		//smooth out the diamond effect even more by removing the second to last top and bottom row
		if (hasTopMiddle) map.GetGrid()[originXPoint][originYPoint + (maxView - 1)].SetIsVisible(false);
		if (hasTopLeft) map.GetGrid()[originXPoint - 1][originYPoint + (maxView - 1)].SetIsVisible(false);
		if (hasTopRight) map.GetGrid()[originXPoint + 1][originYPoint + (maxView - 1)].SetIsVisible(false);
		if (hasBotMid) map.GetGrid()[originXPoint][originYPoint - (maxView-1)].SetIsVisible(false);
		if (hasBotLeft) map.GetGrid()[originXPoint - 1][originYPoint - (maxView - 1)].SetIsVisible(false);
		if (hasBotRight) map.GetGrid()[originXPoint + 1][originYPoint - (maxView - 1)].SetIsVisible(false);
	}

	private void EdgeFixes(ref Board map)
	{
		/* 
		 * Turns visibility on for any edge wall connected a currently visible wall in order to prevent the player seeing sudden "pop-in" walls
		*/

		List<GameTile> visibleWallTiles = new List<GameTile>();

		//gather a list of all visible wall tiles
		for (int x = 0; x < map.GetCols(); x++) {
			for (int y = 0; y < map.GetRows(); y++) {
				if (map.GetGrid()[x][y] != null && map.GetGrid()[x][y].IsVisible() && map.GetGrid()[x][y].IsWall()) {
					visibleWallTiles.Add(map.GetGrid()[x][y]);
				}
			}
		}

		//walk through visible wall tiles and set edge walls connected to those visible walls to also be visible
		for(int x = 0; x < visibleWallTiles.Count; x++) {
			if (visibleWallTiles[x].GetTileNorth() != null && visibleWallTiles[x].GetTileNorth().IsEdge()) {
					visibleWallTiles[x].GetTileNorth().SetIsVisible(true);
			}
			if (visibleWallTiles[x].GetTileSouth() != null && visibleWallTiles[x].GetTileSouth().IsEdge()) {
				visibleWallTiles[x].GetTileSouth().SetIsVisible(true);
			}
			if (visibleWallTiles[x].GetTileEast() != null && visibleWallTiles[x].GetTileEast().IsEdge()) {
				visibleWallTiles[x].GetTileEast().SetIsVisible(true);
			}
			if (visibleWallTiles[x].GetTileWest() != null && visibleWallTiles[x].GetTileWest().IsEdge()) {
				visibleWallTiles[x].GetTileWest().SetIsVisible(true);
			}
		}
	}
		
	private void LimitVisibilityToMaxView(ref GameTile visionOriginTile, ref Board map, int maxView)
	{
		/*
		 * Limit vision to max view by walking through all visible tiles outside of the range limit and setting them to not visible if they are currently visible
		*/

		map.GetAllTilesInRange(ref visionOriginTile, maxView); 
		//grab all tiles within a particular range and mark them, the range corresponds to how many moves it would take the player entity to reach that tile, so a range of 1 would return the tiles in each of the cardnial directions
		for (int x = 0; x < map.GetCols(); x++) {
			for (int y = 0; y < map.GetRows(); y++) {
				if (map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsMarked())
					map.GetGrid()[x][y].SetIsVisible(false);
			}
		}
	}

	private void SetUpVisibility(ref GameTile visionOriginTile, ref Board map, ref List<List<Vector2>> results)
	{
		//TODO:Clean this up even further
		/*
		 * Sets the initial visibility of the entity by walking down each ray within results, setting all floor tiles to visible and stopping once we have reached a wall
		*/

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
	}

	private void UpdateTilesToReflectVisiblityForPlayer(ref GameTile playerTile, ref Board map)
	{
		/* 
		 * Set the colors of floor and wall tiles based off of the player's current visibility, this essentially creates the graphical representation of the player's visibility on screen
		*/

		for (int x = 0; x < map.GetCols(); x++) {
			for (int y = 0; y < map.GetRows(); y++) {
				if (map.GetGrid()[x][y].GetObject() != null && !map.GetGrid()[x][y].IsVisible() && !map.GetGrid()[x][y].IsVisted()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.black;
				}
				if (map.GetGrid()[x][y].GetObject() != null && map.GetGrid()[x][y].IsVisible()) {
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = map.GetGrid()[x][y].GetOriginalColor();
					map.GetGrid()[x][y].SetIsVisited(true);
				}
				if (map.GetGrid()[x][y].GetObject() != null && !map.GetGrid()[x][y].IsVisible() && map.GetGrid()[x][y].IsVisted()) {
					//map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = Color.grey;
					Color32 current = map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color;
					current.a = 75;
					map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = map.GetGrid()[x][y].GetObject().GetComponent<SpriteRenderer>().color = current;
				}
			}
		}
	}
	//
}
