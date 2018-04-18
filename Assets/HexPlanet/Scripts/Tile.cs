using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using cakeslice;

[Serializable]
[ExecuteInEditMode]
public class Tile : MonoBehaviour {

	private GameObject mainCam;
	public bool hover = false;
	public bool selected = false;
	private Color startColor;
	public static float planetScale;
	private static int ID = 0;
	//private static float tileRadius = 0.8f;

	[Tooltip("The instance of the hexsphere which constructed this tile")]
	public Hexsphere parentPlanet;
	
	public List<Tile> neighborTiles;

	//Tile Attributes
	[Tooltip("Whether or not navigation will consider this tile as a valid to move over")]
	public bool navigable = true;
	[Tooltip("The cost of moving across this tile in terms of pathfinding weight.  Pathfinding will prioritize the lowest cost path.")]
	[Range(1, 100)]
	public int pathCost = 1;

	//The center of this tile in worldspace as calculated by the planet.  Is slightly offset downwards from the transforms position in the local Y.
	public Vector3 center {
		get{ return tileRenderer.bounds.center; }
	}

	//The position of this tile as reported by the renderer in world space.  More strict than the above center.
	public Vector3 centerRenderer {
		get{ return tileRenderer.bounds.center; }
	}

	public int color;
	public string item = "";
	public bool itemRevealed = false;
	public int farmTier = 0;
	public string storeName = "";
	public List<string> buildings = new List<string> { "","","" };
	public int storeTier = 0;
	public int id;

	[HideInInspector]
	public Renderer tileRenderer;

	private float maxTileRadius;
	private const float tileAlpha = .75f;
	public Color[] colors;// = new Color[]{new Color(1f, 1f, 1f, 0f), new Color(1f, 0.235f, 0f, tileAlpha), new Color(0.51f, 0.137f, 0.725f, tileAlpha), new Color(0.294f, 0.725f, 0f, tileAlpha), new Color(1f, .5f, 0f, tileAlpha)};

	//Used to specify which tile is currently selected so that any tile can query the selected tile or assign themselves as selected.
	private static Tile selectedTile;
	//The center of the tile in worldspace as assigned by the hexsphere during generation.  Not affected by the scale of the planet.
	[SerializeField, HideInInspector]
	//private Vector3 centerUnscaled;
	private bool hasBeenExtruded;
	public float extrudedHeight;
	public bool isHexagon;

	void Start() {
		mainCam = GameObject.Find ("Main Camera");
	}

	void Awake(){
		colors = GetComponentInParent<Hexsphere> ().colors;
		tileRenderer = GetComponent<Renderer> ();
		if (!GetComponent<Outline> ()) {
			gameObject.AddComponent<Outline> ();
		}
		GetComponent<Outline> ().enabled = false;

	}

	public void Initialize(Vector3 coordinates){
		tileRenderer = GetComponent<Renderer> ();
		//centerUnscaled = coordinates;
		id = ID;
		ID++;
		if (GetComponent<Outline>())
			GetComponent<Outline> ().enabled = false;
	}

	public void randomizeItem() {
		ItemController.Item ri;
		if (Application.isEditor) {
			ItemController ic = GameObject.Find ("Game Controller").GetComponent<ItemController> ();
			ri = ic.items [UnityEngine.Random.Range (0, ic.items.Count)];
			while (ri.biome != color || UnityEngine.Random.Range (0, ri.rarity * 2) > 1) {
				ri = ic.items [UnityEngine.Random.Range (0, ic.items.Count)];
			}
		} else {
			GameObject gameController = GameObject.FindWithTag ("GameController");
			ri = gameController.GetComponent<ItemController>().items [UnityEngine.Random.Range (0, gameController.GetComponent<ItemController>().items.Count)];
			while (ri.biome != color || UnityEngine.Random.Range (0, ri.rarity * 2) > 1) {
				ri = gameController.GetComponent<ItemController>().items [UnityEngine.Random.Range (0, gameController.GetComponent<ItemController>().items.Count)];
			}
		}
		item = ri.name;
	}

	void OnMouseEnter(){
		//GetComponent<Outline> ().enabled = true;
		//Pointer.instance.setPointer (PointerStatus.TILE, transform);
		
	}
	void OnMouseExit(){
		//GetComponent<Outline> ().enabled = false;
		hover = false;
		//Pointer.instance.unsetPointer ();
	}
	void OnMouseOver() {
		hover = true;
	}
	
	void SelectTile(){
		//Demo function
		//pathfindingDrawDemo ();
		selected = true;
		mainCam.GetComponent<DragToOrbit>().ChangeTarget(transform.GetChild(1));
		GameObject gameController = GameObject.FindWithTag ("GameController");
		gameController.GetComponent<UIController>().OpenTileMenu (this);
		transform.GetChild (3).gameObject.SetActive (true);
		for (int c = 0; c < 3; c++) {
			if (buildings [c] == "") {
				transform.GetChild (3).GetChild (c).GetChild (0).gameObject.SetActive (true);
			}
		}
		//mainCam.GetComponent<DragToOrbit> ().enabled = false;
		//StartCoroutine(MoveToPosition(GameObject.Find("Main Camera").transform, transform.GetChild(0).position, .5f));
		//mainCam.transform.position = transform.GetChild(0).position;
		//mainCam.transform.LookAt (transform.GetChild (1));
		//mainCam.GetComponent<DragToOrbit> ().enabled = true;
	}

	public void MoveToTile() {
		mainCam.GetComponent<DragToOrbit> ().enabled = false;
		//StartCoroutine(MoveToPosition(GameObject.Find("Main Camera").transform, transform.GetChild(0).position, .5f));
		mainCam.transform.position = transform.GetChild(0).position;
		mainCam.transform.LookAt (transform.GetChild (1));
		mainCam.GetComponent<DragToOrbit> ().enabled = true;
	}
		
	void Update() {
		if ((hover && !GameObject.Find("Main Camera").GetComponent<DragToOrbit>().CheckIfTile(GameObject.Find("Main Camera").GetComponent<DragToOrbit>().target)) && Time.timeScale != 0)
			GetComponent<Outline> ().enabled = true;
		else
			GetComponent<Outline> ().enabled = false;
		/*if (Input.GetAxis ("Mouse ScrollWheel") > 0f && mainCam.GetComponent<DragToOrbit>().distance == mainCam.GetComponent<DragToOrbit>().distanceMin && !mainCam.GetComponent<DragToOrbit>().CheckIfTile(mainCam.GetComponent<DragToOrbit>().target) && hover) {
			SelectTile ();
		}*/
		if (Input.GetKeyDown (KeyCode.E) && !mainCam.GetComponent<DragToOrbit> ().CheckIfTile (mainCam.GetComponent<DragToOrbit> ().target) && hover) {
			//mainCam.GetComponent<DragToOrbit>().ChangeTarget(transform.GetChild(1));
			SelectTile ();
		}
			
		//GameObject.Find ("Main Camera").transform.LookAt (transform.GetChild (1));
	}

	/// <summary>
	/// Just a simple demo function that allows you to click on two tiles and draw the shortest path between them.
	/// </summary>
	public void pathfindingDrawDemo(){
		if (selectedTile == null) {
			selectedTile = this;
		}
		else if(selectedTile != this){
			Stack<Tile> path = new Stack<Tile>();
			if(parentPlanet.navManager.findPath(selectedTile, this, out path)){
				parentPlanet.navManager.drawPath(path);
				selectedTile = null;
			}
		}
	}

	//NEW FIND NEIGHBORS
	public void FindNeighbors(){
		//Extend a sphere around this tile to find all adjacent tiles within the spheres radius
		Collider[] neighbors = Physics.OverlapSphere (center, maxTileRadius);
		//OverlapSphere detects the current tile so we must omit this tile from the list
		neighborTiles = new List<Tile> ();
		int j = 0;
		for(int i = 0; i < neighbors.Length; i++){
			if(neighbors[i] != this.GetComponent<Collider>() && neighbors[i].gameObject.GetComponent<Tile>() != null){
				neighborTiles.Add(neighbors[i].gameObject.GetComponent<Tile>());
				j++;
			}
		}
	}

	public void placeObject(GameObject obj){
		obj.transform.position = center;
		obj.transform.up = transform.up;
	}

	public void Extrude(float height){
		extrudedHeight += height;
		/*for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).Translate (Vector3.up * height);
		}*/
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		Vector3[] verts = mesh.vertices;
		//Check if this tile has already been extruded
		if(hasBeenExtruded){
			int sides = isHexagon ? 6 : 5;
			//Apply extrusion heights
			for(int i = 0; i < sides; i++){
				Vector3 worldV = (transform.TransformPoint (verts [i]) - parentPlanet.transform.position);
				worldV += height * worldV.normalized;
				verts [i] = transform.InverseTransformPoint (worldV);
			}
			for (int i = sides + 2; i < sides + sides * 4; i += 4) {
				Vector3 worldV = (transform.TransformPoint (verts [i]) - parentPlanet.transform.position);
				worldV += height * worldV.normalized;
				verts [i] = transform.InverseTransformPoint (worldV);

				worldV = (transform.TransformPoint (verts [i + 1]) - parentPlanet.transform.position);
				worldV += height * worldV.normalized;
				verts [i + 1] = transform.InverseTransformPoint (worldV);
			}

			mesh.vertices = verts;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			GetComponent<MeshFilter> ().sharedMesh = mesh;
			return;
		}

		//Sort vertices clockwise
		Array.Sort(verts, new ClockwiseComparer (transform.InverseTransformPoint (center)));
		List<int> tris = new List<int> (mesh.triangles);
		//List<Vector3> normals = new List<Vector3> (mesh.normals);
		//Duplicate the existing vertices
		List<Vector3> faceVerts = new List<Vector3>(verts);
		//Translate duplicated verts along local up
		for(int i = 0; i < faceVerts.Count; i++){
			Vector3 worldV = (transform.TransformPoint (faceVerts [i]) - parentPlanet.transform.position);
			worldV += height * worldV.normalized;
			faceVerts [i] = transform.InverseTransformPoint (worldV);
		}
		//Set triangles for extruded face
		tris [0] = 0;
		tris [1] = 1;
		tris [2] = 2;

		tris [3] = 0;
		tris [4] = 2;
		tris [5] = 3;

		tris [6] = 0;
		tris [7] = 3;
		tris [8] = 4;

		//Only set the last triangle if this is a hexagon
		if (verts.Length == 6) {
			tris [9] = 0;
			tris [10] = 4;
			tris [11] = 5;
		}
		int t = 0;
		//Create side triangles
		for(int i = 0; i < verts.Length - 1; i++, t += 4){
			faceVerts.Add (verts [i]);
			faceVerts.Add (verts [i + 1]);

			faceVerts.Add (faceVerts [i]);
			faceVerts.Add (faceVerts [i + 1]);

			tris.Add (t + verts.Length);
			tris.Add (t + verts.Length + 1);
			tris.Add (t + verts.Length + 2);

			tris.Add (t + verts.Length + 1);
			tris.Add (t + verts.Length + 3);
			tris.Add (t + verts.Length + 2);
		}
		//Manually create last two triangles
		faceVerts.Add(verts[verts.Length - 1]);
		faceVerts.Add(verts[0]);

		faceVerts.Add(faceVerts[verts.Length - 1]);
		faceVerts.Add(faceVerts[0]);

		tris.Add (faceVerts.Count - 4);
		tris.Add (faceVerts.Count - 3);
		tris.Add (faceVerts.Count - 2);

		tris.Add (faceVerts.Count - 3);
		tris.Add (faceVerts.Count - 1);
		tris.Add (faceVerts.Count - 2);


		mesh.vertices = faceVerts.ToArray ();
		mesh.triangles = tris.ToArray ();
		mesh.RecalculateNormals ();
		//Reassign UVs
		mesh.uv = isHexagon ? generateHexUvs() : generatePentUvs();

		//Assign meshes to Mesh Collider and Mesh Filter
		GetComponent<MeshCollider>().sharedMesh = mesh;
		GetComponent<MeshFilter> ().sharedMesh = mesh;
		hasBeenExtruded = true;
		//Assign Tile Material
		//GetComponent<Renderer>().sharedMaterial = isHexagon ? parentPlanet.hexMat_extruded : parentPlanet.pentMat_extruded;

	}

	public Vector2[] generateHexUvs(){
		Vector2[] uvs = new Vector2[30];
		uvs [0] = new Vector2 (0.293f, 0.798f);
		uvs [1] = new Vector2 (0.397f, 0.977f);
		uvs [2] = new Vector2 (0.604f, 0.977f);
		uvs [3] = new Vector2 (0.707f, 0.798f);
		uvs [4] = new Vector2 (0.604f, 0.619f);
		uvs [5] = new Vector2 (0.397f, 0.619f);

		float h = 6f;
		float y = 0.6f;
		for (int i = 6; i < 28; i += 4) {
			uvs [i] = new Vector2 (h / 6f, 0f);
			uvs [i + 1] = new Vector2 ((h - 1) / 6f, 0f);

			uvs [i + 2] = new Vector2 (h / 6f, y);
			uvs [i + 3] = new Vector2 ((h - 1)/ 6f, y);
			h--;
		}
		return uvs;
	}

	public Vector2[] generatePentUvs(){
		Vector2[] uvs = new Vector2[25];
		uvs [0] = new Vector2 (0.389f, 0.97f);
		uvs [1] = new Vector2 (0.611f, 0.97f);
		uvs [2] = new Vector2 (0.68f, 0.758f);
		uvs [3] = new Vector2 (0.5f, 0.627f);
		uvs [4] = new Vector2 (0.32f, 0.758f);

		float h = 5f;
		float y = 0.6f;
		for (int i = 5; i < 22; i += 4) {
			uvs [i] = new Vector2 (h / 5f, 0f);
			uvs [i + 1] = new Vector2 ((h - 1) / 5f, 0f);

			uvs [i + 2] = new Vector2 (h / 5f, y);
			uvs [i + 3] = new Vector2 ((h - 1)/ 5f, y);
			h--;
		}
		return uvs;
	}

	public IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToMove)
	{
		var currentPos = transform.position;
		var t = 0f;
		while(t < 1)
		{
			t += Time.deltaTime / timeToMove;
			transform.position = Vector3.Lerp(currentPos, position, t);
			yield return null;
		}
		GameObject.Find ("Main Camera").transform.LookAt (transform.GetChild (1));
		GameObject.Find ("Main Camera").GetComponent<DragToOrbit> ().enabled = true;
	}

	public void setColor(int col){
		color = col;
		Material tempMaterial = new Material(GetComponent<Renderer>().sharedMaterial);
		tempMaterial.color = colors[color];
		GetComponent<Renderer>().sharedMaterial = tempMaterial;
		//randomizeItem ();
	}

	public void colorTile() {
		Material tempMaterial = new Material (GetComponent<Renderer> ().sharedMaterial);
		tempMaterial.color = colors [color];
		GetComponent<Renderer> ().sharedMaterial = tempMaterial;
	}
	
	public void setTileRadius(float r){
		this.maxTileRadius = r;
	}

	public int getID(){
		return id;
	}
}

public class ClockwiseComparer : IComparer{
	private Vector3 mOrigin;

	public ClockwiseComparer(Vector3 origin){
		mOrigin = origin;
	}

	public int Compare(object first, object second){
		Vector3 v1 = (Vector3)first;
		Vector3 v2 = (Vector3)second;

		return IsClockwise (v2, v1, mOrigin);
	}

	public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin){
		if (first == second) {
			return 0;
		}

		Vector3 firstOffset = first - origin;
		Vector3 secondOffset = second - origin;

		float angle1 = Mathf.Atan2 (firstOffset.x, firstOffset.z);
		float angle2 = Mathf.Atan2 (secondOffset.x, secondOffset.z);

		if (angle1 < angle2) {
			return 1;
		}

		if (angle1 > angle2) {
			return -1;
		}

		return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
	}
}

public class ClockwiseComparer2D : IComparer{
	private Vector2 mOrigin;

	public ClockwiseComparer2D(Vector2 origin){
		mOrigin = origin;
	}

	public int Compare(object first, object second){
		Vector2 v1 = (Vector2)first;
		Vector2 v2 = (Vector2)second;

		return IsClockwise (v2, v1, mOrigin);
	}

	public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin){
		if (first == second) {
			return 0;
		}

		Vector2 firstOffset = first - origin;
		Vector2 secondOffset = second - origin;

		float angle1 = Mathf.Atan2 (firstOffset.x, firstOffset.y);
		float angle2 = Mathf.Atan2 (secondOffset.x, secondOffset.y);

		if (angle1 < angle2) {
			return 1;
		}

		if (angle1 > angle2) {
			return -1;
		}

		return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
	}
}
