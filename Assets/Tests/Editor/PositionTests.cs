using NUnit.Framework;
using com.Gemfile.Merger;

public class PositionTests 
{
	[Test]
	public void IndexTest() 
	{
		Position.Cols = 3;
		Position.Rows = 3;
		
		var initialIndex = 1;
		var firstPosition = new Position(initialIndex);
		var secondPosition = new Position(initialIndex, 1, 0);
		var thirdPosition = new Position(initialIndex, 0, 1);

		Assert.AreEqual(1, firstPosition.col);
		Assert.AreEqual(0, firstPosition.row);

		Assert.AreEqual(2, secondPosition.index);

		Assert.AreEqual(4, thirdPosition.index);
	}
}
