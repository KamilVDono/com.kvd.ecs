using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using KVD.ECS.Serializers;
using KVD.ECS.Systems;
using UnityEngine.Assertions;

#nullable enable

namespace KVD.ECS
{
	public class World
	{
		protected const char ControlCharacter = 'w';
		public readonly ComponentsStorage defaultStorage;

		private readonly Dictionary<ComponentsStorageKey, ComponentsStorage> _componentsStorages = new(4, ComponentsStorageKey.ComponentStorageKeyComparer);
		private readonly List<ComponentsStorage> _componentsStoragesList = new(4);
		private readonly IBootstrapable[] _bootstrapables;
		private readonly Dictionary<Type, object> _singletons = new();
		protected readonly List<ISystem> systems = new();

		public bool IsRestored{ get; private set; }
		public IReadOnlyList<ComponentsStorage> AllComponentsStorages => _componentsStoragesList;

		public World(IBootstrapable[] bootstrapables)
		{
			_bootstrapables = bootstrapables;
			
			defaultStorage = RegisterComponentsStorage(ComponentsStorageKey.Default, new ComponentsStorage());
		}

		#region Lifetime
		public async UniTask Initialize()
		{
			IsRestored = false;
			await Bootstrap();
			await InitInitialSystems();
		}
		
		public async UniTask Restore(BinaryReader reader)
		{
			Deserialize(reader);
			IsRestored = true;
			await Bootstrap();
			await RestoreSystems();
		}

		public async UniTask Destroy()
		{
			foreach (var system in systems)
			{
				await system.Destroy();
			}
			systems.Clear();

			var values = _componentsStorages.Values.ToArray();
			_componentsStorages.Clear();
			foreach (var components in values)
			{
				components.Destroy();
			}
		}

		public void Update()
		{
			var count = systems.Count;
			for (var i = 0; i < count; i++)
			{
				var system = systems[i];
				system.DoUpdate();
			}
			for (var i = 0; i < _componentsStoragesList.Count; i++)
			{
				_componentsStoragesList[i].SafetyCheck();
			}
		}
		#endregion Lifetime

		#region Systems
		public async UniTask RegisterSystem(ISystem system)
		{
			systems.Add(system);
			await system.Init(this);
		}

		public async UniTask RemoveSystem(ISystem system)
		{
			systems.Remove(system);
			await system.Destroy();
		}
		
		public T System<T>() where T : class, ISystem
		{
			for (var i = 0; i < systems.Count; ++i)
			{
				var system = FindSystem<T>(systems[i]);
				if (system != null)
				{
					return system;
				}
			}
			throw new ArgumentException($"There is no system of {typeof(T).Name}");
		}

		private T? FindSystem<T>(ISystem system) where T : class, ISystem
		{
			{
				if (system is T targetSystem)
				{
					return targetSystem;
				}
			}

			for (var i = 0; i < system.InternalSystems.Count; i++)
			{
				var internalSystem = system.InternalSystems[i];
				var targetSystem   = FindSystem<T>(internalSystem);
				if (targetSystem != null)
				{
					return targetSystem;
				}
			}

			return null;
		}
		
		private async UniTask InitInitialSystems()
		{
			foreach (var system in systems)
			{
				await system.Init(this);
			}
		}
		
		private async UniTask RestoreSystems()
		{
			foreach (var system in systems)
			{
				await system.Restore(this);
			}
		}
		#endregion Systems

		#region ComponentsStorages
		public T Storage<T>(ComponentsStorageKey key) where T : ComponentsStorage
		{
			return (T)_componentsStorages[key];
		}
		
		public ComponentsStorage Storage(ComponentsStorageKey key)
		{
			return _componentsStorages[key];
		}

		protected T RegisterComponentsStorage<T>(ComponentsStorageKey key, T storage) where T : ComponentsStorage
		{
			_componentsStorages[key] = storage;
			_componentsStoragesList.Add(storage);
			return storage;
		}
		#endregion ComponentsStorages

		#region Singletons
		// TODO: Remove singletons in favour of setup systems thru editor
		public T RegisterSingleton<T>(T value)
		{
			_singletons[typeof(T)] = value!;
			return value;
		}

		public T Singleton<T>() 
		{
			if (_singletons.TryGetValue(typeof(T), out var value) && value is T casted)
			{
				return casted;
			}
			throw new DataException($"There is no {typeof(T).Name} in World");
		}
		
		private async UniTask Bootstrap()
		{
			foreach (var bootstrapable in _bootstrapables)
			{
				await bootstrapable.Init(this);
			}
		}
		#endregion Singletons

		#region Serialization
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(ControlCharacter);
			
			writer.Write(systems.Count);
			foreach (var system in systems)
			{
				SerializersHelper.ToBytesStatelessInstance(system, writer);
			}
			writer.Write(ControlCharacter);

			foreach (var key in _componentsStorages.Keys)
			{
				SerializersHelper.ToBytesStorageKey(key, writer);
			}
			
			writer.Write(ControlCharacter);
		}
		
		public void Deserialize(BinaryReader reader)
		{
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			
			var systemsCount = reader.ReadInt32();
			if (systems.Count == systemsCount)
			{
				for (var i = 0; i < systemsCount; i++)
				{
					SerializersHelper.FromBytesType(reader);
				}
			}
			else
			{
				var systemTypes = systems.Select(s => s.GetType()).ToHashSet();
				for (var i = 0; i < systemsCount; i++)
				{
					var systemType = SerializersHelper.FromBytesType(reader);
					if (systemTypes.Contains(systemType))
					{
						continue;
					}
					var system = (SystemBase)Activator.CreateInstance(systemType);
					systems.Add(system);
				}
			}
			
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			
			for (var i = 0; i < _componentsStorages.Count; i++)
			{
				var key = SerializersHelper.FromBytesStorageKey(reader);
				var storage = _componentsStorages[key];
				storage.Deserialize(this, reader);
			}

			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		}
		#endregion Serialization
	}
}
