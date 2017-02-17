namespace com.Gemfile.Merger
{
	public interface ICardModel
	{
		CardData Data {
			get;
		}
		bool CantMerge(ICardModel target);
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

		public virtual bool CantMerge(ICardModel target)
		{
			return false;
		}
    }

	class EmptyModel: CardModel
	{
		public EmptyModel(): base(null) {}
	}
}