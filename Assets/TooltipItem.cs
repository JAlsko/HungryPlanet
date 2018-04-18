using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public string itemType;
	public string food;
	public int bonusID = 0;
	private bool isSelected = false;

	void Start () {
		
	}
	
	void Update () {
		
	}

	public void OnPointerEnter(PointerEventData data) {
		Tooltip.Instance.UpdateTooltip (itemType, food, bonusID);
		Tooltip.Instance.ShowTooltip ();
		isSelected = true;
	}

	public void OnPointerExit(PointerEventData data) {
		Tooltip.Instance.HideTooltip ();
		isSelected = false;
	}

	public void OnDisable() {
		if (isSelected) {
			Tooltip.Instance.HideTooltip ();
			isSelected = false;
		}
	}
}
