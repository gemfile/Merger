using UnityEngine;
using Scripts.Game;
using System.Collections;

namespace Scripts {
	public class Root: MonoBehaviour {
		readonly GameMain gameMain;

		Root() {
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
			gameMain = new GameMain();
			PrepareAGame();
		}

		void PrepareAGame() {
			gameMain.fieldEvent.AddListener((string type, int value) => {
				Debug.Log($"hi, {type}, {value}");
			});
		}

		// Use this for initialization
		void Start () {
			StartCoroutine("StartTheGame");
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