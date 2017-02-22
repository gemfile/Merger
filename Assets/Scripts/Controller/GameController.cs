using System;
using System.Collections;
using System.Collections.Generic;
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
		Queue<Func<bool>> commands;
		
		public GameController()
		{
			commands = new Queue<Func<bool>>();
		}
		
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

		public override void Clear()
		{
			View.Swipe.OnSwipeEnd.RemoveAllListeners();
			View.Swipe.OnSwipeMove.RemoveAllListeners();
			View.Swipe.OnSwipeCancel.RemoveAllListeners();
			View.Field.OnSpriteCaptured.RemoveAllListeners();
			Field.OnMerged.RemoveAllListeners();
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
			switch(GetCurrentPhase())
			{
				case PhaseOfGame.PLAY:
					if (commands.Count > 0) {
						Func<bool> command = commands.Dequeue();
						var succeeded = command.Invoke();
						if (succeeded) {
							SetNextPhase();
						} else {
							View.Field.HighlightCards(
								View.Navigation.Set(field.GetWheresCanMerge(), View.Field.Fields, View.Field.CardBounds)
							);
						}
					}
					break;

				case PhaseOfGame.WAIT:
					if (!View.Field.IsPlaying) {
						View.Field.HighlightCards(
							View.Navigation.Set(field.GetWheresCanMerge(), View.Field.Fields, View.Field.CardBounds)
						);
						SetNextPhase();
					}
					break;

				case PhaseOfGame.FILL:
					if (field.IsThereEmptyModel()) {
						field.FillEmptyFields();
						View.UI.UpdateDeckCount(field.Model.DeckQueue.Count);
						SetNextPhase();
					}
					break;
			}
		}

		void ListenToInput()
		{
			View.Swipe.OnSwipeEnd.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					View.Navigation.Clear();
					View.Field.Dehighlight();
					switch(swipeInfo.direction) {
						case Direction.Right: 
							commands.Enqueue(() => field.Merge(1, 0));
							break;
						case Direction.Left: 
							commands.Enqueue(() => field.Merge(-1, 0));
						 	break;
						case Direction.Up: 
							commands.Enqueue(() => field.Merge(0, 1));
						 	break;
						case Direction.Down: 
							commands.Enqueue(() => field.Merge(0, -1));
						 	break;
					}
				}
			});

			View.Swipe.OnSwipeMove.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					View.Navigation.Show(swipeInfo.touchDeltaFirst, swipeInfo.touchDelta);
				}
			});

			View.Swipe.OnSwipeCancel.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					View.Navigation.Hide();
				}
			});
		}
		
		void ListenToView()
		{
			View.Field.OnSpriteCaptured.AddListener(View.UI.AddCardAcquired);
		}

		void ListenToLogic()
		{
			Field.OnMerged.AddListener(mergingInfo => {
				view.UI.UpdateCoin(mergingInfo.playerInfo.coin);
			});
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
	}
}
