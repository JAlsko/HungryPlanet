using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class DragToOrbit : MonoBehaviour
{
	public Transform target;
	public Transform mainTarget;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	public float zoomSpeed = 10f;

	public bool clampX = false;

	public float zoomThreshold = 2;

	public float zoomXRotationLimit = 50f;
	public float zoomYRotationLimit = 50f;

	public float xMinLimit = -80f;
	public float xMaxLimit = 80f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	public float zoomDistMin = 5f;
	public float zoomDistMax = 12.5f;

	public float normalDistMin = 23.5f;
	public float normalDistMax = 30f;

	public float distanceMin = .5f;
	public float distanceMax = 15f;

	public float smoothTime = 2f;
	public float transitionTime = 1f;
	public float transitionDelay = .5f;

	public float rotationYAxis = 0.0f;
	public float rotationXAxis = 0.0f;

	public List<AudioClip> biomeSounds = new List<AudioClip> ();
	public AudioSource ambienceSource;

	float velocityX = 0.0f;
	float velocityY = 0.0f;

	Quaternion planetNewAngle;

	Vector3 angles;

	GameObject gameController;

	// Use this for initialization
	void Start()
	{
		angles = transform.eulerAngles;
		rotationYAxis = angles.y;
		rotationXAxis = angles.x;

		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}

		ambienceSource = transform.Find ("Environmental Ambience").GetComponent<AudioSource> ();

		gameController = GameObject.FindWithTag ("GameController");
	}

	void OnEnable() {
		angles = transform.eulerAngles;
		rotationYAxis = angles.y;
		rotationXAxis = angles.x;

		if (!CheckIfTile(target)) {
			xSpeed = .5f;
			yMaxLimit = 80;
			yMinLimit = -80;
			distanceMax = normalDistMax;
			distanceMin = normalDistMin;
			clampX = false;
		}
		/*else if (CheckIfTile(target)) {
			GetComponent<Camera> ().orthographic = true;
			GetComponent<Camera> ().orthographicSize = zoomDistMax;
			GetComponent<Camera> ().nearClipPlane = -10;
			xSpeed = 3;
			xMaxLimit = rotationYAxis + zoomXRotationLimit;
			xMinLimit = rotationYAxis - zoomXRotationLimit;
			yMaxLimit = rotationXAxis + zoomYRotationLimit;
			yMinLimit = rotationXAxis - zoomYRotationLimit;
			distanceMax = zoomDistMax;
			distanceMin = zoomDistMin;
			clampX = true;
		}*/
	}

	public bool CheckIfTile(Transform toCheck) {
		if (toCheck.parent != null) {
			if (toCheck.parent.gameObject.GetComponent<Tile> ()) {
				return true;
			}
		}
		return false;
	}

	public void ChangeTarget(Transform newTarget) {
		if (newTarget == target) {
			return;
		}
		else if (!CheckIfTile(newTarget)) {
			StopTileAudio ();
			gameController.GetComponent<TileController>().CancelStoreBuild ();
			gameController.GetComponent<TileController>().CancelStoreUpgrade ();
			planetNewAngle = Quaternion.identity;
			Invoke ("EnableSpin", transitionDelay);
			//GetComponent<Camera> ().orthographic = false;
			//GetComponent<Camera> ().nearClipPlane = .3f;
			xSpeed = .5f;
			yMaxLimit = 80;
			yMinLimit = -80;
			distanceMax = normalDistMax;
			distanceMin = normalDistMin;
			clampX = false;
		}

		if (CheckIfTile (newTarget)) {
			//Quaternion newAngle = Quaternion.Inverse (newTarget.transform.rotation) * Quaternion.identity;
			Quaternion Qto = mainTarget.rotation; // Rotation to match the child to.
			DisableSpin();
			planetNewAngle = mainTarget.rotation * (Quaternion.Inverse(newTarget.rotation) * Qto);

			//BuildController.Instance.updateFocusedTile (newTarget.parent.gameObject);
			//newTarget.parent.gameObject.GetComponent<Tile> ().selected = false;

			//GetComponent<Camera> ().orthographic = true;
			//GetComponent<Camera> ().orthographicSize = zoomDistMax;
			//GetComponent<Camera> ().nearClipPlane = -5;
			xSpeed = 3;
			xMaxLimit = rotationYAxis + zoomXRotationLimit;
			xMinLimit = rotationYAxis - zoomXRotationLimit;
			yMaxLimit = 90;
			switch (newTarget.parent.GetComponent<Tile> ().color) {
			case 0:
				yMinLimit = 10;
				break;
			case 1:
				yMinLimit = 10;
				break;
			case 2:
				yMinLimit = 5;
				break;
			case 3:
				yMinLimit = 20;
				break;
			case 4:
				yMinLimit = 30;
				break;
			}
			distanceMax = zoomDistMax;
			distanceMin = zoomDistMin;
			clampX = false;

			gameController.GetComponent<TileController>().selectedTile = newTarget.parent.gameObject.GetComponent<Tile>();
			StartTileAudio ();
			gameController.GetComponent<UIController>().OpenTileMenu (gameController.GetComponent<TileController>().selectedTile);
			gameController.GetComponent<UIController>().UpdateTileMenu ();
		}

		if (CheckIfTile(target)) {
			target.parent.gameObject.GetComponent<Tile> ().selected = false;
			gameController.GetComponent<TileController>().selectedTile = null;
		}
		//rotationXAxis = newTarget.transform.localEulerAngles.x;
		//rotationYAxis = newTarget.transform.localEulerAngles.y;
		target = newTarget;
	}

	void Update() {
		if (Input.GetAxis ("Mouse ScrollWheel") < 0 && distance == distanceMax && CheckIfTile (target) && !EventSystem.current.IsPointerOverGameObject()) {
			//target.parent.GetChild (3).gameObject.SetActive (false);
			for (int c = 0; c < 3; c++) {
				target.parent.GetChild(3).transform.GetChild (c).GetChild (0).gameObject.SetActive (false);
			}
			ChangeTarget (mainTarget);
			gameController.GetComponent<UIController>().CloseTileInfo ();
		}
	}

	void EnableSpin() {
		GetComponent<PlanetSpinner> ().enabled = true;
	}

	void DisableSpin() {
		GetComponent<PlanetSpinner> ().StopSpin ();
		GetComponent<PlanetSpinner> ().enabled = false;
	}

	void LateUpdate()
	{
		if (target)
		{
			if (Input.GetMouseButton(1))
			{
				velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * 0.02f;
				velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
			}

			rotationYAxis += velocityX;
			rotationXAxis -= velocityY;

			rotationXAxis = Mathf.Clamp(rotationXAxis, yMinLimit, yMaxLimit);
			if (clampX) {
				rotationYAxis = Mathf.Clamp(rotationYAxis, xMinLimit, xMaxLimit);
			}

			//Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
			Quaternion rotation = toRotation;

			if (!EventSystem.current.IsPointerOverGameObject())
				distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax);

			/*RaycastHit hit;
			if (Physics.Linecast(target.position, transform.position, out hit))
			{
				distance -= hit.distance;
			}*/
			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position;
			if (GetComponent<Camera> ().orthographic) {
				GetComponent<Camera> ().orthographicSize = distance;
			}
				
			if (Input.GetMouseButton(1)) {
				transform.position = position;
				transform.rotation = rotation;
			} else {
				transform.position = Vector3.Lerp (transform.position, position, Time.deltaTime * smoothTime);
				transform.rotation = Quaternion.Lerp (transform.rotation, rotation, Time.deltaTime * smoothTime);
			}

			if (!GetComponent<PlanetSpinner> ().enabled) {
				mainTarget.rotation = Quaternion.Lerp (mainTarget.rotation, planetNewAngle, Time.deltaTime * transitionTime);
			}
			velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
			velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
		}

	}

	void StartTileAudio() {
		int biome = gameController.GetComponent<TileController>().selectedTile.color;
		if (biomeSounds [biome] != null) {
			ambienceSource.clip = biomeSounds [biome];
			ambienceSource.Play ();
			ambienceSource.GetComponent<AudioFader> ().FadeIn ();
		}
	}

	void StopTileAudio() {
		if (ambienceSource.isPlaying) {
			ambienceSource.GetComponent<AudioFader> ().FadeOut ();
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
		
}