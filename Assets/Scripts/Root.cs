using UnityEngine;
using System.Collections;

namespace com.Gemfile.Merger 
{
	public class Root: MonoBehaviour 
	{
		readonly GameMain gameMain;
		GameView gameView;
		GameUI gameUI;
		Swipe swipe;

		Root() 
		{
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
			gameMain = new GameMain();
		}

		// Use this for initialization
		void Start () 
		{
			ResourceCache.Load("");
			PrepareAView();
			PrepareAGame();
			StartCoroutine("StartTheGame");
			ListenToInput();
			AlignGameViewAtTop();
		}

		void AlignGameViewAtTop()
		{
			var sizeOfScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
			var sizeOfGameView = gameView.gameObject.GetBounds();
			var positionOfGameView = gameView.transform.localPosition;
			gameView.transform.localPosition = new Vector3(positionOfGameView.x, sizeOfScreen.y - sizeOfGameView.extents.y - sizeOfGameView.center.y, positionOfGameView.z);
		}

		void ListenToInput() 
		{
			swipe = gameObject.AddComponent<Swipe>();
			swipe.swipeEvent.AddListener(swipeInfo => {
				if(!gameView.IsPlaying()) {
					switch(swipeInfo.direction) {
						case Direction.Right: gameMain.Merge(1, 0); break;
						case Direction.Left: gameMain.Merge(-1, 0); break;
						case Direction.Up: gameMain.Merge(0, 1); break;
						case Direction.Down: gameMain.Merge(0, -1); break;
					}
				}
			});
		}

		bool initOnce = true;
		void PrepareAGame()
		{
			gameMain.fieldPreparingEvent.AddListener(gameView.PrepareField);
			gameMain.fieldAddingEvent.AddListener((position, cardData, playerPosition) => {
				Debug.Log($"fieldAdding: {position.row}, {position.col}, {cardData.type}, {cardData.value}, {cardData.resourceName}, {cardData.cardName}");
				gameView.MakeField(position, cardData, playerPosition);
			});
			gameMain.fieldAddingCompleteEvent.AddListener((deckCount) => {
				if (initOnce) {
					initOnce = false;
					gameView.Init();
				}
				gameView.SetField();
				gameUI.UpdateDeckCount(deckCount);
			});
			gameMain.fieldMergingEvent.AddListener(mergingInfo => {
				Debug.Log($"fieldMerging: {mergingInfo.sourcePosition.row}, {mergingInfo.sourcePosition.col} -> {mergingInfo.targetPosition.row}, {mergingInfo.targetPosition.col}");
				gameView.MergeField(mergingInfo);
				gameUI.UpdateCoin(mergingInfo.playerData.coin);
			});
			gameMain.fieldMovingEvent.AddListener((targetPosition, cardPosition) => {
				Debug.Log($"fieldMoving: {cardPosition.row}, {cardPosition.col} to {targetPosition.row}, {targetPosition.col}");
				gameView.MoveField(targetPosition, cardPosition);
			});
			gameMain.Prepare();
		}

		void PrepareAView()
		{
			gameView = transform.Find("GameView").GetComponent<GameView>();
			gameUI = transform.Find("GameUI").GetComponent<GameUI>();
			
			gameView.Prepare();
			gameView.spriteCapturedEvent.AddListener((sprite, size, merged, equipments) => {
				gameUI.AddCardAcquired(sprite, size, merged, equipments);
			});
			gameUI.Prepare();
		}

		IEnumerator StartTheGame() 
		{
			while (true) 
			{
				gameMain.Update();

				if (gameMain.IsGameOver) {
					Debug.Log("=== The game is over! ===");
					yield break;
				}

				yield return null;
			}
		}
	}
}