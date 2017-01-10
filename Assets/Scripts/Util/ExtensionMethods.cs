using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Util
{
    public static class ExtensionMethods
    {
        public static void Shuffle<T>(this IList<T> list)  
        {  
            for (int i = 0; i < list.Count; i++) {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

		public static Bounds GetBounds(this GameObject gameObject) 
		{
			var bounds = new Bounds (gameObject.transform.position, Vector3.zero);
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer> ()) 
			{
				bounds.Encapsulate(renderer.bounds);
			}
			return bounds;
		}
    }   
}