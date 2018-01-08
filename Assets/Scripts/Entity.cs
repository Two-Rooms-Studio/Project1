using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Entity : ScriptableObject {
	public int health;
	public int moves;

	public virtual bool move(Sprite tileSprite, GameTile begin, GameTile end){
		return false;
	}
}
