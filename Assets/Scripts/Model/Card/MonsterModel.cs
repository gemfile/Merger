namespace com.Gemfile.Merger
{
    [System.Serializable]
    public class MonsterModel: CardModel, IMerger
    {
        public MonsterModel(CardData cardData): base(cardData)
		{
		}

        public PlayerInfo Merge(ICardModel player, Position playerPosition, Position cardPosition, bool canUseWeapon)
        {
            return (player as IMerger).Merge(this, playerPosition, cardPosition, false);
        }

        public bool CantMerge(ICardModel target)
        {
            return !(target is IPlayerModel);
        }
    }
}