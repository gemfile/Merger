using System.Linq;
using com.Gemfile.Merger;
using NUnit.Framework;

public class FieldControllerTests {

	[Test]
	public void MoveTest() {
		//Arrange
		var fieldController = GetFieldController();

		//Act
		var initialIndex = fieldController.Model.Fields.First(field => field.Value is PlayerModel).Key;
		var firstCardModel = fieldController.GetCard(initialIndex);
		var secondPosition = new Position(initialIndex, 1, 0);
		fieldController.Move(secondPosition.index, 1, 0);

		//Assert
		Assert.AreEqual(2, secondPosition.index);

		var secondCardModel = fieldController.GetCard(secondPosition.index);
		Assert.AreEqual(secondCardModel, firstCardModel);

		//Act
		var thirdPosition = new Position(secondPosition.index, 0, 1);
		fieldController.Move(thirdPosition.index, 0, 1);

		//Asert
		Assert.AreEqual(5, thirdPosition.index);
		var thirdCardModel = fieldController.GetCard(thirdPosition.index);
		Assert.AreEqual(thirdCardModel, firstCardModel);
	}

	IFieldController<FieldModel, FieldView> GetFieldController()
	{
		ResourceCache.LoadAll("GameScene");
		var gameViewObject = ResourceCache.Instantiate("GameView");
		var gameController = new GameController<GameModel, GameView>();
		gameController.Init(gameViewObject.transform.GetComponentInChildren<GameView>());
		return gameController.Field;
	}

	int GetPlayerIndex(IFieldController<FieldModel, FieldView> fieldController)
	{
		return fieldController.Model.Fields.First(field => field.Value is PlayerModel).Key;
	}
	
	[Test]
	public void MergeTest() {
		//Arrange
		var fieldController = GetFieldController();

		//Act
		var initialIndex = GetPlayerIndex(fieldController);
		var secondPosition = new Position(initialIndex, 1, 0);
		fieldController.AddField(
			secondPosition.index, 
			new CoinModel(new CardData { 
				type="Coin", value=8, resourceName="Coin4", cardName="Coin" 
			})
		);
		var isMerged = fieldController.Merge(1, 0);

		//Assert
		Assert.AreEqual(true, isMerged);

		//Act
		var thirdPosition = new Position(secondPosition.index, 0, 1);
		//Assert
		Assert.AreEqual(5, thirdPosition.index);

		//Act
		fieldController.AddField(
			thirdPosition.index, 
			new MonsterModel(new CardData { 
				type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur"
			})
		);
		isMerged = fieldController.Merge(0, 1);

		//Assert
		Assert.AreEqual(false, isMerged);

		//Act
		fieldController.AddField(
			thirdPosition.index, 
			new WeaponModel(new CardData {
				type="Weapon", value=3, resourceName="Club", cardName="Club"
			})
		);
		isMerged = fieldController.Merge(0, 1);

		//Assert
		Assert.AreEqual(3, fieldController.Model.Player.Atk);
		Assert.AreEqual(true, isMerged);

		//Act
		var fourthPosition = new Position(thirdPosition.index, -1, 0);
		fieldController.AddField(
			fourthPosition.index, 
			new MonsterModel(new CardData { 
				type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur"
			})
		);
		isMerged = fieldController.Merge(-1, 0);

		//Assert
		Assert.AreEqual(4, fourthPosition.index);
		Assert.AreEqual(true, isMerged);
		Assert.AreEqual(6, fieldController.Model.Player.Hp);
	}
}
