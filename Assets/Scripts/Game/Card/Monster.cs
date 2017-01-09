using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Card {
	public class Monster: Card {
		public Monster (int value): base(value) {
		}

		public override string GetType() {
			return "Monster";
		}
    }
}