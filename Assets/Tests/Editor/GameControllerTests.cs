﻿using NUnit.Framework;
using com.Gemfile.Merger;

public class GameControllerTests 
{
	GameController gameController;

	[Test]
	public void IsGameOverTest() 
	{
		//Arrange
		ResourceCache.Load("");
		var mockOfGameView = ResourceCache.Instantiate("GameView");

		var gameModel = new GameModel();
		var gameView = mockOfGameView.transform.GetComponentInChildren<GameView>();
		var gameController = new GameController(gameModel, gameView);
		gameController.Init();

		//Act
		gameModel.Player.Merge(
			new MonsterModel(new CardData { type="Monster", value=13, resourceName="Mock", cardName="Mock" }),
			new Position(0),
			new Position(1),
			false
		);

		//Assert
		Assert.AreEqual(true, gameController.IsGameOver());
		Assert.AreEqual(0, gameModel.Player.Hp);
	}

	[Test]
	public void MoveTest() {
		//Arrange
		ResourceCache.Load("");
		var mockOfGameView = ResourceCache.Instantiate("GameView");

		var gameModel = new GameModel();
		var gameView = mockOfGameView.transform.GetComponentInChildren<GameView>();
		var gameController = new GameController(gameModel, gameView);
		gameController.Init();

		//Act
		var initialIndex = 1;
		var firstCardModel = gameController.GetCard(initialIndex);

		var secondPosition = new Position(initialIndex, 1, 0);
		gameController.Move(secondPosition.index, 1, 0);

		//Assert
		Assert.AreEqual(2, secondPosition.index);

		var secondCardModel = gameController.GetCard(secondPosition.index);
		Assert.AreEqual(secondCardModel, firstCardModel);

		//Act
		var thirdPosition = new Position(secondPosition.index, 0, 1);
		gameController.Move(thirdPosition.index, 0, 1);

		//Asert
		Assert.AreEqual(5, thirdPosition.index);
		var thirdCardModel = gameController.GetCard(thirdPosition.index);
		Assert.AreEqual(thirdCardModel, firstCardModel);
	}
}