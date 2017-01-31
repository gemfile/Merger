using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

namespace com.Gemfile.Merger 
{
	public class FieldAddingEvent: UnityEvent<Position, CardData> {}
	public class FieldMergingEvent: UnityEvent<Position, PlayerData> {}
	public class FieldMovingEvent: UnityEvent<Position, Position> {}
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

	public enum PhaseOfGame
	{
		FILL = 0,
		PLAY
	}

	public class GameMain 
	{
		public static readonly int cols = 3;
		public static readonly int rows = 3;
		readonly int countOfFields;

		public FieldPreparingEvent fieldPreparingEvent;
		public FieldAddingEvent fieldAddingEvent;
		public FieldMergingEvent fieldMergingEvent;
		public FieldMovingEvent fieldMovingEvent;

		readonly Dictionary<int, ICard> fields;
		Queue<ICard> deckQueue;
		Player player;
		Queue<Monster> monsterQueue;

		List<PhaseOfGame> phaseOfGame;
		int currentIndexOfPhase;

		public GameMain() 
		{
			fields = new Dictionary<int, ICard>();
			fieldAddingEvent = new FieldAddingEvent();
			fieldMergingEvent = new FieldMergingEvent();
			fieldPreparingEvent = new FieldPreparingEvent();
			fieldMovingEvent = new FieldMovingEvent();
			countOfFields = rows * cols;
			phaseOfGame = new List<PhaseOfGame>{ PhaseOfGame.FILL, PhaseOfGame.PLAY };
			currentIndexOfPhase = phaseOfGame.IndexOf(PhaseOfGame.FILL);
			monsterQueue = new Queue<Monster>();
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
					Type.GetType("com.Gemfile.Merger." + cardData.type),
					cardData
				);
				deckList.Add(card);
			});

			return deckList;
		}

		public void Update()
		{
			var previousPhase = GetPreviousPhase();
			var currentPhase = GetCurrentPhase();

			if(currentPhase == PhaseOfGame.PLAY)
			{
				int playerIndex = GetIndex(player);

				var mergingCoordinates = new List<int[]> {
					new int[2]{ -1, 0 },
					new int[2]{ 1, 0 },
					new int[2]{ 0, -1 },
					new int[2]{ 0, 1 },
				};

				var canMerge = mergingCoordinates.Aggregate(false, (result, mergingCoordinate) => {
					bool isMovable = IsMovable(playerIndex, mergingCoordinate[0], mergingCoordinate[1]);

					if(isMovable) {
						var mergingindex = GetCardIndex(playerIndex, mergingCoordinate[0], mergingCoordinate[1]);
						ICard card = GetCard(mergingindex);
						result = result || (card != null && !CantMerge(card));
					}

					return result;
				});

				if (!canMerge) {
//					SetNextPhase();
					Debug.Log($"Can not merge anywhere! {!canMerge}");
				}
			}
			
			// Fill the fields.
			if (currentPhase == PhaseOfGame.FILL && fields.Values.Any(field => field is Empty))
			{
				var emptyFields = fields.Where(field => field.Value is Empty).ToDictionary(p => p.Key, p => p.Value);
				emptyFields.ForEach(emptyField => AddAField(emptyField.Key, deckQueue.Dequeue()));

				Debug.Log("=== Choose the card ===");
				int count = 0;
				fields.ForEach(icard => Debug.Log(icard.Value.GetType() + ", " + icard.Value.GetValue() + ", " + count++));
				Debug.Log("===============");
				SetNextPhase();
			}
		}

		void SetNextPhase()
		{
			currentIndexOfPhase++;
			if (currentIndexOfPhase >= phaseOfGame.Count)
			{
				currentIndexOfPhase = 0;
			}
			Debug.Log($"What's next? {GetCurrentPhase()}");
		}

		PhaseOfGame GetCurrentPhase()
		{
			return phaseOfGame[currentIndexOfPhase];
		}

		PhaseOfGame GetPreviousPhase()
		{
			var previousIndexOfPhase = currentIndexOfPhase - 1;
			if (previousIndexOfPhase < 0)
			{
				previousIndexOfPhase = phaseOfGame.Count - 1;
			}

			return phaseOfGame[previousIndexOfPhase];
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

		int GetIndex(ICard card)
		{
			return fields.FirstOrDefault(field => field.Value == card).Key;
		}

		public bool IsNearFromPlayer(int index)
		{
			return (Mathf.Abs(index - GetIndex(player)) == 1);
		}

		public void Merge(int colOffset, int rowOffset)
		{
			var playerIndex = GetIndex(player);
//			while (IsMovable(playerIndex, colOffset, rowOffset)) 
// 			{
//				colOffset = colOffset + colOffset;
//				rowOffset = rowOffset + rowOffset;
//			}

			if (IsMovable(playerIndex, colOffset, rowOffset))
			{
				var mergingIndex = GetCardIndex(playerIndex, colOffset, rowOffset);
				ICard card = GetCard(mergingIndex);
				if (card != null && !CantMerge(card))
				{
					PlayerData playerData = player.Merge(card);
					fields[mergingIndex] = player;
					fields[playerIndex] = new Empty();
					fieldMergingEvent.Invoke(
						new Position(mergingIndex),
						playerData
					);
					Move(playerIndex, colOffset, rowOffset);
				}
			}
		}

		void Move(int targetIndex, int colOffset, int rowOffset)
		{
			if (IsMovable(targetIndex, -colOffset, -rowOffset))
			{
				var cardIndex = GetCardIndex(targetIndex, -colOffset, -rowOffset);
				ICard card = GetCard(cardIndex);
				if (card != null)
				{
					fields[targetIndex] = card;
					fields[cardIndex] = new Empty();
					fieldMovingEvent.Invoke(
						new Position(targetIndex),
						new Position(cardIndex)
					);
					Move(cardIndex, colOffset, rowOffset);
				}
			}
			else
			{
				SetNextPhase();
			}
		}

		bool IsMovable(int pivot, int colOffset, int rowOffset)
		{
			var playerPosition = new Position(pivot);
			int colNext = playerPosition.col + colOffset;
			int rowNext = playerPosition.row + rowOffset;
			return colNext >= 0 && colNext < cols && rowNext >= 0 && rowNext < rows;
		}

		int GetCardIndex(int pivot, int colOffset, int rowOffset)
		{
			return pivot + (colOffset + rowOffset * cols);
		}

		ICard GetCard(int index)
		{
			if (index >= 0 && index < fields.Count)
			{
				return fields[index];
			}
			else
			{
				return null;
			}
		}

		bool CantMerge(ICard target)
		{
			return (
				target is Empty ||
				(target is Monster && player.Atk <= 0)
			);
		}

		public bool IsOver
		{
			get { return player.Hp <= 0; }
		}
	}
}