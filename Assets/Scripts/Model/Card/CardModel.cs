namespace com.Gemfile.Merger
{
	public interface ICardModel
	{
		CardData Data {
			get;
		}
	}
	
    public class CardModel: ICardModel
    {
		public CardModel(CardData cardData) 
		{
			this.data = cardData;
		}

		public CardData Data {
			get { return data; }
		}
		readonly CardData data;
    }

	class EmptyModel: CardModel
	{
		public EmptyModel(): base(null) {}
	}
}