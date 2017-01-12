using System.Collections.Generic;
using UnityEngine;
using Scripts.Game.Card;
using System.Linq;
using UnityEngine.Events;
using Scripts.Util;

namespace Scripts.Game {
	[System.Serializable]
	public class FieldAddingEvent: UnityEvent<string, int, string, string> {}
	[System.Serializable]
	public class FieldMergingEvent: UnityEvent<int> {}

	public class GameMain {
		List<ICard> deck;
		int cursorOfDeck;

		Player player;
		readonly List<ICard> field;

		public FieldAddingEvent fieldAddingEvent;
		public FieldMergingEvent fieldMergingEvent;

		public GameMain() {
			field = new List<ICard>();
			fieldAddingEvent = new FieldAddingEvent();
			fieldMergingEvent = new FieldMergingEvent();
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
			deck = new List<ICard>();
			MakeCoins();
			MakePotions();
			MakeMonsters();
			MakeWeapons();
			MakeMagics();
			deck.Shuffle();

			cursorOfDeck = 0;

			int count = 0;
			foreach (ICard card in deck) {
				Debug.Log( card.GetType() + ", " + card.GetValue() + ", " + count++ );
			}
		}

		void MakePotions() {
			foreach (int value in Enumerable.Range(2, 9)) {
				deck.Add( new Potion(value, "Potion", "Potion") );				
			}
		}

		void MakeMonsters() {
			var monsterDic = new Dictionary<int, string>() {
				{ 2, "Slime.B" },
				{ 3, "Rat" },
				{ 4, "Bat" },
				{ 5, "Snake" },
				{ 6, "GoblinHammer" },
				{ 7, "Skeleton" },
				{ 8, "OrcSpear" },
				{ 9, "OrcKnife" },
				{ 10, "Minotaur" },
			};

			foreach (int value in Enumerable.Range(2, 9)) {
				deck.Add( new Monster(value, monsterDic[value], monsterDic[value]) );
				deck.Add( new Monster(value, monsterDic[value], monsterDic[value]) );
			}
		}

		void MakeCoins() {
			int coinIndex = 1;
			int count = 0;
			foreach (int value in Enumerable.Range(2, 9)) {
				deck.Add( new Coin(value, "Coin" + coinIndex, "Coin") );
				if (count % 2 == 1) {
					coinIndex++;
				}
				count++;
			}
		}

		void MakeWeapons() {
			var weaponDic = new Dictionary<int, string>() {
				{ 2, "Knife" },
				{ 3, "Club" },
				{ 4, "Sword" },
				{ 5, "Axe" },
				{ 6, "Hammer" },
				{ 7, "IronMace" },
			};

			foreach (int value in Enumerable.Range(2, 5)) {
				deck.Add( new Weapon(value, weaponDic[value], weaponDic[value]) );
				deck.Add( new Weapon(value, weaponDic[value], weaponDic[value]) );
			}
			deck.Add( new Weapon(7, weaponDic[7], weaponDic[7]) );
		}

		void MakeMagics() {
			foreach (int value in Enumerable.Range(2, 5)) {
				deck.Add( new Magic(value, "Magic", "Magic") );
			}
		}

		public void FillTheField()
		{
			if (field.Count < 3) {
				AddAField(deck[cursorOfDeck++]);

				Debug.Log("=== Choose the card ===");
				int count = 0;
				foreach (var icard in field) {
					Debug.Log( icard.GetType() + ", " + icard.GetValue() + ", " + count++ );
				}
				Debug.Log("===============");
			}
		}

		void AddAField(ICard card) {
			field.Add(card);
			fieldAddingEvent.Invoke(card.GetType().Name, card.GetValue(), card.GetResourceName(), card.GetCardName());
		}

		public bool IsIndexNearFromPlayer(int index) {
			return (Mathf.Abs(index - field.IndexOf(player)) == 1);
		}

		public void Merge(int index) {
			ICard card = field[index];
			player.Merge(card);

			field.RemoveAt(index);
			fieldMergingEvent.Invoke(index);
		}

		public bool IsOver {
			get { 
				//				Debug.Log($"hi {player.Hp <= 0}");
				return player.Hp <= 0; 
			}
		}
	}
}