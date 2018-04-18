using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class Tooltip : MonoBehaviour {

	private static Tooltip instance = null;
	public Canvas parentCanvas;
	public Vector2 offset;

	public Image icon;
	public Text name;
	public Text desc;
	public Text effects;
	public Text special;

	public GameObject gameController;

	private GameObject tooltipPanel;

	public static Tooltip Instance {
		get {
			return instance;
		}
	}

	public void Start()
	{
		tooltipPanel = transform.Find ("TooltipPanel").gameObject;
		gameController = GameObject.FindWithTag ("GameController");
		instance = this;
		Vector2 pos;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			parentCanvas.transform as RectTransform, Input.mousePosition,
			parentCanvas.worldCamera,
			out pos);
	}

	public void Update()
	{
		Vector2 movePos;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			parentCanvas.transform as RectTransform,
			Input.mousePosition, parentCanvas.worldCamera,
			out movePos);

		transform.position = parentCanvas.transform.TransformPoint(movePos+offset);
	}

	public void HideTooltip() {
		tooltipPanel.SetActive (false);
	}
	public void ShowTooltip() {
		tooltipPanel.SetActive (true);
	}

	public void UpdateTooltip(string itemType, string food, int bonusID) {
		string iconToGet = "";
		string nameText = "";
		string descText = "";
		string effectText = "";
		string specialText = "";

		ItemController.Item foodItem = new ItemController.Item();
		BonusController.Bonus bonusItem = new BonusController.Bonus();

		if (itemType != "bonus") {
			foodItem = gameController.GetComponent<ItemController>().GetItem (food);
		} else {
			bonusItem = gameController.GetComponent<BonusController>().allBonuses [bonusID];
		}

		switch (itemType) {
		case "storeBuild":
			iconToGet = food + "1";
			nameText = foodItem.store1Name;
			descText = "\"" + foodItem.store1Desc + "\"";
			effectText = "Earns $" + gameController.GetComponent<MoneyController>().getNewStoreEarning (food) + " /s";
			List<string> groupBonuses = gameController.GetComponent<MoneyController>().getGroupBonuses (food);
			if (groupBonuses.Count == 0) {
				specialText = "No Group Bonuses";
			} else {
				specialText = "Group Bonuses:";
				foreach (string f in groupBonuses) {
					specialText += " " + f + ",";
				}
				specialText = specialText.Substring (0, specialText.Length - 1);
			}
			break;
		case "storeUpgrade":
			iconToGet = food + "2";
			nameText = foodItem.store2Name;
			descText = foodItem.store2Desc;
			effectText = "$Some /s";
			specialText = "No group bonuses";
			break;
		case "farmBuild":
			iconToGet = food + "-1";
			nameText = foodItem.farmName;
			descText = foodItem.farmDesc;
			effectText = "+1 " + formatFoodName(food);
			specialText = "";
			break;
		case "farmUpgrade":
			iconToGet = food + "-1";
			nameText = foodItem.farmName;
			descText = foodItem.farmDesc;
			effectText = "+1 " + formatFoodName(food);
			specialText = "";
			break;
		case "bonus":
			iconToGet = food;
			nameText = bonusItem.bonusName;
			descText = bonusItem.bonusDesc;
			effectText = "x" + bonusItem.bonusMult;
			specialText = "";
			break;
		default:
			return;
		}

		icon.sprite = getIcon (iconToGet);
		name.text = nameText;
		desc.text = descText;
		effects.text = effectText;
		special.text = specialText;
	}

	public Sprite getIcon(string name) {
		Sprite icon = Resources.Load<Sprite> ("Sprites/" + name);
		if (icon == null) {
			icon = Resources.Load<Sprite> ("Sprites/question_mark");
		}
		return icon;
	}

	public string formatFoodName(string foodType) {
		TextInfo ti = new CultureInfo ("en-US", false).TextInfo;
		string newFood = foodType.Replace ("_", " ");
		newFood = ti.ToTitleCase (newFood);
		return newFood;
	}
}
