using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class BuildSpot : MonoBehaviour {

	public bool hover = false;

	private Outline ol;
	private TileController tc;

	private Color buildColor;
	private Color upgradeColor;
	private Color destroyColor;

	void Start () {
		GameObject gameController = GameObject.FindWithTag ("GameController");
		ol = GetComponent<Outline> ();
		tc = gameController.GetComponent<TileController>();
		if (!ol) {
			gameObject.AddComponent<Outline> ();
			ol.color = 2;
			ol.enabled = false;
		}
		buildColor = gameController.GetComponent<TileController>().buildColor;
		upgradeColor = gameController.GetComponent<TileController>().upgradeColor;
		destroyColor = gameController.GetComponent<TileController>().destroyColor;
	}

	void OnMouseOver() {
		hover = true;
	}

	void OnMouseDown() {
		GameObject gameController = GameObject.FindWithTag ("GameController");
		if (UIHoverListener.Instance.isUIOverride) {
			return;
		} else {
			if (gameController.GetComponent<TileController>().building) {
				gameController.GetComponent<TileController>().FinalizeStoreBuild (transform.parent.GetSiblingIndex ());
			} else if (gameController.GetComponent<TileController>().upgrading) {
				gameController.GetComponent<UIController>().OpenUpgradeStoreOption (transform.parent.GetSiblingIndex ());
			} else if (gameController.GetComponent<TileController>().destroying) {
				gameController.GetComponent<UIController>().OpenStoreDestroyOption (transform.parent.GetSiblingIndex ());
			}
		}
		//Debug.Log ("Pressed build spot " + transform.parent.GetSiblingIndex ());
	}

	void OnMouseExit(){
		hover = false;
	}		

	void Update() {
		if (hover && Time.timeScale != 0) {
			if (tc.building) {
				tc.mainCam.GetComponent<OutlineEffect> ().lineColor1 = buildColor;
				ol.color = 1;
				ol.enabled = true;
			} else if (tc.destroying) {
				tc.mainCam.GetComponent<OutlineEffect> ().lineColor1 = destroyColor;
				ol.color = 1;
				ol.enabled = true;
			} else if (tc.upgrading) {
				tc.mainCam.GetComponent<OutlineEffect> ().lineColor1 = upgradeColor;
				ol.color = 1;
				ol.enabled = true;
			}
		} else {
			ol.enabled = false;
		}
		/*if (Input.GetKeyDown (KeyCode.E) && !mainCam.GetComponent<DragToOrbit> ().CheckIfTile (mainCam.GetComponent<DragToOrbit> ().target) && hover) {
			mainCam.GetComponent<DragToOrbit>().ChangeTarget(transform.GetChild(1));
		}

		GameObject.Find ("Main Camera").transform.LookAt (transform.GetChild (1));*/
	}
}
