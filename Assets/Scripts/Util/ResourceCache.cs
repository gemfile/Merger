using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Util {
	public static class ResourceCache {
		static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

		static public void Load(string path) {
			object[] resources = Resources.LoadAll(path, typeof(GameObject));
			for(int i = 0; i < resources.Length; i++) {
				var resource = resources[i] as GameObject;
				cache[resource.name] = resource;
			}
		}

		static public GameObject Get(string key) {
			return cache[key];
		}

		static public GameObject Instantiate(string key) {
			var instance = (GameObject)(Object.Instantiate( cache[key] ));
			instance.name = key;
			return instance;
		}
	}
}