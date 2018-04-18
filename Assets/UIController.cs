using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

	public int moneyUpdatesPerSecond = 5;

	private Tile currentTile = null;

	public Sprite farmIcon;

	public Color unaffordableColor;
	public Color affordableColor;

	public Color disabledColor;

	public Color searchButtonColor;

	public Color bonusButtonColor;

	public List<Color> rarityColors = new List<Color>();

	Text moneyText;
	Text resourceText;
	Text biomeText;
	Text fpsCounter;

	Text mpsText;

	Text spinMultText;
	[SerializeField] private float lastSpinMult = 1;
	float minSpinMultDiff = .05f;

	ObjectPooler fallingMoneyPooler;

	Image resourcePanel;
	Image resourceIcon;

	[SerializeField] private GameObject tileInfoPanel;
	[SerializeField] private GameObject tileBuildPanel;
	UIToggle tileBuildToggler;
	[SerializeField] private GameObject tileBuildList;
	[SerializeField] private GameObject tileBuildOption;
	[SerializeField] private GameObject bonusPanel;
	GameObject bonusList;
	[SerializeField] private GameObject buildWarning;
	[SerializeField] private GameObject destroyWarning;
	[SerializeField] private GameObject destroyPanel;
	[SerializeField] private GameObject upgradeWarning;
	[SerializeField] private GameObject pauseMenu;
	GameObject optionsMenu;

	GameObject searchOption;
	GameObject farmOption;
	GameObject destroyOption;
	GameObject storeOption;
	GameObject upgradeOption;

	GameObject searchButton;
	GameObject searchForeground;
	GameObject searchText;
	GameObject searchPanel;

	[SerializeField] private UIToggle expandedMenuToggler;

	public GameObject bonusPanelPrefab;

	Text tileOutputText;

	Dictionary<string, bool> enoughMoneyForStore = new Dictionary<string, bool>();
	Dictionary<string, bool> enoughResourcesForStore = new Dictionary<string, bool>();

	Dictionary<string, GameObject> resourcePanels = new Dictionary<string, GameObject> ();
	Dictionary<string, GameObject> buildOptions = new Dictionary<string, GameObject> ();

	bool enoughMoneyForBuild = true;
	bool enoughResourcesForBuild = true;
	public bool menuOpen = false;

	DragToOrbit cam;

	ItemController ic;
	MoneyController mc;

	public float prevCamXSpeed;
	float prevCamYSpeed;
	float prevCamZoomSpeed;

	List<string> numberNames;
	List<string> shortNumberNames;

	List<string> showBonuses = new List<string> ();

	void Start () {
		moneyText = GameObject.Find ("Money Text").GetComponent<Text> ();
		//biomeText = GameObject.Find ("Tile Biome Text").GetComponent<Text> ();
		fpsCounter = GameObject.Find ("FPSCounter").GetComponent<Text> ();
		mpsText = GameObject.Find ("MPS Text").GetComponent<Text> ();
		spinMultText = GameObject.Find ("SpinMult Text").GetComponent<Text> ();
		spinMultText.GetComponent<Animator> ().SetBool ("Shaking", true);
		fallingMoneyPooler = GameObject.Find ("FallingMoneyPooler").GetComponent<ObjectPooler> ();
		//tileInfoPanel = GameObject.Find ("Tile Info Panel");
		//tileBuildPanel = GameObject.Find ("Tile Build Panel");
		tileBuildToggler = tileInfoPanel.GetComponentInChildren<UIToggle> ();
		//tileBuildList = GameObject.Find ("Build List");
		//tileBuildOption = GameObject.Find ("Build Option");
		//bonusPanel = GameObject.Find ("Bonus List");
		//buildWarning = GameObject.Find ("Build Warning");
		//destroyWarning = GameObject.Find ("Destroy Warning");
		//destroyPanel = GameObject.Find ("Destroy Panel");
		//upgradeWarning = GameObject.Find ("Upgrade Warning");

		//expandedMenuToggler = GameObject.Find ("ExpandedMenuPanel").GetComponentInChildren<UIToggle> ();
		expandedMenuToggler.ToggleUI ();

		//pauseMenu = GameObject.Find ("Pause Panel");
		tileInfoPanel.SetActive (false);
		tileBuildOption.SetActive (false);

		bonusList = bonusPanel.transform.Find ("Scroll View/Viewport/Content").gameObject;
		optionsMenu = pauseMenu.transform.Find ("Options Panel").gameObject;

		InitializeBonusMenu ();
		bonusPanel.SetActive (false);

		buildWarning.SetActive (false);
		destroyWarning.SetActive (false);
		destroyPanel.SetActive (false);
		upgradeWarning.SetActive (false);
		pauseMenu.SetActive (false);

		searchOption = tileInfoPanel.transform.Find ("SearchButton").gameObject;
		resourceText = tileInfoPanel.transform.Find ("ResourcePanel/ResourceText").gameObject.GetComponent<Text> ();
		resourcePanel = tileInfoPanel.transform.Find ("ResourcePanel").gameObject.GetComponent<Image> ();
		resourceIcon = tileInfoPanel.transform.Find ("ResourcePanel/ResourceIcon").gameObject.GetComponent<Image> ();
		farmOption = tileBuildPanel.transform.GetChild(1).Find("FarmOption").gameObject;
		destroyOption = tileBuildPanel.transform.GetChild(1).Find("DestroyOption").gameObject;
		storeOption = tileBuildPanel.transform.GetChild(1).Find("StoreOption").gameObject;
		upgradeOption = tileBuildPanel.transform.GetChild(1).Find("UpgradeOption").gameObject;
		tileOutputText = tileInfoPanel.transform.Find ("OutputPanel/Tile Output Text").GetComponent<Text> ();

		searchButton = tileInfoPanel.transform.Find ("SearchButton").gameObject;
		searchForeground = tileInfoPanel.transform.Find ("SearchButton/ForegroundPanel").gameObject;
		searchText = tileInfoPanel.transform.Find ("SearchButton/ForegroundPanel/SearchCostPanel/SearchCostText").gameObject;
		searchPanel = tileInfoPanel.transform.Find ("SearchButton/ForegroundPanel/SearchCostPanel").gameObject;

		searchOption.SetActive (false);
		farmOption.SetActive (false);
		destroyOption.SetActive (false);
		storeOption.SetActive (false);
		upgradeOption.SetActive (false);

		ic = GetComponent<ItemController>();
		//tc = GetComponent<TileController>();
		mc = GetComponent<MoneyController> ();

		cam = GameObject.Find ("Main Camera").GetComponent<DragToOrbit> ();

		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			enoughMoneyForStore [food.name.ToLower ()] = true;
			enoughResourcesForStore [food.name.ToLower ()] = true;

			GameObject rp = GameObject.Find (food.name.ToLower() + "_resource_panel");
			if (rp != null) {
				rp.SetActive (false);
				resourcePanels [food.name.ToLower ()] = rp;
			} else {
				resourcePanels [food.name.ToLower ()] = null;
			}
		}

		InitializeBuildListFormat ();

		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			GameObject bo = tileBuildList.transform.Find ("Scroll View/Viewport/Content/" + food.name.ToLower() + "_build_option").gameObject;
			if (bo != null) {
				bo.SetActive (false);
				buildOptions [food.name.ToLower ()] = bo;
			} else {
				buildOptions [food.name.ToLower ()] = null;
			}
		}

		tileBuildList.SetActive (false);

		numberNames = new List<string> { "", "Thousand", "Million", "Billion", "Trillion", "Quadrillion", "Quintillion", "Sextillion", "Septillion", "Octillion", "Nonillion", "Decillion", "Undecillion", "Duodecillion", "Tredecillion", "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septendecillion", "Octodecillion", "Novemdecillion", "Vigintillion" };
		shortNumberNames = new List<string> { "", "K", "M", "B", "T", "Qd", "Qt", "St", "Sp", "Oc", "Non", "Dc", "Ud", "Dd", "Td", "Qtd", "Qnd", "Std", "Spd", "Ocd", "Nd", "Vg" };
	
		InvokeRepeating ("UpdateMoneyText", 0, (1f/moneyUpdatesPerSecond));
	}
	
	void Update () {
		fpsCounter.text = (int)(1.0f / Time.deltaTime) + "";

		ManageStoreBuildAccess ();
		ManageBonusAccess ();
		if (!tileBuildOption.activeInHierarchy && (!enoughMoneyForBuild || !enoughResourcesForBuild) && GetComponent<TileController>().selectedTile != null) {
			enoughMoneyForBuild = true;
			enoughResourcesForBuild = true;
		}

		if (GetComponent<TileController>().toBuild != "" && !(GetComponent<TileController>().toBuild.Contains("1") && !GetComponent<TileController>().toBuild.Contains("-"))) {
			ManageGeneralBuildAccess (GetComponent<TileController>().toBuild);
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			TogglePauseMenu ();
		}

		/*if (tileBuildList.activeInHierarchy || tileBuildOption.activeInHierarchy || destroyPanel.activeInHierarchy || bonusList.activeInHierarchy) {
			menuOpen = true;
		} else {
			menuOpen = false;
		}*/
	}

	void UpdateMoneyText() {
		if (GetComponent<MoneyController>().getMPS() > 0) {
			GameObject fallingText = fallingMoneyPooler.GetPooledObject ();
			fallingText.SetActive (true);
			fallingText.GetComponent<FallingText>().OneShotText (shortFormatNumber (GetComponent<MoneyController>().getMPS ()*GetComponent<MoneyController>().getSpinMult()/moneyUpdatesPerSecond));
		}
		moneyText.text = formatNumber(GetComponent<MoneyController>().getMoney ());
		mpsText.text = formatNumber(GetComponent<MoneyController>().getMPS ()) + " /s";
		float spinMult = GetComponent<MoneyController> ().getSpinMult ();
		if (spinMult > 1.1f) {
			if (spinMult > lastSpinMult + minSpinMultDiff) {
				lastSpinMult = spinMult;
				spinMultText.GetComponent<Animator> ().SetTrigger ("Update");
			} else if (spinMult < lastSpinMult - minSpinMultDiff) {
				lastSpinMult = spinMult;
			}
			spinMultText.text = "x" + formatMultiplier (spinMult);
		} else {
			spinMultText.text = "";
		}
		UpdateSearchOption ();
	}

	public void DisableCameraMovement() {
		prevCamXSpeed = cam.xSpeed;
		prevCamYSpeed = cam.ySpeed;
		prevCamZoomSpeed = cam.zoomSpeed;

		cam.xSpeed = 0;
		cam.ySpeed = 0;
		cam.zoomSpeed = 0;
	}

	public void EnableCameraMovement() {
		cam.xSpeed = prevCamXSpeed;
		cam.ySpeed = prevCamYSpeed;
		cam.zoomSpeed = prevCamZoomSpeed;
	}

	public void CancelSelections() {
		TileController tc = GetComponent<TileController>();
		if (tc.selectedTile != null) {
			CancelStoreBuild ();
			CancelStoreDestroy ();
			CancelStoreUpgrade ();
		}

		DisableCameraMovement ();

		CloseDestroyOption ();
		CloseGeneralBuildOption ();
		CloseStoreBuildList ();
		CloseBonusMenu ();
	}

	public void HideTileMenu() {
		tileBuildToggler.ToggleUIOff ();
		tileInfoPanel.SetActive (false);
	}

	public void ShowTileMenu() {
		tileInfoPanel.SetActive (true);
	}

	public void HandleMenuOpen() {
		CancelSelections ();
		HideTileMenu ();
	}
	public void HandleFullscreenMenuOpen() {
		HandleMenuOpen ();
		menuOpen = true;
		expandedMenuToggler.ToggleUIOff ();
		DisableCameraMovement ();
	}

	public void HandleMenuClose() {
		if (GetComponent<TileController>().selectedTile != null) {
			ShowTileMenu ();
		}
	}
	public void HandleFullscreenMenuClose() {
		HandleMenuClose ();
		menuOpen = false;
		EnableCameraMovement ();
	}

	public void UpdateResourceUI() {
		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			string foodType = food.name.ToLower ();
			GameObject rp = resourcePanels [food.name.ToLower ()];
			if (mc.isResourceDiscovered (foodType)) {
				//Debug.Log (foodType + " is discovered, showing rp");
				if (rp != null) {
					//Debug.Log ("Success showing rp");
					rp.SetActive (true);
					rp.transform.Find ("Count").GetComponent<Text> ().text = GetComponent<MoneyController>().getResourceCount (foodType) + "";
				} else {
					//Debug.Log ("Failure showing rp");
				}
			} else {
				if (rp != null) {
					rp.SetActive (false);
				}
			}

			GameObject bo = buildOptions [food.name.ToLower ()];
			if (mc.isResourceDiscovered (foodType)) {
				//Debug.Log (foodType + " is discovered, showing bo");
				if (bo != null) {
					//Debug.Log ("Success showing bo");
					bo.SetActive (true);
				} else {
					//Debug.Log ("Failure showing bo");
				}
			} else {
				if (bo != null) {
					bo.SetActive (false);
				}
			}
		}
	}

	public void UpdateStoreBuildText() {
		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			string foodType = food.name.ToLower ();
			float buildPrice = GetComponent<MoneyController>().getStoreBuildCost (foodType);
			GameObject buildOption = GameObject.Find (foodType + "_build_option");
			if (buildOption != null) {
				GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
				mct.gameObject.GetComponent<Text> ().text = buildPrice.ToString ("C0");
			}
		}
	}

	public void UpdateDestroyText(int index) {
		float price;
		string buildingType = GetComponent<TileController>().selectedTile.buildings [index];
		if (buildingType.Contains ("-")) {
			price = mc.getFarmDestroyReward (buildingType);
		} else {
			price = mc.getStoreDestroyReward (buildingType);
		}
		Text priceText = destroyPanel.transform.Find ("DestroyRewardPanel/DestroyRewardText").GetComponent<Text>();
		priceText.text = formatNumber(price);

	}

	public void UpdateSearchOption() {
		bool canSearch = mc.getSearchCost () <= mc.getMoney ();

		if (canSearch) {
			searchButton.GetComponent<Image> ().color = searchButtonColor;
			searchPanel.GetComponent<Image> ().color = searchButtonColor;
			searchForeground.GetComponent<Image> ().color = Color.white;
			searchButton.GetComponent<Button> ().interactable = true;
			searchText.GetComponent<Text> ().color = affordableColor;
		} else {
			searchButton.GetComponent<Image> ().color = Color.white;
			searchPanel.GetComponent<Image> ().color = Color.grey;
			searchForeground.GetComponent<Image> ().color = new Color (.8f, .8f, .8f);
			searchButton.GetComponent<Button> ().interactable = false;
			searchText.GetComponent<Text> ().color = unaffordableColor;
		}

		searchText.GetComponent<Text> ().text = formatNumber(mc.getSearchCost ());
	}

	public void UpdateFarmText() {
		string type = GetComponent<TileController>().toBuild;
		string foodType = mc.getFoodType (type);
		ItemController.Item foodItem = ic.GetItem (foodType);
		GameObject foodIcon = tileBuildOption.transform.Find ("build_option_main/FoodIcon").gameObject;
		foodIcon.GetComponent<Image> ().sprite = Resources.Load <Sprite>("Sprites/" + foodType);
		GameObject bgPanel = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel").gameObject;
		ColorBlock ncb = new ColorBlock ();
		bgPanel.GetComponent<Image> ().color = foodItem.uiColor;
		ncb.normalColor = Color.white;
		ncb.highlightedColor = Color.grey;
		ncb.pressedColor = Color.black;
		ncb.disabledColor = disabledColor;
		ncb.colorMultiplier = 1;
		bgPanel.GetComponent<Button> ().colors = ncb;
		bgPanel.GetComponent<Button>().onClick.RemoveAllListeners ();

		UnityEngine.Events.UnityAction action1 = () => {
			GetComponent<TileController>().FinalizeFarmBuild ();
		};
		AddActionToButton (bgPanel.GetComponent<Button> (), action1);
		bgPanel.GetComponent<TooltipItem> ().itemType = "farmBuild";
		bgPanel.GetComponent<TooltipItem> ().food = GetComponent<MoneyController>().getFoodType(GetComponent<TileController>().toBuild);

		GameObject buildName = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/BuildName").gameObject;
		GameObject buildDesc = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/BuildDescription").gameObject;
		GameObject moneyCostPanel = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
		Color mcpColor = foodItem.uiColor; mcpColor.a = 166f / 255f;
		moneyCostPanel.GetComponent<Image> ().color = mcpColor;
		GameObject moneyCostText = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
		GameObject resourceCostPanel = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
		GameObject resourceCostText = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
		Color rcpColor = foodItem.uiColor; mcpColor.a = 216f / 255f;
		resourceCostPanel.GetComponent<Image> ().color = rcpColor;

		buildName.GetComponent<Text> ().text = foodItem.farmName;
		buildDesc.GetComponent<Text> ().text = foodItem.farmDesc;			
		moneyCostText.GetComponent<Text> ().text = mc.getFarmUpgradeCost (type).ToString("C0");
		resourceCostText.GetComponent<Text> ().text = "0";
	}

	public void UpdateUpgradeText(int index) {
		string building = GetComponent<TileController>().selectedTile.buildings [index];
		string foodType = mc.getFoodType (building);
		ItemController.Item foodItem = ic.GetItem (foodType);
		GameObject foodIcon = tileBuildOption.transform.Find ("build_option_main/FoodIcon").gameObject;
		foodIcon.GetComponent<Image> ().sprite = Resources.Load <Sprite>("Sprites/" + foodType);
		GameObject bgPanel = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel").gameObject;
		ColorBlock ncb = new ColorBlock ();
		ncb.normalColor = foodItem.uiColor;
		ncb.highlightedColor = Color.white;
		ncb.pressedColor = Color.black;
		ncb.disabledColor = disabledColor;
		ncb.colorMultiplier = 1;
		bgPanel.GetComponent<Button> ().colors = ncb;
		bgPanel.GetComponent<Button>().onClick.RemoveAllListeners ();

		GameObject buildName = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/BuildName").gameObject;
		GameObject buildDesc = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/BuildDescription").gameObject;
		GameObject moneyCostPanel = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
		Color mcpColor = foodItem.uiColor; mcpColor.a = 166f / 255f;
		moneyCostPanel.GetComponent<Image> ().color = mcpColor;
		GameObject moneyCostText = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
		GameObject resourceCostPanel = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
		GameObject resourceCostText = tileBuildOption.transform.Find ("build_option_main/BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
		Color rcpColor = foodItem.uiColor; mcpColor.a = 216f / 255f;
		resourceCostPanel.GetComponent<Image> ().color = rcpColor;

		//farm case
		if (building.Contains ("-")) {
			buildName.GetComponent<Text> ().text = foodItem.farmName;
			buildDesc.GetComponent<Text> ().text = foodItem.farmDesc;
			moneyCostText.GetComponent<Text> ().text = mc.getFarmUpgradeCost (building).ToString("C0");
			resourceCostText.GetComponent<Text> ().text = "0";
			//Debug.Log ("Updating upgrade farm button");
			UnityEngine.Events.UnityAction action1 = () => {
				GetComponent<TileController>().StartFarmUpgrade ();
			};
			AddActionToButton (bgPanel.GetComponent<Button> (), action1);
			bgPanel.GetComponent<TooltipItem> ().itemType = "farmUpgrade";
			bgPanel.GetComponent<TooltipItem> ().food = GetComponent<MoneyController>().getFoodType(GetComponent<TileController>().toBuild);
			//store upgrade case
		} else {
			buildName.GetComponent<Text> ().text = foodItem.store2Name;
			buildDesc.GetComponent<Text> ().text = foodItem.store2Desc;
			moneyCostText.GetComponent<Text> ().text = mc.getStoreUpgradeCost(foodType).ToString("C0");
			resourceCostText.GetComponent<Text> ().text = "1";
			UnityEngine.Events.UnityAction action1 = () => {
				GetComponent<TileController>().FinalizeStoreUpgrade (index);
				//Debug.Log("Upgrading store");
			};
			AddActionToButton (bgPanel.GetComponent<Button> (), action1);
			bgPanel.GetComponent<TooltipItem> ().itemType = "storeUpgrade";
			bgPanel.GetComponent<TooltipItem> ().food = GetComponent<MoneyController>().getFoodType(GetComponent<TileController>().toBuild);
		}
	}

	public void InitializeBuildListFormat() {
		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			string foodType = food.name.ToLower ();
			GameObject buildOption = tileBuildList.transform.Find ("Scroll View/Viewport/Content/" + foodType + "_build_option").gameObject;
			if (buildOption == null) {
				Debug.Log ("Couldn't find build option");
			} else {
				GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
				foodIcon.GetComponent<Image> ().sprite = getIcon (foodType);

				GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
				mp.GetComponent<Button> ().interactable = true;
				mp.GetComponent<Image> ().color = rarityColors [food.rarity-1];//food.uiColor;

				GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
				fp.GetComponent<Image> ().color = Color.white;

				GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
				Color mcpColor = mp.GetComponent<Image> ().color;
				mcpColor.a = 166f / 255f;
				mcp.GetComponent<Image> ().color = mcpColor;

				GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
				mct.GetComponent<Text> ().color = affordableColor;

				GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
				Color rcpColor = mp.GetComponent<Image> ().color;
				rcpColor.a = 216f / 255f;
				rcp.GetComponent<Image> ().color = rcpColor;

				GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
				rct.GetComponent<Text> ().color = affordableColor;
			}
		}
	}

	public void ManageStoreBuildAccess() {
		foreach (ItemController.Item food in GetComponent<ItemController>().items) {
			string foodType = food.name.ToLower ();
			float buildPrice = GetComponent<MoneyController>().getStoreBuildCost (foodType);
			if ((buildPrice <= GetComponent<MoneyController>().getMoney () && GetComponent<MoneyController>().getResourceCount(foodType) >= 1) && (!enoughMoneyForStore [foodType] || !enoughResourcesForStore[foodType])) {
				GameObject buildOption = GameObject.Find (foodType + "_build_option");
				if (buildOption == null) {

				} else {
					GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
					foodIcon.GetComponent<Image> ().sprite = getIcon (foodType);

					GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
					mp.GetComponent<Button> ().interactable = true;
					mp.GetComponent<Image> ().color = rarityColors [GetComponent<MoneyController>().getFoodTier (food.name.ToLower()) - 1];//food.uiColor;

					GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
					fp.GetComponent<Image> ().color = Color.white;

					GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
					Color mcpColor = mp.GetComponent<Image> ().color;
					mcpColor.a = 166f / 255f;
					mcp.GetComponent<Image> ().color = mcpColor;

					GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
					mct.GetComponent<Text> ().color = affordableColor;

					GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
					Color rcpColor = mp.GetComponent<Image> ().color;
					rcpColor.a = 216f / 255f;
					rcp.GetComponent<Image> ().color = rcpColor;

					GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
					rct.GetComponent<Text> ().color = affordableColor;

					enoughMoneyForStore [foodType] = true;
					enoughResourcesForStore [foodType] = true;
				}
			}

			if (buildPrice > GetComponent<MoneyController>().getMoney () && enoughMoneyForStore [foodType]) {
				GameObject buildOption = GameObject.Find (foodType + "_build_option");
				if (buildOption == null) {
					
				} else {
					GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
					foodIcon.GetComponent<Image> ().sprite = getIcon (foodType + "_greyscale");

					GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
					mp.GetComponent<Button> ().interactable = false;
					mp.GetComponent<Image> ().color = Color.white;

					GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
					fp.GetComponent<Image> ().color = disabledColor * 2.5f;

					GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
					Color mcpColor = disabledColor;
					mcpColor.a = 166f / 255f;
					mcp.GetComponent<Image> ().color = mcpColor;

					GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
					mct.GetComponent<Text> ().color = unaffordableColor;

					GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
					Color rcpColor = disabledColor;
					rcpColor.a = 216f / 255f;
					rcp.GetComponent<Image> ().color = rcpColor;

					enoughMoneyForStore [foodType] = false;
				}
			} else if (buildPrice <= GetComponent<MoneyController>().getMoney () && !enoughMoneyForStore [foodType]) {
				GameObject buildOption = GameObject.Find (foodType + "_build_option");
				if (buildOption == null) {

				} else {

					GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
					mct.GetComponent<Text> ().color = affordableColor;

					enoughMoneyForStore [foodType] = true;
				}
			}

			if (GetComponent<MoneyController>().getResourceCount(foodType) < 1 && enoughResourcesForStore[foodType]) {
				GameObject buildOption = GameObject.Find (foodType + "_build_option");
				if (buildOption == null) {
				
				} else {
					GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
					foodIcon.GetComponent<Image> ().sprite = getIcon(foodType + "_greyscale");

					GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
					mp.GetComponent<Button> ().interactable = false;
					mp.GetComponent<Image> ().color = Color.white;

					GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
					fp.GetComponent<Image> ().color = disabledColor * 2.5f;

					GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
					Color mcpColor = disabledColor;
					mcpColor.a = 166f / 255f;
					mcp.GetComponent<Image> ().color = mcpColor;

					GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
					Color rcpColor = disabledColor;
					rcpColor.a = 216f / 255f;
					rcp.GetComponent<Image> ().color = rcpColor;

					GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
					rct.GetComponent<Text> ().color = unaffordableColor;

					enoughResourcesForStore [foodType] = false;
				}
			} else if (GetComponent<MoneyController>().getResourceCount(foodType) >= 1 && !enoughResourcesForStore[foodType]) {
				GameObject buildOption = GameObject.Find (foodType + "_build_option");
				if (buildOption == null) {
				
				} else {

					GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
					rct.GetComponent<Text> ().color = affordableColor;

					enoughResourcesForStore [foodType] = true;
				}
			}
		}
	}

	public void ManageGeneralBuildAccess(string type) {
		bool isFarm = false;
		if (type.Contains ("-")) {
			isFarm = true;
		}
			
		string foodType = mc.getFoodType (type);
		float buildPrice = 0;
		float resourcePrice = 0;
		if (isFarm) {
			buildPrice = GetComponent<MoneyController>().getFarmUpgradeCost (type);
			resourcePrice = 0;
		} else {
			buildPrice = GetComponent<MoneyController>().getStoreUpgradeCost (foodType);
			resourcePrice = 1;
		}

//		if (enoughMoneyForBuild == null) {
//			enoughMoneyForBuild = true;
//		}
//
//		if (enoughResourcesForBuild == null) {
//			enoughResourcesForBuild = true;
//		}

		if (GameObject.Find ("Build Option") != null) {
			if ((buildPrice <= GetComponent<MoneyController>().getMoney () && GetComponent<MoneyController>().getResourceCount (foodType) >= resourcePrice)) {// && (!enoughMoneyForBuild || !enoughResourcesForBuild)) {
				GameObject buildOption = GameObject.Find ("Build Option").transform.Find ("build_option_main").gameObject;
				if (buildOption == null) {

				} else {
					GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
					foodIcon.GetComponent<Image> ().sprite = getIcon(foodType);

					GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
					mp.GetComponent<Button> ().interactable = true;
					mp.GetComponent<Image> ().color = GetComponent<ItemController>().GetItem (foodType).uiColor;

					GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
					fp.GetComponent<Image> ().color = Color.white;

					GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
					Color mcpColor = mp.GetComponent<Image> ().color;
					mcpColor.a = 166f / 255f;
					mcp.GetComponent<Image> ().color = mcpColor;

					GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
					mct.GetComponent<Text> ().color = affordableColor;

					GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
					Color rcpColor = mp.GetComponent<Image> ().color;
					rcpColor.a = 216f / 255f;
					rcp.GetComponent<Image> ().color = rcpColor;

					GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
					rct.GetComponent<Text> ().color = affordableColor;

					GameObject rci = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostIcon").gameObject;
					rci.GetComponent<Image> ().sprite = getIcon(foodType);

					enoughMoneyForBuild = true;
					enoughResourcesForBuild = true;
				}
			} else {
				if (buildPrice > GetComponent<MoneyController>().getMoney ()) {//&& enoughMoneyForBuild) {
					GameObject buildOption = GameObject.Find ("Build Option").transform.Find ("build_option_main").gameObject;
					if (buildOption == null) {

					} else {
						GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
						foodIcon.GetComponent<Image> ().sprite = getIcon(foodType + "_greyscale");

						GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
						mp.GetComponent<Button> ().interactable = false;
						mp.GetComponent<Image> ().color = Color.white;

						GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
						fp.GetComponent<Image> ().color = disabledColor * 2.5f;

						GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
						Color mcpColor = disabledColor;
						mcpColor.a = 166f / 255f;
						mcp.GetComponent<Image> ().color = mcpColor;

						GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
						mct.GetComponent<Text> ().color = unaffordableColor;

						GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
						Color rcpColor = disabledColor;
						rcpColor.a = 216f / 255f;
						rcp.GetComponent<Image> ().color = rcpColor;

						enoughMoneyForBuild = false;
					}
				} else if (buildPrice <= GetComponent<MoneyController>().getMoney ()) {// && !enoughMoneyForBuild) {
					GameObject buildOption = GameObject.Find (foodType + "_build_option");
					if (buildOption == null) {

					} else {

						GameObject mct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
						mct.GetComponent<Text> ().color = affordableColor;

						enoughMoneyForBuild = true;
					}
				}
		
				if (GetComponent<MoneyController>().getResourceCount (foodType) < resourcePrice) {// && enoughResourcesForBuild) {
					GameObject buildOption = GameObject.Find ("Build Option").transform.Find ("build_option_main").gameObject;
					if (buildOption == null) {
		
					} else {
						GameObject foodIcon = buildOption.transform.Find ("FoodIcon").gameObject;
						foodIcon.GetComponent<Image> ().sprite = getIcon(foodType + "_greyscale");

						GameObject mp = buildOption.transform.Find ("BackgroundPanel").gameObject;
						mp.GetComponent<Button> ().interactable = false;
						mp.GetComponent<Image> ().color = Color.white;

						GameObject fp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
						fp.GetComponent<Image> ().color = disabledColor * 2.5f;

						GameObject mcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
						Color mcpColor = disabledColor;
						mcpColor.a = 166f / 255f;
						mcp.GetComponent<Image> ().color = mcpColor;

						GameObject rcp = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel").gameObject;
						Color rcpColor = disabledColor;
						rcpColor.a = 216f / 255f;
						rcp.GetComponent<Image> ().color = rcpColor;

						GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
						rct.GetComponent<Text> ().color = unaffordableColor;

						GameObject rci = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostIcon").gameObject;
						rci.GetComponent<Image> ().sprite = getIcon(foodType);

						enoughResourcesForBuild = false;
					}
				} else if (GetComponent<MoneyController>().getResourceCount (foodType) >= resourcePrice) {// && !enoughResourcesForBuild) {
					GameObject buildOption = GameObject.Find (foodType + "_build_option");
					if (buildOption == null) {

					} else {

						GameObject rct = buildOption.transform.Find ("BackgroundPanel/ForegroundPanel/ResourceCostPanel/ResourceCostText").gameObject;
						rct.GetComponent<Text> ().color = affordableColor;

						enoughResourcesForStore [type] = true;
					}
				}
			}
		}
	}

	public void OpenTileMenu(Tile t) {
		enoughResourcesForBuild = false;
		enoughMoneyForBuild = false;

		currentTile = t;

		if (!t.itemRevealed) {
			resourceText.text = "Unknown";
			resourcePanel.color = Color.gray;
			resourceIcon.sprite = Resources.Load<Sprite> ("Sprites/question_mark");
		} else {
			resourceText.text = "" + formatFoodName(t.item);
			resourcePanel.color = rarityColors[ic.GetItem(t.item.ToLower()).rarity-1];//ic.GetItem (t.item.ToLower()).uiColor;
			resourceIcon.sprite = getIcon(t.item);
			/*
			GameObject buildOption = Instantiate (Resources.Load("Prefabs/UI/BuildOption") as GameObject);
			BuildOption bo = buildOption.GetComponent<BuildOption> ();
			buildOption.transform.SetParent (tileBuildList.transform);
			if (t.farmTier == 0 && t.storeTier == 0)
				bo.UpdateName (itemInfo.f1Name, "farm", farmIcon);
			else if (t.farmTier == 1)
				bo.UpdateName (itemInfo.f2Name, "farm", farmIcon);
			else
				Destroy (buildOption);
			//--------------------------
			foreach (ItemController.Item item in GetComponent<ItemController>().items) {
				if (t.storeTier == 0 && item.count >= 1) {
					GameObject storeOption = Instantiate (Resources.Load ("Prefabs/UI/BuildOption") as GameObject);
					BuildOption so = storeOption.GetComponent<BuildOption> ();
					storeOption.transform.SetParent (tileBuildList.transform);
					so.UpdateName (item.s1Name, "store", null);
				} else if (t.storeTier == 1 && t.item == item.name && item.count >= 2) {
					GameObject storeOption = Instantiate (Resources.Load ("Prefabs/UI/BuildOption") as GameObject);
					BuildOption so = storeOption.GetComponent<BuildOption> ();
					storeOption.transform.SetParent (tileBuildList.transform);
					so.UpdateName (item.s2Name, "store", null);
				}
			}
			*/

		}
		if (!t.itemRevealed) {
			searchOption.SetActive (true);
		} 
		else {
			int bCount = mc.countBuildings (t.buildings);
			searchOption.SetActive (false);
			if (bCount == 0) {
				farmOption.SetActive (true);
				storeOption.SetActive (true);
			} else if (!t.buildings[0].Contains("-") && bCount < 3) {
				storeOption.SetActive (true);
				destroyOption.SetActive (true);
				upgradeOption.SetActive (true);
			} else if (bCount == 3) {
				destroyOption.SetActive (true);
				upgradeOption.SetActive (true);
			}
		}

		tileInfoPanel.SetActive (true);


		//tileInfoPanel.GetComponent<Animator> ().SetTrigger ("Open");

		//tileBuildPanel.SetActive (true);
		//tileBuildPanel.GetComponent<Animator> ().SetTrigger ("Open");

	}

	public void UpdateTileMenu() {
		enoughMoneyForBuild = false;
		enoughResourcesForBuild = false;
		Tile t = currentTile;
		int bCount = mc.countBuildings (t.buildings);
		farmOption.SetActive (false);
		destroyOption.SetActive (false);
		storeOption.SetActive (false);
		upgradeOption.SetActive (false);
		if (bCount == 0) {
			farmOption.SetActive (true);
			storeOption.SetActive (true);
		} else if (!t.buildings [0].Contains ("-") && bCount < 3) {
			storeOption.SetActive (true);
			destroyOption.SetActive (true);
			upgradeOption.SetActive (false);
			for (int i = 0; i < t.buildings.Count; i++) {
				if (t.buildings [i].Contains ("1")) {
					upgradeOption.SetActive (true);
					break;
				}
			}
		} else if (bCount == 3) {
			destroyOption.SetActive (true);
			upgradeOption.SetActive (false);
			for (int i = 0; i < t.buildings.Count; i++) {
				if (t.buildings [i].Contains ("1")) {
					upgradeOption.SetActive (true);
					break;
				}
			}
		} else if (t.buildings [0].Contains ("-") && mc.getBuildingTier (t.buildings [0]) < 3) {
			upgradeOption.SetActive (true);
			destroyOption.SetActive (true);
		} else {
			destroyOption.SetActive (true);
		}

		if (!t.itemRevealed) {
			searchOption.SetActive (true);
		} else {
			searchOption.SetActive (false);
		}

		tileOutputText.text = formatNumber (mc.getTileOutput (GetComponent<TileController>().selectedTile.getID () % 162)) + "/s";

		enoughResourcesForBuild = false;
		enoughMoneyForBuild = false;

		resourceText.text = "" + formatFoodName(t.item);
		resourcePanel.color = rarityColors[
			ic.GetItem(t.item.ToLower()).
			rarity-1];//ic.GetItem (t.item.ToLower()).uiColor;
		resourceIcon.sprite = getIcon(t.item);
	}

	public void CloseTileInfo() {
		currentTile = null;
		resourceText.text = "Unknown";
		//biomeText.text = "Biome: ???";
		tileInfoPanel.SetActive (false);
		tileBuildPanel.SetActive (false);
		buildWarning.SetActive (false);
		destroyWarning.SetActive (false);
		upgradeWarning.SetActive (false);
		farmOption.SetActive (false);
		destroyOption.SetActive (false);
		storeOption.SetActive (false);
		upgradeOption.SetActive (false);
	}

	public void OpenStoreBuildList() {
		HandleFullscreenMenuOpen ();
		tileBuildList.SetActive (true);
		UpdateStoreBuildText ();
	}

	public void StartStoreBuild() {
		CloseStoreBuildList ();
		buildWarning.SetActive (true);
	}

	public void CancelStoreBuild() {
		buildWarning.SetActive (false);
		GetComponent<TileController>().CancelStoreBuild ();
		HandleMenuClose ();
	}

	public void CancelStoreDestroy() {
		destroyWarning.SetActive (false);
		GetComponent<TileController>().CancelStoreDestroy ();
		HandleMenuClose ();
	}

	public void CancelStoreUpgrade() {
		upgradeWarning.SetActive (false);
		GetComponent<TileController>().CancelStoreUpgrade ();
		HandleMenuClose ();
	}

	public void OpenFarmOption() {
		HandleFullscreenMenuOpen ();
		tileBuildOption.SetActive (true);
		GetComponent<TileController>().StartFarmBuild ();
		UpdateFarmText ();
	}

	public void OpenUpgradeOption() {
		if (GetComponent<TileController>().selectedTile.buildings.Count == 0) {
			return;
		}
		else {
			if (GetComponent<TileController>().selectedTile.buildings[0].Contains("-")) {
				OpenUpgradeFarmOption();
			} else {
				HandleMenuOpen ();
				GetComponent<TileController>().StartStoreUpgrade();
			}
		}
	}

	public void OpenUpgradeFarmOption() {
		HandleFullscreenMenuOpen ();
		tileBuildOption.SetActive (true);
		GetComponent<TileController>().StartFarmUpgrade ();
		UpdateFarmText ();
	}

	public void OpenUpgradeStoreOption(int index) {
		HandleFullscreenMenuOpen ();
		tileBuildOption.SetActive (true);
		GetComponent<TileController>().ChooseStoreUpgrade (index);
		UpdateUpgradeText (index);
	}

	public void CloseGeneralBuildOption() {
		HandleFullscreenMenuClose ();
		tileBuildOption.SetActive (false);
	}

	public void CloseStoreBuildList() {
		HandleFullscreenMenuClose ();
		tileBuildList.SetActive (false);
	}

	public void OpenDestroyOption() {
		if (GetComponent<TileController>().selectedTile.buildings.Count == 0) {
			return;
		}
		else {
			if (GetComponent<TileController>().selectedTile.buildings[0].Contains("-")) {
				OpenFarmDestroyOption();
			} else {
				HandleMenuOpen ();
				GetComponent<TileController>().StartDestroy();
			}
		}
	}

	public void OpenFarmDestroyOption() {
		HandleFullscreenMenuOpen ();
		UnityEngine.Events.UnityAction action1 = () => {
			GetComponent<TileController>().DestroyFarm ();
		};
		AddActionToButton (destroyPanel.transform.Find ("DestroyButtons/DestroyConfirm").GetComponent<Button> (), action1);
		destroyPanel.SetActive (true);
		//GetComponent<TileController>().DestroyFarm ();
		UpdateDestroyText(0);
	}

	public void OpenStoreDestroyOption(int index) {
		HandleFullscreenMenuOpen ();
		UnityEngine.Events.UnityAction action1 = () => {
			GetComponent<TileController>().DestroyStore (index);
		};
		AddActionToButton (destroyPanel.transform.Find ("DestroyButtons/DestroyConfirm").GetComponent<Button> (), action1);
		destroyPanel.SetActive (true);
		UpdateDestroyText(index);
	}

	public void AddActionToButton(Button button, UnityEngine.Events.UnityAction action) {
		button.onClick.RemoveAllListeners ();
		button.onClick.AddListener (action);
	}

	public void CloseDestroyOption() {
		HandleFullscreenMenuClose ();
		destroyPanel.SetActive (false);
	}

	public void FinalizeStoreDestroy(int index) {
		CloseDestroyOption ();
	}

	public void TogglePauseMenu() {
		if (pauseMenu.activeInHierarchy) {
			pauseMenu.SetActive (false);
			optionsMenu.SetActive (false);
			Time.timeScale = 1;
			cam.enabled = true;
		} else {
			pauseMenu.SetActive (true);
			Time.timeScale = 0;
			cam.enabled = false;
		}
	}

	public void SearchTile() {
		if (currentTile != null) {
			GetComponent<MoneyController>().BuySearch ();
		}
		if (currentTile.itemRevealed) {
			searchOption.SetActive (false);
			resourceText.text = "" + formatFoodName(currentTile.item);
			resourcePanel.color = ic.GetItem (currentTile.item.ToLower()).uiColor;
			resourceIcon.sprite = getIcon(currentTile.item);
			farmOption.SetActive (true);
			storeOption.SetActive (true);
		}
		//OpenTileMenu (currentTile);
	}

	public void OpenBonusMenu() {
		HandleFullscreenMenuOpen ();
		UpdateBonusMenu ();
		bonusPanel.SetActive (true);
	}

	public void UpdateBonusMenu() {
		showBonuses = GetComponent<BonusController>().getDiscoveredBonuses ();
		foreach (Transform b in bonusList.transform) {
			if (showBonuses.Contains (b.name)) {
				b.gameObject.SetActive (true);
			} else {
				b.gameObject.SetActive (false);
			}
		}
	}

	public void CloseBonusMenu() {
		HandleFullscreenMenuClose ();
		bonusPanel.SetActive (false);
	}

	void ManageBonusAccess() {
		if (bonusPanel.activeInHierarchy) {
			foreach (Transform b in bonusList.transform) {
				if (!b.gameObject.activeInHierarchy) {
					continue;
				}
				if (!b.GetComponent<BonusOption> ()) {
					continue;
				}

				BonusOption bo = b.GetComponent<BonusOption> ();
				BonusController.Bonus bonus = bo.thisBonus;

				if (bonus.cost <= GetComponent<MoneyController>().getMoney ()) {
					if (bonus.bonusType == "group") {
						bo.DoubleIconImage1.sprite = getIcon (bonus.food1);
						bo.DoubleIconImage2.sprite = getIcon (bonus.food2);
					} else {
						bo.SingleIconImage.sprite = getIcon (bonus.food1);
					}

					GameObject bp = b.transform.Find ("BackgroundPanel").gameObject;
					bp.GetComponent<Button> ().interactable = true;
					bp.GetComponent<Image> ().color = rarityColors[bonus.bonusTier-1];

					GameObject fp = b.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
					fp.GetComponent<Image> ().color = Color.white;

					GameObject mcp = b.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
					Color mcpColor = bp.GetComponent<Image> ().color;
					mcp.GetComponent<Image> ().color = Color.black;

					GameObject mct = b.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
					mct.GetComponent<Text> ().color = affordableColor;
				} else {
					if (bonus.bonusType == "group") {
						bo.DoubleIconImage1.sprite = getIcon (bonus.food1+"_greyscale");
						bo.DoubleIconImage2.sprite = getIcon (bonus.food2+"_greyscale");
					} else {
						bo.SingleIconImage.sprite = getIcon (bonus.food1+"_greyscale");
					}

					GameObject bp = b.transform.Find ("BackgroundPanel").gameObject;
					bp.GetComponent<Button> ().interactable = false;
					bp.GetComponent<Image> ().color = Color.grey;

					GameObject fp = b.transform.Find ("BackgroundPanel/ForegroundPanel").gameObject;
					fp.GetComponent<Image> ().color = disabledColor * 2.5f;

					GameObject mcp = b.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel").gameObject;
					Color mcpColor = bp.GetComponent<Image> ().color;
					mcp.GetComponent<Image> ().color = mcpColor;

					GameObject mct = b.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").gameObject;
					mct.GetComponent<Text> ().color = unaffordableColor;
				}
			}
		}
	}

	public void InitializeBonusMenu() {
		List<BonusController.Bonus> allBonuses = GetComponent<BonusController>().allBonuses;

		foreach (BonusController.Bonus b in allBonuses) {
			GameObject newBonusPanel = Instantiate (bonusPanelPrefab) as GameObject;
			BonusOption bo = newBonusPanel.GetComponent<BonusOption> ();
			TooltipItem ti = newBonusPanel.GetComponentInChildren<TooltipItem> ();

			newBonusPanel.transform.SetParent (bonusList.transform);
			newBonusPanel.GetComponent<RectTransform> ().localScale = Vector3.one;
			newBonusPanel.GetComponent<RectTransform> ().localRotation = Quaternion.Euler (Vector3.zero);
			newBonusPanel.GetComponent<RectTransform> ().localPosition = Vector3.zero;
			newBonusPanel.name = "" + b.bonusID;

			string secondIcon = b.food2;

			GameObject bgPanel = newBonusPanel.transform.Find ("BackgroundPanel").gameObject;

			Text bonusName = newBonusPanel.transform.Find ("BackgroundPanel/ForegroundPanel/BuildName").GetComponent<Text> ();
			bonusName.text = b.bonusName;

			Text bonusDescription = newBonusPanel.transform.Find ("BackgroundPanel/ForegroundPanel/BuildDescription").GetComponent<Text> ();
			if (b.bonusDesc == "") { bonusDescription.text = "This bonus is indescribable."; }
			else { bonusDescription.text = b.bonusDesc; }

			Text bonusCostText = newBonusPanel.transform.Find ("BackgroundPanel/ForegroundPanel/MoneyCostPanel/MoneyCostText").GetComponent<Text> ();
			bonusCostText.text = formatNumber (b.cost);

			if (b.bonusType == "group") {
				bo.SingleIcon.SetActive (false);
				bo.DoubleIconImage1.sprite = getIcon (b.food1);
				bo.DoubleIconImage2.sprite = getIcon (b.food2);
			} else {
				bo.DoubleIcon.SetActive (false);
				bo.SingleIconImage.sprite = getIcon (b.food1);
			}

			if (newBonusPanel.GetComponent<BonusOption> ()) {
				newBonusPanel.GetComponent<BonusOption> ().thisBonus = b;
			}

			UnityEngine.Events.UnityAction action1 = () => {
				GetComponent<BonusController>().UnlockBonus(b.bonusID);
			};
			AddActionToButton (bgPanel.GetComponent<Button> (), action1);

			ti.bonusID = b.bonusID;
		}
	}

	public Sprite getIcon(string name) {
		Sprite icon = Resources.Load<Sprite> ("Sprites/" + name);
		if (icon == null) {
			icon = Resources.Load<Sprite> ("Sprites/question_mark");
		}
		return icon;
	}

	public string formatNumber(float num) {
		int numDigits = (int)Mathf.Floor (Mathf.Log10 (num) + 1);
		int effectivePower = (int)((numDigits - 1) / 3);
		int divisorPow = effectivePower * 3;
		if (numDigits < 7) {
			return num.ToString ("C0");
		} else if (numDigits >= 7 && numDigits < 16) {
			return "$" + (num / Mathf.Pow (10, divisorPow)).ToString ("##.00") + " " + shortNumberNames [effectivePower];
		} else {
			return "$" + (num / Mathf.Pow (10, divisorPow)).ToString ("###.#") + " " + shortNumberNames [effectivePower];
		}
	}

	public string shortFormatNumber(float num) {
		int numDigits = (int)Mathf.Floor (Mathf.Log10 (num) + 1);
		int effectivePower = (int)((numDigits - 1) / 3);
		int divisorPow = effectivePower * 3;
		if (numDigits < 4) {
			if (num % 1 != 0 && numDigits < 3)
				return num.ToString ("C1");
			else
				return num.ToString ("C0");
		} else {
			if (numDigits % 3 == 0)
				return "$" + (num / Mathf.Pow (10, divisorPow)).ToString ("###") + " " + shortNumberNames [effectivePower];
			else
				return "$" + (num / Mathf.Pow (10, divisorPow)).ToString ("###.#") + " " + shortNumberNames [effectivePower];
		}
	}

	public string formatMultiplier(float num) {
		int numDigits = (int)Mathf.Floor (Mathf.Log10 (num) + 1);
		int effectivePower = (int)((numDigits - 1) / 3);
		int divisorPow = effectivePower * 3;
		if (numDigits < 4) {
			if (num % 1 != 0 && numDigits < 3)
				return num.ToString ("F1");
			else
				return num.ToString ("F0");
		} else {
			if (numDigits % 3 == 0)
				return (num / Mathf.Pow (10, divisorPow)).ToString ("###") + " " + shortNumberNames [effectivePower];
			else
				return (num / Mathf.Pow (10, divisorPow)).ToString ("###.#") + " " + shortNumberNames [effectivePower];
		}
	}

	public string formatFoodName(string foodType) {
		TextInfo ti = new CultureInfo ("en-US", false).TextInfo;
		string newFood = foodType.Replace ("_", " ");
		newFood = ti.ToTitleCase (newFood);
		return newFood;
	}
}
