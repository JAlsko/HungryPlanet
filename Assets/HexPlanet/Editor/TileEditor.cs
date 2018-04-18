using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Tile))]
[CanEditMultipleObjects]
public class TileEditor : Editor {

	public Tile tile;
	public GameObject objectToPlace;
	public int color;
	public float extrudeHeight;
	private static string placeObjToolTip = "Place an object on this tile.  If the object is a prefab and not in the scene, it will be instantiated and then placed.  If it does exist in the scene, it will be moved to this tile.";

	void OnEnable(){
		tile = (Tile)target;
	}

	public override void OnInspectorGUI(){
		DrawDefaultInspector ();

		//Place object interface
		EditorGUILayout.BeginHorizontal ();
		objectToPlace = (GameObject)EditorGUILayout.ObjectField (new GUIContent("Object to place", placeObjToolTip), objectToPlace, typeof(GameObject), true); 
		if (GUILayout.Button ("Place Object") && objectToPlace != null) {
			//If its a prefab, spawn it then place it
			if(EditorUtility.IsPersistent(objectToPlace)){
				GameObject o = Instantiate(objectToPlace) as GameObject;
				if (targets.Length > 1) {
					for (int i = 0; i < targets.Length; i++) {
						Tile t = (Tile)targets [i];
						t.placeObject (o);
					}
				} 
				else {
					tile.placeObject(o);
				}

			}
			//If it is a scene object, move its current instance to the tile
			else if(objectToPlace.activeInHierarchy){
				if (targets.Length > 1) {
					for (int i = 0; i < targets.Length; i++) {
						Tile t = (Tile)targets [i];
						t.placeObject (objectToPlace);
					}
				} 
				else {
					tile.placeObject(objectToPlace);
				}
			}
			//Clear the object slot
			objectToPlace = null;
		}
		EditorGUILayout.EndHorizontal ();

		//Color Interface
		EditorGUILayout.BeginHorizontal();
		color = (int)EditorGUILayout.IntField (new GUIContent ("Tile Color", placeObjToolTip), color);
		if (GUILayout.Button ("Set Color") && color > 0 && color != 0) {
			if (targets.Length > 1) {
				for (int i = 0; i < targets.Length; i++) {
					Tile t = (Tile)targets [i];
					t.setColor(color);
				}
			} 
			else {
				tile.setColor(color);
			}
		}
		EditorGUILayout.EndHorizontal ();

		//Extrude interface
		EditorGUILayout.BeginHorizontal ();
		extrudeHeight = (float)EditorGUILayout.FloatField ("Extrude Height", extrudeHeight);
		if (GUILayout.Button ("Extrude") && extrudeHeight != 0) {
			if (targets.Length > 1) {
				for (int i = 0; i < targets.Length; i++) {
					Tile t = (Tile)targets [i];
					t.Extrude(extrudeHeight);
				}
			} 
			else {
				tile.Extrude(extrudeHeight);
			}
		}
		EditorGUILayout.EndHorizontal ();
	}
}
