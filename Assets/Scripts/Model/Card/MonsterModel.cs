using System.Collections.Generic;

namespace com.Gemfile.Merger
{
    [System.Serializable]
    public class MonsterModel: CardModel, IMerger
    {
        public int Def { get; set; }
		public int Coin { get; set; }
		public int Hp { get; set; }
		public int Atk { get; }
		public int LimitOfHp { get; }
        public List<ICardModel> Equipments { 
            get { return new List<ICardModel>(equipments); }
            set { equipments = new List<ICardModel>(value); }
        }
        List<ICardModel> equipments;
        public ICardModel WeaponEquiped { get; set; }

        public MonsterModel(CardData cardData): base(cardData)
		{
            Atk = cardData.value;
            LimitOfHp = Hp = cardData.value;
            equipments = new List<ICardModel>();
		}
    }
}