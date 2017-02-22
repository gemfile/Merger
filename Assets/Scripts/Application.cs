using UnityEngine;

namespace com.Gemfile.Merger
{
	public class Application: MonoBehaviour 
	{
		Application()
		{
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
		}
		
		GameController<GameModel, GameView> gameController;
		
		void Start()
		{
			ResourceCache.Load("");
			
			gameController = new GameController<GameModel, GameView>();
			gameController.Init(transform.GetComponentInChildren<GameView>());
		}

		void Destroy()
		{
			gameController.Clear();
		}
	}
}
