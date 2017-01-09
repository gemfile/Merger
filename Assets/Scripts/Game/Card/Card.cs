using System;

namespace Scripts.Game.Card {
	public class Card: ICard {
		readonly int value;

		public Card(int value) {
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

