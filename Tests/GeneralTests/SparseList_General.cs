using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.GeneralTests.Tests.GeneralTests.Components;
using NUnit.Framework;

#nullable disable

namespace KVD.ECS.GeneralTests.Tests.GeneralTests
{
	[TestFixture]
	public class SparseListGeneral
	{
		private ComponentList<Position> _components;

		[SetUp]
		public void SetUp()
		{
			_components = new(2);
		}

		[TearDown]
		public void TearDown()
		{
			_components.Destroy();
		}
		
		[Test]
		public void Add_Ordinal()
		{
			// Act
			_components.Add(new(0), new());
			_components.Add(new(1), new());

			// Assert
			Assert.IsTrue(_components.Has(new(0)));
			Assert.IsTrue(_components.Has(new(1)));
			Assert.IsFalse(_components.Has(new(2)));
		}

		[Test]
		public void Add_OverCapacity()
		{
			// Act
			_components.Add(new(0), new());
			_components.Add(new(1), new());
			_components.Add(new(5), new());
			_components.Add(new(6), new());
			_components.Add(new(3), new());

			// Assert
			Assert.IsTrue(_components.Has(new(0)));
			Assert.IsTrue(_components.Has(new(1)));
			Assert.IsTrue(_components.Has(new(3)));
			Assert.IsTrue(_components.Has(new(5)));
			Assert.IsTrue(_components.Has(new(6)));
			Assert.IsFalse(_components.Has(new(2)));
		}
		
		[Test]
		public void AddOrReplace()
		{
			// Act
			_components.AddOrReplace(new(0), new());
			_components.AddOrReplace(new(1), new());
			_components.AddOrReplace(new(0), new());

			// Assert
			Assert.IsTrue(_components.Has(new(0)));
			Assert.IsTrue(_components.Has(new(1)));
			Assert.IsFalse(_components.Has(new(2)));
		}

		[Test]
		public void Remove_Present()
		{
			// Act
			_components.Add(0, new());
			var removed = _components.Remove(0);

			// Assert
			Assert.IsFalse(_components.Has(new(0)));
			Assert.IsTrue(removed);
		}
		
		[Test]
		public void Remove_Absent()
		{
			// Act
			var removed = _components.Remove(0);

			// Assert
			Assert.IsFalse(_components.Has(new(0)));
			Assert.IsFalse(removed);
		}
		
		[Test]
		public void Remove_ValidEntities()
		{
			// Act & Assert
			_components.Add(0, new());
			Assert.AreEqual(1, _components.EntitiesMask.Count());

			_components.Add(2, new());
			Assert.AreEqual(2, _components.EntitiesMask.Count());

			_components.Add(10, new());
			Assert.AreEqual(3, _components.EntitiesMask.Count());
			
			_components.Add(5, new());
			Assert.AreEqual(4, _components.EntitiesMask.Count());
			
			_components.Add(15, new());
			Assert.AreEqual(5, _components.EntitiesMask.Count());

			_components.Remove(2);
			Assert.AreEqual(4, _components.EntitiesMask.Count());
			
			_components.Remove(new Entity(10));
			Assert.AreEqual(3, _components.EntitiesMask.Count());
			
			Assert.True(_components.EntitiesMask.Has(0));
			Assert.True(_components.EntitiesMask.Has(5));
			Assert.True(_components.EntitiesMask.Has(15));
			
			Assert.False(_components.EntitiesMask.Has(1));
			Assert.False(_components.EntitiesMask.Has(2));
			Assert.False(_components.EntitiesMask.Has(3));
			Assert.False(_components.EntitiesMask.Has(4));
			Assert.False(_components.EntitiesMask.Has(6));
			Assert.False(_components.EntitiesMask.Has(14));
			Assert.False(_components.EntitiesMask.Has(16));
		}
		
		[Test]
		public void ValidEntities()
		{
			// Act
			_components.Add(0, new());
			_components.Add(5, new());
			_components.Add(2, new());
			var entities = _components.EntitiesMask;

			// Assert
			Assert.AreEqual(3, entities.Count());
			Assert.True(_components.EntitiesMask.Has(0));
			Assert.True(_components.EntitiesMask.Has(2));
			Assert.True(_components.EntitiesMask.Has(5));
			
			Assert.False(_components.EntitiesMask.Has(1));
			Assert.False(_components.EntitiesMask.Has(3));
			Assert.False(_components.EntitiesMask.Has(4));
			Assert.False(_components.EntitiesMask.Has(6));
		}
	}
}
