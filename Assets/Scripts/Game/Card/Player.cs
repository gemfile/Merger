using System;
using UnityEngine;
using System.Collections.Generic;

namespace com.Gemfile.Merger 
{
	public class Player: CardBase 
	{
		int hp;
		readonly int limitOfHp;
		int def;
		int coin;

		ICard weapon;

		public int Hp 
		{
			get { return hp; }
		}
		public int Atk
		{
			get { return weapon != null ? weapon.GetValue() : 0; }
		}
		
		public Player(CardData cardData): base(cardData) 
		{
			hp = cardData.value;
			limitOfHp = hp;
			def = 0;
			coin = 0;
			weapon = null;
		}

		public PlayerData Merge(ICard card, bool useWeapon = true) 
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
					if (useWeapon) {
						monsterValue = Math.Max(0, monsterValue - weapon.GetValue());
						weapon = null;
					}
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
					weapon = card;
					break;
			}

			Debug.Log("Stats after merging : hp " + hp  + ", atk " + Atk + ", def " + def + ", coin " + coin);
			Debug.Log("===============");
			return new PlayerData { hp = hp, coin = coin, atk = Atk, def = def, merged = card, equipments = new List<ICard>{ weapon } };
		}

        internal bool CantMerge(ICard target)
        {
            return (
				target is Empty ||
				(target is Monster && Atk <= 0)
			);
        }
    }
}

