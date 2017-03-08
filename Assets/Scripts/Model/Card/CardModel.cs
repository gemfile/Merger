using System.Collections.Generic;

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
		int Atk { get; }
		int LimitOfHp { get; }
		List<ICardModel> Equipments { get; set; }
		ICardModel WeaponEquiped { get; }
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