using UnityEngine;
using Scripts.Game;
using System.Collections;
using Scripts.Util;
using Scripts.View;

namespace Scripts {
	public class Root: MonoBehaviour {
		readonly GameMain gameMain;
		[SerializeField]
		GameView gameView;
		Swipe swipe;

		Root() {
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
			gameMain = new GameMain();
			Debug.Log("hoi" + swipe);
		}

		// Use this for initialization
		void Start () {
			ResourceCache.Load("");
			PrepareAView();
			PrepareAGame();
			StartCoroutine("StartTheGame");
			swipe = gameObject.AddComponent<Swipe>();
			swipe.swipeEvent.AddListener((SwipeInfo swipeInfo) => {
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

		void PrepareAGame() {
			gameMain.fieldAddingEvent.AddListener((FieldAddingInfo info) => {
				Debug.Log($"fieldAdding: {info.xIndex}, {info.cardData.type}, {info.cardData.value}, {info.cardData.resourceName}, {info.cardData.cardName}");
				gameView.MakeField(
					info.xIndex, 
					info.cardData.type, 
					info.cardData.value, 
					info.cardData.resourceName, 
					info.cardData.cardName
				);
			});
			gameMain.fieldMergingEvent.AddListener((int xOffset) => {
				Debug.Log($"fieldRemoving: {xOffset}");
				gameView.MergeField(xOffset);
			});
			gameMain.Prepare();
		}

		void PrepareAView() {
			gameView.Prepare();
		}

		IEnumerator StartTheGame() {
			while (true) {
				if (!gameView.IsPlayer()) {
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