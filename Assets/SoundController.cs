using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SoundController : MonoBehaviour {

	private static string volumeSaveFileName = "volumeSettings.dat";

	[SerializeField] private AudioSource musicSource;
	float defaultMusicVolume;
	[SerializeField] private List<AudioSource> sfxSources = new List<AudioSource>();
	[SerializeField] private AudioSource notificationSource;
	[SerializeField] private AudioClip buildSound;
	[SerializeField] private AudioClip destroySound;
	private List<float> defaultSFXVolumes = new List<float>();

	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;

	[SerializeField] private float musicVolume = 1;
	[SerializeField] private float sfxVolume = 1;

	[SerializeField] private float savedMusicVolume = 1;
	[SerializeField] private float savedSFXVolume = 1;

	[SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();
	[SerializeField] private float songDelay = 30f;

	private List<int> alreadyPlayed = new List<int> ();
	private bool finishedPlaying = false;

	void Start () {
		UnityEngine.Random.InitState (UnityEngine.Random.Range(0, 255));
		sfxSources.Add (notificationSource);
		QueueNewSong ();
		InitializeDefaultVolumes ();
	}
	
	void Update () {
		if (!musicSource.isPlaying && !finishedPlaying) {
			finishedPlaying = true;

		}
	}

	void QueueNewSong() {			
		if (alreadyPlayed.Count == musicClips.Count) {
			alreadyPlayed.Clear ();
		}

		int songToPlay = UnityEngine.Random.Range (0, musicClips.Count);
		while (alreadyPlayed.Contains(songToPlay)) {
			songToPlay = UnityEngine.Random.Range (0, musicClips.Count);
		}

		alreadyPlayed.Add (songToPlay);
		musicSource.clip = musicClips [songToPlay];

		StartCoroutine(playSongInSeconds(songDelay));
	}

	IEnumerator playSongInSeconds(float delay) {
		yield return new WaitForSeconds (delay);
		musicSource.Play ();
		Invoke("QueueNewSong", musicSource.clip.length);
	}

	Dictionary<string, float> getVolumeSettingsDict() {
		Dictionary<string, float> volumeSettings = new Dictionary<string, float> ();
		volumeSettings ["music"] = musicVolume;
		volumeSettings ["sfx"] = sfxVolume;
		return volumeSettings;
	}

	void extractLoadedSettings(Dictionary<string, float> savedSettings) {
		savedMusicVolume = savedSettings ["music"];
		savedSFXVolume = savedSettings ["sfx"];
		unmuteMusic ();
		unmuteSFX ();
	}

	public void SaveVolume() {
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file1 = File.Open (Application.persistentDataPath + "/" + volumeSaveFileName, FileMode.OpenOrCreate);

		Dictionary<string, float> volumeSettings = getVolumeSettingsDict ();

		bf.Serialize (file1, volumeSettings);
		file1.Close ();
	}

	public void loadSavedVolume(Dictionary<string, float> bs) {
		if (bs == null) {
			Debug.Log ("No saved money file");
			savedMusicVolume = 1;
			savedSFXVolume = 1;
			unmuteMusic ();
			unmuteSFX ();
		} else {
			extractLoadedSettings (bs);
		}
	}

	public void LoadVolume() {
		if (File.Exists (Application.persistentDataPath + "/" + volumeSaveFileName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file1 = File.Open (Application.persistentDataPath + "/" + volumeSaveFileName, FileMode.Open);
			Dictionary<string, float> bs = (Dictionary<string, float>)bf.Deserialize (file1);
			file1.Close ();

			loadSavedVolume (bs);

		} else {
			loadSavedVolume (null);
		}
	}

	public void DeleteVolumeSave() {
		if (File.Exists (Application.persistentDataPath + "/" + volumeSaveFileName)) {
			File.Delete (Application.persistentDataPath + "/" + volumeSaveFileName);
		}
		LoadVolume ();
	}

	public void UpdateMusicVolume() {
		savedMusicVolume = musicSlider.value;
		musicVolume = savedMusicVolume;
		UpdateSoundButtons ();
		UpdateMusicSource ();
	}

	void InitializeDefaultVolumes() {
		defaultMusicVolume = musicSource.volume;
		defaultSFXVolumes.Clear ();
		foreach (AudioSource aSrc in sfxSources) {
			defaultSFXVolumes.Add (aSrc.volume);
		}
	}

	float GetNormalizedVolume(float value, float max) {
		return max * value;
	}

	void UpdateMusicSource() {
		musicSource.volume = GetNormalizedVolume(musicVolume, defaultMusicVolume);
	}

	void UpdateSFXSources() {
		for (int i = 0; i < sfxSources.Count; i++) {
			AudioSource curSrc = sfxSources [i];
			float newVol = GetNormalizedVolume (sfxVolume, defaultSFXVolumes [i]);
			if (curSrc.GetComponent<AudioFader> ()) {
				curSrc.GetComponent<AudioFader> ().updateNormalVolume (newVol);
				if (!curSrc.GetComponent<AudioFader> ().isPlaying ()) {
					curSrc.GetComponent<AudioFader> ().FadeOut ();
					continue;
				}
			}
			curSrc.volume = newVol;
		}
	}

	public void UpdateSFXVolume() {
		savedSFXVolume = sfxSlider.value;
		sfxVolume = savedSFXVolume;
		UpdateSoundButtons ();
		UpdateSFXSources ();
	}

	void UpdateSoundButtons() {
		SoundToggle[] buttons = FindObjectsOfType<SoundToggle> ();
		foreach (SoundToggle st in buttons) {
			if (st.setting == "music") {
				st.SetToggle (!isMusicMuted ());
			} else if (st.setting == "sfx") {
				st.SetToggle (!isSFXMuted ());
			}
		}
	}

	public bool isMusicMuted() {
		return musicVolume == 0;
	}

	public bool isSFXMuted() {
		return sfxVolume == 0;
	}

	public void muteMusic() {
		musicVolume = 0;
		musicSlider.value = 0;
		UpdateSoundButtons ();
		UpdateMusicSource ();
	}

	public void muteSFX() {
		sfxVolume = 0;
		sfxSlider.value = 0;
		UpdateSoundButtons ();
		UpdateSFXSources ();
	}

	public void unmuteMusic() {
		musicVolume = savedMusicVolume;
		musicSlider.value = musicVolume;
		UpdateSoundButtons ();
		UpdateMusicSource ();
	}

	public void unmuteSFX() {
		sfxVolume = savedSFXVolume;
		sfxSlider.value = sfxVolume;
		UpdateSoundButtons ();
		UpdateSFXSources ();
	}

	public float getMusicVolume() {
		return musicVolume;
	}

	public float getSFXVolume() {
		return sfxVolume;
	}

	public void playBuildSound() {
		notificationSource.clip = buildSound;
		notificationSource.Play ();
	}

	public void playDestroySound() {
		notificationSource.clip = destroySound;
		notificationSource.Play ();
	}
}
