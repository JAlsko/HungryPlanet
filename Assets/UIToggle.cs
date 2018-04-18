using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour {

	public bool toggledOn = true;
	public bool disableInMenus = true;

	public UIToggle siblingToggler;

	public GameObject toToggle;
	public Image icon;
	public Sprite toggleOnImg;
	public Sprite toggleOffImg;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ToggleUIIcon() {
		if (toggledOn) {
			icon.sprite = toggleOnImg;
		} else {
			icon.sprite = toggleOffImg;
		}
	}

	public void ToggleUI() {
		GameObject gameController = GameObject.FindWithTag ("GameController");
		if (disableInMenus && gameController.GetComponent<UIController>().menuOpen) {
			return;
		}

		if (siblingToggler != null) {
			siblingToggler.toggledOn = !siblingToggler.toggledOn;
			siblingToggler.ToggleUIIcon ();
		}

		if (toToggle != null) {
			toToggle.SetActive (!toggledOn);
		}

		toggledOn = !toggledOn;
		ToggleUIIcon ();
	}

	public void ToggleUIOff() {
		if (toggledOn) {
			ToggleUI ();
		}
	}
}
