using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

namespace com.Gemfile.Merger 
{
    public static class ExtensionMethods 
	{
		public static IList<T> Shuffle<T>(this IList<T> collection) 
		{  
			for (int i = 0; i < collection.Count; i++)
			{
				T temp = collection[i];
				int randomIndex = UnityEngine.Random.Range(i, collection.Count);
				collection[i] = collection[randomIndex];
				collection[randomIndex] = temp;
            }
			return collection;
        }

		public static Bounds GetBounds(this GameObject gameObject) 
		{
			var bounds = new Bounds (gameObject.transform.position, Vector3.zero);
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>()) 
			{
				bounds.Encapsulate(renderer.bounds);
			}
			return bounds;
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> mapFunction) 
		{
			foreach (var item in enumerable) 
			{
				mapFunction(item);
			}
		}

		public static Tween FadeIn(this TextMesh text, float duration)
		{
			text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
			return DOTween.To(
				() => text.color,
				x => text.color = x,
				new Color(text.color.r, text.color.g, text.color.b, 1),
				duration
			).SetEase(Ease.InCubic);
		}
    }   
}