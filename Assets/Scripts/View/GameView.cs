using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;

namespace com.Gemfile.Merger 
{
	public class GameView : MonoBehaviour 
	{
		string[] deckNames;
		Dictionary<int, GameObject> fields;
		GameObject player;
		bool isPlaying;

		Bounds sizeOfCard;

		GameObject field;

		public void Prepare() 
		{
			deckNames = new string[] { "Deck" };
			isPlaying = false;
			fields = new Dictionary<int, GameObject>();
			field = new GameObject();
			field.transform.SetParent(transform);
			field.name = "Field";
		}

		public void PrepareField(int countOfFields)
		{
			Enumerable.Range(0, countOfFields).ForEach(index => fields.Add(index, null));
		}

		public void MergeField(Position position, PlayerData playerData) 
		{
			StartCoroutine(StartMerging(position, playerData));
		}

		IEnumerator StartMerging(Position position, PlayerData playerData) 
		{
			var targetCard = fields[position.index];
			Debug.Log("StartMerging : " + position.index + ", " + targetCard.name);
			var character = player.transform.GetChild(1);
			var characterAnimator = character.GetChild(0).GetComponent<Animator>();

			isPlaying = true;
			yield return StartCoroutine(StartTakingCapture(player, targetCard));
			yield return StartCoroutine(Move(player, characterAnimator, targetCard.transform));
			yield return StartCoroutine(Merge(targetCard, characterAnimator, playerData));
//			yield return StartCoroutine(End(targetCard));
			isPlaying = false;
		}

		IEnumerator Move(GameObject character, Animator characterAnimator, Transform targetCard) 
		{
			SetVisibleOfValues(character, false);
			characterAnimator.SetTrigger("walk");
			character.transform.DOLocalMove(
				new Vector3(targetCard.localPosition.x, targetCard.localPosition.y, character.transform.localPosition.z), 0.4f
			).SetEase(Ease.OutSine);
			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator Merge(GameObject targetCard, Animator characterAnimator, PlayerData playerData)
		{
			switch(targetCard.name) {
				case "Monster": characterAnimator.SetTrigger("attack"); break;
				default: characterAnimator.SetTrigger("jesture"); break;
			}

			yield return new WaitForSeconds(0.4f);

			SetVisibleOfValues(player, true);
			SetValues(player, playerData.hp.ToString());
			targetCard.SetActive(false);
			Destroy(targetCard);

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator End(GameObject targetCard) 
		{
//			CopyValues(player, targetCard);
//			player.gameObject.SetActive(false);
//			Destroy(player);

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

		void SetValues(GameObject target, string value)
		{
			target.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().text = value;
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

			var deck = ResourceCache.Instantiate(deckNames[Random.Range(0, 1)], transform);
			deck.transform.SetParent(card.transform);
			sizeOfCard = card.GetBounds();
			deck.transform.Find("Name").GetComponent<TextMesh>().text = cardData.cardName;
			deck.transform.Find("Value").GetComponent<TextMesh>().text = cardData.value.ToString();

			var cardResource = ResourceCache.Instantiate(cardData.resourceName, transform);
			cardResource.transform.SetParent(card.transform);

			card.transform.localPosition = new Vector2((sizeOfCard.size.x + 0.1f) * position.col, (sizeOfCard.size.y + 0.1f) * position.row);
			fields[position.index] = card;

			if (cardData.type == "Player")
			{
				player = card;
			}

			Align();
		}

		void Align()
		{
			var sizeOfField = field.GetBounds();
			field.transform.localPosition = new Vector3(
				(sizeOfCard.size.x - sizeOfField.size.x) / 2, 
				(sizeOfCard.size.y - sizeOfField.size.y) / 2, 
				field.transform.localPosition.z
			);
		}

		IEnumerator StartTakingCapture(GameObject player, GameObject targetCard) 
		{
			yield return new WaitForEndOfFrame();

			Bounds bounds = targetCard.GetBounds();
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
			capturedCard.name = targetCard.name;
			capturedCard.transform.SetParent(player.transform);
			capturedCard.transform.localPosition = new Vector3(0, 0, 0);
		}
	}

}