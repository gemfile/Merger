using System;
using System.Collections.Generic;
using System.Linq;
using com.Gemfile.Merger;
using NSubstitute;
using NUnit.Framework;

public class TestForFieldController {

	[Test]
	public void MoveTest() {
		//1. Arrange
		var fieldController = GetFieldController();
		//2. Act
		var initialIndex = fieldController.Model.Fields.First(field => field.Value is PlayerModel).Key;
		var firstCardModel = fieldController.GetCard(initialIndex);
		var secondPosition = new Position(initialIndex, 1, 0);
		fieldController.Move(secondPosition.index, 1, 0);
		//3. Assert
		Assert.AreEqual(2, secondPosition.index);
		var secondCardModel = fieldController.GetCard(secondPosition.index);
		Assert.AreEqual(secondCardModel, firstCardModel);
		
		//2. Act
		var thirdPosition = new Position(secondPosition.index, 0, 1);
		fieldController.Move(thirdPosition.index, 0, 1);
		//3. Asert
		Assert.AreEqual(5, thirdPosition.index);
		var thirdCardModel = fieldController.GetCard(thirdPosition.index);
		Assert.AreEqual(thirdCardModel, firstCardModel);
	}

	IFieldController<FieldModel, IFieldView> GetFieldController()
	{
		var fieldController = new FieldController<FieldModel, IFieldView>();
		var fieldView = Substitute.For<IFieldView>();
		fieldController.Init(fieldView);
		return fieldController;
	}

	int GetModelIndex<T>(IFieldController<FieldModel, IFieldView> fieldController)
	{
		return fieldController.Model.Fields.First(field => field.Value is T).Key;
	}
	
	[Test]
	public void MergeTest() {
		//1. Arrange
		var fieldController = GetFieldController();
		var initialIndex = GetModelIndex<PlayerModel>(fieldController);
		var secondPosition = new Position(initialIndex, 1, 0);
		fieldController.AddField(
			secondPosition.index, 
			new CoinModel(new CardData { 
				type="Coin", value=8, resourceName="Coin4", cardName="Coin" 
			})
		);
		//2. Act
		var isMerged = fieldController.Merge(1, 0);
		//3. Assert
		Assert.AreEqual(true, isMerged);

		//2. Act
		var thirdPosition = new Position(secondPosition.index, 0, 1);
		//3. Assert
		Assert.AreEqual(5, thirdPosition.index);

		//1. Arrange
		fieldController.AddField(
			thirdPosition.index, 
			new MonsterModel(new CardData { 
				type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur"
			})
		);
		//2. Act
		isMerged = fieldController.Merge(0, 1);
		//3. Assert
		Assert.AreEqual(false, isMerged);

		//1. Arrange
		fieldController.AddField(
			thirdPosition.index, 
			new WeaponModel(new CardData {
				type="Weapon", value=3, resourceName="Club", cardName="Club"
			})
		);
		//2. Act
		isMerged = fieldController.Merge(0, 1);
		//3. Assert
		Assert.AreEqual(3, fieldController.Model.Player.Atk);
		Assert.AreEqual(true, isMerged);

		//1. Arrange
		var fourthPosition = new Position(thirdPosition.index, -1, 0);
		fieldController.AddField(
			fourthPosition.index, 
			new MonsterModel(new CardData { 
				type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur"
			})
		);
		//2. Act
		isMerged = fieldController.Merge(-1, 0);
		//3. Assert
		Assert.AreEqual(4, fourthPosition.index);
		Assert.AreEqual(true, isMerged);
		Assert.AreEqual(6, fieldController.Model.Player.Hp);
	}

	[Test]
	public void GetWheresCanMergeTest()
	{
		//1. Arrange
		var fieldController = GetFieldController();
		var dummyDatas = new List<CardData>() {
			new CardData { type="Potion", value=6, resourceName="Potion", cardName="Potion" },
			new CardData { type="Player", value=13, resourceName="Worrior", cardName="Worrior" },
			new CardData { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
			new CardData { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
			new CardData { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },
			new CardData { type="Coin", value=6, resourceName="Coin3", cardName="Coin" },
			new CardData { type="Magic", value=5, resourceName="Magic", cardName="Magic" },
			new CardData { type="Coin", value=3, resourceName="Coin1", cardName="Coin" },
			new CardData { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" }
		};

		var count = 0;
		dummyDatas.ForEach(dummyData => {
			fieldController.AddField(
				count++,
				(CardModel)Activator.CreateInstance(
					Type.GetType("com.Gemfile.Merger." + dummyData.type + "Model, Assembly-CSharp"),
					dummyData
				)
			);
		});
		Position.Cols = 3;
		Position.Rows = 3;
		//2. Act
		List<NavigationInfo> wheresCanMerge = fieldController.GetWheresCanMerge();
		//3. Assert 
		var expects = new List<NavigationInfo>() {
			new NavigationInfo() {
				sourceIndex = 1, 
				wheresCanMerge = new List<Position>() {
					new Position(0)
				}
			},
			new NavigationInfo() {
				sourceIndex = 2,
				wheresCanMerge = new List<Position>() {
					new Position(1)
				}
			},
			new NavigationInfo() {
				sourceIndex = 4,
				wheresCanMerge = new List<Position>() {
					new Position(1)
				}
			},
			new NavigationInfo() {
				sourceIndex = 8,
				wheresCanMerge = new List<Position>()
			}
		};

		for(var i = 0; i < wheresCanMerge.Count; i++)
		{
			var whereCanMerge = wheresCanMerge[i];
			var expect = expects[i];

			Assert.AreEqual(expect.sourceIndex, whereCanMerge.sourceIndex);
			expect.wheresCanMerge.SequenceEqual(expect.wheresCanMerge);
		}
	}
}
