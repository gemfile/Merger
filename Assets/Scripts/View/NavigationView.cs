using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace com.Gemfile.Merger
{
	public interface INavigationView: IView
	{
		void Show(int sourceIndex, List<Position> wheresCanMerge, Dictionary<int, GameObject> fields);
	}
	
	public class NavigationView: MonoBehaviour, INavigationView
	{
		public void Init()
		{
		}

		public void Show(int sourceIndex, List<Position> wheresCanMerge, Dictionary<int, GameObject> fields)
		{
			Debug.Log("ShowGuidelines");
			// VectorLine.SetEndCap ("Arrow", EndCap.Front, lineTex, frontTex, backTex);
			var linePoints = new List<Vector2>();
			var myLine = new VectorLine("Line", linePoints, 2.0f, LineType.Discrete);
			wheresCanMerge.ForEach(whereCanMerge => {
				Debug.Log("whereCanMerge : " + fields[sourceIndex].transform.localPosition + ", " + fields[whereCanMerge.index].transform.localPosition);
				linePoints.Add(Camera.main.WorldToScreenPoint(fields[sourceIndex].transform.position)); 
				linePoints.Add(Camera.main.WorldToScreenPoint(fields[whereCanMerge.index].transform.position)); 
			});
			myLine.endCap = "Arrow";
			myLine.Draw();
		}

	}
}
