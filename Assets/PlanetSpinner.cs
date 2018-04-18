using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpinner : MonoBehaviour {

	private static PlanetSpinner instance = null;

	public GameObject hexsphere;

	[SerializeField] private float minSlowMult = .99f;
	[SerializeField] private float maxSlowMult = .995f;

	[SerializeField] private float currentSlowMult = 1;

	[SerializeField] private float stopMult = 1;
	[SerializeField] private float spinSpeed = 50;
	[SerializeField] private float maxSpeedPerSpin = 100;
	[SerializeField] private float maxSpeed = 2000;
	[SerializeField] private float currentSpeed = 0;

	[SerializeField] private float minPitySpeed = 500;
	[SerializeField] private float pityMPS = 5;

	[SerializeField] private float minSpinPitch = 0;
	[SerializeField] private float maxSpinPitch = 0;
	[SerializeField] private float minWindPitch = 0;
	[SerializeField] private float maxWindPitch = 0;

	[SerializeField] private float minSpinVolume = 0;
	[SerializeField] private float maxSpinVolume = 0;
	[SerializeField] private float minWindVolume = 0;
	[SerializeField] private float maxWindVolume = 0;

	[SerializeField] private AudioSource spinSound;
	[SerializeField] private AudioSource windSound;

	[SerializeField] private float minDaySpeed = .01f;
	[SerializeField] private float maxDaySpeed = .5f;

	private float currentSpin = 0;

	public GameObject gameController;

	void Start () {
		instance = this;

		gameController = GameObject.FindWithTag ("GameController");
		currentSpeed = 0;
		StopSpin ();
	}

	public static PlanetSpinner Instance {
		get {
			return instance;
		}
	}

	public void StopSpin() {
		currentSpeed = 0;
		HandleSpeed ();
	}

	public float GetSpeedRatio() {
		return (Mathf.Abs(currentSpeed) / maxSpeed);
	}

	void Update () {
		if (Input.GetMouseButtonUp (0)) {
			currentSpin = 0;
		}
		if (Input.GetMouseButton (1)) {
			currentSpeed *= stopMult;
			if (Mathf.Abs(currentSpeed) < .1f && currentSpeed != 0) {
				StopSpin ();
			}
		}
		if (Input.GetMouseButton(0))
		{
			if (Input.GetAxis ("Mouse X") == 0) {
				currentSpeed *= stopMult;
				if (Mathf.Abs(currentSpeed) < .1f && currentSpeed != 0) {
					StopSpin ();
				}
			} else {
				if (Mathf.Abs(currentSpeed) < maxSpeed && Mathf.Abs(currentSpin) < maxSpeedPerSpin) {
					currentSpeed += spinSpeed * Input.GetAxis ("Mouse X") * 0.02f;
					currentSpin += spinSpeed * Input.GetAxis ("Mouse X") * 0.02f;
				}
			}
		}
	}

	void HandleSpeed() {
		if (Mathf.Abs (currentSpeed) > minPitySpeed && gameController.GetComponent<MoneyController>().getMPS() <= 0) {
			gameController.GetComponent<MoneyController>().SetPityMPS (pityMPS);
		} else {
			gameController.GetComponent<MoneyController>().SetPityMPS (0);
		}

		float currentSpeedRatio = Mathf.Abs(currentSpeed) / maxSpeed;
		spinSound.pitch = minSpinPitch + (currentSpeedRatio * (maxSpinPitch - minSpinPitch));
		windSound.pitch = minWindPitch + (currentSpeedRatio * (maxWindPitch - minWindPitch));
		spinSound.volume = (minSpinVolume + (currentSpeedRatio * (maxSpinVolume - minSpinVolume))) * gameController.GetComponent<SoundController>().getSFXVolume();
		windSound.volume = (minWindVolume + (currentSpeedRatio * (maxWindVolume - minWindVolume))) * gameController.GetComponent<SoundController>().getSFXVolume();

		currentSlowMult = minSlowMult + (currentSpeedRatio * (maxSlowMult - minSlowMult));

		if (!Input.GetMouseButton(0)) {
			currentSpeed *= Mathf.Pow(currentSlowMult, 2);
			if (gameController.GetComponent<LightController>() != null) {
				gameController.GetComponent<LightController>().daySpeed = minDaySpeed + (currentSpeedRatio * (maxDaySpeed - minDaySpeed));
			}
			if (Mathf.Abs(currentSpeed) < .1f && currentSpeed != 0) {
				StopSpin ();
			}
		}
		hexsphere.transform.Rotate (Vector3.up * Time.deltaTime * currentSpeed);
	}

	void FixedUpdate() {
		HandleSpeed ();
	}
		
}
