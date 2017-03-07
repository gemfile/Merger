namespace com.Gemfile.Merger
{
	public interface ICardModel
	{
		CardData Data {
			get;
		}
	}
	
	public interface IMerger
	{
		int Def { get; set; }
		int Coin { get; set; }
		int Hp { get; set; }
		ICardModel Weapon { get; set; }
		int Atk { get; }
		int LimitOfHp { get; }
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