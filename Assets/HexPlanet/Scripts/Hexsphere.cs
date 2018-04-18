using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Hexsphere : MonoBehaviour {

	public static List<Hexsphere> planetInstances = new List<Hexsphere> ();
	public static int Planet_ID;
	public static float unitScale;

	public List<int> biomeCounts = new List<int>( new int[] {0, 0, 0, 0, 0} );

	public List<List<string>> biomeFoods = new List<List<string>>();
	public Dictionary<string, int> foodTiers = new Dictionary<string, int>();
	public Dictionary<int, List<string>> foodToAdd = new Dictionary<int, List<string>> ();
	public List<int> countPerTier = new List<int> ();
	public List<int> specialCountPerTier = new List<int> ();

	private static System.Random irng = new System.Random();  

	public List<int> poleIndices = new List<int> ();

	public GameObject gameController;

	[SerializeField] private float waterFloatHeight;

	[System.Serializable]
	public class TileInfo
	{
		public int id;
		public int biome;
		public string item;
	}

	public List<TileInfo> tileInfo = new List<TileInfo>();

	[HideInInspector]
	public int planetID;
	[Tooltip("Should this planet build itself when the game enters Play Mode?")]
	public bool generateOnPlay;
	//[Tooltip("Set this to true if you dont want to build the tiles as separate gameobjects.  Will just build a single mesh with no pathfinding features.")]
	//public bool dontBuildSeparateTiles;
	[Tooltip("The reference to this planet's navigation manager.")]
	public NavigationManager navManager;

	[HideInInspector]
	public int TileCount;
	[Range(1, 4)]
	public int detailLevel;
	public int numClouds;
	public float cloudHeight;
	[Tooltip("The number of colors used to color the sphere when using random region generation.  The colors themselves are explicitly defined in the Tile class.")]
	public int numColors;

	private const float tileAlpha = .75f;
	public Color[] colors = new Color[]{new Color(1f, 1f, 1f, 0f), new Color(1f, 0.235f, 0f, tileAlpha), new Color(0.51f, 0.137f, 0.725f, tileAlpha), new Color(0.294f, 0.725f, 0f, tileAlpha), new Color(1f, .5f, 0f, tileAlpha)};

	//The scale multiplier for the entire planet
	[HideInInspector]
	public float planetScale;
	
	public Material hexMat;
	public Material pentMat;
	[Tooltip("The material to be used for extruded hex tiles")]
	public Material hexMat_extruded;
	[Tooltip("The material to be used for extruded pent tiles")]
	public Material pentMat_extruded;

	private float radialScale;
	private float maxEdgeLength;
	private float maxTileRadius;
	//Worldspace radius of the planet
	private float worldRadius;

	[SerializeField, HideInInspector]
	private List<Vector3> vectors;// = new List<Vector3>();
	[SerializeField, HideInInspector]
	private List<int> indices;// = new List<int>();
	[SerializeField, HideInInspector]
	public List<GameObject> TileObjects;// = new List<GameObject>();
	[SerializeField, HideInInspector]
	private List<Tile> tiles;// = new List<Tile>();
	[SerializeField, HideInInspector]
	private List<GameObject> CloudObjects;// = new List<GameObject>();

	[HideInInspector]
	public bool tilesGenerated;

	void Start(){
		gameController = GameObject.FindWithTag ("GameController");

		planetID = Planet_ID;
		Planet_ID++;
		planetInstances.Add (this);

		if (generateOnPlay && !tilesGenerated) {
			//Build the HexSphere
			BuildPlanet ();
			//Assign tile attributes
			MapBuilder ();
		}
		navManager.setWorldTiles (tiles);
		fillTileList ();
	}

	void GetItemInfo() {
		ItemController ic;
		if (Application.isEditor) {
			ic = GameObject.Find ("Game Controller").GetComponent<ItemController> ();
		} else {
			ic = gameController.GetComponent<ItemController>();
		}

		for (int i = 0; i < 5; i++) {
			biomeFoods.Add(new List<string> ());
			foodToAdd [i] = new List<string> ();
		}

		foreach (ItemController.Item i in ic.items) {
			biomeFoods [i.biome].Add (i.name);
			foodTiers [i.name] = i.rarity-1;
		}
	}

	public void BuildPlanet(){
//		if (BuildController.Instance.worldSeed != 0)
//			UnityEngine.Random.InitState (BuildController.Instance.worldSeed);
//		else {
//			BuildController.Instance.worldSeed = (int)UnityEngine.Random.Range(0, Mathf.Pow(2, 16));
//			UnityEngine.Random.InitState (BuildController.Instance.worldSeed);
//		}

		vectors = new List<Vector3>();
		indices = new List<int>();
		TileObjects = new List<GameObject>();
		tiles = new List<Tile>();
		CloudObjects = new List<GameObject>();

		GameObject water = Instantiate (Resources.Load ("Prefabs/Water") as GameObject);
		//Used to scale water so that waves don't pass through land
		int scaleTuning = 0;
		switch (detailLevel) {
		case 1:
			scaleTuning = -4;
			break;
		case 2:
			scaleTuning = 2;
			break;
		case 3:
			scaleTuning = 5;
			break;
		case 4:
			scaleTuning = 12;
			break;
		}
		water.transform.localScale = Vector3.one * (detailLevel * 100 + scaleTuning);
		water.transform.position = transform.position;
		water.transform.SetParent (transform);

		/*for (int i = 0; i < numClouds; i++) {
			GameObject cloud = Instantiate (Resources.Load ("Prefabs/Cloud") as GameObject);
			cloud.transform.eulerAngles = new Vector3 (UnityEngine.Random.Range (0, 360f), UnityEngine.Random.Range (0, 360f), UnityEngine.Random.Range (0, 360f));
			cloud.transform.Translate (Vector3.up * cloudHeight);
			float scale = UnityEngine.Random.Range (0.008f, 0.012f);
			cloud.transform.localScale = new Vector3 (scale, scale, scale);
			cloud.GetComponent<SimpleOrbit> ().speed = UnityEngine.Random.Range (2f, 6f);
			cloud.transform.SetParent (null);
			CloudObjects.Add (cloud);
		}*/

		if (detailLevel < 1) {
			detailLevel = 1;
		}
		
		//Mesh generation freezes up for detail levels greater than 4
		if (detailLevel > 4) {
			detailLevel = 4;
		}
		
		//radialScale = detailLevel;
		unitScale = detailLevel;
		
		#region Generate Icosahedron Vertices
		//HEX VERTICES
		Geometry.Icosahedron(vectors, indices);
		//subdivide
		for (int i = 0; i < detailLevel; i++)
			Geometry.Subdivide(vectors, indices, true);
		
		/// normalize vectors to "inflate" the icosahedron into a sphere.
		for (int i = 0; i < vectors.Count; i++){
			//You can also multiply this by some amount to change the build size
			vectors[i]=Vector3.Normalize(vectors[i]) * detailLevel;
		}

		#endregion
		
		List<Vector3> centers = getTriangleIncenter(vectors, indices);
		
		maxEdgeLength = getMaxEdgeDistance (centers);
		maxTileRadius = getMaxTileRadius (centers, vectors);
		
		generateSubMeshes (centers, vectors);
		TileCount = TileObjects.Count;

		//Useful for establishing the world's size but commenting out to avoid unused field warnings.
		//worldRadius = Vector3.Magnitude (centers [0]);

		//find each tiles neighbors
		for (int i = 0; i < TileObjects.Count; i++) {
			TileObjects[i].GetComponent<Tile>().FindNeighbors();
		}
		tilesGenerated = true;

		//Assign tiles to navManager
		navManager.setWorldTiles (tiles);
	}
	
	private void removeTileColliders(){
		foreach (GameObject t in TileObjects) {
			Destroy(t.GetComponent<Collider>());
		}
	}

	public void RevealAllResources() {
		foreach (Tile t in tiles) {
			if (!t.itemRevealed && t.color != 5) {
				gameController.GetComponent<TileController>().selectedTile = t;
				gameController.GetComponent<TileController>().RevealTileResource (1);
				gameController.GetComponent<TileController>().selectedTile = null;
			}
		}
	}

	public void BuildStoreOnAll(string type) {
		RevealAllResources ();
		foreach (Tile t in tiles) {
			if (t.color != 4 && t.id%2==0) {
				gameController.GetComponent<TileController>().selectedTile = t;
				gameController.GetComponent<TileController>().toBuild = type;
				gameController.GetComponent<TileController>().NewTileStore ();
				//gameController.GetComponent<TileController>().FinalizeStoreBuild (1);
				gameController.GetComponent<TileController>().selectedTile = null;
			}
		}
	}

	private void fillTileList() {
		tileInfo = new List<TileInfo> ();
		for (int i = 0; i < tiles.Count; i++) {
			TileInfo ti = new TileInfo ();
			ti.biome = tiles [i].color;
			ti.id = i;
			ti.item = tiles [i].item;
			tileInfo.Add (ti);
		}
	}

	public void countBiomes() {
		int[] biomeCount = new int[numColors+1];
		for (int i = 0; i < tiles.Count; i++) {
			biomeCount [tiles [i].color]++;
		}
		for (int j = 0; j < numColors+1; j++) {
			Debug.Log ("Number of biome " + j + " tiles: " + biomeCount [j]);
		}
		countItems ();
	}

	public void countItems() {
		Dictionary<string, int> itemCounts = new Dictionary<string, int> ();
		for (int i = 0; i < tiles.Count; i++) {
			if (!itemCounts.ContainsKey (tiles [i].item)) {
				itemCounts.Add (tiles [i].item, 1);
			} else {
				itemCounts [tiles [i].item]++;
			}
		}
		for (int j = 0; j < itemCounts.Count; j++) {
			string itemAt = itemCounts.Keys.ElementAt (j);
			Debug.Log ("Amount of " + itemAt.ToLower() + ": " + itemCounts [itemAt]);
		}
	}

	private void generateSubMeshes(List<Vector3> centers, List<Vector3> vertices){
		//Generate the hex/pent mesh for each vertex in the main mesh by associating it to its surrounding triangle centers
		for(int i = 0; i < vertices.Count; i++){
			GameObject tile = new GameObject ("Tile " + i);
			Mesh submesh = new Mesh ();
			tile.AddComponent<MeshFilter> ();

			tile.transform.parent = this.transform;
			tile.transform.localPosition = vertices[i];
			tile.transform.up = vertices[i];

			List<Vector3> submeshVs = new List<Vector3>();
			for(int j = 0; j < centers.Count; j++){
				if(Vector3.Distance(vertices[i], centers[j]) <= maxTileRadius){
					submeshVs.Add(centers[j]);
				}
			}
			bool isHexagon = false;
			//If its a pentagon
			if(submeshVs.Count == 5){
				isHexagon = false;
				bool[] isUsed = new bool[5];
				List<int> orderedIndices = new List<int>();
				Vector3 current = submeshVs[0];
				orderedIndices.Add(0);
				isUsed[0] = true;
				//starting at the first point in submeshVs, find a point on the perimeter of the tile that is within one edgelength from point current, then add its index to the list
				while(orderedIndices.Count < 5){
					foreach(Vector3 c in submeshVs){
						if(Vector3.Distance(c, current) <= maxEdgeLength && Vector3.Distance(c, current) >= 0.001f && !isUsed[submeshVs.IndexOf(c)]){
							//triangles[h + j] = submeshVs.IndexOf(c);
							orderedIndices.Add(submeshVs.IndexOf(c));
							isUsed[submeshVs.IndexOf(c)] = true;
							current = c;
							break;
						}
					}
				}
				int[] triangles = new int[9];
				triangles[0] = 0;
				triangles[1] = orderedIndices[1];
				triangles[2] = orderedIndices[2];
				
				triangles[3] = orderedIndices[2];
				triangles[4] = orderedIndices[3];
				triangles[5] = orderedIndices[0];
				
				triangles[6] = orderedIndices[3];
				triangles[7] = orderedIndices[4];
				triangles[8] = orderedIndices[0];

				//Convert the vertices to the tile's local space
				for(int k = 0; k < submeshVs.Count; k++){
					submeshVs[k] = tile.transform.InverseTransformPoint(submeshVs[k]);
				}
				Vector3[] subVsArray = submeshVs.ToArray();
				submesh.vertices = subVsArray;
				submesh.triangles = triangles;
				Vector2[] uvs = new Vector2[submeshVs.Count];
				
				uvs[orderedIndices[0]] = new Vector2(0f, 0.625f);
				uvs[orderedIndices[1]] = new Vector2(0.5f, 1f);
				uvs[orderedIndices[2]] = new Vector2(1f, 0.625f);
				uvs[orderedIndices[3]] = new Vector2(0.8f, 0.0162f);
				uvs[orderedIndices[4]] = new Vector2(.1875f, 0.0162f);
				
				submesh.uv = uvs;
		
				tile.AddComponent<MeshRenderer>();
				//Single material
				tile.GetComponent<Renderer>().sharedMaterial = pentMat;
			}
			//If its a hexagon
			else if(submeshVs.Count == 6){
				isHexagon = true;
				bool[] isUsed = new bool[6];
				List<int> orderedIndices = new List<int>();
				Vector3 current = submeshVs[0];
				orderedIndices.Add(0);
				isUsed[0] = true;
				//starting at the first point in submeshVs, find a point on the perimeter of the tile that is within one edgelength from point current, then add its index to the list
				while(orderedIndices.Count < 6){
					foreach(Vector3 c in submeshVs){
						if(Vector3.Distance(c, current) <= maxEdgeLength && Vector3.Distance(c, current) >= 0.001f && !isUsed[submeshVs.IndexOf(c)]){
							orderedIndices.Add(submeshVs.IndexOf(c));
							isUsed[submeshVs.IndexOf(c)] = true;
							current = c;
							break;
						}
					}
				}
				int[] triangles = new int[12];
				triangles[0] = 0;
				triangles[1] = orderedIndices[1];
				triangles[2] = orderedIndices[2];
				
				triangles[3] = orderedIndices[2];
				triangles[4] = orderedIndices[3];
				triangles[5] = 0;
				
				triangles[6] = orderedIndices[3];
				triangles[7] = orderedIndices[4];
				triangles[8] = 0;
				
				triangles[9] = orderedIndices[4];
				triangles[10] = orderedIndices[5];
				triangles[11] = 0;

				//Convert the vertices to the tile's local space
				for(int k = 0; k < submeshVs.Count; k++){
					submeshVs[k] = tile.transform.InverseTransformPoint(submeshVs[k]);
				}
				Vector3[] subVsArray = submeshVs.ToArray();
				submesh.vertices = subVsArray;
				submesh.triangles = triangles;
				
				Vector2[] uvs = new Vector2[6];
				//UV Coords based on geometry of hexagon
				uvs[orderedIndices[0]] = new Vector2(0.0543f, 0.2702f);
				uvs[orderedIndices[1]] = new Vector2(0.0543f, 0.7272f);
				uvs[orderedIndices[2]] = new Vector2(0.5f, 1f);
				uvs[orderedIndices[3]] = new Vector2(0.946f, 0.7272f);
				uvs[orderedIndices[4]] = new Vector2(0.946f, 0.2702f);
				uvs[orderedIndices[5]] = new Vector2(0.5f, 0f);
				
				submesh.uv = uvs;

				tile.AddComponent<MeshRenderer>();
				//Single material
				tile.GetComponent<Renderer>().sharedMaterial = hexMat;
				
			}
			
			//Assign mesh
			tile.GetComponent<MeshFilter>().mesh = submesh;
			submesh.RecalculateBounds();
			submesh.RecalculateNormals();
			tile.AddComponent<Tile>();
			
			//Fix any upsidedown tiles by checking their normal vector
			if((tile.transform.TransformDirection(submesh.normals[0]) + vertices[i]).sqrMagnitude < vertices[i].sqrMagnitude){
				submesh.triangles = submesh.triangles.Reverse().ToArray();
				submesh.RecalculateBounds();
				submesh.RecalculateNormals();
			}
			//Initialize tile attributes
			tile.AddComponent<MeshCollider>();

			Tile t = tile.GetComponent<Tile>();
			t.Initialize(vertices[i] + transform.position);
			t.parentPlanet = this;
			t.setTileRadius(maxTileRadius);
			t.isHexagon = isHexagon;
			t.gameObject.layer = 8;
			tiles.Add(t);

			tile.isStatic = false;
			TileObjects.Add(tile);
		}
	}
	
	
	
	void MapBuilder(){
		//Put your map building logic in here


	}

	public void clearDecorations() {
		for (int i = 0; i < tiles.Count; i++) {
			if (TileObjects [i].transform.childCount > 0) {
				for (int j = 0; j < TileObjects[i].transform.childCount; j++) {
					DestroyImmediate(TileObjects[i].transform.GetChild(j).gameObject);
				}
				if (TileObjects [i].transform.childCount > 0) {
					DestroyImmediate (TileObjects [i].transform.GetChild (0).gameObject);
				}
			}
		}
	}

	public void placeDecorations() {
		clearDecorations ();
		for (int i = 0; i < tiles.Count; i++) {
			GameObject upTransform = null;
			upTransform = Instantiate (Resources.Load ("Prefabs/UpTransform") as GameObject);
			upTransform.transform.rotation = Quaternion.identity;
			upTransform.transform.SetParent (TileObjects [i].transform);
			tiles [i].placeObject (upTransform);
			upTransform.transform.up = TileObjects [i].transform.up;
			upTransform.transform.Translate (Vector3.up * 1);

			GameObject innerDecoration = null;
			GameObject outerDecoration = null;
			GameObject resourceDecoration = null;
			GameObject buildingDecoration = null;
			GameObject buildSpots = null;
			switch (tiles [i].color) {
			case 0:
				innerDecoration = Instantiate (Resources.Load ("Prefabs/snow_inner") as GameObject);
				outerDecoration = Instantiate (Resources.Load ("Prefabs/snow_outer") as GameObject);
				if (!tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .67f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .67f;
				}
				else if (tiles[i].isHexagon)
					innerDecoration.transform.localScale = Vector3.one * planetScale * 1f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * 1f;
				break;
			case 1: //Grass
				innerDecoration = Instantiate (Resources.Load ("Prefabs/trees_inner") as GameObject);
				outerDecoration = Instantiate (Resources.Load ("Prefabs/trees_outer") as GameObject);
				if (!tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .115f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .115f;
				} else if (tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .17f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .17f;
				}
				break;
			case 2: //Mountains
				innerDecoration = Instantiate (Resources.Load ("Prefabs/mountains_inner") as GameObject);
				outerDecoration = Instantiate (Resources.Load ("Prefabs/mountains_outer") as GameObject);
				if (!tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .14f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .14f;
				} else if (tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .2f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .2f;
				}
				break;
			case 3: //Sand
				innerDecoration = Instantiate (Resources.Load ("Prefabs/desert_inner") as GameObject);
				outerDecoration = Instantiate (Resources.Load ("Prefabs/desert_outer") as GameObject);
				if (!tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .67f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .67f;
				} else if (tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * 1f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * 1f;
				}
				break;
			case 4: //Water
				innerDecoration = Instantiate (Resources.Load ("Prefabs/Ocean") as GameObject);
				outerDecoration = Instantiate (Resources.Load ("Prefabs/Ocean") as GameObject);
				if (!tiles [i].isHexagon) {
					innerDecoration.transform.localScale = Vector3.one * planetScale * .1f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .1f;
				}
				else if (tiles[i].isHexagon)
					innerDecoration.transform.localScale = Vector3.one * planetScale * .15f;
					outerDecoration.transform.localScale = Vector3.one * planetScale * .15f;
				break;
			}

			innerDecoration.transform.SetParent (TileObjects [i].transform);
			tiles [i].placeObject (innerDecoration);
			innerDecoration.transform.Rotate (Vector3.up * UnityEngine.Random.Range (0, 180));

			if (outerDecoration != null) {
				outerDecoration.transform.SetParent (TileObjects [i].transform);
				tiles [i].placeObject (outerDecoration);
				outerDecoration.transform.Rotate (Vector3.up * UnityEngine.Random.Range (0, 5) * 60);
			}

			if (tiles [i].item != "" && tiles[i].itemRevealed & tiles[i].buildings.Count == 0) {
				innerDecoration.SetActive (false);
				GameObject resourceModel = Resources.Load<GameObject> ("Prefabs/" + tiles [i].item + "_inner");
				if (resourceModel == null) {
					resourceModel = Resources.Load<GameObject> ("Prefabs/hot_dogs_inner");
				}
				resourceDecoration = Instantiate (resourceModel) as GameObject;

				if (!tiles [i].isHexagon) {
					resourceDecoration.transform.localScale = Vector3.one * planetScale * .06f;
				} else if (tiles [i].isHexagon)
					resourceDecoration.transform.localScale = Vector3.one * planetScale * .09f;
				
				resourceDecoration.transform.SetParent (TileObjects [i].transform);
				tiles [i].placeObject (resourceDecoration);
				resourceDecoration.transform.Rotate (Vector3.up * UnityEngine.Random.Range (0, 180));

			}

			buildSpots = Instantiate (Resources.Load ("Prefabs/BuildSpots") as GameObject);
			if (!tiles [i].isHexagon) {
				buildSpots.transform.localScale = Vector3.one * planetScale * .7f;
			} else if (tiles [i].isHexagon)
				buildSpots.transform.localScale = Vector3.one * planetScale * 1f;
			buildSpots.transform.SetParent (TileObjects [i].transform);
			tiles [i].placeObject (buildSpots);
			buildSpots.transform.rotation = innerDecoration.transform.rotation;

			for (int c = 0; c < 3; c++) {
				buildSpots.transform.GetChild (c).GetChild (0).gameObject.SetActive (false);
			}

			/*if (!(tiles[i].buildings.All(new List<string>{"","",""}.Contains))) {
				innerDecoration.SetActive (false);
				for (int a = 0; a < tiles[i].buildings.Count; a++) {
					if (tiles [i].buildings [a] == "") {
						continue;
					}
					GameObject buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/" + tiles [i].buildings [a]);
					if (buildingModel == null) {
						if (tiles [i].buildings [a].Contains ("-")) {
							buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza-1");
						} else {
							buildingModel = Resources.Load<GameObject> ("Prefabs/Buildings/pizza1");
						}
					}
					buildingDecoration = Instantiate (buildingModel) as GameObject;
					Transform placeAt = TileObjects [i].transform.GetChild (3).GetChild (a);
					buildingDecoration.transform.SetParent (placeAt);
					buildingDecoration.transform.position = placeAt.position;
					buildingDecoration.transform.localRotation = Quaternion.identity;
				}
			}*/

			for (int j = 0; j < TileObjects[i].transform.childCount; j++) {
				TileObjects[i].transform.GetChild (j).Translate (Vector3.up * tiles[i].extrudedHeight / 2);
			}
		}
	}

	public void colorAllTiles() {
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].colorTile ();
		}
	}

	public void randomizeAllItems() {
		GetItemInfo ();
		GetRandomFoodList ();
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].item = foodToAdd [tiles [i].color] [0];
			foodToAdd [tiles [i].color].RemoveAt (0);
		}
		fillTileList ();
	}

	void GetRandomFoodList () {
		foreach (string food in biomeFoods[0]) {
			for (int i = 0; i < specialCountPerTier [foodTiers [food]]; i++) {
				foodToAdd [0].Add (food);
			}
		}

		Shuffle(foodToAdd [0]);
		
		for (int biome = 1; biome < 5; biome++) {
			foreach (string food in biomeFoods[biome]) {
				for (int i = 0; i < countPerTier [foodTiers [food]]; i++) {
					foodToAdd [biome].Add (food);
				}
			}
			Shuffle(foodToAdd [biome]);
		}
	}

	public void Shuffle(List<string> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = irng.Next(n + 1);  
			string value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}


	public void extrudeLand() {
		for (int i = 0; i < tiles.Count; i++) {
			float extrudeScale = 0f;
			switch (tiles[i].color) {
			case 0: //Snow
				extrudeScale = .133f;
				break;
			case 1: //Grass
				extrudeScale = .133f;
				break;
			case 2: //Mountains
				extrudeScale = .166f;
				break;
			case 3: //Sand
				extrudeScale = .1f;
				break;
			case 4: //Water
				extrudeScale = 0f;
				//tiles [i].extrudedHeight += waterFloatHeight;
				break;
				
			}
			tiles [i].Extrude (planetScale * extrudeScale);
		}
		clearDecorations ();
		placeDecorations ();
	}

	public void generateRandomRegions(){
		//Randomly assign colors

		for (int i = 0; i < biomeCounts.Count; i++) {
			biomeCounts [i] = 0;
		}

		clearDecorations();

		//float maxHeight = 0;
		//float minHeight = 0;
		//int highTile = 0;
	 	//int lowTile = 0;

		for (int j = 0; j < tiles [18].neighborTiles.Count; j++) {
			int tid = tiles [18].neighborTiles [j].id;
			poleIndices.Add (tid);
		}
		poleIndices.Add (tiles [18].id);

		for (int j = 0; j < tiles [32].neighborTiles.Count; j++) {
			int tid = tiles [32].neighborTiles [j].id;
			poleIndices.Add (tid);
		}		
		poleIndices.Add (tiles [32].id);

		for (int i = 0; i < tiles.Count; i++) {
			if (poleIndices.Contains (tiles [i].id)) {
				continue;
			}
			int col = UnityEngine.Random.Range(1, numColors);
			int count = 0;
			while (biomeCounts [col] >= 49 && count < 50) {
				col = UnityEngine.Random.Range(1, numColors);
				count++;
			}
			tiles [i].setColor (col);
			biomeCounts [col]++;

			//if (TileObjects [i].transform.position.y > maxHeight) {
				//maxHeight = TileObjects [i].transform.position.y;
				//highTile = i;
			//} else if (TileObjects [i].transform.position.y < minHeight) {
				//minHeight = TileObjects [i].transform.position.y;
				//lowTile = i;
			//}
		}

		for (int i = 0; i < tiles.Count; i++) {
			int similarNeighbors = 0;
			for (int j = 0; j < tiles [i].neighborTiles.Count; j++) {
				if (tiles [i].color != 4) {
					if (tiles [i].neighborTiles [j].color == tiles [i].color)
						similarNeighbors++;
				}
			}
			if (similarNeighbors < 1) {
				for (int j = 0; j < tiles [i].neighborTiles.Count; j++) {
					if (tiles [i].neighborTiles [j].color == tiles [i].color && biomeCounts[tiles[i].neighborTiles[j].color] > 37) {
						biomeCounts [tiles [i].neighborTiles [j].color]--;
						tiles [i].neighborTiles [j].setColor (4);
						biomeCounts [4]++;
					}
				}
				if (biomeCounts[tiles[i].color] > 37) {
					biomeCounts [tiles [i].color]--;
					tiles [i].setColor(4);
					biomeCounts [4]++;
				}
			}



		}

		for (int i = 0; i < tiles.Count; i++) {
			int similarNeighbors = 0;
			for (int j = 0; j < tiles [i].neighborTiles.Count; j++) {
				if (tiles [i].color != 4) {
					if (tiles [i].neighborTiles [j].color == tiles [i].color)
						similarNeighbors++;
				}
			}
			if (similarNeighbors < 2) {
				for (int j = 0; j < tiles [i].neighborTiles.Count; j++) {
					if (tiles [i].neighborTiles [j].color == tiles [i].color && biomeCounts[tiles[i].neighborTiles[j].color] > 37) {
						biomeCounts [tiles [i].neighborTiles [j].color]--;
						tiles [i].neighborTiles [j].setColor (4);
						biomeCounts [4]++;
					}
				}
				if (biomeCounts[tiles[i].color] > 37) {
					biomeCounts [tiles [i].color]--;
					tiles [i].setColor(4);
					biomeCounts [4]++;
				}
			}



		}

		placeDecorations ();
	}

	private float getMaxTileRadius(List<Vector3> centers, List<Vector3> vertices){
		float delta = 1.5f;
		Vector3 v = Vector3.zero;
		if (detailLevel != 0) {
			v = vertices [12];
		}
		else{
			v = vertices [0];
		}
		
		float minDistance = Mathf.Infinity;
		foreach (Vector3 c in centers) {
			
			float dist = Vector3.Distance(v, c);
			
			if (dist < minDistance){
				minDistance = dist;
			}
			
		}
		minDistance = minDistance * (delta);
		
		return minDistance;
	}
	
	private float getMaxEdgeDistance(List<Vector3> centers) {
		//Returns the approximate distance between adjacent triangle centers
		
		//delta is the approximate variation in edge lengths, as not all edges are the same length
		float delta = 1.4f;
		Vector3 point = centers [0];
		// scan all vertices to find nearest
		float minDistance = Mathf.Infinity;
		foreach (Vector3 n in centers) {
			if(!point.Equals(n)){
				float dist = Vector3.Distance(point, n);
				
				if (dist < minDistance){
					minDistance = dist;
				}
			}
		}
		
		minDistance = minDistance * (delta);
		
		return minDistance;
	}
	
	private List<Vector3> getTriangleIncenter(List<Vector3> vertices, List<int> triangles){
		List<Vector3> centers = new List<Vector3> ();
		for (int i = 0; i < triangles.Count - 2; i += 3) {
			Vector3 A = vertices[triangles[i]];
			Vector3 B = vertices[triangles[i + 1]];
			Vector3 C = vertices[triangles[i + 2]];
			
			float a = Vector3.Distance(C, B);
			float b = Vector3.Distance(A, C);
			float c = Vector3.Distance(A, B);
			
			float P = a + b + c;
			
			Vector3 abc = new Vector3(a, b, c);
			
			float x = Vector3.Dot (abc, new Vector3(A.x, B.x, C.x)) / P;
			float y = Vector3.Dot (abc, new Vector3(A.y, B.y, C.y)) / P;
			float z = Vector3.Dot (abc, new Vector3(A.z, B.z, C.z)) / P;
			
			Vector3 center = new Vector3(x, y, z);
			centers.Add(center);
		}
		return centers;
	}

	public void setWorldScale(float scale){
		transform.localScale = Vector3.one * scale;
		planetScale = scale;
	}

	//Destroys all tiles and resets the 
	public void deleteTiles(){
		for (int i = 0; i < biomeCounts.Count; i++) {
			biomeCounts [i] = 0;
		}
		foreach(GameObject t in TileObjects){
			DestroyImmediate(t);
		}

		/*foreach (GameObject c in CloudObjects) {
			DestroyImmediate (c);
		}*/

		DestroyImmediate(transform.Find("Water(Clone)").gameObject);
		indices.Clear ();
		vectors.Clear ();
		tiles.Clear ();
		TileObjects.Clear ();
		CloudObjects.Clear ();
		tilesGenerated = false;
		TileCount = 0;
	}

}
