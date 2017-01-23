using System;
using UnityEngine;

namespace com.Gemfile.Merger 
{
	public class Player: CardBase 
	{
		int hp;
		readonly int limitOfHp;
		int atk;
		int def;
		int coin;

		public int Hp 
		{
			get { return hp; }
		}

		public int Atk
		{
			get { return atk; }
		}

		public Player(CardData cardData): base(cardData) 
		{
			hp = cardData.value;
			limitOfHp = hp;
			atk = 0;
			def = 0;
			coin = 0;
		}

		public PlayerData Merge(ICard card) 
		{
			Debug.Log("=== Merging ===");
			switch (card.GetType().Name) 
			{
				case "Coin":
					coin += card.GetValue();
					break;

				case "Potion":
					hp = Math.Min(limitOfHp, hp + card.GetValue());
					break;

				case "Monster":
					int monsterValue = card.GetValue();
					monsterValue = Math.Max(0, monsterValue - atk);
					atk = 0;

					if (monsterValue > 0) {
						int initialDef = def;
						def = Math.Max(0, def - monsterValue);
						monsterValue -= initialDef;
					}

					if (monsterValue > 0) {
						hp -= monsterValue;
					}
					break;

				case "Magic":
					break;

				case "Weapon":
					atk = card.GetValue();
					break;
			}

			Debug.Log("Stats after merging : hp " + hp  + ", atk " + atk + ", def " + def + ", coin " + coin);
			Debug.Log("===============");
			return new PlayerData { hp = hp, coin = coin, atk = atk, def = def };
		}
	}
}

