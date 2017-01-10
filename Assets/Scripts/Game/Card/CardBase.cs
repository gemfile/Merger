using System;

namespace Scripts.Game.Card {
	public class CardBase: ICard {
		readonly int value;
		readonly string resourceName;

		public CardBase(int value, string resourceName) {
			this.value = value;
			this.resourceName = resourceName;
		}

		public int GetValue() {
			return value;
		}

		public string GetResourceName() {
			return resourceName;
		}

		public virtual string GetType() {
			return "Card";
		}
	}
}

