using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KVD.ECS.ComponentHelpers;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.ECS.GeneralTests.Components;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace KVD.ECS.GeneralTests
{
	public class WorldSerialization : EcsTestsBase
	{
		private ComponentList<Position> _positions;
		private ComponentList<Acceleration> _accelerations;
		private ComponentsStorage _secondStorage;
		private ComponentList<Acceleration> _secondAccelerations;

		private World _restoredWorld;
		private ComponentsStorage _secondStorageRestored;

		protected override async Task OnSetup()
		{
			await world.RegisterSystem(new TestSystem());
			_positions           = world.defaultStorage.List<Position>();
			_accelerations       = world.defaultStorage.List<Acceleration>();
			_secondStorage       = world.RegisterComponentsStorage(new(15), new ComponentsStorage());
			_secondAccelerations = _secondStorage.List<Acceleration>();
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
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new() { x = 1, y = 1, z = 1, });
			nextEntity = world.defaultStorage.NextEntity();
			_accelerations.Add(nextEntity, new() { x = 2, y = 2, z = 2, });

			nextEntity = _secondStorage.NextEntity();
			_secondAccelerations.Add(nextEntity, new() { x = 2, y = 2, z = 2, });

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
			Assert.AreEqual(_positions.Value(0), world.defaultStorage.List<Position>().Value(0));
			Assert.AreEqual(_accelerations.Value(0), world.defaultStorage.List<Acceleration>().Value(0));
			Assert.AreEqual(_accelerations.Value(1), world.defaultStorage.List<Acceleration>().Value(1));
			Assert.AreEqual(_secondAccelerations.Value(0), _secondStorageRestored.List<Acceleration>().Value(0));
		}
		
		private class TestSystem : SystemBase
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
