
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IFieldController<M, V>: IBaseController<M, V>
	{
		void Move(int pivotIndex, int colOffset, int rowOffset);
		void Merge(int colOffset, int rowOffset);
		int GetIndex(ICardModel card);
		ICardModel GetCard(int index);
		void AddField(int key, ICardModel card);
		Action<MergingInfo> OnMerged { get; set; }
		Action OnMoved { get; set; }
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
	
	public class FieldController<M, V>: BaseController<M, V>, IFieldController<M, V>
		where M: FieldModel, new()
		where V: FieldView 
	{
		public Action<MergingInfo> OnMerged { get; set; }
		public Action OnMoved { get; set; }
		
		public override void Init(V view) 
		{
			base.Init(view);

			Position.Cols = Model.Cols;
			Position.Rows = Model.Rows;

			PrepareADeck(Model.CardsData);
			SetField();
			MakePlayer();
		}

		public void Merge(int colOffset, int rowOffset)
		{
			var playerIndex = GetIndex(Model.Player);
			var playerPosition = new Position(playerIndex);
			var infrontofPlayer = new Position(playerIndex, colOffset, rowOffset);
			var inbackofPlayer = new Position(playerIndex, -colOffset, -rowOffset);
			var frontCard = GetCard(infrontofPlayer.index);

			var backMonsterCard = GetCard(inbackofPlayer.index) as IMerger;

			if (infrontofPlayer.IsAcceptableIndex() && frontCard != null && !Model.Player.CantMerge(frontCard))
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
			}
			else if (inbackofPlayer.IsAcceptableIndex() && backMonsterCard != null)
			{
				PlayerInfo playerInfo = backMonsterCard.Merge(Model.Player, playerPosition, inbackofPlayer);
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
					Model.Fields[pivotIndex] = backCard;
					Model.Fields[inbackofPivot.index] = new EmptyModel();
					View.MoveField(new Position(pivotIndex), inbackofPivot);
					Move(inbackofPivot.index, colOffsetFrom, rowOffsetFrom);
				}
			}
			else
			{
				OnMoved.Invoke();
			}
		}

		void PrepareADeck(List<ICardModel> deckList)
		{
			Model.DeckQueue = new Queue<ICardModel>(deckList.Shuffle());
			deckList.Clear();

			int count = 0;
			Model.DeckQueue.ForEach(card => Debug.Log(card.Data.type + ", " + card.Data.value + ", " + count++));
		}

		void SetField()
		{
			Enumerable.Range(0, Model.CountOfFields).ForEach(index => Model.Fields.Add(index, new EmptyModel()));
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
