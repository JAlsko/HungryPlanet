using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {

	public bool xRotation;
	public bool yRotation;
	public bool zRotation;
	public float speed;

	void Start () {
		
	}
	
	void Update () {
		if (xRotation) {
			transform.Rotate (Vector3.right * speed);
		} else if (yRotation) {
			transform.Rotate (Vector3.up * speed);
		} else if (zRotation) {
			transform.Rotate (Vector3.forward * speed);
		}
	}
}
