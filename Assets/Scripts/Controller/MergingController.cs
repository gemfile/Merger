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
		public int hp;
		public int atk;
		public int def;
		public int coin;
		public ICardModel mergerModel;
		public Position mergerPosition;
		public ICardModel mergedModel;
		public Position mergedPosition;
		public List<ICardModel> equipments;
		public ICardModel weaponEquiped;
		public List<ActionLog> actionLogs; 
	}

	public enum ActionType
	{
		GETTING_SOMETHING = 0, USE_POTION, ATTACK, GET_DAMAGED, GETTING_COIN, GETTING_ROOT, GET_MERGED, DEAD
	}

	public class ActionLog
	{
		public ActionType type;
		public Position mergerPosition;
		public Position mergedPosition;
		public int valueAffected;
	}
	
	public class MergingController: IMergingController
	{
		public bool CantMerge(IMerger source, ICardModel target)
		{
			return (
				target is EmptyModel 
				|| (target is IMerger && source.Atk <= 0)
				|| (source is MonsterModel && target is MonsterModel)
			);
		}

		public MergerInfo Merge(IMerger source, ICardModel target, Position mergerPosition, Position mergedPosition, bool canUseWeapon)
		{
			Debug.Log("=== Merging ===");
			Debug.Log("Source name : " + (source as ICardModel).Data.cardName + ", Target name : " + target.Data.cardName);
			Debug.Log("Stats before merging : hp " + source.Hp  + ", atk " + source.Atk + ", def " + source.Def + ", coin " + source.Coin);
			if (target is PlayerModel) {
				return Merge(target as IMerger, source as ICardModel, mergedPosition, mergerPosition, false);
			}
			
			var actionLogs = new List<ActionLog>();
			var coin = source.Coin;
			var limitOfHp = source.LimitOfHp;
			var hp = source.Hp;
			var weaponEquiped = source.WeaponEquiped;
			var equipments = source.Equipments;
			var def = source.Def;

			switch (target.Data.type) {
				case "Coin":
					actionLogs.Add(new ActionLog() {
						type=ActionType.GET_MERGED, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
					});
					actionLogs.Add(new ActionLog() {
						type = ActionType.GETTING_COIN, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
						valueAffected = target.Data.value
					});
					coin += target.Data.value;
					break;

				case "Potion":
					actionLogs.Add(new ActionLog() {
						type=ActionType.GET_MERGED, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
					});
					var nextHp = Math.Min(limitOfHp, hp + target.Data.value);
					actionLogs.Add(new ActionLog() {
						type = ActionType.USE_POTION, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
						valueAffected = nextHp-hp
					});
					hp = nextHp;
					break;

				case "Monster":
					int monsterValue = target.Data.value;
					if (canUseWeapon && weaponEquiped != null) {
						actionLogs.Add(new ActionLog() {
							type = ActionType.ATTACK, 
							mergerPosition = mergerPosition, 
							mergedPosition = mergedPosition, 
							valueAffected = weaponEquiped.Data.value
						});
						monsterValue = Math.Max(0, monsterValue - weaponEquiped.Data.value);
						equipments.Remove(weaponEquiped);
					}
					
					if (monsterValue > 0) {
						actionLogs.Add(new ActionLog() { 
							type = ActionType.GET_DAMAGED, 
							mergerPosition = mergedPosition, 
							mergedPosition = mergerPosition, 
							valueAffected = monsterValue 
						});
						hp -= monsterValue;
					} 

					actionLogs.Add(new ActionLog() {
						type=ActionType.GET_MERGED, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
					});

					if (hp <= 0) {
						actionLogs.Add(new ActionLog() { 
							type = ActionType.DEAD, 
							mergerPosition = mergerPosition, 
							mergedPosition = mergedPosition, 
						});
					} else {
						IMerger targetMerger = target as IMerger;
						coin += targetMerger.Coin;
						equipments.AddRange(targetMerger.Equipments);
						actionLogs.Add(new ActionLog() {
							type = ActionType.GETTING_ROOT, 
							mergerPosition = mergerPosition, 
							mergedPosition = mergerPosition, 
							valueAffected = targetMerger.Coin
						});
					}
					break;

				case "Magic":
					actionLogs.Add(new ActionLog() {
						type=ActionType.GET_MERGED, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
					});
					actionLogs.Add(new ActionLog() {
						type = ActionType.GETTING_SOMETHING, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
						valueAffected = target.Data.value
					});
					break;

				case "Weapon":
					equipments.Add(target);
					actionLogs.Add(new ActionLog() {
						type=ActionType.GET_MERGED, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
					});
					actionLogs.Add(new ActionLog() {
						type=ActionType.GETTING_SOMETHING, 
						mergerPosition = mergerPosition, 
						mergedPosition = mergedPosition, 
						valueAffected = target.Data.value
					});
					break;
			}

			source.Coin = coin;
			source.Hp = hp;
			source.Def = def;
			source.Equipments = equipments;
			
			Debug.Log("Stats after merging : hp " + hp  + ", atk " + source.Atk + ", def " + def + ", coin " + coin);
			Debug.Log("===============");

			return new MergerInfo {
				hp = hp, 
				coin = coin, 
				atk = source.Atk, 
				def = def, 
				mergerModel = source as ICardModel,
				mergerPosition = mergerPosition,
				mergedModel = target, 
				mergedPosition = mergedPosition,
				equipments = equipments,
				weaponEquiped = weaponEquiped,
				actionLogs = actionLogs
			};
		}
	}
}
