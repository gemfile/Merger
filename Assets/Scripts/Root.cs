using UnityEngine;
using System.Collections;

namespace com.Gemfile.Merger 
{
	public class Root: MonoBehaviour 
	{
		readonly GameMain gameMain;
		[SerializeField]
		GameView gameView;
		[SerializeField]
		GameUI gameUi;
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
			gameMain.fieldAddingCompleteEvent.AddListener(() => {
				if (initOnce) {
					initOnce = false;
					gameView.Init();
				}
				gameView.SetField();
			});
			gameMain.fieldMergingEvent.AddListener(mergingInfo => {
				Debug.Log($"fieldMerging: {mergingInfo.sourcePosition.row}, {mergingInfo.sourcePosition.col} -> {mergingInfo.targetPosition.row}, {mergingInfo.targetPosition.col}");
				gameView.MergeField(mergingInfo);
			});
			gameMain.fieldMovingEvent.AddListener((targetPosition, cardPosition) => {
				Debug.Log($"fieldMoving: {cardPosition.row}, {cardPosition.col} to {targetPosition.row}, {targetPosition.col}");
				gameView.MoveField(targetPosition, cardPosition);
			});
			gameMain.Prepare();
		}

		void PrepareAView()
		{
			gameView.Prepare();
			gameView.spriteCapturedEvent.AddListener((sprite, merged, equipments) => {
				gameUi.AddCardAcquired(sprite, merged, equipments);
			});
			gameUi.Prepare();
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