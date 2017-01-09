using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Card {
	public class Magic: Card {
		public Magic (int value): base(value) {
		}

		public override string GetType() {
			return "Magic";
		}
    }
}