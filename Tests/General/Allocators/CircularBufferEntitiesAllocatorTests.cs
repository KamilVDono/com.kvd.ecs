using KVD.ECS.Core.Entities.Allocators;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests.Allocators
{
	public class CircularBufferEntitiesAllocatorTests
	{
		[Test]
		public void Allocate([Values(10, 32, 256, 512)] int bufferSize)
		{
			// Arrange
			var allocator = new CircularBufferEntitiesAllocator(bufferSize);
			
			// Act&Assert
			for (var i = 0; i < bufferSize*3; i++)
			{
				var expected = i%bufferSize;
				var entity   = allocator.Allocate();
				Assert.AreEqual(expected, entity.index);
				allocator.Return(entity);
			}
		}
	}
}
