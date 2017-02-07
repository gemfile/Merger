namespace com.Gemfile.Merger 
{
	public class Monster: CardBase 
	{
		public Monster (CardData cardData): base(cardData) 
		{
		}

		internal PlayerData Merge(Player player) 
		{
			return player.Merge(this, false);
		}
	}
}