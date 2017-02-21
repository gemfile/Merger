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
		Queue<Action> commands;
		
		public GameController()
		{
			commands = new Queue<Action>();
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
			Debug.Log("currentPhase : " + currentPhase);
			switch(currentPhase)
			{
				case PhaseOfGame.PLAY:
					if (commands.Count > 0) {
						Action command = commands.Dequeue();
						command.Invoke();
						SetNextPhase();
					}
					break;

				case PhaseOfGame.WAIT:
					if (!View.Field.IsPlaying) {
						View.Navigation.Show(field.GetWheresCanMerge(), View.Field.Fields);
						SetNextPhase();
					}
					break;

				case PhaseOfGame.FILL:
					if (field.IsThereEmptyModel()) {
						View.Navigation.Hide();
						field.FillEmptyFields();
						View.UI.UpdateDeckCount(field.Model.DeckQueue.Count);
						SetNextPhase();
					}
					break;
			}
		}

		void ListenToInput()
		{
			View.Swipe.OnSwipe.AddListener(swipeInfo => {
				if(!View.Field.IsPlaying) {
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
