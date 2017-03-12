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
		void Reset(bool withFields);
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
		Dictionary<int, ICardView> Fields { get; }
		SpriteCapturedEvent OnSpriteCaptured { get; }
		void RetakeCapture(PlayerInfo mergerModel);
    }

	public class SpriteCapturedEvent: UnityEvent<SpriteCapturedInfo> {}

	public class SpriteCapturedInfo
	{
		public Sprite capturedSprite; 
		public Vector3 size;
		public ICardModel mergedModel;
		public List<ICardModel> equipments;
	}

	class ActionLogCache
	{
		public ICardView mergerCard;
		public ICardView mergedCard;
		public ActionType type;
		public int valueAffected;
	}

    class FieldNewAdded
	{
		public ICardView cardView;
		public Vector2 createdFrom;
	}

	class TakenCaptureArgs
	{
		internal ICapturedCardView capturedCard;
		internal ICardModel capturedModel;
		internal Vector3 size;
	}

	class CardSize
	{
		internal Vector3 size;
		internal Vector3 leftBottom;
	}

    public class FieldView: BaseView, IFieldView
    {
		public Dictionary<int, ICardView> Fields { 
			get { return fields; }
		}
		Dictionary<int, ICardView> fields;
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
		GameObject ceilings;
		public Bounds CardBounds {
			get { return cardBounds; }
		}
        Bounds cardBounds;
		bool needAligning;
		Dictionary<ICapturedCardView, TakenCaptureArgs> takenCaptureArgsDic;

		public SpriteCapturedEvent OnSpriteCaptured { get { return onSpriteCaptured; } }
		readonly SpriteCapturedEvent onSpriteCaptured = new SpriteCapturedEvent();
		
        public FieldView()
        {
			fields = new Dictionary<int, ICardView>();
			fieldsNewAdded = new List<FieldNewAdded>();
			takenCaptureArgsDic = new Dictionary<ICapturedCardView, TakenCaptureArgs>();

			coroutineQueue = new CoroutineQueue(10, StartCoroutine);
			GAPS_BETWEEN_CARDS = 0.04f;
			needAligning = true;
        }

        public override void Init()
        {
            fieldContainer = transform.Find("Field").gameObject;
			background = transform.Find("Background").gameObject;
			backgroundBounds = background.GetBounds();
        }

		public void Reset(bool withFields)
		{
			if (withFields)
			{
				fields.ForEach(field => {
					Destroy(field.Value.GameObject);
				});
			}
			if (ceilings)
			{
				Destroy(ceilings);
			}
			
			needAligning = true;
		}

		public void HighlightCards(List<NavigationColorInfo> navigationColorInfos)
		{
			navigationColorInfos.ForEach(navigationColorInfo => {
				var card = fields[navigationColorInfo.index];
				card.Highlight(true, navigationColorInfo.color);
			});
		}

		public void Dehighlight()
		{
			fields.ForEach(card => {
				card.Value.Highlight(false, default(Color32));
			});
		}

        public void SetField(int countOfFields)
		{
			fields.Clear();
			Enumerable.Range(0, countOfFields).ForEach(index => fields.Add(index, null));
		}

        public void AddField(Position targetPosition, CardData cardData, Vector2 createdFrom)
		{
			var card = ResourceCache.Instantiate("CardView", transform).transform;
			card.name = cardData.type;
			card.SetParent(fieldContainer.transform);
			cardBounds = card.gameObject.GetBounds();

			var cardView = card.GetComponent<ICardView>();
			cardView.SetValue("Name", cardData.cardName);
			cardView.SetValue("Value", cardData.value.ToString());
			cardView.SetVisibleOfChild("Mask", false);
			cardView.SetSuit(card.name);

			cardView.SetCharacter(ResourceCache.Instantiate(cardData.resourceName, card).transform);
			
			card.transform.localPosition = new Vector2(
				(cardBounds.size.x + GAPS_BETWEEN_CARDS) * targetPosition.col, 
				(cardBounds.size.y + GAPS_BETWEEN_CARDS) * targetPosition.row
			);
			fields[targetPosition.index] = cardView;
			fieldsNewAdded.Add(new FieldNewAdded() {
				cardView = cardView, 
				createdFrom = createdFrom
			});
		}

		public void ShowField()
		{
			if (needAligning) 
			{
				AlignFields();
				FillBackground();
				AlignAtTopOfCamera();
				needAligning = false;
			}

			Hide();
			ShowCards();
		}

        void Hide()
		{
			fieldsNewAdded.ForEach((fieldNewAdded) => {
				fieldNewAdded.cardView.GameObject.SetActive(false);
				fieldNewAdded.cardView.SetVisibleOfValues(false);
				fieldNewAdded.cardView.SetVisibleOfResource(false);
			});
		}

		void ShowCards()
		{
			coroutineQueue.Run(StartShow());
		}

		IEnumerator StartShow()
		{
			var hasTweenCompleted = fieldsNewAdded.Count == 0;

			fieldsNewAdded.ForEach(fieldNewAdded => {
				var newCard = fieldNewAdded.cardView;
				var createdFrom = fieldNewAdded.createdFrom;
				Vector3 localPosition = newCard.Transform.localPosition;
				Vector3 creatingOffset = createdFrom * 0.21f;
			
				var duration = .66f;
				var dealy = .14f;
				var sequence = DOTween.Sequence();
				sequence.SetDelay(dealy);
				sequence.InsertCallback(dealy, ()=>newCard.GameObject.SetActive(true));
				sequence.Append(newCard.Transform.DOLocalMove(localPosition + creatingOffset, duration).From().SetEase(Ease.OutBack));
				sequence.Append(newCard.Transform.DOScaleX(-1, duration).From().SetEase(Ease.InOutCubic));
				sequence.Insert(
					dealy + duration, 
					newCard.Transform.DOLocalMoveY(localPosition.y+.07f, duration/2)
						.SetLoops(2, LoopType.Yoyo)
						.SetEase(Ease.InOutCubic)
						.OnStepComplete(()=>{
							newCard.SetVisibleOfChild("Mask", true);
							newCard.SetVisibleOfValues(true);
							newCard.SetVisibleOfResource(true);
							newCard.SetVisibleOfChild("Suit", true);
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
			var mergerCard = fields[mergerInfo.mergerPosition.index];
			var mergedCard = fields[mergerInfo.mergedPosition.index];
			var hp = mergerInfo.hp;
			var mergerModel = mergerInfo.mergerModel;
			var mergedModel = mergerInfo.mergedModel;
			var equipments = mergerInfo.equipments;

			var actionLogCaches = new List<ActionLogCache>();
			mergerInfo.actionLogs.ForEach(actionLog => {
				actionLogCaches.Add(new ActionLogCache() {
					mergerCard = fields[actionLog.mergerPosition.index],
					mergedCard = fields[actionLog.mergedPosition.index],
					type = actionLog.type,
					valueAffected = actionLog.valueAffected
				});
			});

			var sourceCharacter = sourceCard.Character;
			var targetCharacter = targetCard.Character;

			yield return StartCoroutine(TakeCapture(mergerCard, mergedCard, mergerModel, mergedModel, equipments));
			yield return StartCoroutine(MoveCard(sourceCard, targetCard, sourceCharacter, targetCharacter));
			yield return StartCoroutine(MergeCard(mergerCard, hp, actionLogCaches));
			yield return StartCoroutine(EndMerging(targetPosition, mergerCard, mergedCard, mergerModel, equipments));
		}

		public void RetakeCapture(PlayerInfo playerInfo)
		{
			var equipments = (playerInfo.playerModel as IMerger).Equipments;
			var mergerCard = fields[playerInfo.playerPosition.index];

			var takenCaptureArgsEquiped = takenCaptureArgsDic.Values.Where(takenCaptureArgs =>
				equipments.Any(equipment => equipment == takenCaptureArgs.capturedModel)
			).ToList();

			takenCaptureArgsEquiped.ForEach(takenCaptureArgs => {
				onSpriteCaptured.Invoke(new SpriteCapturedInfo() {
					capturedSprite = takenCaptureArgs.capturedCard.Transform.GetComponent<SpriteRenderer>().sprite, 
					size = ReadCardSize(mergerCard).size, 
					mergedModel = takenCaptureArgs.capturedModel, 
					equipments = equipments
				});
			});
		}

		IEnumerator StartRetakeCapture(
			ICardView mergerCard, 
			IBaseView capturedCard,
			ICardModel mergerModel, 
			ICardModel capturedModel,
			List<ICardModel> equipments
		) {
			var originOrder = capturedCard.Transform.GetComponent<SpriteRenderer>().sortingOrder;

			capturedCard.GameObject.SetActive(true);
			mergerCard.SetVisibleOfChild("Background", false);
			capturedCard.Transform.GetComponent<SpriteRenderer>().sortingOrder = 2;

			yield return TakeCapture(
				mergerCard, capturedCard, mergerModel, capturedModel, equipments
			);
			
			capturedCard.Transform.GetComponent<SpriteRenderer>().sortingOrder = originOrder;
			capturedCard.GameObject.SetActive(false);
			mergerCard.SetVisibleOfChild("Background", true);
		}

		CardSize ReadCardSize(IBaseView targetCard)
		{
			Bounds bounds = targetCard.GameObject.GetBounds();
			Vector3 center = Camera.main.WorldToScreenPoint(bounds.center);
			Vector3 leftBottom = Camera.main.WorldToScreenPoint(bounds.min);
			Vector3 rightTop = Camera.main.WorldToScreenPoint(bounds.max);
			Vector3 size = rightTop - leftBottom;
			Debug.Log($"Capturing: {center.x}, {center.y}, {rightTop.x}, {rightTop.y}, {leftBottom.x}, {leftBottom.y}, {size.x}, {size.y}");
			return new CardSize {
				size = size,
				leftBottom = leftBottom
			};
		}

        IEnumerator TakeCapture(
			ICardView mergerCard, 
			IBaseView mergedCard, 
			ICardModel mergerModel, 
			ICardModel mergedModel, 
			List<ICardModel> equipments
		) {
			yield return new WaitForEndOfFrame();

			CardSize cardSize = ReadCardSize(mergedCard);
			var captured = new Texture2D((int)cardSize.size.x, (int)cardSize.size.y, TextureFormat.ARGB32, false);

			captured.ReadPixels(new Rect(
				cardSize.leftBottom.x,
				cardSize.leftBottom.y,
				cardSize.size.x,
				cardSize.size.y
			), 0, 0);
			captured.Apply();

			Sprite capturedSprite = Sprite.Create(
				captured,
				new Rect(0, 0, (int)cardSize.size.x, (int)cardSize.size.y),
				new Vector2(0.5f,0.5f),
				Screen.height / Camera.main.orthographicSize / 2
			);

			var capturedCard = ResourceCache.Instantiate("CapturedCardView", mergerCard.Transform);
			capturedCard.name = mergedCard.Transform.name;
			capturedCard.transform.localPosition = new Vector3(0, 0, 0);
			capturedCard.gameObject.SetActive(false);
			var spriteRenderer = capturedCard.GetComponent<SpriteRenderer>();
			spriteRenderer.sprite = capturedSprite;
			spriteRenderer.sortingOrder = -mergerCard.Transform.childCount;
			var capturedCardView = capturedCard.GetComponent<CapturedCardView>();

			takenCaptureArgsDic[capturedCardView] = new TakenCaptureArgs {
				capturedCard = capturedCardView,
				capturedModel = mergedModel,
				size = cardSize.size
			};

			if (mergerModel is IPlayerModel) {
				onSpriteCaptured.Invoke(new SpriteCapturedInfo {
					capturedSprite = capturedSprite, 
					size = cardSize.size, 
					mergedModel = mergedModel, 
					equipments = equipments
				});
			}
		}

		IEnumerator MoveCard(
			ICardView sourceCard, 
			ICardView targetCard, 
			Transform sourceCharacter,
			Transform targetCharacter
		) {
			sourceCard.SetVisibleOfValues(false);
			targetCard.SetVisibleOfValues(false);
			sourceCard.SetVisibleOfChild("Suit", false);
			targetCard.SetVisibleOfChild("Suit", false);
			
			var x = (targetCard.Transform.localPosition - sourceCard.Transform.localPosition).normalized.x;
			sourceCharacter.transform.localScale = new Vector3(
				Mathf.Abs(x) == 1 ? x : sourceCharacter.transform.localScale.x,
				1,
				1
			);
			sourceCharacter.GetComponent<Animator>().SetTrigger("walk");
			sourceCharacter.GetComponent<SpriteRenderer>().sortingOrder = 1;
			targetCharacter.GetComponent<SpriteRenderer>().sortingOrder = 0;

			sourceCard.Transform.DOLocalMove(
				new Vector2(targetCard.Transform.localPosition.x, targetCard.Transform.localPosition.y), 0.4f
			).SetEase(Ease.OutCubic);

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator MergeCard(
			ICardView mergerCard,
			int hp,
			List<ActionLogCache> actionLogCaches
		) {
			foreach (var actionLogCache in actionLogCaches)
			{
				actionLogCache.mergerCard.Character.GetComponent<SpriteRenderer>().sortingOrder = 1;
				actionLogCache.mergedCard.Character.GetComponent<SpriteRenderer>().sortingOrder = 0;
				
				SetEffectOnAction(actionLogCache);
				yield return SetTriggerOnAction(actionLogCache, actionLogCache.mergerCard, actionLogCache.mergedCard);
			}

			mergerCard.SetVisibleOfValues(true);
			mergerCard.GetText("Value").FadeIn(.4f);
			mergerCard.GetText("Name").FadeIn(.4f);
			mergerCard.SetValue("Value", hp.ToString());

			yield return new WaitForSeconds(.8f);
		}

		IEnumerator EndMerging(
			Position mergedPosition, 
			ICardView mergerCard, 
			ICardView mergedCard, 
			ICardModel mergerModel, 
			List<ICardModel> equipments
		) {
			foreach(var capturedCard in mergedCard.GetCapturedCards())
			{
				var capturedSprite = capturedCard.Transform.GetComponent<SpriteRenderer>();
				var takenCaptureArgs = takenCaptureArgsDic[capturedCard];
				capturedCard.Transform.SetParent(mergerCard.Transform);
				capturedSprite.sortingOrder = -mergerCard.Transform.childCount;

				if (takenCaptureArgs.capturedModel is WeaponModel) {
					onSpriteCaptured.Invoke(new SpriteCapturedInfo() {
						capturedSprite = capturedSprite.sprite, 
						size = takenCaptureArgs.size, 
						mergedModel = takenCaptureArgs.capturedModel, 
						equipments = equipments
					});
				}
			}
			mergedCard.GameObject.SetActive(false);
			Destroy(mergedCard.GameObject);
			
			fields[mergedPosition.index] = mergerCard;
			
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

			movingCard.Transform.DOLocalMove(
				new Vector2(targetLocalPosition.x, targetLocalPosition.y), 0.4f
			).SetEase(Ease.OutSine);
			fields[targetPosition.index] = movingCard;

			yield return new WaitForSeconds(0.4f);
		}

		IEnumerator SetTriggerOnAction(ActionLogCache actionLogCache, ICardView mergerCard, ICardView mergedCard)
		{
			switch (actionLogCache.type)
			{
				case ActionType.GET_MERGED:
					var delay = 0f;
					if (mergedCard.Transform.name == "Monster") {
						mergedCard.CallAnimation("death"); 
						delay += 1f;
					}
					mergedCard.Transform.GetComponent<SpriteRenderer>().DOFade(0, .4f).SetDelay(delay + 1f);
					yield return new WaitForSeconds(delay);
					break;

				case ActionType.ATTACK:
				case ActionType.GET_DAMAGED:
					mergerCard.CallAnimation("attack"); 
					yield return new WaitForSeconds(1f);
					break;

				case ActionType.DEAD:
					mergerCard.CallAnimation("death"); 
					yield return new WaitForSeconds(1f);
					break;

				case ActionType.GETTING_COIN:
				case ActionType.USE_POTION:
				case ActionType.GETTING_ROOT:
				default:
					mergerCard.CallAnimation("jesture"); 
					yield return new WaitForSeconds(1f);
					break;
			}
		}

		void SetEffectOnAction(ActionLogCache actionLogCache)
		{
			switch (actionLogCache.type) {
				case ActionType.ATTACK:
				case ActionType.GET_DAMAGED:
					actionLogCache.mergedCard.SetGettingEffect("Damaged", -actionLogCache.valueAffected);
					break;
				case ActionType.GETTING_COIN:
					actionLogCache.mergedCard.SetGettingEffect("GettingCoin", actionLogCache.valueAffected);
					break;
				case ActionType.USE_POTION:
					actionLogCache.mergedCard.SetGettingEffect("Damaged", actionLogCache.valueAffected);
					break;
				case ActionType.GETTING_ROOT:
					actionLogCache.mergedCard.SetGettingEffect("GettingCoin", actionLogCache.valueAffected);
					break;
			}
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
			ceilings = new GameObject("Ceilings");
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
