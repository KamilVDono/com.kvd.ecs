using System.Collections.Generic;
using System.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.GeneralTests.Components;
using NUnit.Framework;
using Position = KVD.ECS.GeneralTests.Components.Position;

#nullable disable

namespace KVD.ECS.GeneralTests
{
	public class ComponentsViewTests : EcsTestsBase
	{
		private ComponentList<Position> _positions;
		private ComponentList<Radius> _radii;
		private ComponentList<Acceleration> _accelerations;
		
		protected override Task OnSetup()
		{
			_positions     = world.defaultStorage.List<Position>();
			_radii         = world.defaultStorage.List<Radius>();
			_accelerations = world.defaultStorage.List<Acceleration>();
			return base.OnSetup();
		}
		
		[Test]
		public void Has_EmptyLists()
		{
			// Arrange
			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view = builder.With<Position>().With<Acceleration>().Build();
		
			// Act
			CollectEntities(ref view);
		
			// Assert
			Assert.AreEqual(0, view.Size);
		}
		
		[Test]
		public void Has_EmptyTarget_OthersWithData()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			// 3
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			// 4
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			// 5
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());

			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view = builder.With<Radius>().Build();
		
			// Act
			CollectEntities(ref view);
		
			// Assert
			Assert.AreEqual(0, view.Size);
		}
		
		[Test]
		public void Has_FirstStorageEmpty_OthersFill()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_radii.Add(nextEntity, new());
			// 3
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 4
			nextEntity = world.defaultStorage.NextEntity();
			_radii.Add(nextEntity, new());
			// 5
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());

			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view = builder.With<Position>().With<Acceleration>().Build();
		
			// Act
			CollectEntities(ref view);
		
			// Assert
			Assert.AreEqual(0, view.Size);
		}
		
		[Test]
		public void Has_AllStoragesFill_TwoStorages()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			// 3
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			// 4
			world.defaultStorage.NextEntity();
			// 5
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
		
			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view    = builder.With<Position>().With<Acceleration>().Build();
		
			// Act && Assert
			var entities = CollectEntities(ref view);
			Assert.AreEqual(3, view.Size);
			Assert.AreEqual(0, entities[0].index);
			Assert.AreEqual(1, entities[1].index);
			Assert.AreEqual(5, entities[2].index);
		}
		
		[Test]
		public void Has_AllStoragesFill_ThreeStorages()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 3
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 4
			nextEntity = world.defaultStorage.NextEntity();
			_radii.Add(nextEntity, new());
			// 5
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());

			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view    = builder.With<Position>().With<Acceleration>().With<Radius>().Build();

			// Act
			var entities = CollectEntities(ref view);
		
			// Assert
			Assert.AreEqual(1, view.Size);
			Assert.AreEqual(1, entities[0].index);
		}
		
		[Test]
		public void HasAndExclude_SomeOverlap()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			// 3
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			// 4
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 5
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
		
			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view    = builder.With<Position>().With<Acceleration>().Exclude<Radius>().Build();
		
			// Act
			var entities = CollectEntities(ref view);
		
			// Assert
			Assert.AreEqual(2, view.Size);
			Assert.AreEqual(0, entities[0].index);
			Assert.AreEqual(5, entities[1].index);
		}
		
		[Test]
		public void HasAndExclude_FullOverlap()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 3
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 4
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
			// 5
			nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			_radii.Add(nextEntity, new());
		
			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view    = builder.With<Position>().With<Acceleration>().Exclude<Radius>().Build();
		
			// Act
			CollectEntities(ref view);
		
			// Assert
			Assert.AreEqual(0, view.Size);
		}
		
		[Test]
		public void OneGet_TypeCheck()
		{
			var view = new ComponentsViewBuilder(world.defaultStorage).Build<Acceleration>();
			foreach (var iter in view)
			{
				// ReSharper disable once SuggestVarOrType_SimpleTypes
				Acceleration component = iter.Get0();
			}
		}
		
		[Test]
		public void TwoGet_TypeCheck()
		{
			var view = new ComponentsViewBuilder(world.defaultStorage).Build<Acceleration, Circle>();
			foreach (var iter in view)
			{
				// ReSharper disable SuggestVarOrType_SimpleTypes
				Acceleration acc    = iter.Get0();
				Circle       circle = iter.Get1();
				// ReSharper restore SuggestVarOrType_SimpleTypes
			}
		}
		
		[Test]
		public void ThreeGet_TypeCheck()
		{
			var view = new ComponentsViewBuilder(world.defaultStorage).Build<Acceleration, Circle, Borders>();
			foreach (var iter in view)
			{
				// ReSharper disable SuggestVarOrType_SimpleTypes
				Acceleration acc     = iter.Get0();
				Circle       circle  = iter.Get1();
				Borders      borders = iter.Get2();
				// ReSharper restore SuggestVarOrType_SimpleTypes
			}
		}
		
		[Test]
		public void FourGet_TypeCheck()
		{
			var view = new ComponentsViewBuilder(world.defaultStorage).Build<Acceleration, Circle, Borders, Position>();
			foreach (var iter in view)
			{
				// ReSharper disable SuggestVarOrType_SimpleTypes
				Acceleration acc      = iter.Get0();
				Circle       circle   = iter.Get1();
				Borders      borders  = iter.Get2();
				Position     position = iter.Get3();
				// ReSharper restore SuggestVarOrType_SimpleTypes
			}
		}
		
		[Test]
		public void OneGet_WithHasAndExclude_Update_Updated()
		{
			// Arrange
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
		
			var acceleration = new Acceleration { x = 5, y = -5, z = 10, };

			var builder = new ComponentsViewBuilder(world.defaultStorage);
			var view = builder.With<Position>().Exclude<Circle>().Build<Acceleration>();
		
			// Act
			foreach (var iter in view)
			{
				ref var acc = ref iter.Get0();
				acc = acceleration;
			}
			
			// Assert
			Assert.AreEqual(acceleration, _accelerations.Value(nextEntity));
		}
		
		[Test]
		public void OneGet_NoFilter_Remove_Removed()
		{
			// Arrange
			// 0
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());
			// 1
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new());
			// 2
			nextEntity = world.defaultStorage.NextEntity();
			_radii.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new());

			var view = new ComponentsViewBuilder(world.defaultStorage).Build<Acceleration>();
		
			// Act
			foreach (var iter in view)
			{
				iter.Remove0();
			}
			var entities = new List<Entity>();
			foreach (var iter in view)
			{
				entities.Add(iter.entity);
			}
			
			// Assert
			Assert.AreEqual(0, entities.Count);
		}
		
		private static List<Entity> CollectEntities(ref ComponentsView view)
		{
			var entities = new List<Entity>();
			foreach (var entity in view)
			{
				entities.Add(entity);
			}
			return entities;
		}
	}
}
