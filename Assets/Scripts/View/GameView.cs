using System.Collections;

namespace com.Gemfile.Merger
{
	public interface IGameView
	{
		void RequestCoroutine(IEnumerator coroutine);
		IFieldView Field { get; }
		ISwipeInput Swipe { get; }
		IUIView UI { get; }
		INavigationView Navigation { get; }
	}

	public class GameView: BaseView, IGameView
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

		public override void Init()
		{
			swipe = gameObject.GetComponent<SwipeInput>();
			fieldView = transform.GetComponentInChildren<FieldView>();
			uiView = transform.GetComponentInChildren<UIView>();
			navigationView = transform.GetComponentInChildren<NavigationView>();
			navigationView.Init();
		}

		public void RequestCoroutine(IEnumerator coroutine)
		{
			StartCoroutine(coroutine);
		}
	}
}
