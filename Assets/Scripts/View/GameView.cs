using System.Collections;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IGameView
	{
		void Init();
		IFieldView Field { get; }
		ISwipeInput Swipe { get; }
		IUIView UI { get; }
		void RequestCoroutine(IEnumerator coroutine);
	}

	public class GameView: MonoBehaviour, IGameView
	{
		public ISwipeInput Swipe {
			get { return swipe; }
		}
		ISwipeInput swipe;
		public IUIView UI { 
			get { return uiView; }
		}
		IUIView uiView;

		public IFieldView Field {
			get { return fieldView; }
		}
		IFieldView fieldView;

		public void Init()
		{
			swipe = gameObject.AddComponent<SwipeInput>();
			fieldView = transform.GetComponentInChildren<FieldView>();
			uiView = transform.GetComponentInChildren<UIView>();

			fieldView.Init(this);
			uiView.Init(this);
		}

		public void RequestCoroutine(IEnumerator coroutine)
		{
			StartCoroutine(coroutine);
		}
	}
}
