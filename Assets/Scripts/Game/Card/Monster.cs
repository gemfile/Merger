namespace com.Gemfile.Merger 
{
	public class Monster: CardBase 
	{
		public Monster (CardData cardData): base(cardData) 
		{
		}

		internal PlayerData Merge(Player player, Position playerPosition, Position cardPosition, bool canUseWeapon = false) 
		{
			return player.Merge(this, playerPosition, cardPosition, canUseWeapon);
		}
	}
}