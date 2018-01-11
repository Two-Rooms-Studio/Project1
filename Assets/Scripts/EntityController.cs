using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour {
	[Tooltip("The camera that follows the player throughout the map")]
	public Camera gameCamera;
	private PlayerEntity player;
	bool moveNorth = false;
	bool moveSouth = false;
	bool moveEast = false;
	bool moveWest = false;
	bool action = false;

	void Awake () {
		gameCamera.GetComponent<CameraFollow>().SetTarget(player.GetPlayerGameTile().GetObject().transform);
	}

	public void SetPlayer(PlayerEntity p_player){
		player = p_player;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
			moveNorth = true;
		} else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
			moveSouth = true;
		} else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
			moveEast = true;
		} else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
			moveWest = true;
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			action = true;
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (moveNorth) {
			if (player.move('n')) {
				gameCamera.GetComponent<CameraFollow>().SetTarget(player.GetPlayerGameTile().GetObject().transform);
			}
			moveNorth = false;
		} else if (moveSouth) {
			if (player.move('s')) {
				gameCamera.GetComponent<CameraFollow>().SetTarget(player.GetPlayerGameTile().GetObject().transform);
			}
			moveSouth = false;
		} else if (moveEast) {
			if (player.move('e')) {
				gameCamera.GetComponent<CameraFollow>().SetTarget(player.GetPlayerGameTile().GetObject().transform);
			}
			moveEast = false;
		} else if (moveWest) {
			if (player.move('w')) {
				gameCamera.GetComponent<CameraFollow>().SetTarget(player.GetPlayerGameTile().GetObject().transform);
			}
			moveWest = false;
		} else if (action) {
			if (player.GetPlayerGameTile().GetObject().GetComponent<TeleporterTile>() != null) {
				player.updateNewPlayerTile(player.GetPlayerGameTile().GetObject().GetComponent<TeleporterTile>().exitPoint);
				gameCamera.GetComponent<CameraFollow>().SetTarget(player.GetPlayerGameTile().GetObject().transform);
			}
			action = false;
		}
	}
}
