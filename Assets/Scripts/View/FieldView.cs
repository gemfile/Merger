using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace com.Gemfile.Merger
{
    public interface IFieldView: IBaseView
    {
		void Reset();
		void Dehighlight();
		void HighlightCards(List<NavigationColorInfo> navigationColorInfos);
        void SetField(int countOfFields);
        void AddField(Position targetPosition, CardData cardData, Vector2 createdFrom);
        void ShowField();
        void MergeField(MergingInfo mergingInfo);
        void MoveField(Position targetPosition, Position cardPosition);
        Bounds BackgroundBounds { get; }
        Bounds CardBounds { get; }
        bool IsPlaying { get; }
		Dictionary<int, GameObject> Fields { get; }
		SpriteCaptureEvent OnSpriteCaptured { get; }
    }

	public class SpriteCaptureEvent: UnityEvent<Sprite, Vector3, ICardModel, List<ICardModel>> {}

	class ActionLogCache
	{
		public GameObject sourceCard;
		public GameObject targetCard;
		public ActionType type;
		public int valueAffected;
	}

    class FieldNewAdded
	{
		public GameObject card;
		public Vector2 createdFrom;
	}

    public class FieldView : BaseView, IFieldView 
    {
        string[] deckNames;
		public Dictionary<int, GameObject> Fields { 
			get { return fields; }
		}
		Dictionary<int, GameObject> fields;
		List<FieldNewAdded> fieldsNewAdded;
        public bool IsPlaying {
			get { return coroutineQueue.IsRunning(); }
		}
        CoroutineQueue coroutineQueue;
		readonly float GAPS_BETWEEN_CARDS;
        GameObject fieldContainer;
        public Bounds BackgroundBounds {
			get { return backgroundBounds; }
		}
		Bounds backgroundBounds;
		GameObject background;
		public Bounds CardBounds {
			get { return cardBounds; }
		}
        Bounds cardBounds;

		public SpriteCaptureEvent OnSpriteCaptured { get { return onSpriteCaptured; } }
		readonly SpriteCaptureEvent onSpriteCaptured = new SpriteCaptureEvent();
		
        public FieldView()
        {
            deckNames = new string[]{ "Deck" };

			fields = new Dictionary<int, GameObject>();
			fieldsNewAdded = new List<FieldNewAdded>();

			coroutineQueue = new CoroutineQueue(10, StartCoroutine);
			GAPS_BETWEEN_CARDS = 0.04f;
        }

        public override void Init()
        {
            fieldContainer = transform.Find("Field").gameObject;
			background = transform.Find("Background").gameObject;
			backgroundBounds = background.GetBounds();
        }

		public void Reset()
		{
			fields.ForEach(field => {
				Destroy(field.Value);
			});
			Destroy(background.transform.Find("Ceilings").gameObject);
			atFirst = true;
		}

		public void HighlightCards(List<NavigationColorInfo> navigationColorInfos)
		{
			navigationColorInfos.ForEach(navigationColorInfo => {
				var card = fields[navigationColorInfo.index];
				Highlight(card, true, navigationColorInfo.color);
			});
		}

		public void Dehighlight()
		{
			fields.ForEach(card => {
				Highlight(card.Value, false);
			});
		}

        public void SetField(int countOfFields)
		{
			fields.Clear();
			Enumerable.Range(0, countOfFields).ForEach(index => fields.Add(index, null));
		}

        public void AddField(Position targetPosition, CardData cardData, Vector2 createdFrom)
		{
			var card = new GameObject();
			card.transform.SetParent(fieldContainer.transform);
			card.name = cardData.type;

			var deck = ResourceCache.Instantiate(deckNames[UnityEngine.Random.Range(0, 1)], transform);
			deck.transform.SetParent(card.transform);
			cardBounds = card.GetBounds();
			SetValue(card, "Name", cardData.cardName);
			SetValue(card, "Value", cardData.value.ToString());
			SetVisibleOfChild(card, "Mask", false);
			SetSuit(card, card.name);

			var cardResource = ResourceCache.Instantiate(cardData.resourceName, transform);
			cardResource.transform.SetParent(card.transform);
			
			card.transform.localPosition = new Vector2(
				(cardBounds.size.x + GAPS_BETWEEN_CARDS) * targetPosition.col, 
				(cardBounds.size.y + GAPS_BETWEEN_CARDS) * targetPosition.row
			);
			fields[targetPosition.index] = card;
			fieldsNewAdded.Add(new FieldNewAdded() {
				card = card, 
				createdFrom = createdFrom
			});
		}

        bool atFirst = true;
		public void ShowField()
		{
			if (atFirst) 
			{
				AlignFields();
				FillBackground();
				AlignAtTopOfCamera();
				atFirst = false;
			}

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

		void ShowCards()
		{
			coroutineQueue.Run(StartShow());
		}

		IEnumerator StartShow()
		{
			var hasTweenCompleted = false;

			fieldsNewAdded.ForEach(fieldNewAdded => {
				var card = fieldNewAdded.card;
				var createdFrom = fieldNewAdded.createdFrom;
				Vector3 localPosition = card.transform.localPosition;
				Vector3 creatingOffset = createdFrom * 0.21f;
			
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
				sequence.OnComplete(()=>{
					hasTweenCompleted = true;
				});
			});
			fieldsNewAdded.Clear();

			while(!hasTweenCompleted)
			{
				yield return null;
			}
			
		}

        public void MergeField(MergingInfo mergingInfo) 
		{
			coroutineQueue.Run(StartMerging(mergingInfo));
		}

		IEnumerator StartMerging(MergingInfo mergingInfo) 
		{
			var mergerInfo = mergingInfo.mergerInfo;
			var targetPosition = mergingInfo.targetPosition;
			var sourceCard = fields[mergingInfo.sourcePosition.index];
			var targetCard = fields[targetPosition.index];
			var mergedCard = fields[mergerInfo.mergedPosition.index];
			var mergerCard = fields[mergerInfo.mergerPosition.index];
			var hp = mergerInfo.hp;
			var merger = mergerInfo.merger;
			var merged = mergerInfo.merged;
			var equipments = mergerInfo.equipments;

			var actionLogCaches = new List<ActionLogCache>();
			mergerInfo.actionLogs.ForEach(actionLog => {
				actionLogCaches.Add(new ActionLogCache() {
					sourceCard = fields[actionLog.sourcePosition.index],
					targetCard = fields[actionLog.targetPosition.index],
					type = actionLog.type,
					valueAffected = actionLog.valueAffected
				});
			});

			var sourceCharacter = sourceCard.transform.GetChild(1);
			var targetCharacter = targetCard.transform.GetChild(1);

			yield return StartCoroutine(TakeCapture(mergedCard, merger, merged, equipments));
			yield return StartCoroutine(MoveCard(sourceCard, targetCard, sourceCharacter, targetCharacter));
			yield return StartCoroutine(MergeCard(sourceCard, targetCard, mergerCard, mergedCard, hp, actionLogCaches));
			yield return StartCoroutine(EndMerging(targetPosition, mergerCard, hp));
		}

        IEnumerator TakeCapture(GameObject targetCard, ICardModel merger, ICardModel merged, List<ICardModel> equipments)
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

			if (merger is PlayerModel) {
				onSpriteCaptured.Invoke(capturedSprite, size, merged, equipments);
			}
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
			GameObject mergerCard,
			GameObject meredCard, 
			int hp,
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
						SetGettingEffect(actionLogCache.targetCard, "Damaged", -actionLogCache.valueAffected);
						break;
					case ActionType.GET_COIN:
						SetGettingEffect(actionLogCache.targetCard, "GettingCoin", actionLogCache.valueAffected);
						break;
					case ActionType.USE_POTION:
						SetGettingEffect(actionLogCache.targetCard, "Damaged", actionLogCache.valueAffected);
						break;
				}
				
				yield return new WaitForSeconds(1f);
			}

			SetVisibleOfValues(mergerCard, true);
			GetText(mergerCard, "Value").FadeIn(.4f);
			GetText(mergerCard, "Name").FadeIn(.4f);
			SetValue(mergerCard, "Value", hp.ToString());

			SetTriggerOnMerging(meredCard);
			yield return new WaitForSeconds(.8f);

			meredCard.SetActive(false);
			Destroy(meredCard);
		}

		IEnumerator EndMerging(Position targetPosition, GameObject mergerCard, int hp) 
		{
			fields[targetPosition.index] = mergerCard;
			if (hp <= 0) 
			{
				CallAnimation(mergerCard, "death");
			}

			yield return null;
		}

        public void MoveField(Position targetPosition, Position cardPosition)
		{
			coroutineQueue.Run(StartMoving(targetPosition, cardPosition));
		}

		IEnumerator StartMoving(Position targetPosition, Position cardPosition)
		{
			var movingCard = fields[cardPosition.index];
			var targetLocalPosition = new Vector2(
				(cardBounds.size.x + GAPS_BETWEEN_CARDS) * targetPosition.col,
				(cardBounds.size.y + GAPS_BETWEEN_CARDS) * targetPosition.row
			);
			Debug.Log("StartMoving : " + cardPosition.index + ", " + movingCard.name);

			movingCard.transform.DOLocalMove(
				new Vector2(targetLocalPosition.x, targetLocalPosition.y), 0.4f
			).SetEase(Ease.OutSine);
			fields[targetPosition.index] = movingCard;

			yield return new WaitForSeconds(0.4f);
		}

        void SetValue(GameObject targetCard, string key, string value)
		{
			targetCard.transform.GetChild(0).Find(key).GetComponent<TextMesh>().text = value;
		}

		void Highlight(GameObject targetCard, bool visible, Color32 color = default(Color32))
		{
			var spriteOutline = targetCard.transform.GetChild(0).Find("Background").GetComponent<SpriteOutline>();
			spriteOutline.enabled = visible;
			spriteOutline.color = color;
		}

		void SetVisibleOfChild(GameObject targetCard, string key, bool visible)
		{
			targetCard.transform.GetChild(0).Find(key).gameObject.SetActive(visible);
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

        void SetTriggerOnMerging(GameObject mergingCard)
		{
			if (mergingCard.name == "Monster")
			{
				CallAnimation(mergingCard, "death");
			}
			mergingCard.transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, .4f).SetDelay(.4f);
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

		void CallAnimation(GameObject targetCard, string name)
		{
			var sourceCharacter = targetCard.transform.GetChild(1);
			var sourceAnimator = sourceCharacter.GetComponent<Animator>();
			if (sourceAnimator != null) 
			{
				sourceAnimator.SetTrigger(name);
			}
		}

        void SetGettingEffect(GameObject targetCard, string nameAffected, int valueAffected)
		{
			TextMesh valueText = GetText(targetCard, nameAffected);

			var delay = 0.8f;
			var sequence = DOTween.Sequence();
			sequence.SetDelay(delay);
			sequence.AppendCallback(() => {
				SetValue(targetCard, nameAffected, (valueAffected >= 0 ? "+" : "") + valueAffected.ToString());
				SetVisibleOfText(targetCard, nameAffected, true);
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
				SetVisibleOfText(targetCard, nameAffected, false);
			});
		}

		void SetVisibleOfText(GameObject targetCard, string key, bool visible)
		{
			targetCard.transform.GetChild(0).Find(key).GetComponent<TextMesh>().gameObject.SetActive(visible);
		}

		TextMesh GetText(GameObject targetCard, string key)
		{
			return targetCard.transform.GetChild(0).Find(key).GetComponent<TextMesh>();
		}

		void AlignFields()
		{
			var sizeOfField = fieldContainer.GetBounds();
			fieldContainer.transform.localPosition = new Vector3(
				(cardBounds.size.x - sizeOfField.size.x) / 2, 
				(cardBounds.size.y - sizeOfField.size.y) / 2, 
				fieldContainer.transform.localPosition.z
			);
		}

        void AlignAtTopOfCamera()
		{
			var sizeOfScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
			var sizeOfGameView = gameObject.GetBounds();
			var offsetY = sizeOfScreen.y - sizeOfGameView.max.y;

			var positionOfGameView = transform.localPosition;
			transform.localPosition = new Vector3(
				positionOfGameView.x, 
				positionOfGameView.y + offsetY,
				positionOfGameView.z
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

			var sizeOfBackground = BackgroundBounds;
			var startVectorOfRight = new Vector2(
				sizeOfBackground.max.x + sizeOfCeiling.extents.x,
				sizeOfBackground.max.y - sizeOfCeiling.extents.y
			);
			var startVectorOfLeft = new Vector2(
				sizeOfBackground.min.x - sizeOfCeiling.extents.x,
				sizeOfBackground.max.y - sizeOfCeiling.extents.y
			);

			Debug.Log("hoi :" + sizeOfBackground.max.x + ", " + sizeOfCeiling.extents.x + ", " + (sizeOfBackground.min.x - sizeOfCeiling.extents.x));

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
    }
}
