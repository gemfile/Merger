using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Card {
	public class Potion: Card {
		public Potion (int value): base(value) {
		}

		public override string GetType() {
			return "Potion";
		}
    }
}