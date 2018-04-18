using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using UnityEngine;

public class MoneyController : MonoBehaviour {

	private static string moneySaveFileName = "moneyInfo.dat";
	private static float startingMoney = 750;

	[SerializeField] private float money; //current amount of money
	[SerializeField] private float totalMoneyAllTime; //gross income
	[SerializeField] private float mps; //current money per second
	[SerializeField] private float minSpinMult = 1; //minimum spin speed earnings multiplier
	[SerializeField] private float maxSpinMult = 2; //maximum spin speed earnings multiplier
	[SerializeField] private float pityMPS = 0; //money per second without buildings (by spinning)
	[SerializeField] private float mainMult = 1.15f; //cost increase multiplier
	[SerializeField] private float upgradeCostMult = 2f; //upgrade cost vs. normal build cost
	[SerializeField] private float destroyLossMult = .5f;

	public Dictionary<string, Dictionary<string, float>> moneyStats = new Dictionary<string, Dictionary<string, float>>();

	private Dictionary<string, float> resourceCounts = new Dictionary<string, float> ();

	private Dictionary<string, float> rawMults = new Dictionary<string, float>(); //total money per second multipliers for each food
	private Dictionary<string, float> storeCounts = new Dictionary<string, float> (); //number of each store type simplified to food types
	private List<List<string>> buildingGroups = new List<List<string>>(); //groups of buildings simplified to types (for group mults)
	private Dictionary<string, float> storeCostMults = new Dictionary<string, float>(); //multipliers for store costs for each food
	private Dictionary<string, float> farmCostMults = new Dictionary<string, float>(); //multipliers for farm costs for each food
	private float searchCostMult;
	private Dictionary<string, float> groupMults = new Dictionary<string, float>(); //building group-based multipliers
	[SerializeField] private List<float> initialStoreCosts = new List<float>(); //starting build costs of each tier
	[SerializeField] private float initialFarmCost = 100f; //initial cost of farm
	[SerializeField] private List<float> initialStoreEarnings = new List<float>(); //starting build earnings of each tier
	private Dictionary<string, int> foodTiers = new Dictionary<string, int>(); //tiers associated with food types
	private List<float> tileOutputs = new List<float>();

	private Dictionary<string, float> resourcesDiscovered = new Dictionary<string, float> (); //keep track of previously discovered resources (to show in UI)

	private bool initialLoad = false;

	void Start () {
		InitializeMoney ();
		//LoadMoney ();
	}

	void Update() {
		//if (!initialLoad) {
		//	LoadMoney ();
		//	initialLoad = true;
		//}
	}

	void FixedUpdate () {
		if (mps > 0) {
			pityMPS = 0;
		}
		money += (mps * Time.deltaTime * (minSpinMult + (PlanetSpinner.Instance.GetSpeedRatio () * (maxSpinMult - minSpinMult)))) + (pityMPS * Time.deltaTime);
		totalMoneyAllTime += mps * Time.deltaTime;
	}
		
	//---------------------------------------------------------------------------

	public void InitializeMoney() {
		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			string foodType = food.name.ToLower ();
			foodTiers [foodType] = food.rarity;
			rawMults [foodType] = 1;
			storeCostMults [foodType] = 1;
			farmCostMults [foodType] = 1;
			searchCostMult = 1;
			storeCounts [foodType + "1"] = 0;
			storeCounts [foodType + "2"] = 0;
			resourceCounts [foodType] = 0;
			resourcesDiscovered [foodType] = 0;
			foreach (ItemController.Item food2 in GetComponent<ItemController>().items) {
				groupMults [food.name.ToLower() + "-" + food2.name.ToLower()] = 0;
			}
		}
			
		GetComponent<UIController>().UpdateResourceUI ();

		money = startingMoney;
		mps = 0;
		totalMoneyAllTime = 0;
	}

	public void populateMoneyStats() {
		moneyStats.Clear ();
		moneyStats ["money"] = new Dictionary<string, float> { { "default", money } };
		moneyStats ["totalMoneyAllTime"] = new Dictionary<string, float> { { "default", totalMoneyAllTime } };
		moneyStats ["resourceCounts"] = resourceCounts;
		moneyStats ["rawMults"] = rawMults;
		moneyStats ["storeCounts"] = storeCounts;
		moneyStats ["storeCostMults"] = storeCostMults;
		moneyStats ["farmCostMults"] = farmCostMults;
		moneyStats ["searchCostMult"] = new Dictionary<string, float> { { "default", searchCostMult } };
		moneyStats ["groupMults"] = groupMults;
		moneyStats ["resourcesDiscovered"] = resourcesDiscovered;
	}

	public void loadSavedMoney(Dictionary<string, Dictionary<string, float>> ms) {
		if (ms == null) {
			Debug.Log ("No saved money file");
		} else {
			money = ms ["money"] ["default"];
			totalMoneyAllTime = ms ["totalMoneyAllTime"] ["default"];
			resourceCounts = ms ["resourceCounts"];
			rawMults = ms ["rawMults"];
			storeCounts = ms ["storeCounts"];
			storeCostMults = ms ["storeCostMults"];
			farmCostMults = ms ["farmCostMults"];
			searchCostMult = ms ["searchCostMult"] ["default"];
			groupMults = ms ["groupMults"];
			resourcesDiscovered = ms ["resourcesDiscovered"];
		}

		if (GetComponent<TileController>() != null) {
			buildingGroups = GetComponent<TileController>().getTileBuildings ();
			initialLoad = true;
		}

		GetComponent<UIController> ().UpdateResourceUI ();
	}

	public void SaveMoney() {
		populateMoneyStats ();
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file1 = File.Open (Application.persistentDataPath + "/" + moneySaveFileName, FileMode.OpenOrCreate);

		bf.Serialize (file1, moneyStats);
		file1.Close ();
	}

	public void LoadMoney() {
		if (File.Exists (Application.persistentDataPath + "/" + moneySaveFileName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file1 = File.Open (Application.persistentDataPath + "/" + moneySaveFileName, FileMode.Open);
			Dictionary<string, Dictionary<string, float>> ms = (Dictionary<string, Dictionary<string, float>>)bf.Deserialize (file1);
			file1.Close ();

			loadSavedMoney (ms);

		} else {
			loadSavedMoney(null);
		}
	}

	public void DeleteMoneySave() {
		if (File.Exists (Application.persistentDataPath + "/" + moneySaveFileName)) {
			File.Delete (Application.persistentDataPath + "/" + moneySaveFileName);
		}
		InitializeMoney ();
		calculateEarnings ();
	}

	public bool isResourceDiscovered(string foodType) {
		return resourcesDiscovered [foodType] != 0;
	}

	public void discoverResource(string foodType) {
		resourcesDiscovered [foodType] = 1f;
	}

	//---------------------------------------------------------------------------

	public void SetPityMPS(float val) {
		pityMPS = val;
	}

	public string getFoodType(string building) {
		if (building.Contains ("-")) {
			return building.Substring (0, building.IndexOf ("-"));
		} else if (building.Contains ("1")) {
			return building.Substring (0, building.IndexOf ("1"));
		} else if (building.Contains("2")) {
			return building.Substring (0, building.IndexOf ("2"));
		} else {
			return building;
		}
	}

	public int getBuildingTier(string building) {
		if (building.Contains ("-1")) {
			return 1;
		} else if (building.Contains ("-2")) {
			return 2;
		} else if (building.Contains ("-3")) {
			return 3;
		}else if (building.Contains ("1")) {
			return foodTiers [getFoodType (building)];
		} else if (building.Contains ("2")) {
			return foodTiers [getFoodType (building)];
		} else {
			return 0;
		}
	}

	public int getFoodTier(string food) {
		return foodTiers [food];
	}

	public int countBuildings(List<string> buildings) {
		int count = 0;
		foreach (string s in buildings) {
			if (s != "") {
				count++;
			}
		}
		return count;
	}

	public float getNewStoreEarning(string foodType) {
		return initialStoreEarnings [foodTiers [foodType] - 1] * rawMults[foodType];
	}

	public List<string> getGroupBonuses(string foodType) {
		List<string> existingBonuses = new List<string> ();
		foreach (ItemController.Item food2 in GetComponent<ItemController>().items) {
			if (groupMults [foodType + "-" + food2.name.ToLower()] > 0) {
				existingBonuses.Add (food2.name);
			}
		}
		return existingBonuses;
	}

	public float getMoney() {
		return money;
	}

	public float getMPS() {
		return (mps + pityMPS);
	}

	public float getSpinMult() {
		return minSpinMult + (PlanetSpinner.Instance.GetSpeedRatio () * (maxSpinMult - minSpinMult));
	}

	public float getTileOutput(int index) {
		if (tileOutputs.Count < index) {
			return 0;
		} else {
			return tileOutputs [index];
		}
	}

	public float getResourceCount(string foodType) {
		return resourceCounts [foodType];
	}

	public float getSearchCost() {
		return 100 * searchCostMult;
	}

	public float getStoreBuildCost(string foodType) {
		//Debug.Log ("FoodTiers[" + foodType + "] = ...");
		//Debug.Log(foodTiers[foodType]);
		return initialStoreCosts [foodTiers [foodType]-1] * storeCostMults [foodType];
	}

	public float getStoreUpgradeCost(string foodType) {
		return getStoreBuildCost (foodType) * upgradeCostMult;
	}

	public float getStoreDestroyReward(string type) {
		if (type.Contains ("2")) {
			return getStoreBuildCost (getFoodType (type)) * destroyLossMult * upgradeCostMult;
		} else {
			return getStoreBuildCost (getFoodType (type)) * destroyLossMult;
		}
	}

	public float getFarmBuildCost(string foodType) {
		return initialStoreCosts [foodTiers[foodType]-1] * farmCostMults [foodType];
	}

	public float getFarmUpgradeCost(string type) {
		return getFarmBuildCost (getFoodType (type)) * Mathf.Pow (upgradeCostMult, getBuildingTier (type)-1);
	}

	public float getFarmDestroyReward(string type) {
		return getFarmBuildCost (getFoodType (type)) * destroyLossMult * Mathf.Pow (upgradeCostMult, getBuildingTier (type)-1);
	}

	public void makePurchase(float moneyAmount, float resourceAmount, string resourceType) {
		if (moneyAmount < 0) {
			Debug.Log ("Invalid purchase price " + moneyAmount);
		} else if (resourceAmount < 0) {
			Debug.Log ("Invalid purchase resource price " + resourceAmount);
		//} else if (resourceType == "" && resourceAmount == 0) {
		//	Debug.Log ("Empty purchase resource type");
		} else {
			money -= moneyAmount;
			if (resourceAmount > 0)
				resourceCounts [resourceType] -= resourceAmount;
		}
	}

	public void calculateEarnings() {
		Debug.Log ("CalculateEarnings");
		if (buildingGroups.Count <= 0)
			buildingGroups = GetComponent<TileController>().getTileBuildings ();
		
		//buildingGroups = GetComponent<TileController>().getTileBuildings ();
		//update TileController's building list
		GetComponent<TileController>().UpdateTileBuildings(buildingGroups);

		//get base earning of food type, apply raw mults and group mults
		mps = 0;
		/*foreach (KeyValuePair<string, float> entry in storeCounts) {
			mps += entry.Value * rawMults [getFoodType(entry.Key)];
		}*/

		for (int i = 0; i < buildingGroups.Count; i++) {
			List<string> bGroup = buildingGroups [i];
			float tileOutput = 0;

			foreach (string b in bGroup) {
				if (b.Contains("-") || b == "") 
					continue;
				else {
					Debug.Log (b);
					//Debug.Log (b);
					if (b.Contains ("1")) {
						tileOutput += initialStoreEarnings[foodTiers[getFoodType(b)]-1] * rawMults [getFoodType (b)];
					} else {
						tileOutput += initialStoreEarnings[foodTiers[getFoodType(b)]-1] * rawMults [getFoodType (b)] * upgradeCostMult;
					}
				}
			}

			//if there are 0 or 1 buildings in the group, they can't receive any group bonuses
			if (countBuildings(bGroup) < 2) {
				tileOutput += 0;
			} 
			//if there's two buildings in the group, they can receive up to two bonuses
			else if (countBuildings(bGroup) == 2) {
				if (bGroup [2] == "") {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [0])-1] * rawMults [getFoodType (bGroup [0])] * groupMults [getFoodType(bGroup [0]) + "-" + getFoodType(bGroup [1])];
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [1])-1] * rawMults [getFoodType (bGroup [1])] * groupMults [getFoodType(bGroup [1]) + "-" + getFoodType(bGroup [0])];
				} else if (bGroup [1] == "") {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [0])-1] * rawMults [getFoodType (bGroup [0])] * groupMults [getFoodType(bGroup [0]) + "-" + getFoodType(bGroup [2])];
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [2])-1] * rawMults [getFoodType (bGroup [2])] * groupMults [getFoodType(bGroup [2]) + "-" + getFoodType(bGroup [0])];
				} else {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [1])-1] * rawMults [getFoodType (bGroup [1])] * groupMults [getFoodType(bGroup [1]) + "-" + getFoodType(bGroup [2])];
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [2])-1] * rawMults [getFoodType (bGroup [2])] * groupMults [getFoodType(bGroup [2]) + "-" + getFoodType(bGroup [1])];
				}
			} 
			//if there's three buildings in the group, they can receive up to three bonuses
			else if (countBuildings(bGroup) == 3) {
				if (groupMults [getFoodType(bGroup[0])+"-"+getFoodType(bGroup[1])] != 0) {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [0])-1] * rawMults [getFoodType (bGroup [0])] * groupMults [getFoodType(bGroup [0]) + "-" + getFoodType(bGroup [1])];
				} else {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [0])-1] * rawMults [getFoodType (bGroup [0])] * groupMults [getFoodType(bGroup [0]) + "-" + getFoodType(bGroup [2])];
				}

				if (groupMults [getFoodType(bGroup[1])+"-"+getFoodType(bGroup[0])] != 0) {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [1])-1] * rawMults [getFoodType (bGroup [1])] * groupMults [getFoodType(bGroup [1]) + "-" + getFoodType(bGroup [0])];
				} else {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [1])-1] * rawMults [getFoodType (bGroup [1])] * groupMults [getFoodType(bGroup [1]) + "-" + getFoodType(bGroup [2])];
				}

				if (groupMults [getFoodType(bGroup[2])+"-"+getFoodType(bGroup[0])] != 0) {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [2])-1] * rawMults [getFoodType (bGroup [2])] * groupMults [getFoodType(bGroup [2]) + "-" + getFoodType(bGroup [0])];
				} else {
					tileOutput += initialStoreEarnings [getBuildingTier (bGroup [2])-1] * rawMults [getFoodType (bGroup [2])] * groupMults [getFoodType(bGroup [2]) + "-" + getFoodType(bGroup [1])];
				}
			}
			mps += tileOutput;
			if (tileOutputs.Count <= i) {
				tileOutputs.Add (0);
			}
			tileOutputs [i] = tileOutput;
			if (tileOutput > 0) {
				//Debug.Log ("Adding tile output " + tileOutput + " to index " + i);
			}
		}

		GetComponent<UIController>().UpdateResourceUI ();
	}

	public void AddRawMult(string foodType, float mult) {
		rawMults [foodType] *= mult;
		calculateEarnings ();
	}

	public void AddGroupMult(string foodType1, string foodType2, float mult) {
		if (groupMults [foodType1+"-"+foodType2] == 0) {
			groupMults [foodType1+"-"+foodType2] = mult;
		} else {
			groupMults [foodType1+"-"+foodType2] *= mult;
		}
		calculateEarnings ();
	}

	public void BuySearch() {
		float purchaseCost = 100 * searchCostMult;
		makePurchase (purchaseCost, 0, "");
		searchCostMult *= mainMult;
		GetComponent<TileController>().RevealTileResource ();
	}

	public void AddFarm(string type, int groupNum) {
		float purchaseCost = 0;
		if (type.Contains ("1")) {
			purchaseCost = getFarmBuildCost (getFoodType (type));
			farmCostMults [getFoodType (type)] *= mainMult;
		} else {
			purchaseCost = getFarmUpgradeCost (type);
		}
		groupNum = groupNum % 162;
		List<string> tmplist = new List<string> ();
		tmplist.Add (type);
		buildingGroups [groupNum] = tmplist;
		resourceCounts [getFoodType (type)]++;
		makePurchase(purchaseCost, 0, getFoodType(type));
		Debug.Log (getFoodType (type) + " resource count: " + resourceCounts [getFoodType (type)]);
	}

	public void RemoveFarm(string type, int groupNum) {
		groupNum = groupNum % 162;
		resourceCounts [getFoodType (type)] -= getBuildingTier (type);
		buildingGroups [groupNum].Remove (type);
	}

	public void AddStore(string type, int groupNum, int index) {
		float purchaseCost = 0;
		groupNum = groupNum % 162;
		//upgrade case; remove old tier from counts
		if (type.Contains ("2")) {
			storeCounts [getFoodType(type)+"1"]--;
			purchaseCost = getStoreBuildCost(getFoodType(type)) * upgradeCostMult;
		} 
		//new store case; increase build cost, add new store to group list
		else {
			purchaseCost = getStoreBuildCost(getFoodType(type));
			storeCostMults [getFoodType(type)] *= mainMult;
			buildingGroups [groupNum] [index] = type;
		}
		/*Debug.Log (storeCounts.Count);
		foreach (KeyValuePair<string, float> kvp in storeCounts)
		{
			//textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
			Debug.Log("Key = {" + kvp.Key + "}, Value = {" + kvp.Value + "}");
		}
		Debug.Log (type);*/

		makePurchase(purchaseCost, 1, getFoodType(type));
		storeCounts [type]++;
		calculateEarnings ();
	}

	public void RemoveStore(string type, int groupNum) {
		groupNum = groupNum % 162;
		storeCounts [type]--;
		resourceCounts [getFoodType (type)]++;
		buildingGroups [groupNum] [buildingGroups [groupNum].IndexOf (type)] = "";
		storeCostMults [getFoodType(type)] /= mainMult;
		calculateEarnings ();
	}

	public void ApplyBonus(BonusController.Bonus newBonus) {
		switch (newBonus.bonusType) {
		case "base":
			AddRawMult (newBonus.food1, newBonus.bonusMult);
			break;
		case "group":
			AddGroupMult (newBonus.food1, newBonus.food2, newBonus.bonusMult);
			break;
		case "spin":
			break;
		}
		makePurchase (newBonus.cost, 0, "");
	}
}
