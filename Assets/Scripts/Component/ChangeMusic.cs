using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class ChangeMusic: MonoBehaviour 
{
	[SerializeField]
	AudioClip bgmFirstScene;
	[SerializeField]
	AudioClip bgmGameScene;
	Dictionary<string, AudioClip> bgms;
	
	
	void Awake()
	{
		bgms = new Dictionary<string, AudioClip>() {
			{ "FirstScene", bgmFirstScene },
			{ "GameScene", bgmGameScene }
		};
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void Destroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode m)
	{
		Debug.Log("Scene loaded: " + scene.name + " in mode: " + m);
        
		var audioClip = bgms[scene.name];
		GetComponent<AudioSource>().clip = audioClip;
		GetComponent<AudioSource>().Play();
	}
}
