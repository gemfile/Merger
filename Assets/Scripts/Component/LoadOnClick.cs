using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Gemfile.Merger
{
	[RequireComponent(typeof(Canvas))]
	public class LoadOnClick: MonoBehaviour 
	{
		public void LoadScene(string sceneName)
		{
			StartCoroutine(StartShowingCover(sceneName));
		}

		IEnumerator StartShowingCover(string sceneName)
		{
			var loadingCoverObject = ResourceCache.Instantiate("LoadingCover");
			loadingCoverObject.transform.SetParent(transform, false);
			var loadingCover = loadingCoverObject.GetComponent<LoadingCover>();
			yield return loadingCover.Show(sceneName);

			AsyncOperation asyncOpeartion = SceneManager.LoadSceneAsync(sceneName);
			while (!asyncOpeartion.isDone)
			{
				yield return loadingCover.Progress(asyncOpeartion.progress);
			}

			yield return loadingCover.Hide();
		}
	}
}
