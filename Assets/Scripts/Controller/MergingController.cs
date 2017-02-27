using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface IMergingController
	{
		MergerInfo Merge(
			IMerger source, 
			ICardModel target, 
			Position sourcePosition, 
			Position targetPosition, 
			bool canUseWeapon = true
		);
		bool CantMerge(IMerger source, ICardModel target);
	}
	
	public class MergerInfo 
	{
		internal int hp;
		internal int atk;
		internal int def;
		internal int coin;
		internal ICardModel merger;
		internal Position mergerPosition;
		internal ICardModel merged;
		internal Position mergedPosition;
		internal List<ICardModel> equipments;
		internal List<ActionLog> actionLogs; 
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
	
	public class MergingController : IMergingController
	{
		public bool CantMerge(IMerger source, ICardModel target)
		{
			return (
				target is EmptyModel ||
				(target is IMerger && source.Atk <= 0) || 
				(source is MonsterModel && !(target is PlayerModel))
			);
		}

		public MergerInfo Merge(IMerger source, ICardModel target, Position sourcePosition, Position targetPosition, bool canUseWeapon)
		{
			Debug.Log("=== Merging ===");
			Debug.Log("Source name : " + (source as ICardModel).Data.cardName + ", Target name : " + target.Data.cardName);
			Debug.Log("Stats before merging : hp " + source.Hp  + ", atk " + source.Atk + ", def " + source.Def + ", coin " + source.Coin);
			if (target is PlayerModel) {
				return Merge(target as IMerger, source as ICardModel, targetPosition, sourcePosition, false);
			}
			
			var actionLogs = new List<ActionLog>();
			var coin = source.Coin;
			var limitOfHp = source.LimitOfHp;
			var hp = source.Hp;
			var weapon = source.Weapon;
			var def = source.Def;

			switch (target.Data.type) {
				case "Coin":
					actionLogs.Add(new ActionLog() {
						type = ActionType.GET_COIN, 
						sourcePosition = sourcePosition, 
						targetPosition = targetPosition, 
						valueAffected = target.Data.value
					});
					coin += target.Data.value;
					break;

				case "Potion":
					var nextHp = Math.Min(limitOfHp, hp + target.Data.value);
					actionLogs.Add(new ActionLog() { 
						type = ActionType.USE_POTION, 
						sourcePosition = sourcePosition, 
						targetPosition = targetPosition, 
						valueAffected = nextHp-hp
					});
					hp = nextHp;
					break;

				case "Monster":
					int monsterValue = target.Data.value;
					if (canUseWeapon && weapon != null) {
						actionLogs.Add(new ActionLog() { 
							type = ActionType.ATTACK, 
							sourcePosition = sourcePosition, 
							targetPosition = targetPosition, 
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
							sourcePosition = targetPosition, 
							targetPosition = sourcePosition, 
							valueAffected = monsterValue 
						});
						hp -= monsterValue;
					}
					break;

				case "Magic":
					actionLogs.Add(new ActionLog() {
						type = ActionType.GET, 
						sourcePosition = sourcePosition, 
						targetPosition = targetPosition, 
						valueAffected = target.Data.value
					});
					break;

				case "Weapon":
					weapon = target;
					actionLogs.Add(new ActionLog(){ 
						type=ActionType.GET, 
						sourcePosition = sourcePosition, 
						targetPosition = targetPosition, 
						valueAffected = target.Data.value
					});
					break;
			}

			source.Coin = coin;
			source.Hp = hp;
			source.Weapon = weapon;
			source.Def = def;
			
			Debug.Log("Stats after merging : hp " + hp  + ", atk " + source.Atk + ", def " + def + ", coin " + coin);
			Debug.Log("===============");

			return new MergerInfo {
				hp = hp, 
				coin = coin, 
				atk = source.Atk, 
				def = def, 
				merger = source as ICardModel,
				mergerPosition = sourcePosition,
				merged = target, 
				mergedPosition = targetPosition,
				equipments = new List<ICardModel>{ weapon },
				actionLogs = actionLogs
			};
		}
	}
}
