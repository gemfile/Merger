using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;
using UnityEngine.Events;

namespace com.Gemfile.Merger
{
	internal class SpriteCapturedEvent: UnityEvent<Sprite, Vector3, ICard, List<ICard>> {}

    class FieldNewAdded
	{
		internal GameObject card;
		internal Vector2 createdFrom;
	}

	class ActionLogCache
	{
		internal GameObject sourceCard;
		internal GameObject targetCard;
		internal ActionType type;
		internal int valueAffected;
	}

    internal class GameView : MonoBehaviour 
	{
		string[] deckNames;
		Dictionary<int, GameObject> fields;
		List<FieldNewAdded> fieldsNewAdded;
		GameObject player;

		Bounds initialBoundsOfBackground;
		Bounds sizeOfCard;

		GameObject field;
		GameObject background;

		CoroutineQueue coroutineQueue;
		internal SpriteCapturedEvent spriteCapturedEvent;

		const float GAPS_BETWEEN_CARDS = 0.04f;

		internal void Prepare() 
		{
			deckNames = new string[] { "Deck" };

			fields = new Dictionary<int, GameObject>();
			fieldsNewAdded = new List<FieldNewAdded>();

			field = new GameObject();
			field.transform.SetParent(transform);
			field.name = "Field";

			background = transform.Find("Background").gameObject;
			initialBoundsOfBackground = background.GetBounds();

			coroutineQueue = new CoroutineQueue(3, StartCoroutine);
			
			spriteCapturedEvent = new SpriteCapturedEvent();
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
			Hide();
			ShowCards();
        }

		void Hide()
		{
			fieldsNewAdded.ForEach((fieldNewAdded) => {
				fieldNewAdded.card.gameObject.SetActive(false);
				SetVisibleOfValues(fieldNewAdded.card, false);
				SetVisibleOfResource(fieldNewAdded.card, false);
			});
		}

		IEnumerator StartMoving(Position targetPosition, Position cardPosition)
		{
			var movingCard = fields[cardPosition.index];
			var targetLocalPosition = new Vector2(
				(sizeOfCard.size.x + GAPS_BETWEEN_CARDS) * targetPosition.col,
				(sizeOfCard.size.y + GAPS_BETWEEN_CARDS) * targetPosition.row
			);
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
			var actionLogCaches = new List<ActionLogCache>();
			playerData.actionLogs.ForEach(actionLog => {
				actionLogCaches.Add(new ActionLogCache() {
					sourceCard = fields[actionLog.sourcePosition.index],
					targetCard = fields[actionLog.targetPosition.index],
					type = actionLog.type,
					valueAffected = actionLog.valueAffected
				});
			});

			var sourceCharacter = sourceCard.transform.GetChild(1);
			var targetCharacter = targetCard.transform.GetChild(1);

			yield return StartCoroutine(TakeCapture(mergingCard, playerData));
			yield return StartCoroutine(MoveCard(sourceCard, targetCard, sourceCharacter, targetCharacter));
			yield return StartCoroutine(MergeCard(sourceCard, targetCard, mergingCard, playerData, actionLogCaches));
			yield return StartCoroutine(EndMerging(mergingInfo.targetPosition, player, playerData));
		}

        IEnumerator MoveCard(
			GameObject sourceCard, 
			GameObject targetCard, 
			Transform sourceCharacter,
			Transform targetCharacter
		) {
			SetVisibleOfValues(sourceCard, false);
			SetVisibleOfValues(targetCard, false);
			SetVisibleOfChild(sourceCard, "Suit", false);
			SetVisibleOfChild(targetCard, "Suit", false);
			
			var x = (targetCard.transform.localPosition - sourceCard.transform.localPosition).normalized.x;
			sourceCharacter.transform.localScale = new Vector3(
				Mathf.Abs(x) == 1 ? x : sourceCharacter.transform.localScale.x,
				1,
				1
			);
			sourceCharacter.GetComponent<Animator>().SetTrigger("walk");
			sourceCharacter.GetComponent<SpriteRenderer>().sortingOrder = 1;
			targetCharacter.GetComponent<SpriteRenderer>().sortingOrder = 0;

			sourceCard.transform.DOLocalMove(
				new Vector2(targetCard.transform.localPosition.x, targetCard.transform.localPosition.y), 0.4f
			).SetEase(Ease.OutCubic);

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator MergeCard(
			GameObject sourceCard, 
			GameObject targetCard, 
			GameObject mergingCard, 
			PlayerData playerData,
			List<ActionLogCache> actionLogCaches
		) {
			foreach (var actionLogCache in actionLogCaches)
			{
				var sourceCharacter = actionLogCache.sourceCard.transform.GetChild(1);
				var targetCharacter = actionLogCache.targetCard.transform.GetChild(1);
				sourceCharacter.GetComponent<SpriteRenderer>().sortingOrder = 1;
				targetCharacter.GetComponent<SpriteRenderer>().sortingOrder = 0;
				
				SetTriggerOnAction(actionLogCache.targetCard, actionLogCache.sourceCard);

				switch (actionLogCache.type) {
					case ActionType.ATTACK:
					case ActionType.GET_DAMAGED:
						SetDamage(actionLogCache.targetCard, -actionLogCache.valueAffected);
						break;
				}
				
				yield return new WaitForSeconds(1f);
			}

			SetVisibleOfValues(player, true);
			GetText(player, "Value").FadeIn(.4f);
			GetText(player, "Name").FadeIn(.4f);
			SetValue(player, "Value", playerData.hp.ToString());

			SetTriggerOnMerging(mergingCard);
			yield return new WaitForSeconds(.8f);

			mergingCard.SetActive(false);
			Destroy(mergingCard);
		}

		void SetDamage(GameObject targetCard, int damagedValue)
		{
			TextMesh valueText = GetText(targetCard, "Damaged");

			var delay = 0.8f;
			var sequence = DOTween.Sequence();
			sequence.SetDelay(delay);
			sequence.AppendCallback(() => {
				var originValue = int.Parse(GetText(targetCard, "Value").text);
				SetValue(targetCard, "Damaged", damagedValue.ToString());
				SetVisibleOfText(targetCard, "Damaged", true);
			});
			sequence.Append(
				DOTween.To(
					() => valueText.color,
					x => valueText.color = x,
					new Color(valueText.color.r, valueText.color.g, valueText.color.b, 1),
					.8f
				).From().SetEase(Ease.InCubic)
			);
			sequence.Insert(
				delay, 
				valueText.transform.DOLocalMoveY(valueText.transform.localPosition.y - 0.04f, 0.8f).From()
			);
			sequence.AppendCallback(() => {
				SetVisibleOfText(targetCard, "Damaged", false);
			});
		}

		void SetTriggerOnAction(GameObject targetCard, GameObject sourceCard)
		{
			string trigger;
			switch (targetCard.name)
			{
				case "Potion": 
				case "Coin": 
				case "Weapon": 
				case "Magic": trigger = "jesture"; break;
				default: trigger = "attack"; break;
			}
			
			Debug.Log("SetTriggerOnAction : " + trigger);
			CallAnimation(sourceCard, trigger);
		}

		void SetTriggerOnMerging(GameObject mergingCard)
		{
			if (mergingCard.name == "Monster")
			{
				CallAnimation(mergingCard, "death");
			}
			mergingCard.transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, .4f).SetDelay(.4f);
		}

		IEnumerator EndMerging(Position targetPosition, GameObject player, PlayerData playerData) 
		{
			fields[targetPosition.index] = player;
			if (playerData.hp <= 0) 
			{
				CallAnimation(player, "death");
				// player
			}

			yield return null;
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
		
		TextMesh GetText(GameObject targetCard, string key)
		{
			return targetCard.transform.GetChild(0).Find(key).GetComponent<TextMesh>();
		}

		void SetVisibleOfText(GameObject targetCard, string key, bool visible)
		{
			targetCard.transform.GetChild(0).Find(key).GetComponent<TextMesh>().gameObject.SetActive(visible);
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

		void SetVisibleOfChild(GameObject targetCard, string key, bool visible)
		{
			targetCard.transform.GetChild(0).Find(key).gameObject.SetActive(visible);
		}

		void SetSuit(GameObject targetCard, string name)
		{
			var suits = targetCard.transform.GetChild(0).Find("Suit").transform;
			foreach (Transform suit in suits)
			{
				suit.gameObject.SetActive(false);
			}

			var targetSuit = "";
			switch (name)
			{
				case "Potion": targetSuit = "Heart"; break;
				case "Coin": targetSuit = "Diamond"; break;
				case "Weapon": targetSuit = "Club"; break;
				case "Magic": targetSuit = "Club"; break;
				case "Monster": targetSuit = "Spade"; break;
			}

			if (targetSuit != "")
			{
				suits.Find(targetSuit).gameObject.SetActive(true);
			}
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
			SetVisibleOfChild(card, "Mask", false);
			SetSuit(card, card.name);

			var cardResource = ResourceCache.Instantiate(cardData.resourceName, transform);
			cardResource.transform.SetParent(card.transform);
			
			card.transform.localPosition = new Vector2(
				(sizeOfCard.size.x + GAPS_BETWEEN_CARDS) * position.col, 
				(sizeOfCard.size.y + GAPS_BETWEEN_CARDS) * position.row
			);
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
			
				var duration = .66f;
				var dealy = .14f;
				var sequence = DOTween.Sequence();
				sequence.SetDelay(dealy);
				sequence.InsertCallback(dealy, ()=>card.gameObject.SetActive(true));
				sequence.Append(card.transform.DOLocalMove(localPosition + creatingOffset, duration).From().SetEase(Ease.OutBack));
				sequence.Append(card.transform.DOScaleX(-1, duration).From().SetEase(Ease.InOutCubic));
				sequence.Insert(
					dealy + duration, 
					card.transform.DOLocalMoveY(localPosition.y+.07f, duration/2)
						.SetLoops(2, LoopType.Yoyo)
						.SetEase(Ease.InOutCubic)
						.OnStepComplete(()=>{
							SetVisibleOfChild(card, "Mask", true);
							SetVisibleOfValues(card, true);
							SetVisibleOfResource(card, true);
							SetVisibleOfChild(card, "Suit", true);
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
			Destroy(sampleCeiling);

			var sizeOfBackground = initialBoundsOfBackground;
			var startVectorOfRight = new Vector2(
				sizeOfBackground.max.x + sizeOfCeiling.extents.x,
				sizeOfBackground.max.y + sizeOfCeiling.extents.y
			);
			var startVectorOfLeft = new Vector2(
				sizeOfBackground.min.x - sizeOfCeiling.extents.x,
				sizeOfBackground.max.y + sizeOfCeiling.extents.y
			);

			Debug.Log("hoi :" + sizeOfBackground.min.x + ", " + sizeOfCeiling.extents.x + ", " + (sizeOfBackground.min.x - sizeOfCeiling.extents.x));

			var screenSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

			FillEmptyArea(
				ceilings, 
				new Vector2(sizeOfCeiling.size.x, -sizeOfCeiling.size.y),
				startVectorOfRight, 
				new Vector2(startVectorOfRight.x, -screenSize.y),
				new Vector2(screenSize.x, screenSize.y)
			);
			FillEmptyArea(
				ceilings,
				new Vector2(-sizeOfCeiling.size.x, -sizeOfCeiling.size.y),
				startVectorOfLeft,
				new Vector2(-screenSize.x, -screenSize.y),
				new Vector2(-startVectorOfLeft.x, screenSize.y)
			);
		}

		void FillEmptyArea(
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
						break;
					}
					col = 0;
					row++;
				}
			}
		}

		IEnumerator TakeCapture(GameObject targetCard, PlayerData playerData)
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

			spriteCapturedEvent.Invoke(capturedSprite, size, playerData.merged, playerData.equipments);
		}

        internal Bounds GetBackgroundSize()
        {
            return initialBoundsOfBackground;
        }
    }
}