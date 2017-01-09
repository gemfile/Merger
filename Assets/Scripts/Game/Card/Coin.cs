using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Card {
	public class Coin: CardBase {
		public Coin (int value): base(value) {
			
		}

		public override string GetType() {
			return "Coin";
		}
    }
}