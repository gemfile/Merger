using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Game;
using System.Linq;

public class Root : MonoBehaviour {

	// Use this for initialization
	void Start () {
//        new Coin();
//        new Potion();
//        new Weapon();
//        new Magic();
//        new Monster();

        var potionValues = Enumerable.Range(2, 9);
        var decks = new List<>();

        foreach (int value in potionValues)
        {
            new Coin(value);

        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
