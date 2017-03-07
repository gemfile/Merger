namespace com.Gemfile.Merger
{
    [System.Serializable]
    public class MonsterModel: CardModel, IMerger
    {
        public int Def { get; set; }
		public int Coin { get; set; }
		public int Hp { get; set; }
		public ICardModel Weapon { get; set; }
		public int Atk { get; }
		public int LimitOfHp { get; }

        public MonsterModel(CardData cardData): base(cardData)
		{
            Atk = cardData.value;
            LimitOfHp = Hp = cardData.value;
		}
    }
}