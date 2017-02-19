using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IGameController
	{
		void Init();
		bool IsGameOver();
		void Move(int pivotIndex, int colOffset, int rowOffset);
		ICardModel GetCard(int index);
	}

	public class Position
	{
		public int index;
		public int row;
		public int col;
		public static int Cols = 1;
		public static int Rows = 1;

		public Position(int index) 
		{
			this.index = index;
			row = index / Position.Cols;
			col = index % Position.Cols;
		}

		public Position(int pivotIndex, int colOffset, int rowOffset)
		{
			row = (pivotIndex / Position.Cols) + rowOffset;
			col = (pivotIndex % Position.Cols) + colOffset;
			index = row * Position.Cols + col;
		}

		public bool IsAcceptableIndex() 
		{
			return col >= 0 && col < Position.Cols && row >= 0 && row < Position.Rows;
		}
	}

	public class MergingInfo
	{
		public Position sourcePosition;
		public Position targetPosition;
		public PlayerInfo playerInfo;
		public Position mergingPosition;
	}

	public class GameController: IGameController
	{
		readonly IGameModel model;
		readonly IGameView view;
		public GameController(IGameModel model, IGameView view)
		{
			this.model = model;
			this.view = view;

			Position.Cols = model.Cols;
			Position.Rows = model.Rows;
		}

		public void Init()
		{
			model.Init();
			view.Init();

			PrepareADeck(model.CardsData);
			SetField();
			MakePlayer();
			ListenToInput();
			view.RequestCoroutine(BeginTheGame());
		}

		IEnumerator BeginTheGame() 
		{
			while (true) 
			{
				Watch();

				if (IsGameOver()) {
					Debug.Log("=== The game is over! ===");
					yield break;
				}

				yield return null;
			}
		}

		public bool IsGameOver()
		{
			return model.Player.Hp <= 0;
		}

		void Watch()
		{
			// var previousPhase = GetPreviousPhase();
			var currentPhase = GetCurrentPhase();

			if (currentPhase == PhaseOfGame.PLAY)
			{
				int playerIndex = GetIndex(model.Player);

				var mergingCoordinates = new List<int[]> {
					new int[2]{ -1, 0 },
					new int[2]{ 1, 0 },
					new int[2]{ 0, -1 },
					new int[2]{ 0, 1 },
				};

				var canMerge = mergingCoordinates.Aggregate(false, (result, mergingCoordinate) => {
					var nearbyPosition = new Position(playerIndex, mergingCoordinate[0], mergingCoordinate[1]);
					if(nearbyPosition.IsAcceptableIndex()) {
						ICardModel nearbyCard = GetCard(nearbyPosition.index);
						result = ( result || (nearbyCard != null && !model.Player.CantMerge(nearbyCard)) || nearbyCard is MonsterModel );
					}

					return result;
				});

				if (!canMerge) {
					Debug.Log($"Can not merge anywhere! {!canMerge}");
				}
			}
			
			// Fill the fields.
			if (currentPhase == PhaseOfGame.FILL && model.Fields.Values.Any(field => field is EmptyModel))
			{
				var emptyFields = model.Fields.Where(field => field.Value is EmptyModel).ToDictionary(p => p.Key, p => p.Value);
				emptyFields.ForEach(emptyField => {
					AddField(emptyField.Key, model.DeckQueue.Dequeue());
				});

				Debug.Log("=== Choose the card ===");
				int count = 0;
				model.Fields.ForEach(iCardModel => Debug.Log(iCardModel.Value.Data.type + ", " + iCardModel.Value.Data.value + ", " + count++));
				Debug.Log("===============");

				view.Field.ShowField();
				view.UI.UpdateDeckCount(model.DeckQueue.Count);
				SetNextPhase();
			}
		}

		void ListenToInput()
		{
			view.Swipe.OnSwipe.AddListener(swipeInfo => {
				if(!view.Field.IsPlaying) {
					switch(swipeInfo.direction) {
						case Direction.Right: Merge(1, 0); break;
						case Direction.Left: Merge(-1, 0); break;
						case Direction.Up: Merge(0, 1); break;
						case Direction.Down: Merge(0, -1); break;
					}
				}
			});
		}

		void Merge(int colOffset, int rowOffset)
		{
			var playerIndex = GetIndex(model.Player);
			var playerPosition = new Position(playerIndex);
			var infrontofPlayer = new Position(playerIndex, colOffset, rowOffset);
			var inbackofPlayer = new Position(playerIndex, -colOffset, -rowOffset);
			var frontCard = GetCard(infrontofPlayer.index);

			var backMonsterCard = GetCard(inbackofPlayer.index) as IMerger;

			if (infrontofPlayer.IsAcceptableIndex() && frontCard != null && !model.Player.CantMerge(frontCard))
			{
				var playerCard = model.Player as IMerger;
				PlayerInfo playerInfo = playerCard.Merge(frontCard, playerPosition, infrontofPlayer);
				model.Fields[infrontofPlayer.index] = model.Player;
				model.Fields[playerIndex] = new EmptyModel();

				var mergingInfo = new MergingInfo() {
					sourcePosition = playerPosition,
					targetPosition = infrontofPlayer,
					playerInfo = playerInfo,
					mergingPosition = infrontofPlayer
				};
				view.Field.MergeField(mergingInfo);
				view.UI.UpdateCoin(mergingInfo.playerInfo.coin);
				Move(playerIndex, colOffset, rowOffset);
			}
			else if (inbackofPlayer.IsAcceptableIndex() && backMonsterCard != null)
			{
				PlayerInfo playerInfo = backMonsterCard.Merge(model.Player, playerPosition, inbackofPlayer);
				model.Fields[inbackofPlayer.index] = new EmptyModel();

				var mergingInfo = new MergingInfo() {
					sourcePosition = inbackofPlayer,
					targetPosition = playerPosition,
					playerInfo = playerInfo,
					mergingPosition = inbackofPlayer
				};
				view.Field.MergeField(mergingInfo);
				view.UI.UpdateCoin(mergingInfo.playerInfo.coin);
				Move(inbackofPlayer.index, colOffset, rowOffset);
			}
		}

		public void Move(int pivotIndex, int colOffsetFrom, int rowOffsetFrom)
		{
			var inbackofPivot = new Position(pivotIndex, -colOffsetFrom, -rowOffsetFrom);
			Debug.Log($"Move to : {pivotIndex}, {colOffsetFrom}, {rowOffsetFrom}");
			Debug.Log($"inbackofPosition : {inbackofPivot.row}, {inbackofPivot.col}");
			if (inbackofPivot.IsAcceptableIndex())
			{
				ICardModel backCard = GetCard(inbackofPivot.index);
				if (backCard != null)
				{
					model.Fields[pivotIndex] = backCard;
					model.Fields[inbackofPivot.index] = new EmptyModel();
					view.Field.MoveField(new Position(pivotIndex), inbackofPivot);
					Move(inbackofPivot.index, colOffsetFrom, rowOffsetFrom);
				}
			}
			else
			{
				SetNextPhase();
			}
		}

		void SetNextPhase()
		{
			model.CurrentIndexOfPhase++;
			if (model.CurrentIndexOfPhase >= model.PhasesOfGame.Count)
			{
				model.CurrentIndexOfPhase = 0;
			}
			Debug.Log($"What's next? {GetCurrentPhase()}");
		}

		PhaseOfGame GetCurrentPhase()
		{
			return model.PhasesOfGame[model.CurrentIndexOfPhase];
		}

		PhaseOfGame GetPreviousPhase()
		{
			var previousIndexOfPhase = model.CurrentIndexOfPhase - 1;
			if (previousIndexOfPhase < 0)
			{
				previousIndexOfPhase = model.PhasesOfGame.Count - 1;
			}

			return model.PhasesOfGame[previousIndexOfPhase];
		}

		public ICardModel GetCard(int index)
		{
			if (index >= 0 && index < model.Fields.Count)
			{
				return model.Fields[index];
			}
			else
			{
				return null;
			}
		}

		void PrepareADeck(List<ICardModel> deckList)
		{
			model.DeckQueue = new Queue<ICardModel>(deckList.Shuffle());
			deckList.Clear();

			int count = 0;
			model.DeckQueue.ForEach(card => Debug.Log(card.Data.type + ", " + card.Data.value + ", " + count++));
		}

		void SetField()
		{
			Enumerable.Range(0, model.CountOfFields).ForEach(index => model.Fields.Add(index, new EmptyModel()));
			view.Field.SetField(model.CountOfFields);
		}

		void MakePlayer()
		{
			AddField(1, model.Player);
		}

		void AddField(int key, ICardModel card)
		{
			model.Fields[key] = card;
			view.Field.AddField(
				new Position(key),
				card.Data,
				new Position(GetIndex(model.Player))
			);
		}

		int GetIndex(ICardModel card)
		{
			return model.Fields.FirstOrDefault(field => field.Value == card).Key;
		}
	}
}
