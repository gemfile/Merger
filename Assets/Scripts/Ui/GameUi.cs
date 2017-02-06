using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.Gemfile.Merger 
{
	public class GameUI : MonoBehaviour
    {
		[SerializeField]
		private GameView gameView;
        [SerializeField]
        private RectTransform handContainer;
    
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

        internal void AddCardAcquired(Sprite sprite)
        {
            var uiCardMask = ResourceCache.Instantiate("UICardMask");
            uiCardMask.transform.SetParent(handContainer, false);
            uiCardMask.transform.Find("UICard").GetComponent<Image>().sprite = sprite;
        }
    }
}