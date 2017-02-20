using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IGameController<M, V>: IBaseController<M, V>
	{
		bool IsGameOver();
		IFieldController<FieldModel, FieldView> Field { get; }
	}

	public class GameController<M, V>: BaseController<M, V>, IGameController<M, V>
		where M: GameModel, new()
		where V: GameView 
	{
		public IFieldController<FieldModel, FieldView> Field {
			get { return field; }
		}
		IFieldController<FieldModel, FieldView> field;
		
		public override void Init(V view)
		{
			base.Init(view);

			field = new FieldController<FieldModel, FieldView>();
			field.Init(view.GetComponentInChildren<FieldView>());
			
			view.UI.Align(view.Field.BackgroundBounds);
			
			ListenToInput();
			ListenToView();
			ListenToLogic();
			View.RequestCoroutine(BeginTheGame());
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
			return field.Model.Player.Hp <= 0;
		}

		void Watch()
		{
			var currentPhase = GetCurrentPhase();
			var fieldModel = field.Model;
			if (!View.Field.IsPlaying && currentPhase == PhaseOfGame.PLAY)
			{
				int playerIndex = field.GetIndex(fieldModel.Player);

				var wheresWannaMerge = new List<int[]> {
					new int[2]{ -1, 0 },
					new int[2]{ 1, 0 },
					new int[2]{ 0, -1 },
					new int[2]{ 0, 1 },
				};

				var wheresCanMerge = new List<Position>();
				wheresWannaMerge.ForEach(whereWannaMerge => {
					var nearbyPosition = new Position(playerIndex, whereWannaMerge[0], whereWannaMerge[1]);
					if (nearbyPosition.IsAcceptableIndex())
					{
						ICardModel nearbyCard = field.GetCard(nearbyPosition.index);
						if (nearbyCard != null && !fieldModel.Player.CantMerge(nearbyCard))
						{
							wheresCanMerge.Add(nearbyPosition);
						}
					}
				});

				View.Navigation.Show(playerIndex, wheresCanMerge, View.Field.Fields);
				SetNextPhase();
			}

			if (currentPhase == PhaseOfGame.WAIT)
			{

			}
			
			// Fill the fields.
			if (currentPhase == PhaseOfGame.FILL && fieldModel.Fields.Values.Any(field => field is EmptyModel))
			{
				var emptyFields = fieldModel.Fields.Where(field => field.Value is EmptyModel).ToDictionary(p => p.Key, p => p.Value);
				emptyFields.ForEach(emptyField => {
					field.AddField(emptyField.Key, fieldModel.DeckQueue.Dequeue());
				});

				Debug.Log("=== Choose the card ===");
				int count = 0;
				fieldModel.Fields.ForEach(iCardModel => Debug.Log(iCardModel.Value.Data.type + ", " + iCardModel.Value.Data.value + ", " + count++));
				Debug.Log("===============");

				View.Field.ShowField();
				View.UI.UpdateDeckCount(fieldModel.DeckQueue.Count);
				SetNextPhase();
			}
		}

		void ListenToInput()
		{
			View.Swipe.OnSwipe.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					switch(swipeInfo.direction) {
						case Direction.Right: field.Merge(1, 0); break;
						case Direction.Left: field.Merge(-1, 0); break;
						case Direction.Up: field.Merge(0, 1); break;
						case Direction.Down: field.Merge(0, -1); break;
					}
				}
			});
		}
		
		void ListenToView()
		{
			View.Field.OnSpriteCaptured += View.UI.AddCardAcquired;
		}

		void ListenToLogic()
		{
			Field.OnMerged += (MergingInfo mergingInfo) => {
				view.UI.UpdateCoin(mergingInfo.playerInfo.coin);
			};
			Field.OnMoved += () => {
				SetNextPhase();
			};
		}

		void SetNextPhase()
		{
			Model.CurrentIndexOfPhase++;
			if (Model.CurrentIndexOfPhase >= Model.PhasesOfGame.Count)
			{
				Model.CurrentIndexOfPhase = 0;
			}
			Debug.Log($"What's next? {GetCurrentPhase()}");
		}

		PhaseOfGame GetCurrentPhase()
		{
			return Model.PhasesOfGame[Model.CurrentIndexOfPhase];
		}

		PhaseOfGame GetPreviousPhase()
		{
			var previousIndexOfPhase = Model.CurrentIndexOfPhase - 1;
			if (previousIndexOfPhase < 0)
			{
				previousIndexOfPhase = Model.PhasesOfGame.Count - 1;
			}

			return Model.PhasesOfGame[previousIndexOfPhase];
		}
	}
}
