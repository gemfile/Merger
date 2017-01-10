namespace Scripts.Game.Card {
	public class CardBase: ICard {
		readonly int value;
		readonly string resourceName;
		readonly string cardName;

		public CardBase(int value, string resourceName, string cardName) {
			this.value = value;
			this.resourceName = resourceName;
			this.cardName = cardName;
		}

		public int GetValue() {
			return value;
		}

		public string GetResourceName() {
			return resourceName;
		}

		public string GetCardName() {
			return cardName;
		}
	}
}

