using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;

public class BonusController : MonoBehaviour {

	private static string bonusSaveFileName = "bonusInfo.dat";

	public enum BonusType{ Base = 1, Group = 2, Click = 3 }
	public TextAsset bonusFile;

	public float minCostRatioToReveal = 1;

	[System.Serializable]
	public class Bonus
	{
		public string bonusName;
		public string bonusType;
		public int bonusTier;
		public int bonusID;
		public int upgradeID;
		public string food1;
		public string food2;
		public float bonusMult;
		public float cost;
		public string bonusDesc;
	}

	[SerializeField] private float baseMult = 3;
	[SerializeField] private float groupMult = 2;

	[SerializeField] private int maxTiers = 3;
	[SerializeField] private List<float> tierCostMults = new List<float> ();

	public List<Bonus> allBonuses = new List<Bonus>();
	private List<string> unlockedBonuses = new List<string>();
	private List<string> discoveredBonuses = new List<string> ();

	void Start () {
		InitializeBonusList ();
	}
	
	void Update () {
		
	}

	float GetBonusMult(string bonusType) {
		switch (bonusType) {
		case "base":
			return baseMult;
		case "group":
			return groupMult;
		case "spin":
			return 1;
		default:
			return 1;
		}
	}

	string GetBonusName(Bonus b) {
		string bName = "";
		switch (b.bonusType) {
		case "base":
			bName = formatFoodName (b.food1) + " Bonus I";
			break;
		case "group":
			bName = formatFoodName (b.food1) + "-" + formatFoodName (b.food2) + " Group Bonus I";
			break;
		case "spin":
			bName = formatFoodName (b.food1) + " Bonus I";
			break;
		}
		return bName;
	}

	public string formatFoodName(string foodType) {
		TextInfo ti = new CultureInfo ("en-US", false).TextInfo;
		string newFood = foodType.Replace ('_', ' ');
		newFood = ti.ToTitleCase (newFood);
		return newFood;
	}

	void InitializeBonusList() {
		string fileText = bonusFile.text;
		string[] fileLines = Regex.Split (fileText, "\n|\r|\r\n");

		for (int i = 0; i < fileLines.Length; i++) {
			string valueLine = fileLines [i];
			if (valueLine.Length == 0) { continue; }
			if (valueLine [0] == '#' || valueLine[0] == ' ') { continue; }

			string[] values = Regex.Split (valueLine, " ; ");
			if (values.Length < 7) { continue; }

			Bonus newBonus = new Bonus ();
			newBonus.bonusID = allBonuses.Count;
			newBonus.bonusType = values [0];
			int newTier;
			if (int.TryParse (values [1], out newTier)) {
				newBonus.bonusTier = newTier;
			} else {
				Debug.Log ("Not adding bonus, tier isn't valid number...");
				continue;
			}
			newBonus.bonusName = values [2];
			newBonus.food1 = values [3];
			newBonus.food2 = values [4];
			if (newBonus.bonusName == "") {
				newBonus.bonusName = GetBonusName (newBonus);
			}
			newBonus.bonusMult = GetBonusMult (newBonus.bonusType);
			float newCost = 0;
			if (float.TryParse (values [5], out newCost)) {
				newBonus.cost = newCost;
			} else {
				Debug.Log ("Not adding bonus, cost isn't valid number...");
				continue;
			}
			newBonus.bonusDesc = values [6];

			if (newBonus.bonusID >= 1) {
				Bonus prev = allBonuses [newBonus.bonusID - 1];
				if (prev.bonusTier == newBonus.bonusTier - 1 && prev.food1 == newBonus.food1 && prev.food2 == newBonus.food2) {
					prev.upgradeID = newBonus.bonusID;
				}
			}

			allBonuses.Add (newBonus);
		}
	}

	Bonus getBonusCopy(Bonus b) {
		Bonus nb = new Bonus ();
		nb.bonusName = b.bonusName;
		nb.bonusID = b.bonusID;
		nb.bonusDesc = b.bonusDesc;
		nb.bonusMult = b.bonusMult;
		nb.bonusType = b.bonusType;
		nb.cost = b.cost;
		nb.food1 = b.food1;
		nb.food2 = b.food2;
		nb.upgradeID = b.upgradeID;
		return nb;
	}

	public void SaveBonuses() {
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file1 = File.Open (Application.persistentDataPath + "/" + bonusSaveFileName, FileMode.OpenOrCreate);

		bf.Serialize (file1, unlockedBonuses);
		file1.Close ();
	}

	public void loadSavedBonuses(List<string> bs) {
		if (bs == null) {
			Debug.Log ("No saved money file");
		} else {
			unlockedBonuses = bs;
			GetComponent<UIController>().UpdateBonusMenu ();
		}
	}

	public void LoadBonuses() {
		if (File.Exists (Application.persistentDataPath + "/" + bonusSaveFileName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file1 = File.Open (Application.persistentDataPath + "/" + bonusSaveFileName, FileMode.Open);
			List<string> bs = (List<string>)bf.Deserialize (file1);
			file1.Close ();

			loadSavedBonuses (bs);

		} else {
			loadSavedBonuses (null);
		}
	}

	public void DeleteBonusSave() {
		if (File.Exists (Application.persistentDataPath + "/" + bonusSaveFileName)) {
			File.Delete (Application.persistentDataPath + "/" + bonusSaveFileName);
		}
		unlockedBonuses.Clear ();
		discoveredBonuses.Clear ();
		LoadBonuses ();
	}

	Bonus getBonus(int id) {
		if (allBonuses.Count > id) {
			return allBonuses [id];
		} else {
			return null;
		}
	}

	public void UnlockBonus(int id) {
		unlockedBonuses.Add ("" + id);
		GetComponent<MoneyController>().ApplyBonus (getBonus (id));
		GetComponent<UIController>().UpdateBonusMenu ();
	}

	bool isBonusUnlocked(int id) {
		return unlockedBonuses.Contains ("" + id);
	}

	public List<string> getDiscoveredBonuses() {
		updateDiscoveredBonuses ();
		return discoveredBonuses;
	}

	public void updateDiscoveredBonuses() {
		float currentMoney = GetComponent<MoneyController>().getMoney ();
		List<string> purchaseableBonuses = new List<string>();
		List<string> unpurchaseableBonuses = new List<string> ();
		for (int i = 0; i < allBonuses.Count; i++){
			if (purchaseableBonuses.Contains("" + i) || unpurchaseableBonuses.Contains("" + i)) { continue; }
			Bonus b = allBonuses [i];
			if (b.food2 == "all") { continue; }
			switch (b.bonusType) {
			case "base":
				if (!GetComponent<MoneyController>().isResourceDiscovered (b.food1)) { continue; }
				break;
			case "group":
				if (!GetComponent<MoneyController>().isResourceDiscovered (b.food1) || (!GetComponent<MoneyController>().isResourceDiscovered(b.food2))) { continue; }
				break;
			default:
				continue;
				break;
			}

			Bonus bonusCheck = b;
			bool foundPurchaseable = false;
			while (bonusCheck.upgradeID != 0) {
				if (!isBonusUnlocked (bonusCheck.bonusID) && !foundPurchaseable && bonusCheck.cost*minCostRatioToReveal <= currentMoney) {
					foundPurchaseable = true;
					purchaseableBonuses.Add ("" + bonusCheck.bonusID);
				} else {
					unpurchaseableBonuses.Add ("" + bonusCheck.bonusID);
				}
				bonusCheck = allBonuses [bonusCheck.upgradeID];
			}

			if (!foundPurchaseable && !isBonusUnlocked (bonusCheck.bonusID) && bonusCheck.cost*minCostRatioToReveal <= currentMoney) {
				purchaseableBonuses.Add ("" + bonusCheck.bonusID);
			} else {
				unpurchaseableBonuses.Add ("" + bonusCheck.bonusID);
			}

		}
		var newDiscovered = discoveredBonuses.Union (purchaseableBonuses);
		var ignoreUnlocked = newDiscovered.Except (unlockedBonuses);
		discoveredBonuses = ignoreUnlocked.ToList();
	}
}
