using KVD.ECS.Core.Entities.Allocators;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests.Allocators
{
	public class ReusableEntitiesAllocatorTests
	{
		[Test, TestCase(1, 1), TestCase(10, 1), TestCase(10, 10), TestCase(50, 1), TestCase(50, 10), TestCase(100, 50),]
		public void Allocate(int everyXReturn, int returnSize)
		{
			// Arrange
			var allocator = new ReusableEntitiesAllocator();
			
			// Act&Assert
			var expected = 0;
			for (var i = 0; i < 50000; i++)
			{
				var entity = allocator.Allocate();
				Assert.AreEqual(expected, entity.index);
				expected++;

				if (i%everyXReturn != 0)
				{
					continue;
				}
				
				for (var e = 0; e < returnSize; e++)
				{
					var toReturn = entity-e;
					allocator.Return(toReturn);
					expected--;
				}
			}
		}
	}
}
