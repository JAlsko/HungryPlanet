using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

	public float elapsedTime = 0;

	public bool isDay = true;

	public MeshRenderer sky1;
	public MeshRenderer sky2;
	public Light sun;

	public float daySpeed = 1;

	public Material voxel_material;

	public Gradient sunGradient;
	public Gradient ambientGradient;
	public Gradient skyGradient;

	public float offset = 0;

	[Serializable]
	public class TimeSetting
	{
		public Color sunColor;
		public Color ambientColor;
		public float sunIntensity;
		public float skyOffset;
	}
	//.35 and .7
	public List<TimeSetting> timeSettings = new List<TimeSetting>();

	void Start () {

		sky1 = GameObject.Find ("SkyDome").GetComponent<MeshRenderer>();
		sky2 = GameObject.Find ("SkyDome(1)").GetComponent<MeshRenderer>();
		sun = GameObject.Find ("Sunlight").GetComponent<Light>();
	}

	public void SetTime(int time) {
		sky1.material.mainTextureOffset = Vector2.right * timeSettings [time].skyOffset;
		sky2.material.mainTextureOffset = Vector2.right * timeSettings [time].skyOffset;
		sun.color = timeSettings [time].sunColor;
		sun.intensity = timeSettings [time].sunIntensity;
		RenderSettings.ambientLight = timeSettings [time].ambientColor;
	}

	void FixedUpdate() {
		elapsedTime += Time.deltaTime * daySpeed;

		if ((elapsedTime % 1 >= .35f && elapsedTime % 1 < .7f) && isDay) {
			isDay = false;
			ManageWorldLights ();
		} else if ((elapsedTime % 1 >= .7f || elapsedTime % 1 < .35f) && !isDay) {
			isDay = true;
			ManageWorldLights ();
		}
	}

	void Update () {
		if (daySpeed != 0) {
			skyGradient.mode = GradientMode.Blend;
			sun.color = sunGradient.Evaluate ((elapsedTime % 1));
			RenderSettings.ambientLight = ambientGradient.Evaluate ((elapsedTime % 1));
			offset = (skyGradient.Evaluate ((elapsedTime % 1)).r);
			sky1.material.mainTextureOffset = Vector2.right * offset;
			sky2.material.mainTextureOffset = Vector2.right * offset;
		}
	}

	public void ManageWorldLights() {
		ManagePointLights ();
		ManageMaterialLights ();
	}

	void ManagePointLights() {
		Light[] allLights = FindObjectsOfType<Light>();
		foreach (Light l in allLights) {
			if (l.type == LightType.Point) {
				l.enabled = !isDay;
			}
		}
	}

	void ManageMaterialLights() {
		if (isDay) {
			voxel_material.SetColor ("_EmissionColor", Color.black);
		} else {
			voxel_material.SetColor ("_EmissionColor", Color.white);
		}
	}
}
