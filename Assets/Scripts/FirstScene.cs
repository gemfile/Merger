using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Gemfile.Merger
{
	public class FirstScene: MonoBehaviour 
	{
		FirstScene()
		{
#if DIABLE_LOG
			Debug.logger.logEnabled=false;
#endif
		}
		
		void Start()
		{
			ResourceCache.LoadAll(SceneManager.GetActiveScene().name);
		}

		void Destroy()
		{
		}
	}
}
