using System;
using UnityEngine;
using Scripts.Game.Card;

namespace Scripts.Game
{
	public class Player
	{
		int hp;
		readonly int limitOfHp;
		int atk = 0;
		int def = 0;
		int coin = 0;

		public int Hp {
			get { return hp; }
		}

		public Player(int hp)
		{
			this.hp = hp;
			this.limitOfHp = hp;
		}

		public void Merge(ICard card)
		{
			Debug.Log("=== Merging ===");
			switch (card.GetType().Name) {
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
		}
	}
}

