using NUnit.Framework;
using com.Gemfile.Merger;
using NSubstitute;

public class TestForGameController
{
	[Test]
	public void IsGameOverTest() 
	{
		// Arrange
		var gameController = new GameController<GameModel, IGameView>();
		var gameView = Substitute.For<IGameView>();

		gameView.Swipe.OnSwipeCancel.Returns(new SwipeEvent());
		gameView.Swipe.OnSwipeEnd.Returns(new SwipeEvent());
		gameView.Swipe.OnSwipeMove.Returns(new SwipeEvent());
		gameView.Field.OnSpriteCaptured.Returns(new SpriteCaptureEvent());
		gameView.Orientation.OnOrientationChange.Returns(new OrientationEvent());
		gameController.Init(gameView);

		// Act
		gameController.Field.Merging.Merge(
			gameController.Field.Model.Player,
			new MonsterModel(new CardData { type="Monster", value=13, resourceName="Mock", cardName="Mock" }),
			new Position(0),
			new Position(1)
		);
		
		// Assert
		Assert.AreEqual(true, gameController.IsGameOver());
		Assert.AreEqual(0, gameController.Field.Model.Player.Hp);
	}
}