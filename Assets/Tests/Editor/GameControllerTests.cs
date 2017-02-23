using NUnit.Framework;
using com.Gemfile.Merger;

public class GameControllerTests 
{
	[Test]
	public void IsGameOverTest() 
	{
		//Arrange
		ResourceCache.LoadAll("GameScene");
		var gameViewObject = ResourceCache.Instantiate("GameView");
		var gameController = new GameController<GameModel, GameView>();
		gameController.Init(gameViewObject.transform.GetComponentInChildren<GameView>());

		//Act
		gameController.Field.Model.Player.Merge(
			new MonsterModel(new CardData { type="Monster", value=13, resourceName="Mock", cardName="Mock" }),
			new Position(0),
			new Position(1),
			false
		);

		//Assert
		Assert.AreEqual(true, gameController.IsGameOver());
		Assert.AreEqual(0, gameController.Field.Model.Player.Hp);
	}
}