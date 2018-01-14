using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	private Transform target;
	public bool followAxisY;
//	public float width = 16f;
//	public float height = 9f;

	void Awake()
	{
		//Camera.main.fieldOfView = 20.0f;
		//Camera.main.aspect = width / height;

	}

	public void SetTarget(Transform p_target){
		target = p_target;
	}

	void Update () {
		if(followAxisY) transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
		else
			transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
	}
}