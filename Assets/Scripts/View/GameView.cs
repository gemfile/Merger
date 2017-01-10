using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Util;

public class GameView : MonoBehaviour {
	GameObject[] decks;
	GameObject worrior;

	public void Prepare() {
		decks = new GameObject[] {
			ResourceCache.Instantiate("BlueDeck", transform), 
			ResourceCache.Instantiate("WhiteDeck", transform),
		};
		worrior = ResourceCache.Instantiate("Worrior", transform);
	}

	public void MakeField(string type, int value, string resourceName) {
		var gameObject = ResourceCache.Instantiate(resourceName, transform);
		gameObject.SetActive(true);
	}
}
