using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FallingText : MonoBehaviour {

	private Animator thisAnim;
	private Text thisText;
	public Animator startGlow;

	void Start () {
		thisAnim = GetComponent<Animator> ();
		thisText = GetComponent<Text> ();
	}
	
	void Update () {
		
	}

	public void OneShotText(string toShow) {
		if (thisText != null) {
			thisText.text = toShow;
			thisAnim.SetTrigger ("Fall");
			startGlow.SetTrigger ("Fire");
		}
	}
}
