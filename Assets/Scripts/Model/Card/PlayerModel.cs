using System.Collections.Generic;
using System.Linq;

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
		public List<ICardModel> Equipments {
			get { return new List<ICardModel>(equipments); }
			set { equipments = new List<ICardModel>(value); }
		}
		List<ICardModel> equipments;
		public int Atk {
			get {
				var weapon = WeaponEquiped;
				return weapon != null ? weapon.Data.value : 0; 
			}
		}
		public ICardModel WeaponEquiped {
			get {
				return equipments.FirstOrDefault(equipment => equipment is WeaponModel);
			}
		}

        public PlayerModel(CardData cardData): base(cardData)
		{
			hp = cardData.value;
			limitOfHp = hp;
			def = 0;
			coin = 0;
			equipments = new List<ICardModel>();
		}
    }
}