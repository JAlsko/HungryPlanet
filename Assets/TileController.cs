using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using cakeslice;

public class TileController : MonoBehaviour {

	private static string tileSaveFileName = "tileInfo.dat";
	private static string seedSaveFileName = "worldSeed.dat";

	public GameObject mainCam;
	public GameObject focusedTile;
	public int worldSeed = 0;
	public Material pavement;

	public Color buildColor;
	public Color destroyColor;
	public Color upgradeColor;

	public bool building;
	public bool upgrading;
	public bool destroying;

	public string toBuild;

	public List<List<string>> tileBuildings = new List<List<string>> ();
	public Tile selectedTile;
	private MoneyController mc;

	private int selectedTileID = -1;

	public ParticleSystem storeBuildParticles;
	public ParticleSystem farmBuildParticles;

	[Serializable]
	public class TileStat
	{
		public int biome;
		public string item;
		public bool revealed;
		public List<string> buildings;
	}

	public List<TileStat> tileStats = new List<TileStat>();

	void Start () {
		mc = GetComponent<MoneyController>();
		mainCam = GameObject.FindWithTag ("MainCamera");
	}

	void Update () {
		if (selectedTile != null) {
			selectedTileID = selectedTile.getID ();
		}
	}

	public string getTileFood() {
		if (selectedTile.itemRevealed) {
			return selectedTile.item.ToLower();
		} else {
			return "???";
		}
	}

	public List<List<string>> getTileBuildings() {
		Debug.Log ("GetTileBuildings");
		List<List<string>> tb = new List<List<string>> ();
		Transform world = GameObject.Find ("Hexsphere").transform;
		for (int i = 0; i < world.GetComponent<Hexsphere>().TileObjects.Count; i++) {
			List<string> b = world.GetComponent<Hexsphere> ().TileObjects [i].GetComponent<Tile> ().buildings;
			if (b != null) {
				//Debug.Log (b[0] + b[1] + b[2]);
				tb.Add (b);
			}
			else
				tb.Add (new List<string> { "","","" });
		}
		return tb;
	}

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

	public void loadSavedWorld(List<TileStat> ts, int worldSeed) {
		Hexsphere world = GameObject.Find ("Hexsphere").GetComponent<Hexsphere> ();
		world.setWorldScale (1);
		world.deleteTiles ();
		world.BuildPlanet ();
		if (ts == null) {
			int seed = (int)UnityEngine.Random.Range (0, Mathf.Pow (2, 16));
			UnityEngine.Random.InitState (seed);
			world.generateRandomRegions ();
			world.randomizeAllItems ();
		} else {
			UnityEngine.Random.InitState (worldSeed);
			int count = 0;
			foreach (Transform t in world.transform) {
				if (t.GetComponent<Tile> ()) {
					t.GetComponent<Tile> ().setColor (ts [count].biome);
					t.GetComponent<Tile> ().item = ts [count].item;
					t.GetComponent<Tile> ().itemRevealed = ts [count].revealed;
					t.GetComponent<Tile> ().buildings = ts [count].buildings;
					count++;
				}
			}
		}
		world.setWorldScale (5);
		world.extrudeLand ();

		foreach (Transform t in world.transform) {
			if (t.GetComponent<Tile> ()) {
				if (t.GetComponent<Tile> ().itemRevealed) {
					PlaceResourceDecoration (t.GetComponent<Tile> ());
					PlaceBuildingDecorations (t);
				}
			}
		}

		GetComponent<LightController> ().ManageWorldLights ();
		GetComponent<MoneyController> ().calculateEarnings ();
	}

	void PlaceBuildingDecorations(Transform t) {
		if (t.GetComponent<Tile> ().buildings.Count <= 0)
			return;

		GameObject innerDecoration = t.transform.GetChild (1).gameObject;
		GameObject resourceDecoration = t.transform.GetChild (4).gameObject;
		GameObject roadDecoration = t.transform.GetChild (3).GetChild (3).gameObject;

		innerDecoration.SetActive (false);

		if (t.GetComponent<Tile> ().buildings [0].Contains ("-")) {
			resourceDecoration.SetActive (false);
			GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + t.GetComponent<Tile> ().item.ToLower () + "-1");
			if (buildingModel == null) {
				buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza-1");
			}
			GameObject buildingDecoration = Instantiate ((buildingModel) as GameObject);
			PlaceDecoration (t.GetComponent<Tile> (), buildingDecoration, .1f, .15f);
		} else {
			int bCount = 0;
			foreach (string toBuild in t.GetComponent<Tile>().buildings) {
				if (toBuild == "") {
					bCount++;
					continue;
				}
				roadDecoration.SetActive (true);
				resourceDecoration.SetActive (false);
				GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + toBuild);
				if (buildingModel == null) {
					buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza1");
				}
				GameObject buildingDecoration = Instantiate (buildingModel) as GameObject;
				Transform placeAt = t.GetChild (3).GetChild (bCount);

				PlaceBuilding (t.GetComponent<Tile> (), bCount, placeAt, buildingDecoration, .1f, .15f);
				bCount++;
			}
		}
	}

	public void SaveTiles() {
		populateTileStats ();
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file1 = File.Open (Application.persistentDataPath + "/" + tileSaveFileName, FileMode.OpenOrCreate);

		bf.Serialize (file1, tileStats);
		file1.Close ();

		FileStream file2 = File.Open (Application.persistentDataPath + "/" + seedSaveFileName, FileMode.OpenOrCreate);
		bf.Serialize (file2, worldSeed);
		file2.Close ();
	}

	public void LoadTiles() {
		if (File.Exists (Application.persistentDataPath + "/" + tileSaveFileName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file1 = File.Open (Application.persistentDataPath + "/" + tileSaveFileName, FileMode.Open);
			List<TileStat> ts = (List<TileStat>)bf.Deserialize (file1);
			file1.Close ();

			FileStream file2 = File.Open (Application.persistentDataPath + "/" + seedSaveFileName, FileMode.Open);
			worldSeed = (int)bf.Deserialize (file2);

			loadSavedWorld (ts, worldSeed);

			file2.Close ();
		} else {
			loadSavedWorld (null, 0);
		}

	}

	public void DeleteTileSave() {
		if (File.Exists (Application.persistentDataPath + "/" + tileSaveFileName)) {
			File.Delete (Application.persistentDataPath + "/" + tileSaveFileName);
		}
		if (File.Exists(Application.persistentDataPath + "/")) {
			File.Delete (Application.persistentDataPath + "/");
		}

		GetComponent<MoneyController>().DeleteMoneySave ();

		LoadTiles ();
	}

	//------------------------------------------

	public void HandleBuildSpots() {
		int i = 0;
		for (i = 0; i < selectedTile.buildings.Count; i++) {
			if ((selectedTile.buildings[i] == "" && building) || (selectedTile.buildings[i] != "" && upgrading && selectedTile.buildings[i].Contains("1") || (selectedTile.buildings[i] != "" && destroying))) {
				selectedTile.transform.GetChild (3).GetChild (i).GetChild (0).gameObject.SetActive (true);
			} else {
				selectedTile.transform.GetChild (3).GetChild (i).GetChild (0).gameObject.SetActive (false);
			}
		}
	}

	public void StartStoreBuild(string storeToBuild) {
		building = true;
		HandleBuildSpots ();
		toBuild = storeToBuild;
		if (mc.countBuildings (selectedTile.buildings) == 0) {
			NewTileStore ();
		}
	}

	public void ConfirmStoreBuild() {
		return;
	}

	public void FinalizeStoreBuild(int index) {
		BuildStore (index);
		GetComponent<UIController>().CancelStoreBuild ();
		GetComponent<UIController>().UpdateTileMenu ();
	}

	public void CancelStoreBuild() {
		building = false;
		HandleBuildSpots ();
		toBuild = "";
		if (mc.countBuildings (selectedTile.buildings) == 0) {
			CancelNewTileStore ();
		}
	}

	public void StartFarmBuild() {
		toBuild = selectedTile.item.ToLower() + "-1";
	}

	public void StartFarmUpgrade() {
		int currentTier = mc.getBuildingTier (selectedTile.buildings [0]);
		toBuild = mc.getFoodType (selectedTile.buildings [0]) + "-" + (int)(currentTier + 1);
	}

	public void FinalizeFarmBuild() {
		BuildFarm();
		GetComponent<UIController>().UpdateTileMenu ();
		GetComponent<UIController>().CloseGeneralBuildOption ();
		//CancelFarmBuild ();
	}

	public void StartStoreUpgrade() {
		upgrading = true;
		HandleBuildSpots ();
	}

	public void ChooseStoreUpgrade(int index) {
		toBuild = mc.getFoodType (selectedTile.buildings [index]) + "2";
	}

	public void CancelStoreUpgrade() {
		upgrading = false;
		HandleBuildSpots ();
		toBuild = "";
	}

	public void FinalizeStoreUpgrade(int index) {
		//toBuild = mc.getFoodType (selectedTile.buildings [index]) + "2";
		UpgradeStore (index);
		CancelStoreUpgrade ();
		GetComponent<UIController>().CloseGeneralBuildOption ();
		GetComponent<UIController>().UpdateTileMenu ();
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

	void PlaceResourceDecoration(Tile t) {
		GameObject innerDecoration = t.transform.GetChild (1).gameObject;

		innerDecoration.SetActive (false);
		GameObject resourceModel = Resources.Load<GameObject> ("Prefabs/" + t.item + "_inner");
		if (resourceModel == null) {
			resourceModel = Resources.Load<GameObject> ("Prefabs/hot_dogs_inner");
		}
		GameObject resourceDecoration = Instantiate (resourceModel) as GameObject;
		PlaceDecoration (t, resourceDecoration, .06f, .09f);
	}

	public void RevealTileResource() {
		selectedTile.itemRevealed = true;
		mc.discoverResource (selectedTile.item.ToLower ());

		GetComponent<UIController>().UpdateResourceUI ();
		GetComponent<UIController>().UpdateTileMenu ();

		PlaceResourceDecoration (selectedTile);

	}

	public void RevealTileResource(int num) {
		selectedTile.itemRevealed = true;
		mc.discoverResource (selectedTile.item.ToLower ());

		GetComponent<UIController>().UpdateResourceUI ();

		GameObject innerDecoration = selectedTile.transform.GetChild (1).gameObject;

		innerDecoration.SetActive (false);
		GameObject resourceModel = Resources.Load<GameObject> ("Prefabs/" + selectedTile.item + "_inner");
		if (resourceModel == null) {
			resourceModel = Resources.Load<GameObject> ("Prefabs/hot_dogs_inner");
		}
		GameObject resourceDecoration = Instantiate (resourceModel) as GameObject;
		PlaceDecoration (selectedTile, resourceDecoration, .06f, .09f);

	}

	public void PlaceDecoration(Tile t, GameObject deco, float pentScale, float hexScale) {
		if (!t.isHexagon) {
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * pentScale;
		} else if (t.isHexagon)
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * hexScale;
		deco.transform.SetParent (t.transform);
		t.placeObject (deco);
		deco.transform.Translate (Vector3.up * t.extrudedHeight / 2);
		deco.transform.Rotate (Vector3.up * UnityEngine.Random.Range (0, 180));
	}

	public void PlaceBuilding(Tile t, int index, Transform placeOn, GameObject deco, float pentScale, float hexScale) {
		if (!t.isHexagon) {
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * pentScale;
		} else if (t.isHexagon)
			deco.transform.localScale = Vector3.one * GameObject.Find("Hexsphere").GetComponent<Hexsphere>().planetScale * hexScale;

		placeOn.GetChild (0).gameObject.SetActive (false);

		deco.transform.SetParent (placeOn);
		deco.transform.position = placeOn.position;
		deco.transform.Translate (Vector3.up * 0.03f);
		deco.transform.localRotation = Quaternion.identity;
	}

	public void UpdateTileBuildings(List<List<string>> buildings) {
		tileBuildings = buildings;
	}

	public void NewTileStore() {
		GameObject resourceDecoration = selectedTile.transform.GetChild (4).gameObject;
		GameObject roadDecoration = selectedTile.transform.GetChild (3).GetChild (3).gameObject;

		resourceDecoration.SetActive (false);
		roadDecoration.SetActive (true);
	}

	public void CancelNewTileStore() {
		if (selectedTile == null) {
			return;
		}
		if (selectedTile.transform.childCount >= 5) {
			GameObject resourceDecoration = selectedTile.transform.GetChild (4).gameObject;
			resourceDecoration.SetActive (true);
		}

		GameObject roadDecoration = selectedTile.transform.GetChild (3).GetChild (3).gameObject;
		roadDecoration.SetActive (false);
	}

	void BuildStore(int index) {
		GameObject innerDecoration = selectedTile.transform.GetChild (1).gameObject;
		GameObject resourceDecoration = selectedTile.transform.GetChild (4).gameObject;

		innerDecoration.SetActive (false);
		resourceDecoration.SetActive (false);
		GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + toBuild);
		if (buildingModel == null) {
			buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza1");
		}
		GameObject buildingDecoration = Instantiate (buildingModel) as GameObject;
		Transform placeAt = selectedTile.transform.GetChild (3).GetChild (index);

		PlaceBuilding (selectedTile, index, placeAt, buildingDecoration, .1f, .15f);
		EmitStoreParticles (placeAt);

		GetComponent<SoundController> ().playBuildSound ();

		selectedTile.buildings [index] = toBuild;

		mc.AddStore (toBuild, selectedTileID, index);
		GetComponent<LightController>().ManageWorldLights ();
	}

	void BuildFarm() {
		GameObject innerDecoration = selectedTile.transform.GetChild (1).gameObject;
		GameObject resourceDecoration = selectedTile.transform.GetChild (4).gameObject;
		if (mc.getBuildingTier (toBuild) > 1) {
			GameObject currentFarmDecoration = selectedTile.transform.GetChild (5).gameObject;
			Destroy (currentFarmDecoration);
		}

		innerDecoration.SetActive (false);
		resourceDecoration.SetActive (false);
		GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + selectedTile.item + "-1");
		if (buildingModel == null) {
			buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza-1");
		}
		GameObject buildingDecoration = Instantiate ((buildingModel) as GameObject);
		PlaceDecoration (selectedTile, buildingDecoration, .1f, .15f);
		EmitFarmParticles (selectedTile);
		GetComponent<SoundController> ().playBuildSound ();

		selectedTile.buildings [0] = toBuild;

		mc.AddFarm (toBuild, selectedTileID);
		GetComponent<UIController>().UpdateResourceUI ();
		GetComponent<UIController>().UpdateTileMenu ();
		GetComponent<LightController>().ManageWorldLights ();
	}

	void EmitStoreParticles (Transform placeOn) {
		storeBuildParticles.transform.SetParent (placeOn);
		storeBuildParticles.transform.position = placeOn.position;
		storeBuildParticles.transform.Translate (Vector3.up * 0.03f);
		storeBuildParticles.transform.rotation = Quaternion.identity;
		storeBuildParticles.Play ();
	}

	void EmitFarmParticles (Tile t) {
		farmBuildParticles.transform.SetParent (t.transform);
		t.placeObject (farmBuildParticles.gameObject);
		storeBuildParticles.transform.rotation = Quaternion.identity;
		farmBuildParticles.transform.Translate (Vector3.up * t.extrudedHeight / 2);
		farmBuildParticles.Play ();
	}

	void UpgradeStore(int index) {
		GameObject currentStoreDecoration = selectedTile.transform.GetChild (3)
			.GetChild (index).
			GetChild(1).gameObject;
		Destroy (currentStoreDecoration);

		GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + toBuild);
		if (buildingModel == null) {
			buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza2");
		}
		GameObject buildingDecoration = Instantiate ((buildingModel) as GameObject);
		Transform placeAt = selectedTile.transform.GetChild (3).GetChild (index);

		PlaceBuilding (selectedTile, index, placeAt, buildingDecoration, .1f, .15f);

		selectedTile.buildings [index] = toBuild;

		mc.AddStore (toBuild, selectedTileID, index);

		GetComponent<UIController>().UpdateTileMenu ();
		GetComponent<LightController>().ManageWorldLights ();
	}

	public void DestroyStore(int index) {
		Transform placeAt = selectedTile.transform.GetChild (3).GetChild (index);
		placeAt.GetChild (0).gameObject.SetActive (true);
		Destroy(placeAt.GetChild (1).gameObject);

		GetComponent<UIController>().CloseDestroyOption ();
		GetComponent<SoundController> ().playDestroySound ();

		mc.RemoveStore (selectedTile.buildings[index]/*tileBuildings[selectedTileID][index]*/, selectedTileID);
		selectedTile.buildings [index] = "";

		if (mc.countBuildings (selectedTile.buildings) == 0) {
			selectedTile.transform.GetChild (4).gameObject.SetActive(true);
			selectedTile.transform.GetChild (3).GetChild (3).gameObject.SetActive (false);
		}

		destroying = false;

		GetComponent<UIController>().UpdateTileMenu ();
	}

	public void DestroyFarm() {
		GameObject resourceDecoration = selectedTile.transform.GetChild (4).gameObject;

		resourceDecoration.SetActive (true);

		Destroy(selectedTile.transform.GetChild (5).gameObject);

		selectedTile.buildings [0] = "";

		GetComponent<UIController>().CloseDestroyOption ();
		GetComponent<SoundController> ().playDestroySound ();

		mc.RemoveFarm(tileBuildings[selectedTileID][0], selectedTileID);

		destroying = false;

		GetComponent<UIController>().UpdateTileMenu ();
	}

	void UpgradeFarm() {
		string building = tileBuildings [selectedTileID] [0];
		mc.AddFarm(mc.getFoodType(building)+"-"+(mc.getBuildingTier(building)+1), selectedTileID);
	}
}
