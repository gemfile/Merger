using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace com.Gemfile.Merger
{
	public interface INavigationView: IBaseView
	{
		void Show(int sourceIndex, List<Position> wheresCanMerge, Dictionary<int, GameObject> fields);
	}
	
	public class NavigationView: BaseView, INavigationView
	{
		[SerializeField]
		Texture2D lineTexture;
		[SerializeField]
		Texture2D frontTexture;
		
		public override void Init()
		{
			VectorLine.SetEndCap ("Arrow", EndCap.Front, -1, lineTexture, frontTexture);
		}

		public void Show(int sourceIndex, List<Position> wheresCanMerge, Dictionary<int, GameObject> fields)
		{
			wheresCanMerge.ForEach(whereCanMerge => {
				var linePoints = new List<Vector2>() {
					Camera.main.WorldToScreenPoint(fields[whereCanMerge.index].transform.position),
					Camera.main.WorldToScreenPoint(fields[sourceIndex].transform.position),
				};
				var myLine = new VectorLine("Line", linePoints, 90.0f, LineType.Continuous);
				myLine.rectTransform.SetParent(transform);
				myLine.endCap = "Arrow";

				var myColors = new List<Color32>() {
					Color.red,
				}; 
				myLine.smoothColor = true;
				myLine.SetColors (myColors);

				myLine.Draw();
			});
		}

	}
}
