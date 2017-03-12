using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace com.Gemfile.Merger
{
	public interface IFieldController<M, V>: IBaseController<M, V>
	{
		void Init(V view, int cols, int rows);
		List<NavigationInfo> GetWheresCanMerge();
		void FillEmptyFields();
		void Move(int targetIndex, int colFrom, int rowFrom);
		bool Merge(int colOffset, int rowOffset);
		ICardModel GetCard(int index);
		void AddField(int key, ICardModel card);
		MergingEvent OnMerged { get; }
		bool IsThereEmptyModel();
		IMergingController Merging { get; }
		PlayerInfo GetPlayerInfo();
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
		public MergerInfo mergerInfo;
	}

	public class PlayerInfo
	{
		public Position playerPosition;
		public ICardModel playerModel;
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
		readonly MergingEvent onMerged;

		public IMergingController Merging { get { return merging; } }
		readonly IMergingController merging;
		Vector2 latestMergingDelta = new Vector2(0, -1);
        

		public FieldController()
		{
			onMerged = new MergingEvent();
			merging = new MergingController();
		}
		
		public void Init(V view, int cols = 3, int rows = 3) 
		{
			base.Init(view);

			Position.Cols = Model.Cols = cols;
			Position.Rows = Model.Rows = rows;

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
						if (nearbyCard != null && !merging.CantMerge(merger.Value as IMerger, nearbyCard))
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

		object ReflectPropertyValue(object source, string property)
		{
			return source.GetType().GetField(property).GetValue(source);
		}

		class ChainingInfo: IComparable<ChainingInfo>
		{
			public int orderBy;
			public int positionIndex;
			public Position sourcePosition;
			public Position targetPosition;
			public IMerger merger;

			public int CompareTo(ChainingInfo that) {
				if (orderBy < 0) {
					if (this.positionIndex < that.positionIndex) {
						return -1;
					}
					if (this.positionIndex > that.positionIndex) {
						return 1;
					}
				} else if (orderBy > 0) {
					if (this.positionIndex > that.positionIndex) {
						return -1;
					}
					if (this.positionIndex > that.positionIndex) {
						return 1;
					}
				}
				return 0;
			}
		}

		void SaveMergingDelta(Vector2 mergingDelta)
		{
			latestMergingDelta = mergingDelta;
		}
		
		public bool Merge(int colOffset, int rowOffset)
		{
			SaveMergingDelta(new Vector2(colOffset, rowOffset));
			
			var isMerged = false;
			var mergerChains = CreateMergerChains(colOffset, rowOffset);
			mergerChains.Values.ForEach(mergerChain => {
				var leadingMerger = mergerChain[0];

				var merger = leadingMerger.merger;
				var sourcePosition = leadingMerger.sourcePosition;
				var targetPosition = leadingMerger.targetPosition;
				var cardNearby = GetCard(targetPosition.index);
				Debug.Log(sourcePosition.index + " -> " + targetPosition.index);

				if (!merging.CantMerge(merger, cardNearby)) {
					MergerInfo mergerInfo = merging.Merge(merger, cardNearby, sourcePosition, targetPosition);
					Model.Fields[targetPosition.index] = mergerInfo.mergerModel;
					Model.Fields[sourcePosition.index] = new EmptyModel();
					
					var mergingInfo = new MergingInfo() {
						sourcePosition = sourcePosition,
						targetPosition = targetPosition,
						mergerInfo = mergerInfo,
					};
					View.MergeField(mergingInfo);
					OnMerged.Invoke(mergingInfo);
					Move(sourcePosition.index, colOffset, rowOffset);
					isMerged = true;
				}
			});

			return isMerged;
		}

		public void Move(int targetIndex, int colFrom, int rowFrom)
		{
			var inbackofTarget = new Position(targetIndex, -colFrom, -rowFrom);
			Debug.Log($"Move to : {targetIndex}, {colFrom}, {rowFrom}");
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

		Dictionary<int, List<ChainingInfo>> CreateMergerChains(int colOffset, int rowOffset)
		{
			var mergers = Model.Fields
				.Where(field => field.Value is IMerger)
				.ToDictionary(p => p.Key, p => p.Value);

			string chainingProperty = (colOffset != 0) ? "row" : (rowOffset != 0) ? "col" : "";
			string positionProperty = chainingProperty == "row" ? "col" : "row";
			int orderBy = (colOffset > 0 || rowOffset > 0) ? 1 : -1;

			var mergerChains = new Dictionary<int, List<ChainingInfo>>();
			mergers.ForEach((merger) => {
				var positionNearby = new Position(merger.Key, colOffset, rowOffset);
				if (positionNearby.IsAcceptableIndex() 
					&& !merging.CantMerge(merger.Value as IMerger, GetCard(positionNearby.index)))
				{
					Debug.Log(merger.Key + " => " + positionNearby.index);
					var chainingIndex = (int)ReflectPropertyValue(positionNearby, chainingProperty);
					var positionIndex =	(int)ReflectPropertyValue(positionNearby, positionProperty);
					List<ChainingInfo> mergerChain = null;
					if (!mergerChains.TryGetValue(chainingIndex, out mergerChain)) {
						mergerChain = mergerChains[chainingIndex] = new List<ChainingInfo>();
					}
					mergerChain.Add(new ChainingInfo() {
						positionIndex = positionIndex, 
						merger = merger.Value as IMerger, 
						sourcePosition = new Position(merger.Key),
						targetPosition = positionNearby,
						orderBy = orderBy,
					});
				}
			});

			mergerChains.ForEach(mergerChain => {
				mergerChain.Value.Sort();
			});

			return mergerChains;
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
				-latestMergingDelta
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

		public PlayerInfo GetPlayerInfo()
		{
			return new PlayerInfo {
				playerPosition = new Position(GetIndex(Model.Player)),
				playerModel = Model.Player
			};
		}

		int GetIndex(ICardModel card)
		{
			return Model.Fields.FirstOrDefault(field => field.Value == card).Key;
		}
	}
}
