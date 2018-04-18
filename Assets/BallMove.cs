using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMove : MonoBehaviour {

	public float speed = 1;
	public float jumpForce = 100;
	Rigidbody rb;

	void Start () {
		rb = GetComponent<Rigidbody> ();
	}
	
	void Update () {
		if (Input.GetKey (KeyCode.W)) {
			rb.AddForce (Vector3.forward * speed);
		}
		if (Input.GetKey (KeyCode.S)) {
			rb.AddForce (Vector3.forward * -speed);
		}
		if (Input.GetKey (KeyCode.D)) {
			rb.AddForce (Vector3.right * speed);
		}
		if (Input.GetKey (KeyCode.A)) {
			rb.AddForce (Vector3.right * -speed);
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			rb.AddForce (Vector3.up * jumpForce);
		}
	}
}
