using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IGameController<M, V>: IBaseController<M, V>
	{
		bool IsGameOver();
		IFieldController<FieldModel, IFieldView> Field { get; }
	}

	public enum PhaseOfGame
	{
		FILL = 0, PLAY, WAIT
	}

	public class GameController<M, V>: BaseController<M, V>, IGameController<M, V>
		where M: IGameModel, new()
		where V: IGameView 
	{
		public IFieldController<FieldModel, IFieldView> Field {
			get { return field; }
		}
		IFieldController<FieldModel, IFieldView> field;
		Queue<Func<bool>> commands;
		
		public GameController()
		{
			commands = new Queue<Func<bool>>();
		}
		
		public override void Init(V view)
		{
			base.Init(view);

			field = new FieldController<FieldModel, IFieldView>();
			field.Init(view.Field, 3, 3);
			
			view.UI.Align(view.Field.BackgroundBounds);
			
			ListenToInput();
			ListenToView();
			ListenToLogic();
			View.RequestCoroutine(BeginTheGame());
		}

		void Reset()
		{
			View.RequestCoroutine(StartReset());
		}

		IEnumerator StartReset()
		{
			Clear();
			yield return null;
			View.Reset();
			yield return new WaitForSeconds(0.4f);
			Init(View);
		}

		public override void Clear()
		{
			View.Swipe.OnSwipeEnd.RemoveAllListeners();
			View.Swipe.OnSwipeMove.RemoveAllListeners();
			View.Swipe.OnSwipeCancel.RemoveAllListeners();
			View.Orientation.OnOrientationChange.RemoveAllListeners();
			View.Field.OnSpriteCaptured.RemoveAllListeners();
			Field.OnMerged.RemoveAllListeners();
		}

		IEnumerator BeginTheGame()
		{
			while (true)
			{
				Watch();
				
				if (!View.Field.IsPlaying && IsGameOver()) {
					View.UI.SuggestRetry(() => {
						Reset();
					});
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
							SetNavigation();
						}
					}
					break;

				case PhaseOfGame.WAIT:
					if (!View.Field.IsPlaying) {
						SetNavigation();
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

		void SetNavigation()
		{
			View.Field.HighlightCards(
				View.Navigation.Set(
					field.GetWheresCanMerge(), 
					View.Field.Fields, 
					View.Field.CardBounds
				)
			);
		}

		void ListenToInput()
		{
			View.Swipe.OnSwipeEnd.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					View.Navigation.Clear();
					View.Field.Dehighlight();
					var direction = swipeInfo.direction;
					commands.Enqueue(() => field.Merge((int)direction.x, (int)direction.y));
				}
			});

			View.Swipe.OnSwipeMove.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					View.Navigation.Show(swipeInfo.directionFirst, swipeInfo.touchDelta);
				}
			});

			View.Swipe.OnSwipeCancel.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
					View.Navigation.Hide();
				}
			});

			View.Orientation.OnOrientationChange.AddListener(() => {
				View.RequestCoroutine(StartChangeOrientation());
			});
		}

		IEnumerator StartChangeOrientation()
		{
			yield return null;
			View.ChangeOrientation();
			yield return null;
			View.UI.Align(View.Field.BackgroundBounds);
			View.Field.RetakeCapture(Field.GetPlayerInfo());
			SetNavigation();
		}

		void ListenToView()
		{
			View.Field.OnSpriteCaptured.AddListener(View.UI.AddCardAcquired);
		}

		void ListenToLogic()
		{
			Field.OnMerged.AddListener(mergingInfo => 
				view.UI.UpdateCoin(mergingInfo.mergerInfo.coin)
			);
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
