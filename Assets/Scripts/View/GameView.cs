using System.Collections;
using UnityEngine;

namespace com.Gemfile.Merger
{
	interface IGameView
	{
		void Init();
		IFieldView Field { get; }
		ISwipeInput Swipe { get; }
		IUIView UI { get; }
		void RequestCoroutine(IEnumerator coroutine);
	}

	class GameView: MonoBehaviour, IGameView
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
			fieldView = transform.Find("FieldView").GetComponent<FieldView>() as IFieldView;
			uiView = transform.Find("UIView").GetComponent<UIView>() as IUIView;

			fieldView.Init(this);
			uiView.Init(this);
		}

		public void RequestCoroutine(IEnumerator coroutine)
		{
			StartCoroutine(coroutine);
		}
	}
}
