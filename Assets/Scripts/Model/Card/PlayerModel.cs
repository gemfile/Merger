using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.Gemfile.Merger
{
	interface IPlayerModel
	{
		int Hp { get; }
		int Atk { get; }
	}

	interface IMerger
	{
		PlayerInfo Merge(
			ICardModel card, 
			Position playerPosition, 
			Position cardPosition, 
			bool canUseWeapon = true
		);

		bool CantMerge(ICardModel target);
	}

	public class PlayerInfo 
	{
		internal int hp;
		internal int atk;
		internal int def;
		internal int coin;
		internal ICardModel merged;
		internal List<ICardModel> equipments;
		internal List<ActionLog> actionLogs; 
	}

	public enum PhaseOfGame
	{
		FILL = 0, PLAY, WAIT
	}

	enum ActionType
	{
		GET = 0, USE_POTION, ATTACK, GET_DAMAGED, GET_COIN
	}

	class ActionLog
	{
		internal ActionType type;
		internal Position sourcePosition;
		internal Position targetPosition;
		internal int valueAffected;
	}

    [System.Serializable]
    public class PlayerModel: CardModel, IPlayerModel, IMerger
    {
		int hp;
		readonly int limitOfHp;
		int def;
		int coin;
		ICardModel weapon;

		public int Hp {
			get { return hp; }
		}
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

		public bool CantMerge(ICardModel target)
        {
            return (
				target is EmptyModel ||
				(target is MonsterModel && Atk <= 0)
			);
        }

		public PlayerInfo Merge(ICardModel card, Position playerPosition, Position cardPosition, bool canUseWeapon)
		{
			Debug.Log("=== Merging ===");
			var actionLogs = new List<ActionLog>();
			switch (card.Data.type)
			{
				case "Coin":
					actionLogs.Add(new ActionLog() { 
						type = ActionType.GET_COIN, 
						sourcePosition = playerPosition, 
						targetPosition = cardPosition, 
						valueAffected = card.Data.value
					});
					coin += card.Data.value;
					break;

				case "Potion":
					var nextHp = Math.Min(limitOfHp, hp + card.Data.value);
					actionLogs.Add(new ActionLog() { 
						type = ActionType.USE_POTION, 
						sourcePosition = playerPosition, 
						targetPosition = cardPosition, 
						valueAffected = nextHp-hp 
					});
					hp = nextHp;
					break;

				case "Monster":
					int monsterValue = card.Data.value;
					if (canUseWeapon && weapon != null) {
						actionLogs.Add(new ActionLog() { 
							type = ActionType.ATTACK, 
							sourcePosition = playerPosition, 
							targetPosition = cardPosition, 
							valueAffected = weapon.Data.value
						});
						monsterValue = Math.Max(0, monsterValue - weapon.Data.value);
						weapon = null;
					}
					if (monsterValue > 0) {
						int initialDef = def;
						def = Math.Max(0, def - monsterValue);
						monsterValue -= initialDef;
					}
					if (monsterValue > 0) {
						actionLogs.Add(new ActionLog() { 
							type = ActionType.GET_DAMAGED, 
							sourcePosition = cardPosition, 
							targetPosition = playerPosition, 
							valueAffected = monsterValue 
						});
						hp -= monsterValue;
					}
					break;

				case "Magic":
					actionLogs.Add(new ActionLog() {
						type = ActionType.GET, 
						sourcePosition = playerPosition, 
						targetPosition = cardPosition, 
						valueAffected = card.Data.value
					});
					break;

				case "Weapon":
					weapon = card;
					actionLogs.Add(new ActionLog(){ 
						type=ActionType.GET, 
						sourcePosition = playerPosition, 
						targetPosition = cardPosition, 
						valueAffected = card.Data.value
					});
					break;
			}

			Debug.Log("Stats after merging : hp " + hp  + ", atk " + Atk + ", def " + def + ", coin " + coin);
			Debug.Log("===============");
			return new PlayerInfo {
				hp = hp, 
				coin = coin, 
				atk = Atk, 
				def = def, 
				merged = card, 
				equipments = new List<ICardModel>{ weapon },
				actionLogs = actionLogs
			};
		}
    }
}