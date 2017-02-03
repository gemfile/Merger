namespace com.Gemfile.Merger 
{
	public class Monster: CardBase 
	{
		public Monster (CardData cardData): base(cardData) 
		{
		}

		public PlayerData Merge(Player player) 
		{
			return player.Merge(this, false);
		}
	}
}