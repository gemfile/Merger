using System.Collections.Generic;
using UnityEngine;
using Scripts.Game.Card;
using UnityEngine.Events;
using Scripts.Util;
using System;
using System.Linq;

namespace Scripts.Game 
{
	public class FieldAddingEvent: UnityEvent<Position, CardData> {}
	public class FieldMergingEvent: UnityEvent<Position, PlayerData> {}
	public class FieldPreparingEvent: UnityEvent<int> {}

	public class Position
	{
		public int index;
		public int row;
		public int col;

		public Position(int index) 
		{
			this.index = index;
			row = index / GameMain.cols;
			col = index % GameMain.cols;
		}
	}

	public class CardData 
	{
		public string type;
		public int value;
		public string resourceName;
		public string cardName;
	}

	public class PlayerData 
	{
		public int hp;
		public int atk;
		public int def;
		public int coin;
	}

	public class Empty: CardBase 
	{
		public Empty(): base(null) {}
	}

	public class GameMain 
	{
		public static readonly int cols = 3;
		public static readonly int rows = 3;
		readonly int countOfFields;

		public FieldPreparingEvent fieldPreparingEvent;
		public FieldAddingEvent fieldAddingEvent;
		public FieldMergingEvent fieldMergingEvent;

		readonly Dictionary<int, ICard> fields;
		Queue<ICard> deckQueue;
		Player player;

		public GameMain() 
		{
			fields = new Dictionary<int, ICard>();
			fieldAddingEvent = new FieldAddingEvent();
			fieldMergingEvent = new FieldMergingEvent();
			fieldPreparingEvent = new FieldPreparingEvent();
			countOfFields = rows * cols;
		}

		public void Prepare() 
		{
			PrepareADeck(MakeCardDatas());
			SetFields();
			MakeAPlayer();
		}

		void SetFields() 
		{
			Enumerable.Range(0, countOfFields).ForEach(index => fields.Add(index, new Empty()));
			fieldPreparingEvent.Invoke(countOfFields);
		}

		void MakeAPlayer()
		{
			player = new Player(new CardData { value=13, resourceName="Worrior", cardName="Worrior" });
			AddAField(1, player);
		}

		void PrepareADeck(List<ICard> deckList) 
		{
			deckQueue = new Queue<ICard>(deckList.Shuffle());
			deckList.Clear();

			int count = 0;
			deckQueue.ForEach(card => Debug.Log(card.GetType() + ", " + card.GetValue() + ", " + count++));
		}

		List<ICard> MakeCardDatas() 
		{
			var cardDataList = new List<CardData> {
				// Potion 9
				new CardData { type="Potion", value=2, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=3, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=4, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=5, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=6, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=7, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=8, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=9, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=10, resourceName="Potion", cardName="Potion" },

				// Monster 18
				new CardData { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
				new CardData { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
				new CardData { type="Monster", value=3, resourceName="Rat", cardName="Rat" },
				new CardData { type="Monster", value=3, resourceName="Rat", cardName="Rat" },
				new CardData { type="Monster", value=4, resourceName="Bat", cardName="Bat" },
				new CardData { type="Monster", value=4, resourceName="Bat", cardName="Bat" },
				new CardData { type="Monster", value=5, resourceName="Snake", cardName="Snake" },
				new CardData { type="Monster", value=5, resourceName="Snake", cardName="Snake" },
				new CardData { type="Monster", value=6, resourceName="GoblinHammer", cardName="Goblin\nHammer" },
				new CardData { type="Monster", value=6, resourceName="GoblinHammer", cardName="Goblin\nHammer" },
				new CardData { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" },
				new CardData { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" },
				new CardData { type="Monster", value=8, resourceName="OrcSpear", cardName="OrcSpear" },
				new CardData { type="Monster", value=8, resourceName="OrcSpear", cardName="OrcSpear" },
				new CardData { type="Monster", value=9, resourceName="OrcKnife", cardName="OrcKnife" },
				new CardData { type="Monster", value=9, resourceName="OrcKnife", cardName="OrcKnife" },
				new CardData { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },
				new CardData { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },

				// Coin 9
				new CardData { type="Coin", value=2, resourceName="Coin1", cardName="Coin" },
				new CardData { type="Coin", value=3, resourceName="Coin1", cardName="Coin" },
				new CardData { type="Coin", value=4, resourceName="Coin2", cardName="Coin" },
				new CardData { type="Coin", value=5, resourceName="Coin2", cardName="Coin" },
				new CardData { type="Coin", value=6, resourceName="Coin3", cardName="Coin" },
				new CardData { type="Coin", value=7, resourceName="Coin3", cardName="Coin" },
				new CardData { type="Coin", value=8, resourceName="Coin4", cardName="Coin" },
				new CardData { type="Coin", value=9, resourceName="Coin4", cardName="Coin" },
				new CardData { type="Coin", value=10, resourceName="Coin5", cardName="Coin" },

				// Weapon 11
				new CardData { type="Weapon", value=2, resourceName="Knife", cardName="Knife" },
				new CardData { type="Weapon", value=2, resourceName="Knife", cardName="Knife" },
				new CardData { type="Weapon", value=3, resourceName="Club", cardName="Club" },
				new CardData { type="Weapon", value=3, resourceName="Club", cardName="Club" },
				new CardData { type="Weapon", value=4, resourceName="Sword", cardName="Sword" },
				new CardData { type="Weapon", value=4, resourceName="Sword", cardName="Sword" },
				new CardData { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
				new CardData { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
				new CardData { type="Weapon", value=6, resourceName="Hammer", cardName="Hammer" },
				new CardData { type="Weapon", value=6, resourceName="Hammer", cardName="Hammer" },
				new CardData { type="Weapon", value=7, resourceName="IronMace", cardName="IronMace" },

				// Magic 5
				new CardData { type="Magic", value=2, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=3, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=4, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=5, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=6, resourceName="Magic", cardName="Magic" },
			};

			var deckList = new List<ICard>();
			cardDataList.ForEach(cardData => {
				var card = (ICard)Activator.CreateInstance(
					Type.GetType("Scripts.Game.Card." + cardData.type), 
					cardData
				);
				deckList.Add(card);
			});

			return deckList;
		}

		public void FillTheField() 
		{
			if (fields.Values.Any(field => field is Empty)) 
			{
				var emptyFields = fields.Where(field => field.Value is Empty).ToDictionary(p => p.Key, p => p.Value);
				emptyFields.ForEach(emptyField => AddAField(emptyField.Key, deckQueue.Dequeue()));

				Debug.Log("=== Choose the card ===");
				int count = 0;
				fields.ForEach(icard => Debug.Log(icard.Value.GetType() + ", " + icard.Value.GetValue() + ", " + count++));
				Debug.Log("===============");
			}
		}

		void AddAField(int key, ICard card) 
		{
			fields[key] = card;
			fieldAddingEvent.Invoke(
				new Position(key),
				new CardData {
					type = card.GetType().Name, 
					value = card.GetValue(), 
					resourceName = card.GetResourceName(), 
					cardName = card.GetCardName()
				}
			);
		}

		int GetPlayerIndex() 
		{
			return fields.FirstOrDefault(x => x.Value == player).Key;
		}

		public bool IsNearFromPlayer(int index) 
		{
			return (Mathf.Abs(index - GetPlayerIndex()) == 1);
		}

		public void Merge(int xOffset, int yOffset) 
		{
			var playerIndex = GetPlayerIndex();
			var mergingIndex = playerIndex + (xOffset + yOffset * cols);

			if (mergingIndex >= 0 && mergingIndex < fields.Count) 
			{
				ICard card = fields[mergingIndex];
				PlayerData playerData = player.Merge(card);
				fields[mergingIndex] = player;
				fields[playerIndex] = new Empty();
				fieldMergingEvent.Invoke(
					new Position(mergingIndex),
					playerData
				);				
			}
		}

		public bool IsOver 
		{
			get { return player.Hp <= 0; }
		}
	}
}