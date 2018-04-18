using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverListener : MonoBehaviour
{
	private static UIHoverListener instance;

	public bool isUIOverride { get; private set; }

	public static UIHoverListener Instance {
		get {
			return instance;
		}
	}

	void Start() {
		instance = this;
	}

	void Update()
	{
		// It will turn true if hovering any UI Elements
		isUIOverride = EventSystem.current.IsPointerOverGameObject ();
	}
}