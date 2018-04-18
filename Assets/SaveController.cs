using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveController : MonoBehaviour {

	[SerializeField] private float autoSaveRepeat = 30;
	private float elapsedTime = 0;

	void Start () {
		LoadGame ();
	}
	
	void FixedUpdate () {
		elapsedTime += Time.deltaTime;
		if (elapsedTime > autoSaveRepeat) {
			elapsedTime = 0;
			SaveGame ();
		}
	}

	public void SaveGame() {
		GetComponent<TileController>().SaveTiles ();
		GetComponent<MoneyController>().SaveMoney ();
		GetComponent<BonusController>().SaveBonuses ();
		GetComponent<SoundController>().SaveVolume ();
	}

	public void LoadGame() {
		GetComponent<TileController>().LoadTiles ();
		GetComponent<MoneyController>().LoadMoney ();
		GetComponent<BonusController>().LoadBonuses ();
		GetComponent<SoundController>().LoadVolume ();
	}

	public void DeleteSave() {
		GetComponent<TileController>().DeleteTileSave ();
		GetComponent<MoneyController>().DeleteMoneySave ();
		GetComponent<BonusController>().DeleteBonusSave ();
		GetComponent<SoundController>().DeleteVolumeSave ();
	}
}
