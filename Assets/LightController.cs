using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

	private float elapsedTime = 0;							//Keeps track of time elapsed since game start
	private bool isDay = true;								//Tracks whether or not currently in daytime

	[SerializeField] private MeshRenderer sky1;				//Skybox1
	[SerializeField] private MeshRenderer sky2;				//Skybox2
	[SerializeField] private Light sun;						//Directional light

	private float daySpeed = 0.01f;							//Speed multiplier for time elapse

	[SerializeField] private Material voxel_material;		//Material used for buildings and props

	[SerializeField] private Gradient sunGradient;			//Color gradient for sunlight throughout day
	[SerializeField] private Gradient ambientGradient;		//Value gradient for ambient light throughout day
	[SerializeField] private Gradient skyGradient;			//Value gradient for skybox texture offset throughout day

	private static float DAY_END_TIME = .35f;				//Based on day length of 1, end time of daylight
	private static float DAY_START_TIME = .7f;				//Start time of daylight

	//Unused variables for preset time settings
	[Serializable]
	public class TimeSetting
	{
		public Color sunColor;
		public Color ambientColor;
		public float sunIntensity;
		public float skyOffset;
	}
	[SerializeField] private List<TimeSetting> timeSettings = new List<TimeSetting>();

	void Start () {
		skyGradient.mode = GradientMode.Blend;
	}

	void FixedUpdate() {
		elapsedTime += Time.deltaTime * daySpeed; //Update time based on current day speed

		if ((elapsedTime % 1 >= DAY_END_TIME && elapsedTime % 1 < DAY_START_TIME) && isDay) { //If current time is after dusk and haven't switched to night settings, do so
			isDay = false;
			ManageWorldLights ();
		} else if ((elapsedTime % 1 >= DAY_START_TIME || elapsedTime % 1 < DAY_END_TIME) && !isDay) { //If current time is after dawn and haven't switched to day settings, do so
			isDay = true;
			ManageWorldLights ();
		}
	}

	void Update () {
		if (daySpeed != 0) {
			sun.color = sunGradient.Evaluate ((elapsedTime % 1)); //Update sunlight color based on sunlight gradient
			RenderSettings.ambientLight = ambientGradient.Evaluate ((elapsedTime % 1)); //Update ambient light level from ambient gradient

			//Update skybox material with offset to reflect time of day
			float offset = (skyGradient.Evaluate ((elapsedTime % 1)).r); //Get texture offset amount based on gradient
			sky1.material.mainTextureOffset = Vector2.right * offset;
			sky2.material.mainTextureOffset = Vector2.right * offset;
		}
	}

	//Unused currently, sets time of day to preset
	public void SetTime(int time) { 
		if (time >= timeSettings.Count) {
			Debug.Log ("Invalid time setting index: " + time);
			return;
		}
		sky1.material.mainTextureOffset = Vector2.right * timeSettings [time].skyOffset;
		sky2.material.mainTextureOffset = Vector2.right * timeSettings [time].skyOffset;
		sun.color = timeSettings [time].sunColor;
		sun.intensity = timeSettings [time].sunIntensity;
		RenderSettings.ambientLight = timeSettings [time].ambientColor;
	}

	//Updates point lights in scene
	void ManagePointLights() {
		Light[] allLights = FindObjectsOfType<Light>(); //Get all active lights in scene
		//Go through all point lights and turn them on/off if it's night/daytime
		foreach (Light l in allLights) {
			if (l.type == LightType.Point) {
				l.enabled = !isDay;
			}
		}
	}

	//Updates emission value for building material based on day/night
	void ManageMaterialLights() {
		if (isDay) {
			voxel_material.SetColor ("_EmissionColor", Color.black);
		} else {
			voxel_material.SetColor ("_EmissionColor", Color.white);
		}
	}

	public void ManageWorldLights() {
		ManagePointLights ();
		ManageMaterialLights ();
	}

	public void setDaySpeed(float val) {
		daySpeed = val;
	}

	public float getDaySpeed() {
		return daySpeed;
	}
}
