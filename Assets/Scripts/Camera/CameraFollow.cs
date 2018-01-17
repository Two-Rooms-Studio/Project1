using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	private Transform target;
	public bool followAxisY;

	public void SetTarget(Transform p_target){
		target = p_target;
	}

	void Update () {
		if (target != null) {
			if (followAxisY) {
				transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
			} else {
				transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
			}
		}
	}
}


