using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.Gemfile.Merger
{
    public interface IUIView
    {
        void UpdateCoin(int coin);
        void UpdateDeckCount(int deckCount);
        void AddCardAcquired(SpriteCapturedInfo spriteCapturedInfo);
        void Align(Bounds backgroundBounds);
        void SuggestRetry(UnityAction callback);
        void ClearCards();
    }

    class UIHandCard
    {
        internal ICardModel card;
        internal GameObject gameObject;
    }

    public class UIView: BaseView, IUIView
    {
        [SerializeField]
        Text coinText;
        [SerializeField]
        Text deckCountText;
        [SerializeField]
        RectTransform handContainer;
        List<UIHandCard> handCards;
        [SerializeField]
        ModalPanel modalPanel;

        public UIView()
        {
            handCards = new List<UIHandCard>();
        }

        public void Align(Bounds backgroundBounds)
        {
            StartCoroutine(StartAligning(backgroundBounds));
        }

        IEnumerator StartAligning(Bounds backgroundBounds)
        {
            yield return null;
            Vector3 leftBottom = Camera.main.WorldToScreenPoint(backgroundBounds.min);
            Vector3 rightTop = Camera.main.WorldToScreenPoint(backgroundBounds.max);
            var backgroundSize = rightTop - leftBottom;

            var gameWidth = Mathf.Min(backgroundSize.x, Screen.width);
            var bottomHeight = Screen.height - backgroundSize.y;
            transform.Find("Bottom").GetComponent<RectTransform>().sizeDelta = new Vector2(
                gameWidth, 
                bottomHeight
            );

            transform.Find("Top").GetComponent<RectTransform>().sizeDelta = new Vector2(
                gameWidth, 
                bottomHeight/4
            );
        }

        public void AddCardAcquired(SpriteCapturedInfo spriteCapturedInfo)
        {
            Sprite capturedSprite = spriteCapturedInfo.capturedSprite;
            Vector3 size = spriteCapturedInfo.size;
            ICardModel mergedModel = spriteCapturedInfo.mergedModel; 
            List<ICardModel> equipments = spriteCapturedInfo.equipments;

            var delay = 0.0f;
            var duration = 0.4f;
            if (equipments.Exists(equipment => equipment == mergedModel))
            {
                var uiCardMask = ResourceCache.Instantiate("UICardMask");
                uiCardMask.transform.SetParent(handContainer, false);
                uiCardMask.transform.SetAsFirstSibling();
                uiCardMask.transform.Find("UICard").GetComponent<Image>().sprite = capturedSprite;

                var uiCardTransform = uiCardMask.GetComponent<RectTransform>();
                uiCardTransform.sizeDelta = size;
                uiCardTransform.anchoredPosition = new Vector2(
                    (handContainer.childCount-1) * size.x/2, 
                    uiCardTransform.anchoredPosition.y
                );
                uiCardTransform.DOAnchorPosY(uiCardTransform.anchoredPosition.y - size.y, duration)
                    .From()
                    .SetEase(Ease.OutCubic)
                    .SetDelay(delay);
                handCards.Add(new UIHandCard(){ gameObject = uiCardMask, card = mergedModel });
            }

            handCards.RemoveAll(handCard => {
                if (!equipments.Exists(equipment => equipment == handCard.card))
                {
                    var sequence = DOTween.Sequence();
                    sequence.SetDelay(delay);
                    sequence.Append(handCard.gameObject.transform.GetChild(0).GetComponent<Image>().DOFade(0, duration));
                    sequence.Insert(delay, handCard.gameObject.GetComponent<RectTransform>().DOAnchorPosY(-40, duration));
                    sequence.AppendCallback(() => Destroy(handCard.gameObject));
                    return true;
                }
                return false;
            });

            var count = 0;
            handCards.ForEach(handCard => {
                handCard.gameObject.transform.DOLocalMoveX(count * size.x/2, duration)
                    .SetEase(Ease.OutCubic)
                    .SetDelay(delay);
                count++;
            });
        }

        public void ClearCards()
        {
            handCards.RemoveAll(handCard => {
                handCard.gameObject.SetActive(false);
                handCard.gameObject.transform.SetParent(null);
                Destroy(handCard.gameObject);
                return true;
            });
        }
        
        public void UpdateCoin(int coin)
        {
            coinText.text = coin.ToString();
        }
        
        public void UpdateDeckCount(int deckCount)
        {
            deckCountText.text = deckCount.ToString();
        }

        public void SuggestRetry(UnityAction callback)
        {
            modalPanel.Choice(callback);
        }
    }
}
