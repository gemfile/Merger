using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;

namespace com.Gemfile.Merger
{
	class FieldNewAdded
	{
		internal GameObject card;
		internal Vector2 createdFrom;
	}

    internal class GameView : MonoBehaviour 
	{
		string[] deckNames;
		Dictionary<int, GameObject> fields;
		List<FieldNewAdded> fieldsNewAdded;
		GameObject player;
		bool isPlaying;

		Bounds sizeOfCard;

		GameObject field;

		internal void Prepare() 
		{
			deckNames = new string[] { "Deck" };
			isPlaying = false;
			fields = new Dictionary<int, GameObject>();
			fieldsNewAdded = new List<FieldNewAdded>();
			field = new GameObject();
			field.transform.SetParent(transform);
			field.name = "Field";
		}

		internal void Init()
		{
			Align();
		}

		internal void PrepareField(int countOfFields)
		{
			Enumerable.Range(0, countOfFields).ForEach(index => fields.Add(index, null));
		}

		internal void MergeField(Position position, PlayerData playerData) 
		{
			StartCoroutine(StartMerging(position, playerData));
		}

		internal void MoveField(Position targetPosition, Position cardPosition)
		{
			StartCoroutine(StartMoving(targetPosition, cardPosition));
		}

        internal void SetField()
        {
			StartCoroutine(StartSetting());
        }

		IEnumerator StartSetting()
		{
			yield return null;
			Hide();
			ShowCards();
		}

		void Hide()
		{
			fieldsNewAdded.ForEach((FieldNewAdded) => {
				SetVisibleOfValues(FieldNewAdded.card, false);
				SetVisibleOfResource(FieldNewAdded.card, false);
			});
		}

		IEnumerator StartMoving(Position targetPosition, Position cardPosition)
		{
			var movingCard = fields[cardPosition.index];
			var targetLocalPosition = new Vector2((sizeOfCard.size.x + 0.1f) * targetPosition.col, (sizeOfCard.size.y + 0.1f) * targetPosition.row);
			Debug.Log("StartMoving : " + cardPosition.index + ", " + movingCard.name);

			movingCard.transform.DOLocalMove(
				new Vector2(targetLocalPosition.x, targetLocalPosition.y), 0.4f
			).SetEase(Ease.OutSine);
			fields[targetPosition.index] = movingCard;

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator StartMerging(Position targetPosition, PlayerData playerData) 
		{
			var mergingCard = fields[targetPosition.index];
			Debug.Log("StartMerging : " + targetPosition.index + ", " + mergingCard.name);
			var character = player.transform.GetChild(1);
			var characterAnimator = character.GetChild(0).GetComponent<Animator>();

			yield return StartCoroutine(TakeCapture(player, mergingCard));
			yield return StartCoroutine(MovePlayer(player, characterAnimator, mergingCard.transform));
			yield return StartCoroutine(MergeCard(mergingCard, characterAnimator, playerData));
			yield return StartCoroutine(EndMerging(targetPosition, player));
		}

        IEnumerator MovePlayer(GameObject player, Animator characterAnimator, Transform targetCard) 
		{
			SetVisibleOfValues(player, false);
			characterAnimator.SetTrigger("walk");
			player.transform.DOLocalMove(
				new Vector2(targetCard.localPosition.x, targetCard.localPosition.y), 0.4f
			).SetEase(Ease.OutSine);
			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator MergeCard(GameObject targetCard, Animator characterAnimator, PlayerData playerData)
		{
			switch(targetCard.name) 
			{
				case "Monster": characterAnimator.SetTrigger("attack"); break;
				default: characterAnimator.SetTrigger("jesture"); break;
			}

			yield return new WaitForSeconds(0.4f);

			SetVisibleOfValues(player, true);
			SetValue(player, "Value", playerData.hp.ToString());
			targetCard.SetActive(false);
			Destroy(targetCard);

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator EndMerging(Position targetPosition, GameObject player) 
		{
			fields[targetPosition.index] = player;
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

		void SetValue(GameObject target, string key, string value)
		{
			target.transform.GetChild(0).Find(key).GetComponent<TextMesh>().text = value;
		}

		void SetVisibleOfValues(GameObject target, bool visible) 
		{
			target.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().gameObject.SetActive(visible);
			target.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().gameObject.SetActive(visible);
		}

		void SetVisibleOfResource(GameObject target, bool visible)
		{
			target.transform.GetChild(1).gameObject.SetActive(visible);
		}

		internal bool IsPlaying()
		{
			return isPlaying;
		}

		internal void MakeField(Position position, CardData cardData, Position playerPosition) 
		{
			var card = new GameObject();
			card.transform.SetParent(field.transform);
			card.name = cardData.type;

			var deck = ResourceCache.Instantiate(deckNames[UnityEngine.Random.Range(0, 1)], transform);
			deck.transform.SetParent(card.transform);
			sizeOfCard = card.GetBounds();
			SetValue(card, "Name", cardData.cardName);
			SetValue(card, "Value", cardData.value.ToString());

			var cardResource = ResourceCache.Instantiate(cardData.resourceName, transform);
			cardResource.transform.SetParent(card.transform);
			
			card.transform.localPosition = new Vector2((sizeOfCard.size.x + 0.1f) * position.col, (sizeOfCard.size.y + 0.1f) * position.row);
			fields[position.index] = card;
			fieldsNewAdded.Add(new FieldNewAdded() { 
				card = card, 
				createdFrom = new Vector2(position.col - playerPosition.col, position.row - playerPosition.row).normalized
			});

			if (cardData.type == "Player")
			{
				player = card;
			}
		}

		void ShowCards()
		{
			fieldsNewAdded.ForEach(fieldNewAdded => {
				var card = fieldNewAdded.card;
				var createdFrom = fieldNewAdded.createdFrom;
				Vector3 localPosition = card.transform.localPosition;
				Vector3 creatingOffset = createdFrom * 0.28f;
				Debug.Log("hoi : " + creatingOffset);
			
				var duration = .8f;
				var sequence = DOTween.Sequence();
				sequence.Append(card.transform.DOLocalMove(localPosition + creatingOffset, duration).From().SetEase(Ease.OutBack));
				sequence.Append(card.transform.DOScaleX(-1, duration).From().SetEase(Ease.InOutCubic));
				sequence.Insert(
					duration, 
					card.transform.DOLocalMoveY(localPosition.y+.07f, duration/2)
						.SetLoops(2, LoopType.Yoyo)
						.SetEase(Ease.InOutCubic)
						.OnStepComplete(()=>{
							SetVisibleOfValues(card, true);
							SetVisibleOfResource(card, true);
						})
				);
			});
			fieldsNewAdded.Clear();
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

		IEnumerator TakeCapture(GameObject player, GameObject targetCard)
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