using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ResourceRandomizer : MonoBehaviour {

	public List<List<string>> biomeFoods = new List<List<string>>();
	public Dictionary<string, int> foodTiers = new Dictionary<string, int>();
	public Dictionary<int, List<string>> foodToAdd = new Dictionary<int, List<string>> ();
	public List<int> countPerTier = new List<int> ();

	void GetRandomFoodList () {
		foodToAdd [0].Add (biomeFoods [0] [0]);
		for (int biome = 0; biome < biomeFoods.Count; biome++) {
			foreach (string food in biomeFoods[biome]) {
				for (int i = 0; i < countPerTier [foodTiers [food]]; i++) {
					foodToAdd [biome].Add (food);
				}
			}
			Shuffle(foodToAdd [biome]);
		}
	}

	private static System.Random rng = new System.Random();  

	public void Shuffle(List<string> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			string value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}

	void Update () {
		
	}
}
