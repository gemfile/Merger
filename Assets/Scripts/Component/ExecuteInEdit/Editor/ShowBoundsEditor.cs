using UnityEngine;
using UnityEditor;

namespace com.Gemfile.Merger
{
	[CustomEditor(typeof(ShowBounds))]
	internal class ShowBoundsEditor : Editor
	{
		void OnSceneGUI( )
		{
			ShowBounds t = target as ShowBounds;

			if(t == null || t.edges == null)
				return;

			Handles.color = Color.magenta;

			Vector3 endPoint = t.edges[0];
			t.edges.ForEach(edge => {
				Handles.DrawLine(endPoint, edge);
				endPoint = edge;
			});
		}
	}
}
