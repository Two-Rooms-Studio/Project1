using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterTile : MonoBehaviour {
	[Tooltip("The point the teleporter will warp the player to")]
	public GameTile exitPoint;
	public GameObject exitPointObject;
}

public class ExitTile : MonoBehaviour {

}
