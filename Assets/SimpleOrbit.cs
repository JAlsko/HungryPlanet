using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleOrbit : MonoBehaviour {

	public GameObject target;
	public float speed = 20;
	public bool randomAxis = true;
	Vector3 thisAxis;

	void Start () {
		if (target == null) {
			target = GameObject.Find ("Hexsphere");
		}
		if (randomAxis) {
			thisAxis = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f), Random.Range (-1f, 1f));
		} else {
			thisAxis = Vector3.up;
		}
	}

	void Update () {
		transform.RotateAround (target.transform.position, thisAxis, speed * Time.deltaTime);
	}
}
