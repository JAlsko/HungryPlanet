//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.IO;
//using UnityEngine;
//using cakeslice;
//
//public class BuildController : MonoBehaviour {
//
//	private static BuildController instance = null;
//	public GameObject mainCam;
//	public GameObject focusedTile;
//	public int worldSeed = 0;
//	public Material pavement;
//
//	public Color buildColor;
//	public Color destroyColor;
//	public Color upgradeColor;
//
//	public bool buildingStore = false;
//	public bool destroying = false;
//	public bool upgrading = false;
//
//	public string itemToBuild = "";
//
//	//--------------------------------------------------------
//
//	[Serializable]
//	public class TileStat
//	{
//		public int biome;
//		public string item;
//		public bool revealed;
//		public List<BuildController.Building> buildings;
//	}
//
//	public List<TileStat> tileStats = new List<TileStat>();
//
//	//--------------------------------------------------------
//
//	public Dictionary<string, int> bc = new Dictionary<string, int>();
//
//	[Serializable]
//	public class BuildingCost
//	{
//		public string name;
//		public int cost;
//	}
//
//	public List<BuildingCost> buildingCosts = new List<BuildingCost>();    
//
//	//--------------------------------------------------------
//
//	public class Building
//	{
//		public string item;
//		public int tier;
//	}
//
//	//--------------------------------------------------------
//
//	public static BuildController Instance {
//		get {
//			return instance;
//		}
//	}
//
//	void Awake() {
//		if (instance != null && instance != this) 
//		{
//			Destroy(this.gameObject);
//		}
//			
//		instance = this;
//		DontDestroyOnLoad (this.gameObject);
//	}
//
//	void Start () {
//		for (int i = 0; i < buildingCosts.Count; i++) {
//			bc.Add (buildingCosts [i].name, buildingCosts [i].cost);
//			mainCam = GameObject.FindWithTag ("MainCamera");
//		}
//		//LoadTiles ();
//	}
//
//	public void buildNewStore(string item) {
//		focusedTile.GetComponent<MeshRenderer> ().material = pavement;
//		//focusedTile.transform.GetChild (3).gameObject.SetActive (true);
//		focusedTile.transform.GetChild (1).gameObject.SetActive (false);
//		GameObject buildingDecoration = Instantiate (Resources.Load ("Prefabs/Buildings/" + item + "_S1") as GameObject);
//		BuildController.Building newBuilding = new BuildController.Building ();
//		newBuilding.item = item;
//		newBuilding.tier = 1;
//		focusedTile.GetComponent<Tile> ().buildings.Add (newBuilding);
//		buildingDecoration.transform.SetParent (focusedTile.transform);
//		focusedTile.GetComponent<Tile>().placeObject (buildingDecoration);
//		buildingDecoration.transform.localScale = Vector3.one * .0025f;
//		buildingDecoration.transform.position = focusedTile.transform.GetChild (3).GetChild (0).transform.position;
//	}
//
//	public void chooseStoreSpot(string item) {
//		buildingStore = true;
//		focusedTile.transform.GetChild (3).gameObject.SetActive (true);
//		itemToBuild = item;
//		foreach (Transform t in focusedTile.transform.GetChild(3)) {
//			t.GetComponent<Outline> ().color = 2;
//		}
//	}
//
//	public void buildStore(Transform spot) {
//		int index = spot.GetSiblingIndex ();
//		if (itemToBuild != "") {
//			GameObject buildingDecoration = Instantiate (Resources.Load ("Prefabs/Buildings/" + itemToBuild + "_S1") as GameObject);
//			BuildController.Building newBuilding = new BuildController.Building ();
//			newBuilding.item = itemToBuild;
//			newBuilding.tier = 1;
//			focusedTile.GetComponent<Tile> ().buildings [index] = newBuilding;
//			buildingDecoration.transform.SetParent (focusedTile.transform);
//			focusedTile.GetComponent<Tile>().placeObject (buildingDecoration);
//			buildingDecoration.transform.localScale = Vector3.one * .0025f;
//			buildingDecoration.transform.position = focusedTile.transform.GetChild (3).GetChild (0).transform.position;
//			focusedTile.transform.GetChild (3).gameObject.SetActive (false);
//			itemToBuild = "";
//		}
//	}
//
//	public void populateTileStats() {
//		tileStats.Clear ();
//		foreach (Transform t in GameObject.Find("Hexsphere").transform) {
//			if (t.GetComponent<Tile> ()) {
//				TileStat ts = new TileStat ();
//				ts.biome = t.GetComponent<Tile> ().color;
//				ts.item = t.GetComponent<Tile> ().item;
//				ts.revealed = t.GetComponent<Tile> ().itemRevealed;
//				ts.buildings = t.GetComponent<Tile> ().buildings;
//				tileStats.Add (ts);
//			}
//		}
//	}
//
//	public void loadSavedWorld(List<TileStat> ts) {
//		Hexsphere world = GameObject.Find ("Hexsphere").GetComponent<Hexsphere> ();
//		world.setWorldScale (1);
//		world.deleteTiles ();
//		world.BuildPlanet ();
//		if (ts == null) {
//			worldSeed = (int)UnityEngine.Random.Range (0, Mathf.Pow (2, 16));
//			UnityEngine.Random.InitState (worldSeed);
//			world.generateRandomRegions ();
//			world.randomizeAllItems ();
//		} else {
//			int count = 0;
//			foreach (Transform t in world.transform) {
//				if (t.GetComponent<Tile> ()) {
//					t.GetComponent<Tile> ().setColor (ts [count].biome);
//					t.GetComponent<Tile> ().item = ts [count].item;
//					t.GetComponent<Tile> ().itemRevealed = ts [count].revealed;
//					//t.GetComponent<Tile>().building = ts[count].building;
//					count++;
//				}
//			}
//		}
//		world.setWorldScale (5);
//		world.extrudeLand ();
//	}
//
//	public void SaveTiles() {
//		populateTileStats ();
//		BinaryFormatter bf = new BinaryFormatter ();
//		FileStream file1 = File.Open (Application.persistentDataPath + "/tileInfo.dat", FileMode.OpenOrCreate);
//
//		bf.Serialize (file1, tileStats);
//		file1.Close ();
//
//		FileStream file2 = File.Open (Application.persistentDataPath + "/worldSeed.dat", FileMode.OpenOrCreate);
//		bf.Serialize (file2, worldSeed);
//		file2.Close ();
//	}
//
//	public void LoadTiles() {
//		if (File.Exists (Application.persistentDataPath + "/tileInfo.dat")) {
//			BinaryFormatter bf = new BinaryFormatter ();
//			FileStream file1 = File.Open (Application.persistentDataPath + "/tileInfo.dat", FileMode.Open);
//			List<TileStat> ts = (List<TileStat>)bf.Deserialize (file1);
//			file1.Close ();
//
//			loadSavedWorld (ts);
//
//			FileStream file2 = File.Open (Application.persistentDataPath + "/worldSeed.dat", FileMode.Open);
//			worldSeed = (int)bf.Deserialize (file2);
//			file2.Close ();
//		} else {
//			loadSavedWorld (null);
//		}
//	}
//
//	public void DeleteTileSave() {
//		if (File.Exists (Application.persistentDataPath + "/tileInfo.dat")) {
//			File.Delete (Application.persistentDataPath + "/tileInfo.dat");
//		}
//		if (File.Exists(Application.persistentDataPath + "/worldSeed.dat")) {
//			File.Delete (Application.persistentDataPath + "/worldSeed.dat");
//		}
//		LoadTiles ();
//	}
//
//	public void updateFocusedTile(GameObject tile) {
//		focusedTile = tile;
//	}
//
//	/*public void buyStore(string type, int level) {
//		if (focusedTile != null) {
//			float storeCost = 0;
//			foreach (BuildingCost building in buildingCosts) {
//				if (building.name == type + "_s" + level) {
//					storeCost = building.cost;
//				}
//			}
//			if (ItemController.Instance.GetMoney () >= storeCost && storeCost != 0 && focusedTile.GetComponent<Tile>().farm == "") {
//				ItemController.Instance.TransferMoney (-storeCost);
//				focusedTile.GetComponent<Tile> ().store = type + "_s" + level;
//			}
//		}
//	}*/
//
//	void FixedUpdate () {
//		//GameObject.Find ("Hexsphere").GetComponent<Hexsphere> ().setWorldScale (GameObject.Find ("Hexsphere").GetComponent<Hexsphere> ().planetScale + (.1f * Time.deltaTime));
//	}
//
//}
