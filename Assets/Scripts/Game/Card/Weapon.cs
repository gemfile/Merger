using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Card {
	public class Weapon: CardBase {
		public Weapon (int value, string resourceName): base(value, resourceName) {
		}

		public override string GetType() {
			return "Weapon";
		}
    }
}