using UnityEngine;
using Scripts.Game;
using System.Collections;
using Scripts.Util;
using Scripts.View;

namespace Scripts 
{
	public class Root: MonoBehaviour 
	{
		readonly GameMain gameMain;
		[SerializeField]
		GameView gameView;
		Swipe swipe;

		Root() 
		{
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
			gameMain = new GameMain();
			Debug.Log("hoi" + swipe);
		}

		// Use this for initialization
		void Start () 
		{
			ResourceCache.Load("");
			PrepareAView();
			PrepareAGame();
			StartCoroutine("StartTheGame");
			swipe = gameObject.AddComponent<Swipe>();
			swipe.swipeEvent.AddListener(swipeInfo => {
				if(swipeInfo.hasPass) {
					switch(swipeInfo.direction) {
						case Direction.Right: gameMain.Merge(1, 0); break;
						case Direction.Left: gameMain.Merge(-1, 0); break;
						case Direction.Up: gameMain.Merge(0, 1); break;
						case Direction.Down: gameMain.Merge(0, -1); break;
					}
				}
			});
		}

		void PrepareAGame() 
		{
			gameMain.fieldPreparingEvent.AddListener(gameView.PrepareField);
			gameMain.fieldAddingEvent.AddListener((position, cardData) => {
				Debug.Log($"fieldAdding: {position.row}, {position.col}, {cardData.type}, {cardData.value}, {cardData.resourceName}, {cardData.cardName}");
				gameView.MakeField(position, cardData);
			});
			gameMain.fieldMergingEvent.AddListener((position, playerData) => {
				Debug.Log($"fieldMerging: {position.row}, {position.col}");
				gameView.MergeField(position, playerData);
			});
			gameMain.Prepare();
		}

		void PrepareAView() 
		{
			gameView.Prepare();
		}

		IEnumerator StartTheGame() 
		{
			while (true) 
			{
				if (!gameView.IsPlaying()) {
					gameMain.FillTheField();
				}

				if (gameMain.IsOver) {
					Debug.Log("=== The game is over! ===");
					yield break;
				}

				yield return null;
			}
		}
	}
}