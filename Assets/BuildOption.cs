using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildOption : MonoBehaviour {

	public int tier = 0;
	public float cost = 0;
	public Text title;
	public Text costText;
	public Image iconImage;

	void Start () {
		
	}


	void Update () {
		
	}

	public void UpdateName(string newTitle) {
		title.text = newTitle;
	}

	public void UpdateCost(float newCost) {
		cost = newCost;
		costText.text = "$" + newCost;
	}
}
