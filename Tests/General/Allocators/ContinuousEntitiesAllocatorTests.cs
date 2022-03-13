using KVD.ECS.Core.Entities.Allocators;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests.Allocators
{
	public class ContinuousEntitiesAllocatorTests
	{
		[Test]
		public void Allocate()
		{
			// Arrange
			var allocator = new ContinuousEntitiesAllocator();
			
			// Act&Assert
			for (var expected = 0; expected < 1000; expected++)
			{
				var entity = allocator.Allocate();
				Assert.AreEqual(expected, entity.index);
				allocator.Return(entity);
			}
		}
	}
}
