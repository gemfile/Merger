using com.Gemfile.Merger;
using NUnit.Framework;

public class TestForMergeController 
{
	public IMergingController GetMergingController()
	{
		return new MergingController();
	}
	
	[Test]
	public void CantMergeTest() 
	{
		//1. Arrange
		var mergingController = GetMergingController();
		var player = new PlayerModel(new CardData {
			type="Player", value=13, resourceName="Worrior", cardName="Worrior"
		});
		var potion = new PotionModel(new CardData {
			type="Potion", value=6, resourceName="Potion", cardName="Potion"
		});
		var coin = new CoinModel(new CardData {
			type="Coin", value=4, resourceName="Coin4", cardName="Coin"
		});
		var monster = new MonsterModel(new CardData {
			type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B"
		});
		var weapon = new WeaponModel(new CardData {
			type="Weapon", value=5, resourceName="Axe", cardName="Axe"
		});
		
		//2. Act
		var cantPlayerMergePotion = mergingController.CantMerge(player, potion);
		var cantPlayerMergeCoin = mergingController.CantMerge(player, coin);
		var cantPlayerMergeWeapon = mergingController.CantMerge(player, weapon);
		var cantPlayerMergeMonster = mergingController.CantMerge(player, monster);

		var cantMonsterMergePotion = mergingController.CantMerge(monster, potion);
		var cantMonsterMergeCoin = mergingController.CantMerge(monster, coin);
		var cantMonsterMergeWeapon = mergingController.CantMerge(monster, weapon);
		var cantMonsterMergePlayer = mergingController.CantMerge(monster, player);

		//3. Assert
		Assert.False(cantPlayerMergePotion);
		Assert.False(cantPlayerMergeCoin);
		Assert.False(cantPlayerMergeWeapon);
		Assert.True(cantPlayerMergeMonster);

		Assert.False(cantMonsterMergePotion);
		Assert.False(cantMonsterMergeCoin);
		Assert.False(cantMonsterMergeWeapon);
		Assert.False(cantMonsterMergePlayer);

		//1. Arrange
		var equipments = player.Equipments;
		equipments.Add(weapon);
		player.Equipments = equipments;

		//3. Assert
		Assert.False(mergingController.CantMerge(player, monster));
	}

	[Test]
	public void MergeTest() 
	{
		//1. Arrange
		var mergingController = GetMergingController();
		var player = new PlayerModel(new CardData {
			type="Player", value=13, resourceName="Worrior", cardName="Worrior"
		});
		var potion = new PotionModel(new CardData {
			type="Potion", value=6, resourceName="Potion", cardName="Potion"
		});
		var coin = new CoinModel(new CardData {
			type="Coin", value=4, resourceName="Coin4", cardName="Coin"
		});
		var monster = new MonsterModel(new CardData {
			type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur"
		});
		var weapon = new WeaponModel(new CardData {
			type="Weapon", value=5, resourceName="Axe", cardName="Axe"
		});
		var secondWeapon = new WeaponModel(new CardData {
			type="Weapon", value=2, resourceName="Knife", cardName="Knife"
		});
		//2. Act
		MergerInfo playerMergesCoin = mergingController.Merge(player, coin, new Position(0), new Position(1));
		//3. Assert
		var actionLogs = playerMergesCoin.actionLogs;
		Assert.AreEqual(4, playerMergesCoin.coin);
		Assert.AreEqual(ActionType.GETTING_COIN, actionLogs[1].type);
		Assert.AreEqual(4, actionLogs[1].valueAffected);

		//2. Act
		MergerInfo playerMergesWeapon = mergingController.Merge(player, weapon, new Position(0), new Position(1));
		//3. Assert
		actionLogs = playerMergesWeapon.actionLogs;
		Assert.AreEqual(5, playerMergesWeapon.atk);
		Assert.AreEqual(ActionType.GETTING_SOMETHING, actionLogs[1].type);
		Assert.AreEqual(5, actionLogs[1].valueAffected);
		Assert.True(playerMergesWeapon.equipments.Exists(equipment => equipment == weapon));

		//2. Act
		MergerInfo monsterMergesWeapon = mergingController.Merge(monster, secondWeapon, new Position(0), new Position(1));
		//3. Assert
		Assert.True(monsterMergesWeapon.equipments.Exists(equipment => equipment == secondWeapon));

		//2. Act
		MergerInfo playerMergesMonster = mergingController.Merge(player, monster, new Position(0), new Position(1));
		//3. Assert
		actionLogs = playerMergesMonster.actionLogs;
		Assert.AreEqual(8, playerMergesMonster.hp);
		Assert.AreEqual(ActionType.ATTACK, actionLogs[0].type);
		Assert.AreEqual(ActionType.GET_DAMAGED, actionLogs[1].type);
		Assert.AreEqual(5, actionLogs[0].valueAffected);
		Assert.AreEqual(5, actionLogs[1].valueAffected);
		Assert.True(playerMergesMonster.equipments.Exists(equipment => equipment == secondWeapon));

		//2. Act
		MergerInfo playerMergesPotion = mergingController.Merge(player, potion, new Position(0), new Position(1));
		//3. Assert
		actionLogs = playerMergesPotion.actionLogs;
		Assert.AreEqual(13, playerMergesPotion.hp);
		Assert.AreEqual(ActionType.USE_POTION, actionLogs[1].type);
		Assert.AreEqual(5, actionLogs[1].valueAffected);
	}
}
