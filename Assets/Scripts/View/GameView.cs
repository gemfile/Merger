using System.Collections;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IView
	{
		void Init();
	}

	public interface IGameView
	{
		void Init();
		void RequestCoroutine(IEnumerator coroutine);
		IFieldView Field { get; }
		ISwipeInput Swipe { get; }
		IUIView UI { get; }
		INavigationView Navigation { get; }
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

		public INavigationView Navigation {
			get { return navigationView; }
		}
		INavigationView navigationView;

		public void Init()
		{
			swipe = gameObject.GetComponent<SwipeInput>();
			fieldView = transform.GetComponentInChildren<FieldView>();
			uiView = transform.GetComponentInChildren<UIView>();
			navigationView = transform.GetComponentInChildren<NavigationView>();

			foreach (IView view in GetComponentsInChildren<IView>()) 
			{
				view.Init();
			}
			uiView.Align(Field.BackgroundBounds);
		}

		public void RequestCoroutine(IEnumerator coroutine)
		{
			StartCoroutine(coroutine);
		}
	}
}
