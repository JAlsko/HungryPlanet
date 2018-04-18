using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundToggle : MonoBehaviour {

	public bool toggledOn = true;
	public string setting = "sfx";

	public Image icon;
	public Sprite toggleOnImg;
	public Sprite toggleOffImg;

	void Start () {

	}

	void Update () {

	}

	public void ToggleUIIcon() {
		if (toggledOn) {
			icon.sprite = toggleOnImg;
		} else {
			icon.sprite = toggleOffImg;
		}
	}

	public void SetToggle(bool to) {
		if (toggledOn != to) {
			ToggleSound ();
		}
	}

	public void ToggleSound() {
		GameObject gameController = GameObject.FindWithTag ("GameController");
		toggledOn = !toggledOn;
		if (setting == "music") {
			if (toggledOn) { gameController.GetComponent<SoundController>().unmuteMusic (); }
			else { gameController.GetComponent<SoundController>().muteMusic (); } 
		} else if (setting == "sfx") {
			if (toggledOn) { gameController.GetComponent<SoundController>().unmuteSFX (); }
			else { gameController.GetComponent<SoundController>().muteSFX (); }
		}
		ToggleUIIcon ();
	}
}
