using System.Collections.Generic;
using UnityEngine;
using Scripts.Util;
using System.Collections;
using DG.Tweening;
using Scripts.Game;

namespace Scripts.View 
{
	public class GameView : MonoBehaviour 
	{
		string[] deckNames;
		List<GameObject> fields;
		GameObject player;
		bool isPlaying;

		Bounds sizeOfCard;

		GameObject field;

		public void Prepare() 
		{
			deckNames = new string[] { "BlueDeck", "WhiteDeck" };
			fields = new List<GameObject>();
			isPlaying = false;
			field = new GameObject();
			field.transform.SetParent(transform);
			field.name = "Field";
		}

		public void MergeField(Position position, PlayerData playerData) 
		{
			StartCoroutine(StartMerging(position, playerData));
		}

		IEnumerator StartMerging(Position position, PlayerData playerData) 
		{
			int index = fields.IndexOf(player) + position.index;
			var field = fields[index];
			var character = player.transform.GetChild(1);
			var characterAnimator = character.GetChild(0).GetComponent<Animator>();

			isPlaying = true;
			yield return StartCoroutine(StartTakingCapture(player, field));
			yield return StartCoroutine(Move(player, characterAnimator, field.transform));
			yield return StartCoroutine(Merge(field, characterAnimator));
			yield return StartCoroutine(End(field));
			isPlaying = false;
		}

		IEnumerator Move(GameObject character, Animator characterAnimator, Transform field) 
		{
			SetVisibleOfValues(character, false);
			characterAnimator.SetTrigger("walk");
			character.transform.DOLocalMoveX(field.localPosition.x, 1.5f).SetEase(Ease.Linear);
			yield return new WaitForSeconds(1.5f);
		}

		IEnumerator Merge(GameObject field, Animator characterAnimator) 
		{
			switch(field.name) {
				case "Monster": characterAnimator.SetTrigger("attack"); break;
				default: characterAnimator.SetTrigger("jesture"); break;
			}

//			DOTween.Sequence()
//				.SetDelay(0.4f)
//				.Append(field.transform.DOScale(new Vector3(0.88f, 0.88f, 1), 0.05f).SetEase(Ease.OutExpo))
//				.Append(field.transform.DOScale(new Vector3(1, 1, 1), 0.35f).SetEase(Ease.InElastic));

			yield return new WaitForSeconds(0.5f);

			SetVisibleOfValues(player, true);
			field.SetActive(false);
			Destroy(field);

			yield return new WaitForSeconds(0.5f);
		}

		IEnumerator End(GameObject field) 
		{
//			CopyValues(player, field);
//			player.gameObject.SetActive(false);
//			Destroy(player);
			player = field;

			yield return null;
		}

		void CopyValues(GameObject source, GameObject target) 
		{
			target.name = source.name;

			target.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().text = 
				source.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().text;	
			
			target.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().text = 
				source.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().text;
		}

		void SetVisibleOfValues(GameObject target, bool visible) 
		{
			target.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().gameObject.SetActive(visible);
			target.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().gameObject.SetActive(visible);
		}

		public bool IsPlaying() 
		{
			return isPlaying;
		}

		public void MakeField(Position position, CardData cardData) 
		{
			var card = new GameObject();
			card.transform.SetParent(field.transform);
			card.name = cardData.type;

			var deck = ResourceCache.Instantiate(deckNames[Random.Range(0, 2)], transform);
			deck.transform.SetParent(card.transform);
			sizeOfCard = card.GetBounds();
			deck.transform.Find("Name").GetComponent<TextMesh>().text = cardData.cardName;
			deck.transform.Find("Value").GetComponent<TextMesh>().text = cardData.value.ToString();

			var cardResource = ResourceCache.Instantiate(cardData.resourceName, transform);
			cardResource.transform.SetParent(card.transform);

			card.transform.localPosition = new Vector2((sizeOfCard.size.x + 0.1f) * position.col, (sizeOfCard.size.y + 0.1f) * position.row);
			fields.Add(card);

			if (cardData.type == "Player")
			{
				player = card;
			}

			Align();
		}

		void Align() 
		{
//			var sizeOfField = field.GetBounds();
//			field.transform.localPosition = new Vector3(
//				(sizeOfCard.size.x - sizeOfField.size.x) / 2, 
//				(sizeOfCard.size.y - sizeOfField.size.y) / 2, 
//				field.transform.localPosition.z
//			);
		}

		IEnumerator StartTakingCapture(GameObject player, GameObject field) 
		{
			yield return new WaitForEndOfFrame();

			Bounds bounds = field.GetBounds();
			Vector3 center = Camera.main.WorldToScreenPoint(bounds.center);
			Vector3 leftBottom = Camera.main.WorldToScreenPoint(bounds.min);
			Vector3 rightTop = Camera.main.WorldToScreenPoint(bounds.max);
			Vector3 size = rightTop - leftBottom;
			var captured = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);

			Debug.Log($"Capturing: {center.x}, {center.y}, {rightTop.x}, {rightTop.y}, {leftBottom.x}, {leftBottom.y}, {size.x}, {size.y}");
			captured.ReadPixels(new Rect(
				leftBottom.x,
				leftBottom.y,
				size.x,
				size.y
			), 0, 0);
			captured.Apply();

			Sprite capturedSprite = Sprite.Create(
				captured,
				new Rect(0, 0, (int)size.x, (int)size.y),
				new Vector2(0.5f,0.5f),
				Screen.height / Camera.main.orthographicSize / 2
			);

			var capturedCard = new GameObject();
			var spriteRenderer = capturedCard.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = capturedSprite;
			spriteRenderer.sortingOrder = -player.transform.childCount;
			capturedCard.name = field.name;
			capturedCard.transform.SetParent(player.transform);
		}
	}

}