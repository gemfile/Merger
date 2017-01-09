using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Game;
using System.Linq;
using Scripts.Game.Card;
using UnityEngine;

namespace Scripts {
	public class Root: MonoBehaviour {
		List<ICard> deck;
		int cursorOfDeck = 0;

		Player player;
		List<ICard> field;

		Root() {
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
		}

		// Use this for initialization
		void Start () {
			MakeADeck();
			MakeAPlayer();
			StartCoroutine("StartTheGame");
		}

		IEnumerator StartTheGame() {
			field = new List<ICard>();

			while (true) {
				if (field.Count < 3) {
					field.Add(deck[cursorOfDeck++]);

					Debug.Log("=== Choose the card ===");
					int count = 0;
					foreach (var card in field) {
						Debug.Log( card.GetType() + ", " + card.GetValue() + ", " + count++ );
					}
					Debug.Log("===============");
				}

				if (Input.GetKeyDown(KeyCode.Alpha1)) {
					Merge(field[0]);
					field.RemoveAt(0);
				}
				if (Input.GetKeyDown(KeyCode.Alpha2)) {
					Merge(field[1]);
					field.RemoveAt(1);
				}
				if (Input.GetKeyDown(KeyCode.Alpha3)) {
					Merge(field[2]);
					field.RemoveAt(2);
				}

				if (player.Hp <= 0) {
					Debug.Log("=== The game is over! ===");
					yield break;
				}

				yield return null;
			}
		}

		void MakeAPlayer() {
			player = new Player(13);
		}

		void MakeADeck() {
			var potionValues = Enumerable.Range(2, 9);
			deck = new List<ICard>();

			foreach (int value in potionValues) {
				deck.Add( new Coin(value) );
				deck.Add( new Potion(value) );
				deck.Add( new Weapon(value) );
				deck.Add( new Magic(value) );
				deck.Add( new Monster(value) );
				deck.Add( new Monster(value) );
			}

			deck.Shuffle();

#if UNITY_EDITOR
			int count = 0;
			foreach (Card card in deck) {
				Debug.Log( card.GetType() + ", " + card.GetValue() + ", " + count++ );
			}
#endif
		}

		// Update is called once per frame
		void Update () {
			
		}

		void Merge(ICard card) {
			player.Merge(card);
		}
	}

}