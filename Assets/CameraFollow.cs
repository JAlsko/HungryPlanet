using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject toFollow;
	Vector3 offset;

	void Start () {
		offset = transform.position - toFollow.transform.position;
	}
	

	void FixedUpdate () {
		transform.position = toFollow.transform.position + offset;
	}
}
