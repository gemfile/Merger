using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace com.Gemfile.Merger 
{
    class UIHandCard
    {
        internal ICard card;
        internal GameObject gameObject;
    }

	public class GameUI : MonoBehaviour
    {
		[SerializeField]
		private GameView gameView;
        [SerializeField]
        private RectTransform handContainer;
        private List<UIHandCard> handCards;
        [SerializeField]
        private Text deckCountText;
        [SerializeField]
        private Text coinText;

        public GameUI()
        {
            handCards = new List<UIHandCard>();
        }
    
        internal void Prepare()
        {
            StartCoroutine(StartPrepare());
        }

        IEnumerator StartPrepare()
        {
            yield return null;
            Bounds backgroundBounds = gameView.GetBackgroundSize();
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

        internal void AddCardAcquired(Sprite sprite, Vector3 size, ICard merged, List<ICard> equipments)
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

        internal void UpdateCoin(int coin)
        {
            coinText.text = coin.ToString();
        }

        internal void UpdateDeckCount(int deckCount)
        {
            deckCountText.text = deckCount.ToString();
        }
    }
}