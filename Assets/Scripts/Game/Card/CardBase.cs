using System;

namespace Scripts.Game.Card {
	public class CardBase: ICard {
		readonly int value;

		public CardBase(int value) {
			this.value = value;
		}

		public int GetValue() {
			return value;
		}

		public virtual string GetType()
		{
			return "Card";
		}
	}
}

