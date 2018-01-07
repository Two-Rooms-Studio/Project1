using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBoard : Board {
	new public void init (int p_rows, int p_cols, GameObject floorObject, GameObject wallObject)
	{
		//set up demo grass board
		base.init (p_rows, p_cols, floorObject, wallObject);
		//generates square grassland, original testing purposes
		Transform boardHolder = new GameObject ("GrasslandGrid").transform;
		List<GameTile> row = new List<GameTile>();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				GameTile tile = new GameTile((float)x, (float)y, floorSprite);
				GameObject instance = Instantiate (floorObject, new Vector3 ((float)x + xPadding, (float)y + yPadding, 0.0f), Quaternion.identity, boardHolder);
				instance.GetComponent<SpriteRenderer>().sprite = floorSprite;
				instance.name = "(" + x + "," + y + ")";
				tile.SetObject (instance);
				row.Add (tile);
				yPadding += 0.33f;
			}
			grid.Add (row);
			row = new List<GameTile>();
			yPadding = 0.0f;
			xPadding += 0.33f;
		}
	}
}
