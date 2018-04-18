using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour {

	public static ObjectPooler current;
	public Transform targetParent;

	public List<GameObject> pooledObjects;

	void Awake() {
		current = this;
	}

	void Start () {
		for (int i = 0; i < pooledObjects.Count; i++) {
			pooledObjects[i].SetActive (false);
		}

	}

	public GameObject GetPooledObject() {
		for (int i = 0; i < pooledObjects.Count; i++) {
			if (!pooledObjects [i].activeInHierarchy) {
				return pooledObjects [i];
			}
		}

		return null;
	}

}
