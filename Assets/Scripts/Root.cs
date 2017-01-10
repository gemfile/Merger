using UnityEngine;
using Scripts.Game;
using System.Collections;
using Scripts.Util;

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
			PrepareAGame();
			PrepareAView();
			StartCoroutine("StartTheGame");
		}

		void PrepareAGame() {
			gameMain.Prepare();
			gameMain.fieldEvent.AddListener((string type, int value, string resourceName, string cardName) => {
				Debug.Log($"hi, {type}, {value}");
				gameView.MakeField(type, value, resourceName, cardName);
			});
		}

		void PrepareAView() {
			gameView.Prepare();
		}

		IEnumerator StartTheGame() {
			while (true) {
				gameMain.FillTheField();

				if (Input.GetKeyDown(KeyCode.Alpha1)) {
					gameMain.Merge(0);
				}
				if (Input.GetKeyDown(KeyCode.Alpha2)) {
					gameMain.Merge(1);
				}
				if (Input.GetKeyDown(KeyCode.Alpha3)) {
					gameMain.Merge(2);
				}

				if (gameMain.IsOver) {
					Debug.Log("=== The game is over! ===");
					yield break;
				}

				yield return null;
			}
		}


		// Update is called once per frame
		void Update () {

		}
	}
}