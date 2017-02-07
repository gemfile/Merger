using System;
using UnityEngine;
using System.Collections.Generic;

namespace com.Gemfile.Merger 
{
	internal enum ActionType
	{
		GET = 0, USE, ATTACK, GET_DAMAGED
	}
	
	internal class ActionLog
	{
		internal ActionType type;
		internal ICard card;
		internal int value;
	}

	public class Player: CardBase 
	{
		int hp;
		readonly int limitOfHp;
		int def;
		int coin;

		ICard weapon;

		internal int Hp 
		{
			get { return hp; }
		}
		internal int Atk
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

		internal PlayerData Merge(ICard card, bool useWeapon = true) 
		{
			Debug.Log("=== Merging ===");
			var actionLogs = new List<ActionLog>();
			switch (card.GetType().Name) 
			{
				case "Coin":
					actionLogs.Add(new ActionLog(){ type=ActionType.GET, card=card, value=card.GetValue() });
					coin += card.GetValue();
					break;

				case "Potion":
					var nextHp = Math.Min(limitOfHp, hp + card.GetValue());
					actionLogs.Add(new ActionLog(){ type=ActionType.USE, card=card, value=nextHp-hp });
					hp = nextHp;
					break;

				case "Monster":
					int monsterValue = card.GetValue();
					if (useWeapon) {
						actionLogs.Add(new ActionLog(){ type=ActionType.ATTACK, card=weapon, value=weapon.GetValue() });
						monsterValue = Math.Max(0, monsterValue - weapon.GetValue());
						weapon = null;
					}
					if (monsterValue > 0) {
						int initialDef = def;
						def = Math.Max(0, def - monsterValue);
						monsterValue -= initialDef;
					}
					if (monsterValue > 0) {
						actionLogs.Add(new ActionLog(){ type=ActionType.GET_DAMAGED, card=card, value=monsterValue });
						hp -= monsterValue;
					}
					break;

				case "Magic":
					break;

				case "Weapon":
					weapon = card;
					actionLogs.Add(new ActionLog(){ type=ActionType.GET, card=card, value=card.GetValue() });
					break;
			}

			Debug.Log("Stats after merging : hp " + hp  + ", atk " + Atk + ", def " + def + ", coin " + coin);
			Debug.Log("===============");
			return new PlayerData { 
				hp = hp, 
				coin = coin, 
				atk = Atk, 
				def = def, 
				merged = card, 
				equipments = new List<ICard>{ weapon },
				actionLogs = actionLogs
			};
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

