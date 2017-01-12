using System.Collections.Generic;
using UnityEngine;
using Scripts.Util;
using System.Collections;
using DG.Tweening;

namespace Scripts.View {
	public class GameView : MonoBehaviour {
		string[] deckNames;
		List<GameObject> fields;
		GameObject player;
		bool isPlayer;
		Bounds fieldSize;

		public void Prepare() {
			deckNames = new string[] { "BlueDeck", "WhiteDeck" };
			fields = new List<GameObject>();
			isPlayer = false;
		}

		public void MergeField(int xOffset) {
			var mergingCoroutine = StartCoroutine(StartMerging(xOffset));
		}

		IEnumerator StartMerging(int xOffset) {
			int xIndex = fields.IndexOf(player) + xOffset;
			var field = fields[xIndex];
			var character = player.transform.GetChild(1);
			var characterAnimator = character.GetChild(0).GetComponent<Animator>();

			isPlayer = true;
			yield return StartCoroutine(Move(character.transform, characterAnimator, field.transform));
			yield return StartCoroutine(Merge(field, characterAnimator));
			yield return StartCoroutine(End(field));
			isPlayer = false;
		}

		IEnumerator Move(Transform character, Animator characterAnimator, Transform field) {
			characterAnimator.SetTrigger("walk");
			character.SetParent(field.transform);
			character.DOLocalMoveX(0, 2.0f).SetEase(Ease.Linear);
			yield return new WaitForSeconds(2.0f);
		}

		IEnumerator Merge(GameObject field, Animator characterAnimator) {
			switch(field.name) {
				case "Monster": characterAnimator.SetTrigger("attack"); break;
				default: characterAnimator.SetTrigger("jesture"); break;
			}

			yield return new WaitForSeconds(0.75f);
			var card = field.transform.GetChild(1).gameObject;
			card.SetActive(false);
			Destroy(card);
			yield return new WaitForSeconds(0.75f);
		}

		IEnumerator End(GameObject field) {
			CopyValues(player, field);
			player.gameObject.SetActive(false);
			Destroy(player);

			player = field;
			yield return null;
		}

		void CopyValues(GameObject source, GameObject target) {
			target.name = source.name;

			target.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().text = 
				source.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().text;	
			
			target.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().text = 
				source.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().text;
		}

		public bool IsPlayer() {
			return isPlayer;
		}

		public void MakeField(int xIndex, string type, int value, string resourceName, string cardName) {
			var field = new GameObject();
			field.transform.SetParent(transform);
			field.name = type;

			var deck = ResourceCache.Instantiate(deckNames[Random.Range(0, 2)], transform);
			deck.transform.SetParent(field.transform);
			deck.transform.Find("Name").GetComponent<TextMesh>().text = cardName;
			deck.transform.Find("Value").GetComponent<TextMesh>().text = value.ToString();
			fieldSize = field.GetBounds();

			var card = ResourceCache.Instantiate(resourceName, transform);
			card.transform.SetParent(field.transform);

			field.transform.localPosition = new Vector2((fieldSize.size.x + 0.1f) * xIndex, field.transform.localPosition.y);
			fields.Add(field);

			if (type == "Player") {
				player = field;
			}

			Align();
		}

		void Align() {
			//		Bounds bounds = gameObject.GetBounds();

			var playerIndex = fields.IndexOf(player);
			var endIndexOfFields = fields.Count - 1;
			int offset = (int)((endIndexOfFields - playerIndex) / 2);
			var centerField = fields[playerIndex+offset];

			var mainCamera = Camera.main;
			mainCamera.transform.DOLocalMoveX(centerField.transform.localPosition.x, .4f);


			//		var screenX = Camera.main.ScreenToWorldPoint(new Vector3(1080, 0, 0)).x;
			//		transform.DOMove(new Vector2(
			//			fieldSize.extents.x - bounds.size.x/2,
			//			transform.localPosition.y
			//		), 1);
		}
	}

}