using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace com.Gemfile.Merger
{
	public interface IFieldController<M, V>: IBaseController<M, V>
	{
		List<NavigationInfo> GetWheresCanMerge();
		void FillEmptyFields();
		void Move(int targetIndex, int colFrom, int rowFrom);
		bool Merge(int colOffset, int rowOffset);
		int GetIndex(ICardModel card);
		ICardModel GetCard(int index);
		void AddField(int key, ICardModel card);
		MergingEvent OnMerged { get; }
		bool IsThereEmptyModel();
	}

	public class MergingEvent: UnityEvent<MergingInfo> {}

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

	public class NavigationInfo
	{
		public int sourceIndex;
		public List<Position> wheresCanMerge;
	}
	
	public class FieldController<M, V>: BaseController<M, V>, IFieldController<M, V>
		where M: IFieldModel, new()
		where V: IFieldView
	{
		public MergingEvent OnMerged { get { return onMerged; } } 
		readonly MergingEvent onMerged = new MergingEvent();
		
		public override void Init(V view) 
		{
			base.Init(view);

			Position.Cols = Model.Cols;
			Position.Rows = Model.Rows;

			PrepareADeck(Model.CardsData);
			SetField();
			MakePlayer();
		}

		public void FillEmptyFields()
		{
			Debug.Log("FillEmptyFields");
			var emptyFields = Model.Fields
				.Where(field => field.Value is EmptyModel)
				.ToDictionary(p => p.Key, p => p.Value);
				
			emptyFields.ForEach(emptyField => 
				AddField(emptyField.Key, Model.DeckQueue.Dequeue())
			);
			View.ShowField();
		}

		public bool IsThereEmptyModel()
		{
			return Model.Fields.Values.Any(field => field is EmptyModel);
		}

		public List<NavigationInfo> GetWheresCanMerge()
		{
			var wheresWannaMerge = new List<int[]> {
				new int[2]{ -1, 0 },
				new int[2]{ 1, 0 },
				new int[2]{ 0, -1 },
				new int[2]{ 0, 1 },
			};

			var navigationInfos = new List<NavigationInfo>();

			var mergers = Model.Fields
				.Where(field => field.Value is IMerger)
				.ToDictionary(p => p.Key, p => p.Value);

			mergers.ForEach((merger) => {
				int sourceIndex = merger.Key;
				var wheresCanMerge = new List<Position>();

				wheresWannaMerge.ForEach(whereWannaMerge => {
					var positionNearby = new Position(sourceIndex, whereWannaMerge[0], whereWannaMerge[1]);

					if (positionNearby.IsAcceptableIndex())
					{
						ICardModel nearbyCard = GetCard(positionNearby.index);
						if (nearbyCard != null && !(merger.Value as IMerger).CantMerge(nearbyCard))
						{
							wheresCanMerge.Add(positionNearby);
						}
					}
				});

				navigationInfos.Add(
					new NavigationInfo {sourceIndex = merger.Key, wheresCanMerge = wheresCanMerge}
				);
			});

			return navigationInfos;
		}

		public bool Merge(int colOffset, int rowOffset)
		{
			var isMerged = false;
			
			var playerIndex = GetIndex(Model.Player);
			var playerPosition = new Position(playerIndex);
			var infrontofPlayer = new Position(playerIndex, colOffset, rowOffset);
			var inbackofPlayer = new Position(playerIndex, -colOffset, -rowOffset);
			var frontCard = GetCard(infrontofPlayer.index);

			var backMonsterCard = GetCard(inbackofPlayer.index) as IMerger;

			if (infrontofPlayer.IsAcceptableIndex() 
				&& frontCard != null 
				&& !Model.Player.CantMerge(frontCard))
			{
				var playerCard = Model.Player as IMerger;
				PlayerInfo playerInfo = playerCard.Merge(frontCard, playerPosition, infrontofPlayer);
				Model.Fields[infrontofPlayer.index] = Model.Player;
				Model.Fields[playerIndex] = new EmptyModel();

				var mergingInfo = new MergingInfo() {
					sourcePosition = playerPosition,
					targetPosition = infrontofPlayer,
					playerInfo = playerInfo,
					mergingPosition = infrontofPlayer
				};
				View.MergeField(mergingInfo);
				OnMerged.Invoke(mergingInfo);
				Move(playerIndex, colOffset, rowOffset);
				isMerged = true;
			}
			else if (inbackofPlayer.IsAcceptableIndex() && backMonsterCard != null)
			{
				PlayerInfo playerInfo = backMonsterCard.Merge(
					Model.Player, playerPosition, inbackofPlayer
				);
				Model.Fields[inbackofPlayer.index] = new EmptyModel();

				var mergingInfo = new MergingInfo() {
					sourcePosition = inbackofPlayer,
					targetPosition = playerPosition,
					playerInfo = playerInfo,
					mergingPosition = inbackofPlayer
				};
				View.MergeField(mergingInfo);
				OnMerged.Invoke(mergingInfo);
				Move(inbackofPlayer.index, colOffset, rowOffset);
				isMerged = true;
			}

			return isMerged;
		}

		public void Move(int targetIndex, int colFrom, int rowFrom)
		{
			var inbackofTarget = new Position(targetIndex, -colFrom, -rowFrom);
			Debug.Log($"Move to : {targetIndex}, {colFrom}, {rowFrom}");
			Debug.Log($"inbackofPosition : {inbackofTarget.row}, {inbackofTarget.col}");
			if (inbackofTarget.IsAcceptableIndex())
			{
				ICardModel backCard = GetCard(inbackofTarget.index);
				if (backCard != null)
				{
					Model.Fields[targetIndex] = backCard;
					Model.Fields[inbackofTarget.index] = new EmptyModel();
					View.MoveField(new Position(targetIndex), inbackofTarget);
					Move(inbackofTarget.index, colFrom, rowFrom);
				}
			}
		}

		void PrepareADeck(List<ICardModel> deckList)
		{
			Model.DeckQueue = new Queue<ICardModel>(deckList.Shuffle());
			deckList.Clear();

			int count = 0;
			Model.DeckQueue.ForEach(card => 
				Debug.Log(card.Data.type + ", " + card.Data.value + ", " + count++)
			);
		}

		void SetField()
		{
			Enumerable.Range(0, Model.CountOfFields).ForEach(index => 
				Model.Fields.Add(index, new EmptyModel())
			);
			View.SetField(Model.CountOfFields);
		}

		void MakePlayer()
		{
			AddField(1, Model.Player);
		}

		public void AddField(int key, ICardModel card)
		{
			Model.Fields[key] = card;
			View.AddField(
				new Position(key),
				card.Data,
				new Position(GetIndex(Model.Player))
			);
		}

		public ICardModel GetCard(int index)
		{
			if (index >= 0 && index < Model.Fields.Count)
			{
				return Model.Fields[index];
			}
			else
			{
				return null;
			}
		}

		public int GetIndex(ICardModel card)
		{
			return Model.Fields.FirstOrDefault(field => field.Value == card).Key;
		}
	}
}
