using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Scripts.Util
//{
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
    }   
//}