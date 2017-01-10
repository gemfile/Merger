using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Util;

public class GameView : MonoBehaviour {
	GameObject[] decks;
	GameObject worrior;
	GameObject potion;
	GameObject magic;
	Dictionary<int, GameObject> Monsters;
	Dictionary<int, GameObject> Weapons;

	public void Prepare()
	{
		decks = new GameObject[] {
			ResourceCache.Instantiate("BlueDeck"), 
			ResourceCache.Instantiate("Deck"),
		};
		worrior = ResourceCache.Instantiate("Worrior");
		potion = ResourceCache.Instantiate("Potion");
		magic = ResourceCache.Instantiate("Magic");
	}

	public void MakeField(string type, int value)
	{
		
	}
}
