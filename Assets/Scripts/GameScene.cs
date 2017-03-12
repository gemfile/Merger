using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Gemfile.Merger
{
	public class GameScene: MonoBehaviour 
	{
		GameScene()
		{
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
		}
		
		IGameController<GameModel, IGameView> gameController;
		
		void Start()
		{
			ResourceCache.LoadAll(SceneManager.GetActiveScene().name);
			
			gameController = new GameController<GameModel, IGameView>();
			gameController.Init(transform.GetComponentInChildren<GameView>());
		}

		void Destroy()
		{
			gameController.Clear();
		}
	}
}
