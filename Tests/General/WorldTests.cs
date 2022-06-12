using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using NUnit.Framework;

#nullable disable

namespace KVD.ECS.GeneralTests
{
	// Singletons will be removed so do not cover them
	// Serialization would be another tests suit so them are not here
	public class WorldTests
	{
		[Test]
		public void Initialization()
		{
			// Arrange
			var bootstrapable = new Bootstrapable();
			var world          = new World(new IBootstrapable[] { bootstrapable, });
			
			// Act
			world.Initialize().GetAwaiter().GetResult();
			
			// Assert
			Assert.IsTrue(bootstrapable.Initialized);
			Assert.IsFalse(bootstrapable.Restored);
		}
		
		[Test]
		public void RegisterSystem_BeforeInitialization_InitializedAtInitializationTime()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act&Assert
			world.RegisterSystem(system).GetAwaiter().GetResult();
			Assert.IsFalse(system.Initialized);
			world.Initialize().GetAwaiter().GetResult();
			Assert.IsTrue(system.Initialized);
			Assert.Zero(system.UpdatesCount);
		}
		
		[Test]
		public void RegisterSystem_AfterInitialization_Initialized()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act&Assert
			world.Initialize().GetAwaiter().GetResult();
			world.RegisterSystem(system).GetAwaiter().GetResult();
			Assert.IsTrue(system.Initialized);
			Assert.Zero(system.UpdatesCount);
		}

		[Test]
		public void GetSystem_SystemNotPresent()
		{
			// Arrange
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();

			// Assert
			Assert.Catch(() =>
			{
				var _ = world.System<System>();
			});
		}
		
		[Test]
		public void GetSystem_SystemPresent()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();
			world.RegisterSystem(system).GetAwaiter().GetResult();
			var systemFromWorld = world.System<System>();
			
			// Assert
			Assert.AreSame(system, systemFromWorld);
		}
		
		[Test]
		public void GetSystem_InnerSystem_SystemPresent()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();
			world.RegisterSystem(system).GetAwaiter().GetResult();
			var innerSystem = world.System<InnerSystem>();
			
			// Assert
			Assert.NotNull(innerSystem);
		}
		
		[Test]
		public void Update()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();
			world.RegisterSystem(system).GetAwaiter().GetResult();
			
			// Assert
			Assert.Zero(system.UpdatesCount);
			world.Update();
			Assert.NotZero(system.UpdatesCount);
		}
		
		[Test]
		public void Destroy()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();
			world.RegisterSystem(system).GetAwaiter().GetResult();
			world.Destroy().GetAwaiter().GetResult();
			
			// Assert
			Assert.IsTrue(system.Destroyed);
		}
		
		[Test]
		public void RemoveSystem_SystemRemovedAndDestroyed()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();
			world.RegisterSystem(system).GetAwaiter().GetResult();
			world.RemoveSystem(system).GetAwaiter().GetResult();
			
			// Assert
			Assert.IsTrue(system.Destroyed);
		}
		
		[Test]
		public void RemoveSystem_SystemNotPresent()
		{
			// Arrange
			var system = new System();
			var world  = new World(Array.Empty<IBootstrapable>());
			
			// Act
			world.Initialize().GetAwaiter().GetResult();

			// Assert
			Assert.DoesNotThrow(() => { world.RemoveSystem(system).GetAwaiter().GetResult(); });
		}

		[Test]
		public void RegisterStorage()
		{
			// Arrange
			var storageKey = new ComponentsStorageKey("Tests");
			var world      = new World(Array.Empty<IBootstrapable>());
			world.Initialize().GetAwaiter().GetResult();
			
			// Act
			//world.RegisterComponentsStorage(storageKey, new ComponentsStorage());
			//var storage = world.Storage(storageKey);
			
			// Assert
			//Assert.NotNull(storage);
		}
		
		[Test]
		public void GetStorage_StorageNotPresent_Throws()
		{
			// Arrange
			var storageKey = new ComponentsStorageKey("Tests");
			var world      = new World(Array.Empty<IBootstrapable>());
			world.Initialize().GetAwaiter().GetResult();
			
			// Act&Assert
			//Assert.Catch(() => world.Storage(storageKey));
		}
		
		private class System : ISystem
		{
			// ReSharper disable once UnassignedGetOnlyAutoProperty
			public World World{ get; }
			
			// ReSharper disable once UnassignedGetOnlyAutoProperty
			public string Name{ get; }
			public IReadOnlyList<ISystem> InternalSystems => new List<ISystem> { new InnerSystem(), };
			
			public bool Initialized{ get; private set; }
			public bool Restored{ get; private set; }
			public bool Destroyed{ get; private set; }
			public int UpdatesCount{ get; private set; }

			public void Prepare()
			{
			}
			
			public UniTask Init(World world)
			{
				Initialized = true;
				return UniTask.CompletedTask;
			}
			public UniTask Restore(World world)
			{
				Restored = true;
				return UniTask.CompletedTask;
			}
			public void DoUpdate()
			{
				UpdatesCount++;
			}
			public UniTask Destroy()
			{
				Destroyed = true;
				return UniTask.CompletedTask;
			}
		}
		
		private class InnerSystem : ISystem
		{
			// ReSharper disable once UnassignedGetOnlyAutoProperty
			public World World{ get; }
			
			// ReSharper disable once UnassignedGetOnlyAutoProperty
			public string Name{ get; }
			public IReadOnlyList<ISystem> InternalSystems => new List<ISystem>();
			
			public bool Initialized{ get; private set; }
			public bool Restored{ get; private set; }
			public bool Destroyed{ get; private set; }
			public int UpdatesCount{ get; private set; }

			public void Prepare()
			{
			}
			
			public UniTask Init(World world)
			{
				Initialized = true;
				return UniTask.CompletedTask;
			}
			public UniTask Restore(World world)
			{
				Restored = true;
				return UniTask.CompletedTask;
			}
			public void DoUpdate()
			{
				UpdatesCount++;
			}
			public UniTask Destroy()
			{
				Destroyed = true;
				return UniTask.CompletedTask;
			}
		}
		
		private class Bootstrapable : IBootstrapable
		{
			public bool Initialized{ get; private set; }
			public bool Restored{ get; private set; }
			
			public UniTask Init(World world)
			{
				Initialized = true;
				return UniTask.CompletedTask;
			}
			
			public UniTask Restore(World world)
			{
				Restored = true;
				return UniTask.CompletedTask;
			}
		}
	}
}
