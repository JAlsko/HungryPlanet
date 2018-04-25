using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using cakeslice;

public class TileController : MonoBehaviour {

	private static string TILE_SAVE_NAME = "tileInfo.dat";					//Filename for saving tile stats
	private static string SEED_SAVE_NAME = "worldSeed.dat";					//Filename for saving world random seed

	private int worldSeed = 0;												//World random seed

	[SerializeField] private Color buildColor;								//Highlight color for building
	[SerializeField] private Color destroyColor;							//Highlight color for destroying
	[SerializeField] private Color upgradeColor;							//Highlight color for upgrading

	[SerializeField] private bool building;									//Currently building check
	[SerializeField] private bool upgrading;								//Currently upgrading check
	[SerializeField] private bool destroying;								//Currently destroying check

	[SerializeField] private string toBuild;								//Name of building set to build next

	[SerializeField] private List<List<string>> tileBuildings = 			//Accessable list of buildings per tile
		new List<List<string>> ();
	[SerializeField] private Tile selectedTile;								//Currently focused tile

	private int selectedTileID = -1;										//Quicker way to access the tileID of the focused tile

	[SerializeField] private ParticleSystem storeBuildParticles;			//Particle system to play when building stores
	[SerializeField] private ParticleSystem farmBuildParticles;				//Particle system to play when building farms

	//Format for encapsulating stats for each tile
	[Serializable]
	public class TileStat
	{
		public int biome;
		public string item;
		public bool revealed;
		public List<string> buildings;
	}

	private List<TileStat> tileStats = new List<TileStat>();				//List of all tile stats used for loading/saving

	//Controller access
	private MoneyController mc;	
	private LightController lc;
	private UIController uic;
	private SoundController sc;

	void Start () {
		mc = GetComponent<MoneyController>();
		lc = GetComponent<LightController>();
		uic = GetComponent<UIController>();
		sc = GetComponent<SoundController>();
	}

	void Update () {
		//Update selectedTileID from selected tile
		if (selectedTile != null) {
			selectedTileID = selectedTile.getID ();
		}
	}

	//Get name of food on selected tile
	public string getTileFood() {
		if (selectedTile.itemRevealed) {
			return selectedTile.item.ToLower();
		} else {
			return "???";
		}
	}

	//Build up list of buildings per tile
	public List<List<string>> getTileBuildings() {
		List<List<string>> tb = new List<List<string>> ();
		Transform world = GameObject.Find ("Hexsphere").transform;
		Hexsphere hex = world.GetComponent<Hexsphere> ();
		for (int i = 0; i < hex.TileObjects.Count; i++) {
			List<string> b = hex.TileObjects [i].GetComponent<Tile> ().buildings;
			if (b != null) {
				tb.Add (b);
			}
			else
				tb.Add (new List<string> { "","","" });
		}
		return tb;
	}

	//Update tile stats list
	public void populateTileStats() {
		tileStats.Clear ();
		foreach (Transform t in GameObject.Find("Hexsphere").transform) {
			if (t.GetComponent<Tile> ()) {
				TileStat ts = new TileStat ();
				ts.biome = t.GetComponent<Tile> ().color;
				ts.item = t.GetComponent<Tile> ().item;
				ts.revealed = t.GetComponent<Tile> ().itemRevealed;
				ts.buildings = t.GetComponent<Tile> ().buildings;
				tileStats.Add (ts);
			}
		}
	}

	//World loading sequence
	public void loadSavedWorld(List<TileStat> ts, int worldSeed) {
		Hexsphere world = GameObject.Find ("Hexsphere").GetComponent<Hexsphere> ();
		world.setWorldScale (1); //Shrink world (to handle resizing issue);
		world.deleteTiles (); //Clear world
		world.BuildPlanet (); //Build base tiles

		//If no save, generate new world
		if (ts == null) {
			int seed = (int)UnityEngine.Random.Range (0, Mathf.Pow (2, 16)); //New random world seed
			UnityEngine.Random.InitState (seed);
			world.generateRandomRegions (); //Generate biomes randomly
			world.randomizeAllItems (); //Generate food placements randomly
		} 

		//If save, load tile stats
		else {
			UnityEngine.Random.InitState (worldSeed);
			int count = 0;
			//Loop through tile objects of parent planet
			foreach (Transform t in world.transform) {
				if (t.GetComponent<Tile> ()) {
					t.GetComponent<Tile> ().setColor (ts [count].biome); //Update tile biome from save
					t.GetComponent<Tile> ().item = ts [count].item; //Update tile food from save
					t.GetComponent<Tile> ().itemRevealed = ts [count].revealed; //Show food if tile is searched 
					t.GetComponent<Tile> ().buildings = ts [count].buildings; //Update tile buildings from save
					count++;
				}
			}
		}
		world.setWorldScale (5); //Resize planet to full
		world.extrudeLand (); //Raise tiles based on biome

		//Placing food and building models
		foreach (Transform t in world.transform) {
			if (t.GetComponent<Tile> ()) {
				if (t.GetComponent<Tile> ().itemRevealed) {
					PlaceResourceDecoration (t.GetComponent<Tile> ()); //If tile is searched, place food model
					PlaceBuildingDecorations (t); //If tile has buildings, place models for those
				}
			}
		}

		lc.ManageWorldLights (); //After placing buildings, update their lights for daytime
		mc.calculateEarnings (); //Update money-per-second based on loaded buildings
	}

	GameObject findChildWithTag(Transform parent, string findTag) {
		foreach (Transform t in parent) {
			if (t.tag == findTag) {
				return t.gameObject;
			}
		}
		Debug.Log ("Couldn't find child with tag " + findTag);
		return null;
	}

	GameObject tryLoadBuilding(string buildingName) {
		GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + buildingName);
		if (buildingModel == null) {
			buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza-1");
		}
		return buildingModel;
	}

	GameObject tryLoadProp(string propName) {
		GameObject propModel = Resources.Load<GameObject> ("Prefabs/" + propName);
		if (propModel == null) {
			propModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza_inner");
		}
		return propModel;
	}

	void PlaceBuildingDecorations(Transform t) {
		if (t.GetComponent<Tile> ().buildings.Count <= 0)
			return;

		GameObject innerDecoration = findChildWithTag (t.transform, "BiomeInnerDeco"); //Get biome inner decoration
		GameObject resourceDecoration = findChildWithTag (t.transform, "FoodDeco"); //Get food decoration
		GameObject roadDecoration = findChildWithTag (findChildWithTag (t.transform, "BuildSpot").transform, "RoadDeco"); //Get stores' base decoration

		if (innerDecoration != null)
			innerDecoration.SetActive (false); //Hide inner decoration

		//For farm tiles
		if (t.GetComponent<Tile> ().buildings [0].Contains ("-")) {
			if (resourceDecoration != null)
				resourceDecoration.SetActive (false); //Hide food decoration

			//Load farm model and instantiate/place it
			GameObject buildingModel = tryLoadBuilding(t.GetComponent<Tile> ().item.ToLower () + "-1");
			GameObject buildingDecoration = Instantiate ((buildingModel) as GameObject);
			PlaceDecoration (t.GetComponent<Tile> (), buildingDecoration, .1f, .15f);
		} 

		//If no farm, check for stores
		else {
			int bCount = 0; //Keep track of index in buildings list
			foreach (string toBuild in t.GetComponent<Tile>().buildings) {
				//If no building, move to next slot
				if (toBuild == "") {
					bCount++;
					continue;
				}

				//Cancel building placement if road decoration doesn't exist
				if (roadDecoration == null) {
					Debug.Log ("No road decoration for this tile!");
					break;
				}

				roadDecoration.SetActive (true);
				if (resourceDecoration != null)
					resourceDecoration.SetActive (false); //Hide food decoration
				GameObject buildingModel = tryLoadBuilding(toBuild);
				GameObject buildingDecoration = Instantiate (buildingModel) as GameObject;

				//Get proper build spot to place building, place it
				Transform placeAt = findChildWithTag(t.transform, "BuildSpot").transform.GetChild (bCount);
				PlaceBuilding (t.GetComponent<Tile> (), bCount, placeAt, buildingDecoration, .1f, .15f);

				bCount++;
			}
		}
	}

	public void SaveTiles() {
		//Fill and save tile stats
		populateTileStats ();
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file1 = File.Open (Application.persistentDataPath + "/" + TILE_SAVE_NAME, FileMode.OpenOrCreate);
		bf.Serialize (file1, tileStats);
		file1.Close ();

		//Get and save world random seed
		FileStream file2 = File.Open (Application.persistentDataPath + "/" + SEED_SAVE_NAME, FileMode.OpenOrCreate);
		bf.Serialize (file2, worldSeed);
		file2.Close ();
	}

	public void LoadTiles() {
		if (File.Exists (Application.persistentDataPath + "/" + TILE_SAVE_NAME)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file1 = File.Open (Application.persistentDataPath + "/" + TILE_SAVE_NAME, FileMode.Open);
			List<TileStat> ts = (List<TileStat>)bf.Deserialize (file1);
			file1.Close ();

			if (File.Exists (Application.persistentDataPath + "/" + SEED_SAVE_NAME)) {
				FileStream file2 = File.Open (Application.persistentDataPath + "/" + SEED_SAVE_NAME, FileMode.Open);
				worldSeed = (int)bf.Deserialize (file2);
				file2.Close ();
			} else {
				Debug.Log ("No seed save file found! Generating new seed");
				worldSeed = (int)UnityEngine.Random.Range (0, Mathf.Pow (2, 16)); //New random world seed
			}

			loadSavedWorld (ts, worldSeed);
		} else {
			loadSavedWorld (null, 0);
		}

	}

	public void DeleteTileSave() {
		if (File.Exists (Application.persistentDataPath + "/" + TILE_SAVE_NAME)) {
			File.Delete (Application.persistentDataPath + "/" + TILE_SAVE_NAME);
		}
		if (File.Exists(Application.persistentDataPath + "/")) {
			File.Delete (Application.persistentDataPath + "/");
		}

		//Mandatory to delete money save as it pulls from tile save
		mc.DeleteMoneySave ();

		//Reload new tiles
		LoadTiles ();
	}

	//------------------------------------------

	//Show or hide build spot highlights depending on build status and type of building on that spot
	public void HandleBuildSpots() {
		int i = 0;
		for (i = 0; i < selectedTile.buildings.Count; i++) {
			GameObject buildSpots = findChildWithTag (selectedTile.transform, "BuildSpot");
			if (buildSpots == null) {
				Debug.Log ("No build spots! Cancelling 'HandleBuildSpots'");
				return;
			}

			//If no existing building and in 'build' mode -- or if lvl 1 building and in 'upgrade' mode -- or if some building and in 'destroy' mode... show highlight
			if ((selectedTile.buildings[i] == "" && building) || (selectedTile.buildings[i] != "" && upgrading && selectedTile.buildings[i].Contains("1") || (selectedTile.buildings[i] != "" && destroying))) {
				buildSpots.transform.GetChild (i).GetChild (0).gameObject.SetActive (true);
			} else {
				buildSpots.transform.GetChild (i).GetChild (0).gameObject.SetActive (false);
			}
		}
	}

	//Start store build selection process
	public void StartStoreBuild(string storeToBuild) {
		building = true;
		HandleBuildSpots ();
		toBuild = storeToBuild;

		//If first store on tile, use new tile store method
		if (mc.countBuildings (selectedTile.buildings) == 0) {
			NewTileStore ();
		}
	}

	public void ConfirmStoreBuild() {
		return;
	}

	//On selection of build spot, build the store
	public void FinalizeStoreBuild(int index) {
		BuildStore (index);
		uic.CancelStoreBuild (); //Close store build UI
		uic.UpdateTileMenu (); //Update tile menu to show proper tile earnings
	}

	//Disable all building selection
	public void CancelStoreBuild() {
		building = false;
		HandleBuildSpots ();
		toBuild = "";

		//If was previously building first store on tile and canceled, hide road decoration
		if (mc.countBuildings (selectedTile.buildings) == 0) {
			CancelNewTileStore ();
		}
	}

	//Queue up proper farm type to be built after confirmation
	public void StartFarmBuild() {
		toBuild = selectedTile.item.ToLower() + "-1";
	}

	//Queue up proper farm upgrade to be built after confirmation
	public void StartFarmUpgrade() {
		int currentTier = mc.getBuildingTier (selectedTile.buildings [0]);
		toBuild = mc.getFoodType (selectedTile.buildings [0]) + "-" + (int)(currentTier + 1);
	}
		
	public void FinalizeFarmBuild() {
		BuildFarm();
		uic.UpdateTileMenu (); //Update resource counts
		uic.CloseGeneralBuildOption (); //Close farm confirmation UI
		//CancelFarmBuild ();
	}

	//Start store upgrade selection
	public void StartStoreUpgrade() {
		upgrading = true;
		HandleBuildSpots ();
	}

	//Queue up store upgrade to be built after confirmation
	public void ChooseStoreUpgrade(int index) {
		toBuild = mc.getFoodType (selectedTile.buildings [index]) + "2";
	}

	//End upgrade selection
	public void CancelStoreUpgrade() {
		upgrading = false;
		HandleBuildSpots ();
		toBuild = "";
	}

	//Apply upgrade to proper building
	public void FinalizeStoreUpgrade(int index) {
		UpgradeStore (index);
		CancelStoreUpgrade ();
		uic.CloseGeneralBuildOption (); //Close upgrade confirmation UI
		uic.UpdateTileMenu (); //Update tile menu to reflect upgraded tile output
	}

	public void StartDestroy() {
		if (selectedTile.buildings [0].Contains ("-")) {
			//open destroy option with farm price
		} else {
			destroying = true;
			HandleBuildSpots ();
		}
	}

	public void CancelStoreDestroy() {
		destroying = false;
		HandleBuildSpots ();
	}

	//Place proper food model on tile
	void PlaceResourceDecoration(Tile t) {
		GameObject innerDecoration = findChildWithTag(t.transform, "BiomeInnerDeco");
		if (innerDecoration != null)
			innerDecoration.SetActive (false);
		GameObject resourceModel = tryLoadProp(t.item + "_inner");
		GameObject resourceDecoration = Instantiate (resourceModel) as GameObject;
		PlaceDecoration (t, resourceDecoration, .06f, .09f);
	}

	//Reveal current tile food
	public void RevealTileResource() {
		selectedTile.itemRevealed = true;
		mc.discoverResource (selectedTile.item.ToLower ());

		uic.UpdateResourceUI ();
		uic.UpdateTileMenu ();

		PlaceResourceDecoration (selectedTile);

	}

	//Reveal current tile food
	public void RevealTileResource(int num) {
		selectedTile.itemRevealed = true;
		mc.discoverResource (selectedTile.item.ToLower ());

		uic.UpdateResourceUI ();

		GameObject innerDecoration = findChildWithTag(selectedTile.transform, "BiomeInnerDeco");
		if (innerDecoration != null)
			innerDecoration.SetActive (false);
		GameObject resourceModel = tryLoadProp(selectedTile.item + "_inner");
		GameObject resourceDecoration = Instantiate (resourceModel) as GameObject;
		PlaceDecoration (selectedTile, resourceDecoration, .06f, .09f);

	}

	//Place decoration in middle of tile
	public void PlaceDecoration(Tile t, GameObject deco, float pentScale, float hexScale) {
		//Scale model depending if tile is hexagon or pentagon
		if (!t.isHexagon) {
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * pentScale;
		} else if (t.isHexagon)
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * hexScale;

		deco.transform.SetParent (t.transform); //Parent decoration to tile
		t.placeObject (deco); //Use tile placement to orient decoration to tile rotation
		deco.transform.Translate (Vector3.up * t.extrudedHeight / 2); //Move decoration up to match tile elevation
		deco.transform.Rotate (Vector3.up * UnityEngine.Random.Range (0, 180)); //Spin random amount
	}

	//Place store model at indexed build spot
	public void PlaceBuilding(Tile t, int index, Transform placeOn, GameObject deco, float pentScale, float hexScale) {
		//Scale model depending if tile is hexagon or pentagon
		if (!t.isHexagon) {
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * pentScale;
		} else if (t.isHexagon)
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * hexScale;

		placeOn.GetChild (0).gameObject.SetActive (false); //Hide selection box

		deco.transform.SetParent (placeOn); //Parent building to build spot
		deco.transform.position = placeOn.position; //Update position to build spot
		deco.transform.Translate (Vector3.up * 0.03f); //Move building up small amount to avoid clipping
		deco.transform.localRotation = Quaternion.identity; //Match building to build spot rotation
	}

	public void UpdateTileBuildings(List<List<string>> buildings) {
		tileBuildings = buildings;
	}

	//Method for placing store on empty tile
	public void NewTileStore() {
		GameObject resourceDecoration = findChildWithTag(selectedTile.transform, "FoodDeco");
		GameObject roadDecoration = findChildWithTag (findChildWithTag (selectedTile.transform, "BuildSpot").transform, "RoadDeco");

		if (resourceDecoration != null)
			resourceDecoration.SetActive (false);
		if (roadDecoration == null) {
			Debug.Log ("Can't find road decoration for 'NewTileStore'");
			return;
		}
		roadDecoration.SetActive (true);
	}

	//Hide road decoration when new tile store building is cancelled
	public void CancelNewTileStore() {
		if (selectedTile == null) {
			return;
		}
		GameObject resourceDecoration = findChildWithTag(selectedTile.transform, "FoodDeco");
		if (resourceDecoration != null)
			resourceDecoration.SetActive (true);

		GameObject roadDecoration = findChildWithTag (findChildWithTag (selectedTile.transform, "BuildSpot").transform, "RoadDeco");
		if (roadDecoration != null)
			roadDecoration.SetActive (false);
	}

	void BuildStore(int index) {
		//Hide biome and food models
		GameObject innerDecoration = findChildWithTag(selectedTile.transform, "BiomeInnerDeco");
		GameObject resourceDecoration = findChildWithTag(selectedTile.transform, "FoodDeco");
		if (innerDecoration != null)
			innerDecoration.SetActive (false);
		if (resourceDecoration != null)
			resourceDecoration.SetActive (false);
		
		GameObject buildingModel = tryLoadBuilding(toBuild); //Get model of building
		GameObject buildingDecoration = Instantiate (buildingModel) as GameObject; //Add model to scene
		Transform placeAt = findChildWithTag(selectedTile.transform, "BuildSpot").transform.GetChild (index); //Get build spot to attach building to

		PlaceBuilding (selectedTile, index, placeAt, buildingDecoration, .1f, .15f); //Place building model

		EmitStoreParticles (placeAt); //Play building particle effect
		sc.playBuildSound (); //Play building sound effect

		selectedTile.buildings [index] = toBuild; //Update tile building list with new building

		mc.AddStore (toBuild, selectedTileID, index); //Manage store addition with money controller
		lc.ManageWorldLights (); //Update new building's lights
	}

	void BuildFarm() {
		//Hide biome and food models
		GameObject innerDecoration = selectedTile.transform.GetChild (1).gameObject;
		GameObject resourceDecoration = selectedTile.transform.GetChild (4).gameObject;
		if (innerDecoration != null)
			innerDecoration.SetActive (false);
		if (resourceDecoration != null)
			resourceDecoration.SetActive (false);

		//If upgrading farm, replace farm model (unused for now since upgraded farms have same models)
		if (mc.getBuildingTier (toBuild) > 1) {
			GameObject currentFarmDecoration = selectedTile.transform.GetChild (5).gameObject;
			Destroy (currentFarmDecoration);
		}
			
		GameObject buildingModel = tryLoadBuilding(selectedTile.item + "-1"); //Get model of farm
		GameObject buildingDecoration = Instantiate ((buildingModel) as GameObject); //Add model to scene
		PlaceDecoration (selectedTile, buildingDecoration, .1f, .15f); //Place farm model

		EmitFarmParticles (selectedTile); //Play farm building particle effect
		sc.playBuildSound (); //Play building sound effect

		selectedTile.buildings [0] = toBuild; //Update first index of tile's building list with farm

		mc.AddFarm (toBuild, selectedTileID); //Manage farm addition in money controller

		uic.UpdateResourceUI (); //Reload UI to reflect new resource
		uic.UpdateTileMenu (); //Update tile menu to reflect new farm
		lc.ManageWorldLights (); //Update new farm's lights
	}

	//Place store particles at proper build spot and then emit
	void EmitStoreParticles (Transform placeOn) {
		storeBuildParticles.transform.SetParent (placeOn);
		storeBuildParticles.transform.position = placeOn.position;
		storeBuildParticles.transform.Translate (Vector3.up * 0.03f); //Prevent ground clipping
		storeBuildParticles.transform.rotation = Quaternion.identity;
		storeBuildParticles.Play ();
	}

	//Place farm particles at proper tile and then emit
	void EmitFarmParticles (Tile t) {
		farmBuildParticles.transform.SetParent (t.transform);
		t.placeObject (farmBuildParticles.gameObject);
		storeBuildParticles.transform.rotation = Quaternion.identity;
		farmBuildParticles.transform.Translate (Vector3.up * t.extrudedHeight / 2); //Handle tile's elevation
		farmBuildParticles.Play ();
	}
		
	void UpgradeStore(int index) {
		//Get old store model and destroy it
		GameObject currentStoreDecoration = findChildWithTag (findChildWithTag (selectedTile.transform, "BuildSpot").transform.GetChild (index).transform, "StoreDeco");
		Destroy (currentStoreDecoration);

		GameObject buildingModel = tryLoadBuilding(toBuild); //Get new model
		GameObject buildingDecoration = Instantiate ((buildingModel) as GameObject); //Place building in scene
		Transform placeAt = findChildWithTag (selectedTile.transform, "BuildSpot").transform.GetChild (index);

		PlaceBuilding (selectedTile, index, placeAt, buildingDecoration, .1f, .15f); //Place building model at proper build spot

		selectedTile.buildings [index] = toBuild; //Update tile's building list with new upgrade

		mc.AddStore (toBuild, selectedTileID, index); //Handle new building in money controller

		uic.UpdateTileMenu (); //Update tile menu to reflect new tile output
		lc.ManageWorldLights (); //Update store's lights
	}

	public void DestroyStore(int index) {
		Transform placeAt = findChildWithTag (selectedTile.transform, "BuildSpot").transform.GetChild (index);
		placeAt.GetChild (0).gameObject.SetActive (true); //Enable selection box for build spot
		Destroy(findChildWithTag(placeAt, "StoreDeco"));

		uic.CloseDestroyOption (); //End destroy selection
		sc.playDestroySound ();

		mc.RemoveStore (selectedTile.buildings[index], selectedTileID); //Handle building destruction in money controller
		selectedTile.buildings [index] = ""; //Change the tile's building list to reflect building removal

		if (mc.countBuildings (selectedTile.buildings) == 0) {
			GameObject resourceDecoration = findChildWithTag (selectedTile.transform, "FoodDeco");
			if (resourceDecoration != null)
				resourceDecoration.SetActive (true);
			findChildWithTag (findChildWithTag (selectedTile.transform, "BuildSpot").transform, "RoadDeco").SetActive (false);
		}

		destroying = false;

		uic.UpdateTileMenu (); //Update tile menu to reflect new tile output
	}

	public void DestroyFarm() {
		GameObject resourceDecoration = findChildWithTag (selectedTile.transform, "FoodDeco");
		if(resourceDecoration != null)
			resourceDecoration.SetActive (true);

		//Remove old farm decoration
		GameObject oldFarmDecoration = findChildWithTag (selectedTile.transform, "FarmDeco");
		Destroy(oldFarmDecoration);

		selectedTile.buildings [0] = ""; //Change the tile's building list to reflect farm removal

		uic.CloseDestroyOption (); //End destroy selection
		sc.playDestroySound ();

		mc.RemoveFarm(tileBuildings[selectedTileID][0], selectedTileID);

		destroying = false;

		uic.UpdateTileMenu ();
	}

	void UpgradeFarm() {
		string building = tileBuildings [selectedTileID] [0]; //Get current farm
		mc.AddFarm(mc.getFoodType(building)+"-"+(mc.getBuildingTier(building)+1), selectedTileID); //Handle addition of upgraded farm in money controller
	}

	public Tile getSelectedTile() {
		return selectedTile;
	}

	public void setSelectedTile(Tile newTile) {
		selectedTile = newTile;
	}

	public string getToBuild() {
		return toBuild;
	}

	public void setToBuild(string newBuild) {
		toBuild = newBuild;
	}

	public bool getBuildStatus(string buildAction) {
		switch(buildAction) {
		case "build":
			return building;
		case "upgrade":
			return upgrading;
		case "destroy":
			return destroying;
		default:
			return false;
		}
	}

	public Color getBuildColor(string buildAction) {
		switch(buildAction) {
		case "build":
			return buildColor;
		case "upgrade":
			return upgradeColor;
		case "destroy":
			return destroyColor;
		default:
			return Color.white;
		}
	}
}
