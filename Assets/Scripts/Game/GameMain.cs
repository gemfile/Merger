using System.Collections.Generic;
using UnityEngine;
using Scripts.Game.Card;
using UnityEngine.Events;
using Scripts.Util;
using System;

namespace Scripts.Game {
	public class FieldAddingInfo {
		public int xIndex;
		public CardData cardData;
	}
	public class FieldAddingEvent: UnityEvent<FieldAddingInfo> {}
	public class FieldMergingEvent: UnityEvent<int> {}

	public class CardData {
		public string type;
		public int value;
		public string resourceName;
		public string cardName;
	}

	public class GameMain {
		Queue<ICard> deckQueue;
		List<ICard> deckList;

		int xIndexOfFields;

		Player player;
		readonly List<ICard> fields;

		public FieldAddingEvent fieldAddingEvent;
		public FieldMergingEvent fieldMergingEvent;

		public GameMain() {
			fields = new List<ICard>();
			fieldAddingEvent = new FieldAddingEvent();
			fieldMergingEvent = new FieldMergingEvent();
			xIndexOfFields = 0;
		}

		public void Prepare() {
			PrepareADeck();
			MakeAPlayer();
		}

		void MakeAPlayer() {
			player = new Player(13, "Worrior", "Worrior");
			AddAField(player);
		}

		void PrepareADeck() {
			MakeCardDatas();

			deckQueue = new Queue<ICard>(deckList.Shuffle());
			deckList.Clear();
			deckList = null;

			xIndexOfFields = 0;

			int count = 0;
			foreach (ICard card in deckQueue) {
				Debug.Log( card.GetType() + ", " + card.GetValue() + ", " + count++ );
			}
		}

		void MakeCardDatas() {
			var cardDataList = new List<CardData>() {
				// Potion 9
				new CardData() { type="Potion", value=2, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=3, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=4, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=5, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=6, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=7, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=8, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=9, resourceName="Potion", cardName="Potion" },
				new CardData() { type="Potion", value=10, resourceName="Potion", cardName="Potion" },

				// Monster 18
				new CardData() { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
				new CardData() { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
				new CardData() { type="Monster", value=3, resourceName="Rat", cardName="Rat" },
				new CardData() { type="Monster", value=3, resourceName="Rat", cardName="Rat" },
				new CardData() { type="Monster", value=4, resourceName="Bat", cardName="Bat" },
				new CardData() { type="Monster", value=4, resourceName="Bat", cardName="Bat" },
				new CardData() { type="Monster", value=5, resourceName="Snake", cardName="Snake" },
				new CardData() { type="Monster", value=5, resourceName="Snake", cardName="Snake" },
				new CardData() { type="Monster", value=6, resourceName="GoblinHammer", cardName="Goblin\nHammer" },
				new CardData() { type="Monster", value=6, resourceName="GoblinHammer", cardName="Goblin\nHammer" },
				new CardData() { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" },
				new CardData() { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" },
				new CardData() { type="Monster", value=8, resourceName="OrcSpear", cardName="OrcSpear" },
				new CardData() { type="Monster", value=8, resourceName="OrcSpear", cardName="OrcSpear" },
				new CardData() { type="Monster", value=9, resourceName="OrcKnife", cardName="OrcKnife" },
				new CardData() { type="Monster", value=9, resourceName="OrcKnife", cardName="OrcKnife" },
				new CardData() { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },
				new CardData() { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },

				// Coin 9
				new CardData() { type="Coin", value=2, resourceName="Coin1", cardName="Coin" },
				new CardData() { type="Coin", value=3, resourceName="Coin1", cardName="Coin" },
				new CardData() { type="Coin", value=4, resourceName="Coin2", cardName="Coin" },
				new CardData() { type="Coin", value=5, resourceName="Coin2", cardName="Coin" },
				new CardData() { type="Coin", value=6, resourceName="Coin3", cardName="Coin" },
				new CardData() { type="Coin", value=7, resourceName="Coin3", cardName="Coin" },
				new CardData() { type="Coin", value=8, resourceName="Coin4", cardName="Coin" },
				new CardData() { type="Coin", value=9, resourceName="Coin4", cardName="Coin" },
				new CardData() { type="Coin", value=10, resourceName="Coin5", cardName="Coin" },

				// Weapon 11
				new CardData() { type="Weapon", value=2, resourceName="Knife", cardName="Knife" },
				new CardData() { type="Weapon", value=2, resourceName="Knife", cardName="Knife" },
				new CardData() { type="Weapon", value=3, resourceName="Club", cardName="Club" },
				new CardData() { type="Weapon", value=3, resourceName="Club", cardName="Club" },
				new CardData() { type="Weapon", value=4, resourceName="Sword", cardName="Sword" },
				new CardData() { type="Weapon", value=4, resourceName="Sword", cardName="Sword" },
				new CardData() { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
				new CardData() { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
				new CardData() { type="Weapon", value=6, resourceName="Hammer", cardName="Hammer" },
				new CardData() { type="Weapon", value=6, resourceName="Hammer", cardName="Hammer" },
				new CardData() { type="Weapon", value=7, resourceName="IronMace", cardName="IronMace" },

				// Magic 5
				new CardData() { type="Magic", value=2, resourceName="Magic", cardName="Magic" },
				new CardData() { type="Magic", value=3, resourceName="Magic", cardName="Magic" },
				new CardData() { type="Magic", value=4, resourceName="Magic", cardName="Magic" },
				new CardData() { type="Magic", value=5, resourceName="Magic", cardName="Magic" },
				new CardData() { type="Magic", value=6, resourceName="Magic", cardName="Magic" },
			};

			deckList = new List<ICard>();
			foreach (CardData cardData in cardDataList) {
				var card = (ICard)Activator.CreateInstance(
					Type.GetType("Scripts.Game.Card." + cardData.type), 
					cardData.value, 
					cardData.resourceName, 
					cardData.cardName
				);
				deckList.Add(card);
			}
		}

		public void FillTheField() {
			if (fields.Count < 3) {
				AddAField(deckQueue.Dequeue());

				Debug.Log("=== Choose the card ===");
				int count = 0;
				foreach (var icard in fields) {
					Debug.Log( icard.GetType() + ", " + icard.GetValue() + ", " + count++ );
				}
				Debug.Log("===============");
			}
		}

		void AddAField(ICard card) {
			fields.Add(card);
			fieldAddingEvent.Invoke(new FieldAddingInfo() {
				xIndex = xIndexOfFields++,
				cardData = new CardData() {
					type = card.GetType().Name, 
					value = card.GetValue(), 
					resourceName = card.GetResourceName(), 
					cardName = card.GetCardName()
				}
			});
		}

		public bool IsNearFromPlayer(int index) {
			return (Mathf.Abs(index - fields.IndexOf(player)) == 1);
		}

		public void Merge(int xOffset, int yOffset) {
			var xIndex = xOffset;
			if (xIndex >= 0 && xIndex < fields.Count) {
				ICard card = fields[xIndex];
				player.Merge(card);

				fields.RemoveAt(xIndex);
				fieldMergingEvent.Invoke(xOffset);				
			}
		}

		public bool IsOver {
			get { return player.Hp <= 0; }
		}
	}
}