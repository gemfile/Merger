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

		Assert.True(cantMonsterMergePotion);
		Assert.True(cantMonsterMergeCoin);
		Assert.True(cantMonsterMergeWeapon);
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
		//2. Act
		MergerInfo playerMergesCoin = mergingController.Merge(player, coin, new Position(0), new Position(1));
		//3. Assert
		Assert.AreEqual(4, playerMergesCoin.coin);
		Assert.AreEqual(ActionType.GETTING_COIN, playerMergesCoin.actionLogs[0].type);
		Assert.AreEqual(4, playerMergesCoin.actionLogs[0].valueAffected);

		//2. Act
		MergerInfo playerMergesWeapon = mergingController.Merge(player, weapon, new Position(0), new Position(1));
		//3. Assert
		Assert.AreEqual(5, playerMergesWeapon.atk);
		Assert.AreEqual(ActionType.GETTING_SOMETHING, playerMergesWeapon.actionLogs[0].type);
		Assert.AreEqual(5, playerMergesWeapon.actionLogs[0].valueAffected);

		//2. Act
		MergerInfo playerMergesMonster = mergingController.Merge(player, monster, new Position(0), new Position(1));
		//3. Assert
		Assert.AreEqual(8, playerMergesMonster.hp);
		Assert.AreEqual(ActionType.ATTACK, playerMergesMonster.actionLogs[0].type);
		Assert.AreEqual(ActionType.GET_DAMAGED, playerMergesMonster.actionLogs[1].type);
		Assert.AreEqual(5, playerMergesMonster.actionLogs[0].valueAffected);
		Assert.AreEqual(5, playerMergesMonster.actionLogs[1].valueAffected);

		//2. Act
		MergerInfo playerMergesPotion = mergingController.Merge(player, potion, new Position(0), new Position(1));
		//3. Assert
		Assert.AreEqual(13, playerMergesPotion.hp);
		Assert.AreEqual(ActionType.USE_POTION, playerMergesPotion.actionLogs[0].type);
		Assert.AreEqual(5, playerMergesPotion.actionLogs[0].valueAffected);
	}
}
