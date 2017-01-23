namespace com.Gemfile.Merger 
{
	public class CardBase: ICard 
	{
		readonly CardData cardData;

		public CardBase(CardData cardData) 
		{
			this.cardData = cardData;
		}

		public int GetValue() 
		{
			return cardData.value;
		}

		public string GetResourceName() 
		{
			return cardData.resourceName;
		}

		public string GetCardName() 
		{
			return cardData.cardName;
		}
	}
}

