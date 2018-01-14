using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Entity : ScriptableObject {
	private float health;
	protected static Board map; 

	public virtual bool move(Sprite tileSprite, GameTile begin, GameTile end){
		return false;
	}

	public void SetMapForEntityUse(Board p_map)
	{
		map = p_map;
	}
}
