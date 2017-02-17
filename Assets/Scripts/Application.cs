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
			
			var gameModel = new GameModel();
			var gameView = transform.GetComponentInChildren<GameView>();
			var gameController = new GameController(gameModel, gameView);

			gameModel.Init();
			gameView.Init();
			gameController.Init();
		}
	}
}
