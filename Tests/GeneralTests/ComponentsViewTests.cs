using System.Collections.Generic;
using System.Threading.Tasks;
using KVD.ECS.Entities;
using KVD.ECS.Tests.Components;
using NUnit.Framework;
using Position = KVD.ECS.Tests.Components.Position;

#nullable disable

namespace KVD.ECS.Tests
{
	public class ComponentsViewTests : EcsTestsBase
	{
		private SparseList<Position> _positions;
		private SparseList<Radius> _radii;
		private SparseList<Acceleration> _accelerations;
		
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
			using ComponentsView view = new(ViewDescriptor.New<Position, Acceleration>(world.defaultStorage));

			// Act
			ZipForEntities(view);

			// Assert
			Assert.AreEqual(0, view.Size);
		}
		
		[Test]
		public void Has_Empty()
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

			using ComponentsView view = new(
				ViewDescriptor.New<Radius>(world.defaultStorage)
				);

			// Act
			ZipForEntities(view);

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

			using ComponentsView view = new(ViewDescriptor.New<Position, Acceleration>(world.defaultStorage));

			// Act
			ZipForEntities(view);

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

			using ComponentsView view = new(ViewDescriptor.New<Position, Acceleration>(world.defaultStorage));

			// Act && Assert
			var entities = ZipForEntities(view);
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

			using ComponentsView view = new(
				ViewDescriptor.New<Position, Acceleration, Radius>(world.defaultStorage)
				);

			// Act
			var entities = ZipForEntities(view);

			// Assert
			Assert.AreEqual(1, view.Size);
			Assert.AreEqual(1, entities[0].index);
		}

		[Test]
		public void AllFilters_SomeOverlap()
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

			using ComponentsView view = new(
				ViewDescriptor.New<Position, Acceleration>(world.defaultStorage, new[] { typeof(Radius) })
				);

			// Act
			var entities = ZipForEntities(view);

			// Assert
			Assert.AreEqual(2, view.Size);
			Assert.AreEqual(0, entities[0].index);
			Assert.AreEqual(5, entities[1].index);
		}
		
		[Test]
		public void AllFilters_FullOverlap()
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

			using ComponentsView view = new(
				ViewDescriptor.New<Position, Acceleration>(world.defaultStorage, new[] { typeof(Radius) })
				);

			// Act
			ZipForEntities(view);

			// Assert
			Assert.AreEqual(0, view.Size);
		}

		private static List<Entity> ZipForEntities(ComponentsView view)
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
