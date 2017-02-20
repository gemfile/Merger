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
		
		void Start()
		{
			ResourceCache.Load("");
			
			var gameController = new GameController<GameModel, GameView>();
			gameController.Init(transform.GetComponentInChildren<GameView>());
		}
	}
}
