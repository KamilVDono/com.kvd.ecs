using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.ECS.GeneralTests.Components;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests
{
	public class WorldSerialization : EcsTestsBase
	{
		ComponentListPtr<Position> _positions;
		ComponentListPtr<Acceleration> _accelerations;
		ComponentsStorage _secondStorage;
		ComponentListPtr<Acceleration> _secondAccelerations;

		World _restoredWorld;
		ComponentsStorage _secondStorageRestored;

		protected override async Task OnSetup()
		{
			await world.RegisterSystem(new TestSystem());
			_positions           = world.defaultStorage.ListPtr<Position>();
			_accelerations       = world.defaultStorage.ListPtr<Acceleration>();
			_secondStorage       = world.RegisterComponentsStorage(new(15), new ComponentsStorage());
			_secondAccelerations = _secondStorage.ListPtr<Acceleration>();
			_restoredWorld       = new(Array.Empty<IBootstrapable>());
			await _restoredWorld.RegisterSystem(new TestSystem());
			_secondStorageRestored = _restoredWorld.RegisterComponentsStorage(new(15), new ComponentsStorage());
			await base.OnSetup();
		}
		
		[Test]
		public void SerializeAndDeserializeWorld()
		{
			// Arrange
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.AsList().Add(nextEntity, new());
			_accelerations.AsList().Add(nextEntity, new() { x = 1, y = 1, z = 1, });
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.AsList().Add(nextEntity, new() { x = 2, y = 2, z = 2, });

			nextEntity = _secondStorage.NextEntity();
			_secondAccelerations.AsList().Add(nextEntity, new() { x = 2, y = 2, z = 2, });

			var border = new Borders { xs = new(1, 2), ys = new(3, 4), };
			world.defaultStorage.Singleton(border);

			var circle = new Circle { id = 5, };
			_secondStorage.Singleton(circle, true);
			
			// Act
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);
			world.Save(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);
			_restoredWorld.Restore(reader).GetAwaiter().GetResult();

			// Assert
			var singletonBorder = _restoredWorld.defaultStorage.Singleton<Borders>();
			Assert.AreEqual(border, singletonBorder);
			var singletonCircle = _secondStorageRestored.Singleton<Circle>();
			Assert.AreEqual(circle, singletonCircle);
			Assert.AreEqual(world.defaultStorage.NextEntity(), _restoredWorld.defaultStorage.NextEntity());
			Assert.AreEqual(_secondStorage.NextEntity(), _secondStorageRestored.NextEntity());
			Assert.AreEqual(_secondStorage.NextEntity(), _secondStorageRestored.NextEntity());
			Assert.AreEqual(_positions.AsList().Value(0), world.defaultStorage.ListPtr<Position>().AsList().Value(0));
			Assert.AreEqual(_accelerations.AsList().Value(0), world.defaultStorage.ListPtr<Acceleration>().AsList().Value(0));
			Assert.AreEqual(_accelerations.AsList().Value(1), world.defaultStorage.ListPtr<Acceleration>().AsList().Value(1));
			Assert.AreEqual(_secondAccelerations.AsList().Value(0), _secondStorageRestored.ListPtr<Acceleration>().AsList().Value(0));
		}

		class TestSystem : SystemBase
		{
			public bool restored;
			
			protected override void Update()
			{
				
			}

			protected override UniTask RestoreSetup()
			{
				restored = true;
				return base.RestoreSetup();
			}
		}
	}
}
