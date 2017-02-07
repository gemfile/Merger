using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
			var backgroundSize = Camera.main.WorldToScreenPoint(new Vector2(backgroundBounds.size.x, backgroundBounds.size.y));
            transform.GetChild(0).Find("EmptyArea").GetComponent<LayoutElement>().minHeight = backgroundSize.y/2;
            transform.GetChild(0).Find("Bottom").GetChild(0).GetComponent<LayoutElement>().preferredWidth = backgroundSize.x;
        }

        internal void AddCardAcquired(Sprite sprite, ICard merged, List<ICard> equipments)
        {
            if (equipments.Exists(equipment => equipment == merged))
            {
                var uiCardMask = ResourceCache.Instantiate("UICardMask");
                uiCardMask.transform.SetParent(handContainer, false);
                uiCardMask.transform.Find("UICard").GetComponent<Image>().sprite = sprite;
                uiCardMask.GetComponent<RectTransform>().DOAnchorPosY(-382, .4f).From().SetEase(Ease.OutCubic).SetDelay(0.8f);
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
    }
}