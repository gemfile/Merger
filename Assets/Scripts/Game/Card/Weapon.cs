using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Card {
	public class Weapon: Card {
		public Weapon (int value): base(value) {
		}

		public override string GetType() {
			return "Weapon";
		}
    }
}