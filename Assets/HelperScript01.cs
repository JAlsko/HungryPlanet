using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperScript01 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Mouse0)) {
			GetComponent<DragToOrbit> ().enabled = true;
		} else {
			GetComponent<DragToOrbit> ().enabled = false;
		}
	}
}
