using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ItemController : MonoBehaviour {

	//--------------------------------------------------------

	public Dictionary<string, int> ic = new Dictionary<string, int>();
	public Dictionary<string, int> ir = new Dictionary<string, int>();
	public Dictionary<string, int> ib = new Dictionary<string, int>();

	[System.Serializable]
	public class Item
	{
		public string name;
		public int rarity;
		public int biome;
		public string store1Name;
		public string store1Desc;
		public string store2Name;
		public string store2Desc;
		public string farmName;
		public string farmDesc;
		public Color uiColor;
		public Sprite icon;
	}
		
	public List<Item> items = new List<Item>();    

	//--------------------------------------------------------

	void Awake() {
		DontDestroyOnLoad (this.gameObject);

		//for (int i = 0; i < items.Count; i++) {
			//ic.Add (items [i].name, items [i].count);
		//}
		for (int i = 0; i < items.Count; i++) {
			ir.Add (items [i].name, items [i].rarity);
		}
		for (int i = 0; i < items.Count; i++) {
			ib.Add (items [i].name, items [i].biome);
		}
	}
	
	void Update () {
		
	}

	public Item GetItem(string name) {
		foreach (Item i in items) {
			if (i.name.ToLower () == name) {
				return i;
			}
		}
		return null;
	}

	public int GetItemCount(string item) {
		return ic [item];
	}

	public int GetItemRarity(string item) {
		return ir [item];
	}

	public int GetItemBiome(string item) {
		return ib [item];
	}
}
