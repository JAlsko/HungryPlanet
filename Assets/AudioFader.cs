using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFader : MonoBehaviour {

	[SerializeField] private float fadeTime = .5f;

	private AudioSource asrc;
	private float normalVolume;
	private float targetVolume;

	void Start () {
		asrc = GetComponent<AudioSource> ();
		normalVolume = asrc.volume;
	}

	void LateUpdate () {
		if (Mathf.Abs(asrc.volume - targetVolume) > .001f) {
			asrc.volume = Mathf.Lerp (asrc.volume, targetVolume, Time.deltaTime * fadeTime);
		}
	}

	public bool isPlaying() {
		return targetVolume != 0;
	}

	public void updateNormalVolume(float newVol) {
		normalVolume = newVol;
	}

	public void FadeOut() {
		targetVolume = 0;
	}

	public void FadeIn() {
		targetVolume = normalVolume;
	}
}
