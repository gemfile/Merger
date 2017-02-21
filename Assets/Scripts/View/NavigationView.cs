using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace com.Gemfile.Merger
{
	public interface INavigationView: IBaseView
	{
		void Show(List<NavigationInfo> navigationInfos, Dictionary<int, GameObject> fields);
		void Hide();
	}
	
	public class NavigationView: BaseView, INavigationView
	{
		[SerializeField]
		Texture2D lineTexture;
		[SerializeField]
		Texture2D frontTexture;
		
		List<VectorLine> vectorLines;

		public NavigationView()
		{
			vectorLines = new List<VectorLine>();
		}
		
		public override void Init()
		{
			VectorLine.SetEndCap ("Arrow", EndCap.Front, -1, lineTexture, frontTexture);
		}

		public void Show(List<NavigationInfo> navigationInfos, Dictionary<int, GameObject> fields)
		{
			navigationInfos.ForEach(navigationInfo => {
				var sourceIndex = navigationInfo.sourceIndex;
				var wheresCanMerge = navigationInfo.wheresCanMerge;

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
					vectorLines.Add(myLine);
				});
			});
		}

		public void Hide()
		{
			VectorLine.Destroy(vectorLines);
			vectorLines.Clear();
		}
	}
}
