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

		Root() {
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
			gameMain = new GameMain();
		}

		// Use this for initialization
		void Start () {
			ResourceCache.Load("");
			PrepareAView();
			PrepareAGame();
			StartCoroutine("StartTheGame");
		}

		void PrepareAGame() {
			gameMain.fieldAddingEvent.AddListener((string type, int value, string resourceName, string cardName) => {
				Debug.Log($"fieldAdding: {type}, {value}, {resourceName}, {cardName}");
				gameView.MakeField(type, value, resourceName, cardName);
			});
			gameMain.fieldMergingEvent.AddListener((int index) => {
				Debug.Log($"fieldRemoving: {index}");
				gameView.MergeField(index);
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

				if (Input.GetKeyDown(KeyCode.Alpha1)) {
					if (gameMain.IsIndexNearFromPlayer(0)) {
						gameMain.Merge(0);
					}
				}
				if (Input.GetKeyDown(KeyCode.Alpha2)) {
					if (gameMain.IsIndexNearFromPlayer(1)) {
						gameMain.Merge(1);
					}
				}
				if (Input.GetKeyDown(KeyCode.Alpha3)) {
					if (gameMain.IsIndexNearFromPlayer(2)) {
						gameMain.Merge(2);
					}
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