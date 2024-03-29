using System;
using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.GeneralTests.Components;
using KVD.Utils.DataStructures;
using NUnit.Framework;
using Unity.Collections;

#pragma warning disable CS0618

#nullable disable

namespace KVD.ECS.GeneralTests
{
	[TestFixture]
	public class ComponentListGeneralTests
	{
		private const int InitCapacity = 2;
		private ComponentList<Position> _components;

		[SetUp]
		public void SetUp()
		{
			_components = new(InitCapacity);
		}

		[TearDown]
		public void TearDown()
		{
			_components.Dispose();
		}
		
		#region Add
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
		public unsafe void AddTypeless()
		{
			// Arrange
			var pos = new Position();
			
			// Act
			_components.typeless.Add(new(0), &pos);
			_components.typeless.Add(new(1), &pos);
		
			// Assert
			Assert.IsTrue(_components.Has(new(0)));
			Assert.IsTrue(_components.Has(new(1)));
			Assert.IsFalse(_components.Has(new(2)));
		}
		
		[Test]
		public unsafe void AddOrReplaceTypeless()
		{
			// Arrange
			var pos = new Position();
			
			// Act
			_components.typeless.AddOrReplace(new(0), &pos);
			_components.typeless.AddOrReplace(new(1), &pos);
			_components.typeless.AddOrReplace(new(0), &pos);
		
			// Assert
			Assert.IsTrue(_components.Has(new(0)));
			Assert.IsTrue(_components.Has(new(1)));
			Assert.IsFalse(_components.Has(new(2)));
		}
		
		[Test]
		public void BulkAdd_DefaultValue()
		{
			// Arrange
			var entities = new UnsafeArray<Entity>(8u, Allocator.Temp)
			{
				[0] = 0,
				[1] = 1,
				[2] = 3,
				[3] = 4,
				[4] = 5,
				[5] = 7,
				[6] = 8,
				[7] = 9,
			};

			// Act
			_components.BulkAdd(entities);
			
			// Assert
			Assert.IsTrue(_components.Has(0));
			Assert.IsTrue(_components.Has(1));
			Assert.IsFalse(_components.Has(2));
			Assert.IsTrue(_components.Has(3));
			Assert.IsTrue(_components.Has(4));
			Assert.IsTrue(_components.Has(5));
			Assert.IsFalse(_components.Has(6));
			Assert.IsTrue(_components.Has(7));
			Assert.IsTrue(_components.Has(8));
			Assert.IsTrue(_components.Has(9));
			Assert.IsFalse(_components.Has(10));

			entities.Dispose();
		}
		
		[Test]
		public void BulkAdd_CustomValue()
		{
			// Arrange
			var entities = new UnsafeArray<Entity>(8u, Allocator.Temp);
			entities[0] = 0;
			entities[1] = 1;
			entities[2] = 3;
			entities[3] = 4;
			entities[4] = 5;
			entities[5] = 7;
			entities[6] = 8;
			entities[7] = 9;
		
			var pos = new Position { x = 15, y = 0, z = -10, };
			
			// Act
			_components.BulkAdd(entities, pos);
			
			// Assert
			Assert.IsTrue(_components.Has(0));
			Assert.IsTrue(_components.Has(1));
			Assert.IsFalse(_components.Has(2));
			Assert.IsTrue(_components.Has(3));
			Assert.IsTrue(_components.Has(4));
			Assert.IsTrue(_components.Has(5));
			Assert.IsFalse(_components.Has(6));
			Assert.IsTrue(_components.Has(7));
			Assert.IsTrue(_components.Has(8));
			Assert.IsTrue(_components.Has(9));
			Assert.IsFalse(_components.Has(10));
			
			Assert.AreEqual(_components.Value(0), pos);
			Assert.AreEqual(_components.Value(5), pos);
			Assert.AreEqual(_components.Value(9), pos);

			entities.Dispose();
		}
		#endregion Add
		
		#region Remove
		// Remove
		
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
			Assert.AreEqual(1, _components.Length);
			Assert.AreEqual(1, _components.EntitiesMask.CountOnes());
		
			_components.Add(2, new());
			Assert.AreEqual(2, _components.Length);
			Assert.AreEqual(2, _components.EntitiesMask.CountOnes());
		
			_components.Add(10, new());
			Assert.AreEqual(3, _components.Length);
			Assert.AreEqual(3, _components.EntitiesMask.CountOnes());
			
			_components.Add(5, new());
			Assert.AreEqual(4, _components.Length);
			Assert.AreEqual(4, _components.EntitiesMask.CountOnes());
			
			_components.Add(15, new());
			Assert.AreEqual(5, _components.Length);
			Assert.AreEqual(5, _components.EntitiesMask.CountOnes());
		
			_components.Remove(2);
			Assert.AreEqual(4, _components.Length);
			Assert.AreEqual(4, _components.EntitiesMask.CountOnes());
			
			_components.Remove(new Entity(10));
			Assert.AreEqual(3, _components.Length);
			Assert.AreEqual(3, _components.EntitiesMask.CountOnes());
			
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
		#endregion Remove
		
		#region Value
		[Test]
		public void Value_Present()
		{
			// Arrange
			var pos = new Position { x = 5, y = 8, z = 9, };
			_components.Add(5, pos);
			_components.Add(15, pos);
			
			// Act
			var posFromList = _components.Value(5);
			
			// Assert
			Assert.AreEqual(pos, posFromList);
		}
		
		[Test]
		public void RefValue_Present_Updated()
		{
			// Arrange
			var pos = new Position { x = 5, y = 8, z = 9, };
			_components.Add(5, pos);
			
			// Act & Assert
			ref var posFromList = ref _components.Value(5);
			Assert.AreEqual(pos, posFromList);
		
			posFromList.x = 18;
			pos.x         = 18;
			posFromList = ref _components.Value(5);
			Assert.AreEqual(pos, posFromList);
		}
		
		[Test]
		public void TryValue_Present()
		{
			// Arrange
			var pos = new Position { x = 5, y = 8, z = 9, };
			_components.Add(5, pos);
			_components.Add(15, pos);
			
			// Act
			var posFromList = _components.TryValue(5, out var present);
			
			// Assert
			Assert.IsTrue(present);
			Assert.AreEqual(pos, posFromList);
		}
		
		[Test]
		public void TryValue_NotPresent()
		{
			// Arrange
			var pos = new Position { x = 5, y = 8, z = 9, };
			_components.Add(1, pos);
			_components.Add(15, pos);
			
			var _ = _components.TryValue(5, out var present);
			
			// Assert
			Assert.IsFalse(present);
		}
		
		[Test]
		public void RefTryValue_Present_Updated()
		{
			// Arrange
			var pos = new Position { x = 5, y = 8, z = 9, };
			_components.Add(5, pos);
			_components.Add(15, pos);
			
			// Act & Assert
			ref var posFromList = ref _components.TryValue(5, out var present);
			Assert.IsTrue(present);
			Assert.AreEqual(pos, posFromList);
		
			posFromList.x = 15;
			pos.x         = 15;
			posFromList   = ref _components.Value(5);
			Assert.AreEqual(pos, posFromList);
		}
		
		[Test]
		public void RefTryValue_NotPresent()
		{
			// Arrange
			var pos = new Position { x = 5, y = 8, z = 9, };
			_components.Add(1, pos);
			_components.Add(15, pos);
			
			ref var _ = ref _components.TryValue(5, out var present);
			
			// Assert
			Assert.IsFalse(present);
		}
		#endregion Value
		
		#region Single frame
		[Test]
		public void SingleFrame_Added_ThenCleared()
		{
			// Act&Assert
			_components.AddSingleFrame(5, new());
			_components.AddSingleFrame(1, new());
			_components.AddSingleFrame(15, new());
			
			Assert.IsTrue(_components.Has(1));
			Assert.IsFalse(_components.Has(2));
			Assert.IsTrue(_components.Has(5));
			Assert.IsTrue(_components.Has(15));
		
			_components.AddOrReplace(5, new() { x = 15 });
			var pos = _components.Value(5);
			Assert.AreEqual(new Position { x = 15 }, pos);
			
			_components.ClearSingleFrameEntities();
			Assert.IsFalse(_components.Has(1));
			Assert.IsFalse(_components.Has(2));
			Assert.IsFalse(_components.Has(5));
			Assert.IsFalse(_components.Has(15));
		}
		#endregion Single frame
		
		#region EnsureCapacity
		[Test]
		public void EnsureCapacity()
		{
			// Act&Assert
			Assert.AreEqual(_components.Length, 0);
			Assert.AreEqual(_components.Capacity, 64);
			Assert.AreEqual(_components.IndexByEntityCount, 64);
			_components.EnsureCapacity(128, 250);
			Assert.AreEqual(_components.Length, 0);
			Assert.GreaterOrEqual(_components.Capacity, 128);
			Assert.GreaterOrEqual(_components.IndexByEntityCount, 250);
		}
		#endregion EnsureCapacity
		
		#region Bitmask state
		[Test]
		public void MultipleOperations_ValidEntitiesMask()
		{
			// Act
			_components.Add(0, new());
			_components.Add(5, new());
			_components.Add(2, new());
			_components.Add(12, new());
			_components.Add(15, new());
			_components.Remove(15);
			_components.Remove(12);
			var entities = _components.EntitiesMask;
		
			// Assert
			Assert.AreEqual(3, entities.CountOnes());
			Assert.True(_components.EntitiesMask.Has(0));
			Assert.True(_components.EntitiesMask.Has(2));
			Assert.True(_components.EntitiesMask.Has(5));
			
			Assert.False(_components.EntitiesMask.Has(1));
			Assert.False(_components.EntitiesMask.Has(3));
			Assert.False(_components.EntitiesMask.Has(4));
			Assert.False(_components.EntitiesMask.Has(6));
		}
		#endregion Bitmask state
		
		#region Entities version
		[Test]
		public unsafe void Add_EntitiesVersionChanged()
		{
			// Act
			var startVersion = _components.EntitiesVersion;
			_components.Add(1, new());
			var add1Version = _components.EntitiesVersion;
			var position = new Position();
			_components.typeless.Add(2, &position);
			var add2Version = _components.EntitiesVersion;
			_components.typeless.AddOrReplace(3, &position);
			var add3Version = _components.EntitiesVersion;
			_components.typeless.AddOrReplace(3, &position);
			var add4Version = _components.EntitiesVersion;
			_components.AddOrReplace(4, new());
			var add5Version = _components.EntitiesVersion;
			_components.AddOrReplace(4, new());
			var add6Version = _components.EntitiesVersion;
			_components.Remove(1);
			var add7Version = _components.EntitiesVersion;
			_components.Remove(4);
			var add8Version = _components.EntitiesVersion;

			// Assert
			Assert.Less(startVersion, add1Version);
			Assert.Less(add1Version, add2Version);
			Assert.Less(add2Version, add3Version);
			Assert.AreEqual(add3Version, add4Version);
			Assert.Less(add4Version, add5Version);
			Assert.AreEqual(add5Version, add6Version);
			Assert.Less(add6Version, add7Version);
			Assert.Less(add7Version, add8Version);
		}
		// Remove
		#endregion Entities version
	}
}
