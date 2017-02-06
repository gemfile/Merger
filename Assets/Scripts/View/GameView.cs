using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;
using System;

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

		Bounds sizeOfCard;

		GameObject field;
		GameObject background;

		CoroutineQueue coroutineQueue;

		internal void Prepare() 
		{
			deckNames = new string[] { "Deck" };

			fields = new Dictionary<int, GameObject>();
			fieldsNewAdded = new List<FieldNewAdded>();

			field = new GameObject();
			field.transform.SetParent(transform);
			field.name = "Field";

			background = transform.Find("Background").gameObject;
			coroutineQueue = new CoroutineQueue(3, StartCoroutine);
		}

		internal void Init()
		{
			Align();
			FillBackground();

			// Replace the easing vector when fields show up the first time.
			fieldsNewAdded.ForEach(fieldNewAdded => {
				fieldNewAdded.createdFrom = new Vector2(0, 1);
			});
		}

		internal void PrepareField(int countOfFields)
		{
			Enumerable.Range(0, countOfFields).ForEach(index => fields.Add(index, null));
		}

		internal void MergeField(MergingInfo mergingInfo) 
		{
			coroutineQueue.Run(StartMerging(mergingInfo));
		}

		internal void MoveField(Position targetPosition, Position cardPosition)
		{
			coroutineQueue.Run(StartMoving(targetPosition, cardPosition));
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
			fieldsNewAdded.ForEach((fieldNewAdded) => {
				SetVisibleOfValues(fieldNewAdded.card, false);
				SetVisibleOfResource(fieldNewAdded.card, false);
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

		IEnumerator StartMerging(MergingInfo mergingInfo) 
		{
			var mergingCard = fields[mergingInfo.mergingPosition.index];
			var sourceCard = fields[mergingInfo.sourcePosition.index];
			var targetCard = fields[mergingInfo.targetPosition.index];
			var playerData = mergingInfo.playerData;

			var sourceCharacter = sourceCard.transform.GetChild(1);
			var sourceAnimator = sourceCharacter.GetComponent<Animator>();
			var sourceRenderer = sourceCharacter.GetComponent<SpriteRenderer>();
			var targetRenderer = targetCard.transform.GetChild(1).GetComponent<SpriteRenderer>();

			yield return StartCoroutine(TakeCapture(player, mergingCard));
			yield return StartCoroutine(MoveCard(sourceCard, targetCard, sourceAnimator, sourceRenderer, targetRenderer));
			yield return StartCoroutine(MergeCard(sourceCard, targetCard, mergingCard, playerData, sourceAnimator));
			yield return StartCoroutine(EndMerging(mergingInfo.targetPosition, player, playerData));
		}

        IEnumerator MoveCard(
			GameObject sourceCard, 
			GameObject targetCard, 
			Animator sourceAnimator, 
			SpriteRenderer sourceRenderer,
			SpriteRenderer targetRenderer
		) {
			SetVisibleOfValues(sourceCard, false);
			SetVisibleOfValues(targetCard, false);
			sourceAnimator.SetTrigger("walk");	
			sourceRenderer.sortingOrder = 1;
			targetRenderer.sortingOrder = 0;

			sourceCard.transform.DOLocalMove(
				new Vector2(targetCard.transform.localPosition.x, targetCard.transform.localPosition.y), 0.4f
			).SetEase(Ease.OutSine);

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator MergeCard(
			GameObject sourceCard, 
			GameObject targetCard, 
			GameObject mergingCard, 
			PlayerData playerData,
			Animator sourceAnimator
		) {
			switch(targetCard.name) 
			{
				case "Potion": 
				case "Coin": 
				case "Weapon": 
				case "Magic": sourceAnimator.SetTrigger("jesture"); break;
				default: sourceAnimator.SetTrigger("attack"); break;
			}

			yield return new WaitForSeconds(1f);

			SetVisibleOfValues(sourceCard, true);
			SetVisibleOfValues(targetCard, true);
			SetValue(player, "Value", playerData.hp.ToString());
			mergingCard.SetActive(false);
			Destroy(mergingCard);

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator EndMerging(Position targetPosition, GameObject player, PlayerData playerData) 
		{
			fields[targetPosition.index] = player;
			if(playerData.hp <= 0) 
			{
				CallAnimation(player, "death");
				// player
			}

			yield return new WaitForSeconds(1f);
		}

		void CallAnimation(GameObject targetCard, string name)
		{
			var sourceCharacter = targetCard.transform.GetChild(1);
			var sourceAnimator = sourceCharacter.GetComponent<Animator>();
			sourceAnimator.SetTrigger(name);
		}

		void SetValue(GameObject targetCard, string key, string value)
		{
			targetCard.transform.GetChild(0).Find(key).GetComponent<TextMesh>().text = value;
		}

		void SetVisibleOfValues(GameObject targetCard, bool visible) 
		{
			targetCard.transform.GetChild(0).Find("Value").GetComponent<TextMesh>().gameObject.SetActive(visible);
			targetCard.transform.GetChild(0).Find("Name").GetComponent<TextMesh>().gameObject.SetActive(visible);
		}

		void SetVisibleOfResource(GameObject targetCard, bool visible)
		{
			targetCard.transform.GetChild(1).gameObject.SetActive(visible);
		}

		void SetVisibleOfMask(GameObject targetCard, bool visible)
		{
			targetCard.transform.GetChild(0).Find("Mask").gameObject.SetActive(visible);
		}

		internal bool IsPlaying()
		{
			return coroutineQueue.IsRunning();
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
			SetVisibleOfMask(card, false);

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
				Vector3 creatingOffset = createdFrom * 0.21f;
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
							SetVisibleOfMask(card, true);
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

		void FillBackground()
		{
			StartCoroutine(StartFillBackground());
		}

		IEnumerator StartFillBackground()
		{
			yield return null;

			var ceilings = new GameObject("Ceilings");
			ceilings.transform.SetParent(background.transform);
			ceilings.transform.SetAsFirstSibling();

			var sampleCeiling = ResourceCache.Instantiate("Ceilings");
			var sizeOfCeiling = sampleCeiling.GetBounds();
			var sizeOfBackground = background.GetBounds();
			Destroy(sampleCeiling);

			var startVectorOfRight = new Vector2(
				sizeOfBackground.max.x + sizeOfCeiling.extents.x,
				sizeOfBackground.max.y - sizeOfCeiling.extents.y
			);
			var startVectorOfLeft = new Vector2(
				sizeOfBackground.min.x - sizeOfCeiling.extents.x,
				sizeOfBackground.max.y - sizeOfCeiling.extents.y
			);

			Debug.Log("hoi :" + sizeOfBackground.min.x + ", " + sizeOfCeiling.extents.x + ", " + (sizeOfBackground.min.x - sizeOfCeiling.extents.x));

			var screenSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

			yield return StartFillEmptyArea(
				ceilings, 
				new Vector2(sizeOfCeiling.size.x, -sizeOfCeiling.size.y),
				startVectorOfRight, 
				new Vector2(startVectorOfRight.x, -screenSize.y),
				new Vector2(screenSize.x, screenSize.y)
			);
			yield return StartFillEmptyArea(
				ceilings,
				new Vector2(-sizeOfCeiling.size.x, -sizeOfCeiling.size.y),
				startVectorOfLeft,
				new Vector2(-screenSize.x, -screenSize.y),
				new Vector2(-startVectorOfLeft.x, screenSize.y)
			);
		}

		IEnumerator StartFillEmptyArea(
			GameObject ceilings, 
			Vector2 offsetOfCeiling, 
			Vector2 startVector, 
			Vector2 minBoundary,
			Vector2 maxBoundary
		) {
			int row = 0;
			int col = 0; 
			while(true)
			{
				var ceiling = ResourceCache.Instantiate("Ceilings");
				ceiling.transform.SetParent(ceilings.transform);

				var currentPosition = new Vector2(
					startVector.x + col * offsetOfCeiling.x, 
					startVector.y + row * offsetOfCeiling.y
				);
				ceiling.transform.localPosition = currentPosition;

				col++;
				if(currentPosition.x < minBoundary.x || currentPosition.x > maxBoundary.x)
				{
					if(currentPosition.y < minBoundary.y || currentPosition.y > maxBoundary.y)
					{
						yield break;
					}
					col = 0;
					row++;
				}

				yield return null;
			}
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

        internal Bounds GetBackgroundSize()
        {
            return background.GetBounds();
        }
    }
}