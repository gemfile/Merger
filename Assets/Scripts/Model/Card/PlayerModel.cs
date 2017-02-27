namespace com.Gemfile.Merger
{
	public interface IPlayerModel: IMerger, ICardModel
	{
	}

    [System.Serializable]
    public class PlayerModel: CardModel, IPlayerModel
    {
		public int Hp {
			get { return hp; }
			set { hp = value; }
		}
		int hp;
		public int LimitOfHp {
			get { return limitOfHp; }
		}
		readonly int limitOfHp;
		public int Def {
			get { return def; }
			set { def = value; }
		}
		int def;
		public int Coin {
			get { return coin; } 
			set { coin = value; }
		}
		int coin;
		public ICardModel Weapon {
			get { return weapon; }
			set { weapon = value; }
		}
		ICardModel weapon;
		public int Atk {
			get { return weapon != null ? weapon.Data.value : 0; }
		}

        public PlayerModel(CardData cardData): base(cardData)
		{
			hp = cardData.value;
			limitOfHp = hp;
			def = 0;
			coin = 0;
			weapon = null;
		}
    }
}