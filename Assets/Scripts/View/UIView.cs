using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace com.Gemfile.Merger
{
    public interface IUIView
    {
        void UpdateCoin(int coin);
        void UpdateDeckCount(int deckCount);
        void AddCardAcquired(Sprite sprite, Vector3 size, ICardModel merged, List<ICardModel> equipments);
        void Init(IGameView gameView);
    }

    class UIHandCard
    {
        internal ICardModel card;
        internal GameObject gameObject;
    }

    public class UIView: MonoBehaviour, IUIView
    {
        [SerializeField]
        Text coinText;
        [SerializeField]
        Text deckCountText;
        [SerializeField]
        RectTransform handContainer;
        List<UIHandCard> handCards;
        IGameView gameView;

        public UIView()
        {
            handCards = new List<UIHandCard>();
        }

        public void Init(IGameView gameView)
        {
            this.gameView = gameView;

            StartCoroutine(StartAligning());
        }

        IEnumerator StartAligning()
        {
            yield return null;
            Bounds backgroundBounds = gameView.Field.BackgroundBounds;
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

        public void AddCardAcquired(Sprite sprite, Vector3 size, ICardModel merged, List<ICardModel> equipments)
        {
            if (equipments.Exists(equipment => equipment == merged))
            {
                var uiCardMask = ResourceCache.Instantiate("UICardMask");
                uiCardMask.transform.SetParent(handContainer, false);
                uiCardMask.transform.Find("UICard").GetComponent<Image>().sprite = sprite;

                var uiCardTransform = uiCardMask.GetComponent<RectTransform>();
                uiCardTransform.sizeDelta = size;
                uiCardTransform.DOAnchorPosY(uiCardTransform.anchoredPosition.y - size.y, .4f).From().SetEase(Ease.OutCubic).SetDelay(0.8f);
                handCards.Add(new UIHandCard(){ gameObject = uiCardMask, card = merged });
            }

            handCards.RemoveAll(handCard => {
                if (!equipments.Exists(equipment => equipment == handCard.card))
                {
                    var delay = 0.8f;
                    var sequence = DOTween.Sequence();
                    sequence.SetDelay(delay);
                    sequence.Append(handCard.gameObject.transform.GetChild(0).GetComponent<Image>().DOFade(0, .4f));
                    sequence.Insert(delay, handCard.gameObject.GetComponent<RectTransform>().DOAnchorPosY(-40, .4f));
                    sequence.AppendCallback(() => Destroy(handCard.gameObject));
                    return true;
                }
                return false;
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
    }
}
