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
		
		GameController<GameModel, GameView> gameController;
		
		void Start()
		{
			ResourceCache.LoadAll(SceneManager.GetActiveScene().name);
			
			gameController = new GameController<GameModel, GameView>();
			gameController.Init(transform.GetComponentInChildren<GameView>());
		}

		void Destroy()
		{
			gameController.Clear();
		}
	}
}
