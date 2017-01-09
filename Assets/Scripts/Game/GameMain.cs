using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Game.Card;
using System.Linq;
using UnityEngine.Events;

namespace Scripts.Game {
	[System.Serializable]
	public class FieldEvent: UnityEvent<string, int> {}

	public class GameMain {
		List<ICard> deck;
		int cursorOfDeck;

		Player player;
		readonly List<ICard> field;

		public FieldEvent fieldEvent;

		public GameMain() {
			field = new List<ICard>();
			fieldEvent = new FieldEvent();
			PrepareADeck();
			MakeAPlayer();
		}

		void MakeAPlayer() {
			player = new Player(13);
		}

		void PrepareADeck() {
			cursorOfDeck = 0;

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

			int count = 0;
			foreach (ICard card in deck) {
				Debug.Log( card.GetType() + ", " + card.GetValue() + ", " + count++ );
			}
		}

		public void FillTheField()
		{
			if (field.Count < 3) {
				ICard card = deck[cursorOfDeck++];
				field.Add(card);
				fieldEvent.Invoke(card.GetType(), card.GetValue());

				Debug.Log("=== Choose the card ===");
				int count = 0;
				foreach (var icard in field) {
					Debug.Log( icard.GetType() + ", " + icard.GetValue() + ", " + count++ );
				}
				Debug.Log("===============");
			}
		}

		public void Merge(int index) {
			ICard card = field[index];
			field.RemoveAt(index);

			player.Merge(card);
		}

		public bool IsOver {
			get { 
				//				Debug.Log($"hi {player.Hp <= 0}");
				return player.Hp <= 0; 
			}
		}
	}
}