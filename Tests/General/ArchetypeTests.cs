using KVD.ECS.Core;
using KVD.ECS.GeneralTests.Components;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests
{
	public class ArchetypeTests : EcsGeneralTests
	{
		[Test]
		public void Has_Empty()
		{
			// Arrange
			var archetype = new Archetype<Acceleration>(world.defaultStorage);
			
			// Assert
			Assert.IsFalse(archetype.Has(0));
			Assert.IsFalse(archetype.Has(1));
			Assert.IsFalse(archetype.Has(5));
			Assert.IsFalse(archetype.Has(32));
		}
		
		[Test]
		public void Add_then_Has()
		{
			// Arrange
			var archetype = new Archetype<Acceleration>(world.defaultStorage);
			
			// Act
			var e1 = world.defaultStorage.NextEntity();
			archetype.Create(e1, new());
			var e2 = world.defaultStorage.NextEntity();
			archetype.Create(e2, new());
			
			// Assert
			Assert.IsTrue(archetype.Has(e1));
			Assert.IsTrue(archetype.Has(e2));
			Assert.IsFalse(archetype.Has(5));
			Assert.IsFalse(archetype.Has(32));
		}
	}
}
