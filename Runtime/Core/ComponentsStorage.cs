

#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Threading.Tasks;
using KVD.ECS.ComponentHelpers;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Entities.Allocators;
using KVD.ECS.Core.Helpers;
using KVD.ECS.Serializers;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public class ComponentsStorage
	{
		private const char ControlCharacter = 's';
		private const char HasStorageKeyCharacter = 'S';
		private static readonly Type MonoComponent = typeof(IMonoComponent);
		private static readonly Type SparseListGenericType = typeof(ComponentList<>);
	
		#region DEBUG
		#if DEBUG
		private readonly Dictionary<Entity, string> _debugNames = new(Entity.IndexComparer);
		#endif
		private ComponentsStorageKey? _storageKey;
		#endregion Debug
		private readonly Dictionary<Type, IComponentList> _listsByType = new(16);
		private readonly List<IComponentList> _lists = new(16);
		
		private readonly SingletonComponentsStorage _singletons = new(64);
		private readonly List<int> _singleFrameSingletons = new(4);
	
		private IEntityAllocator _entityAllocator;
	
		public Entity CurrentEntity{ get; private set; } = Entity.Null;
		public IReadOnlyList<IComponentList> AllLists => _lists;
	
		public ComponentsStorage(ComponentsStorageKey? storageKey = null, IEntityAllocator? entityAllocator = null)
		{
			_entityAllocator = entityAllocator ?? new ContinuousEntitiesAllocator();
			_storageKey      = storageKey;
		}
	
		#region Lists
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentList<T>? TryList<T>() where T : struct, IComponent
		{
			var key = typeof(T);
			if (_listsByType.TryGetValue(key, out var list))
			{
				return (ComponentList<T>)list;
			}
			return null;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentList<T> List<T>(int initialSize = ComponentListConstants.InitialCapacity) where T : struct, IComponent
		{
			var key = typeof(T);
			if (!_listsByType.TryGetValue(key, out var list))
			{
				list = new ComponentList<T>(initialSize);
				_listsByType.Add(key, list);
				_lists.Add(list);
			}
			return (ComponentList<T>)list;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IComponentList List(Type componentType, int initialSize = ComponentListConstants.InitialCapacity)
		{
			CheckComponentType(componentType);
			if (!_listsByType.TryGetValue(componentType, out var list))
			{
				var storageType = typeof(ComponentList<>).MakeGenericType(componentType);
				list = (IComponentList)Activator.CreateInstance(storageType, initialSize);
				_listsByType.Add(componentType, list);
				_lists.Add(list);
			}
			return list;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadonlyComponentList ReadonlyListView(Type componentType)
		{
			CheckComponentType(componentType);
			if (_listsByType.TryGetValue(componentType, out var list))
			{
				return list;
			}
			var storageType = typeof(ReadonlyComponentListViewView<>).MakeGenericType(componentType);
			return (IReadonlyComponentList)Activator.CreateInstance(storageType, this);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadonlyComponentListView<T> ReadonlyListView<T>() where T : struct, IComponent
		{
			var key = typeof(T);
			return _listsByType.TryGetValue(key, out var list) ?
				(IReadonlyComponentListView<T>)list :
				new ReadonlyComponentListViewView<T>(this);
		}
		#endregion Lists
	
		#region Entities
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetAllocator(IEntityAllocator allocator)
		{
			if (CurrentEntity != Entity.Null)
			{
				return false;
			}
			_entityAllocator = allocator;
			return true;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity NextEntity()
		{
			CurrentEntity = _entityAllocator.Allocate();
			return CurrentEntity;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity NextEntity(string name)
		{
			CurrentEntity = _entityAllocator.Allocate();
			#if DEBUG
			name                       = string.IsNullOrWhiteSpace(name) ? CurrentEntity.index.ToString() : name;
			_debugNames[CurrentEntity] = name;
			#endif
			return CurrentEntity;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReturnEntity(Entity entity)
		{
			_entityAllocator.Return(entity);
		}
		#endregion Entities
		
		public void ClearSingleFrameEntities()
		{
			for (var i = 0; i < _lists.Count; i++)
			{
				_lists[i].ClearSingleFrameEntities();
			}
			for (var i = 0; i < _singleFrameSingletons.Count; i++)
			{
				_singletons.Remove(_singleFrameSingletons[i]);
			}
			_singleFrameSingletons.Clear();
		}
	
		#region Singleton components
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Singleton<T>() where T : struct, IComponent
		{
			return ref _singletons.Value<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T SingletonOrNew<T>() where T : struct, IComponent
		{
			if (!_singletons.Has<T>())
			{
				_singletons.Add(default(T));
			}
			return ref _singletons.Value<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SingletonOrDefault<T>() where T : struct, IComponent
		{
			return !_singletons.Has<T>() ? default : _singletons.Value<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TrySingleton<T>(out bool has) where T : struct, IComponent
		{
			return ref _singletons.TryValue<T>(out has);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasSingleton<T>() where T : struct, IComponent
		{
			return _singletons.Has<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Singleton<T>(T singleton, bool singleFrame = false) where T : struct, IComponent
		{
			_singletons.Add(singleton);
			if (singleFrame)
			{
				_singleFrameSingletons.Add(SingletonComponentsStorage.Index<T>());
			}
			else
			{
				_singleFrameSingletons.Remove(SingletonComponentsStorage.Index<T>());
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSingleton<T>() where T : struct, IComponent
		{
			_singletons.Remove<T>();
			_singleFrameSingletons.Remove(SingletonComponentsStorage.Index<T>());
		}
		#endregion Singleton components
		
		public virtual void Destroy()
		{
			foreach (var components in _listsByType.Values)
			{
				components.Destroy();
			}
			_listsByType.Clear();
			_lists.Clear();
	
			_singletons.Clear();
			
			_singleFrameSingletons.Clear();
		}
	
		public override string ToString()
		{
			var estimatedSize = _listsByType.Count*32;
			estimatedSize += 32;
			var stringBuilder = new StringBuilder(estimatedSize);
			if (_storageKey.HasValue)
			{
				#if DEBUG
				stringBuilder.Append(ComponentsStorageKey.Name(_storageKey.Value));
				#else
				stringBuilder.Append(_storageKey.Value);
				#endif
				stringBuilder.Append("; ");
			}
			foreach (var listType in _listsByType.Keys)
			{
				stringBuilder.Append(listType.Name);
				stringBuilder.Append(", ");
			}
			if (_listsByType.Count > 0)
			{
				stringBuilder.Length -= 2;
			}
			return stringBuilder.ToString();
		}
	
		#region Serialization
		public virtual void Serialize(BinaryWriter writer)
		{
			writer.Write(ControlCharacter);
			SerializersHelper.ToBytesType(_entityAllocator.GetType(), writer);
			_entityAllocator.Serialize(writer);
			
			writer.Write(ControlCharacter);
			if (_storageKey.HasValue)
			{
				writer.Write(HasStorageKeyCharacter); //Storage key
				writer.Write(_storageKey.Value.Value);
			}
	
			writer.Write(ControlCharacter);
			byte monoCount = 0;
			foreach (var (type, _) in _listsByType)
			{
				if (MonoComponent.IsAssignableFrom(type))
				{
					monoCount++;
				}
			}
			writer.Write((byte)(_listsByType.Count-monoCount));
			foreach (var (type, sparseList) in _listsByType)
			{
				if (MonoComponent.IsAssignableFrom(type))
				{
					continue;
				}
				SerializersHelper.ToBytesType(type, writer);
				sparseList.Serialize(writer);
			}
			
			writer.Write(ControlCharacter);
			// Unfortunately we need to serialize transform "manually"
			if (monoCount > 0)
			{
				var transformStorage = List<MonoComponentWrapper<Transform>>();
				var entityByIndex    = transformStorage.EntityByIndex;
				var values           = transformStorage.DenseArray;
	
				for (var i = 0; i < transformStorage.Length; i++)
				{
					var entity     = entityByIndex[i];
					var transform = values[i].value;
	
					var position = (float3)transform.position;
					var rotation = (quaternion)transform.rotation;
	
					writer.Write(entity);
					SerializersHelper.ToBytesStruct(position, writer);
					SerializersHelper.ToBytesStruct(rotation, writer);
				}
			}
			
			writer.Write(ControlCharacter);
			_singletons.Serialize(writer);
			
			writer.Write(ControlCharacter);
			writer.Write(CurrentEntity.index);
			writer.Write(ControlCharacter);
		}
		
		public virtual void Deserialize(World world, BinaryReader reader)
		{
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
	
			SerializersHelper.FromBytesStatelessInstance(ref _entityAllocator, reader);
			_entityAllocator.Deserialize(reader);
	
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			if (reader.PeekChar() == HasStorageKeyCharacter)
			{
				reader.ReadChar();
				_storageKey = new ComponentsStorageKey(reader.ReadInt32());
			}
	
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			int count          = reader.ReadByte();
			var readerAsParams = new object[] { reader, };
			for (var i = 0; i < count; i++)
			{
				var componentType  = SerializersHelper.FromBytesType(reader);
				var sparseListType = SparseListGenericType.MakeGenericType(componentType);
				var deserializeMethod = sparseListType.GetMethod(
					"Deserialize", 
					BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public
					)!;
				var storage = deserializeMethod.Invoke(null, readerAsParams) as IComponentList;
				if (storage == null)
				{
					continue;
				}
	
				_listsByType[componentType] = storage;
				_lists.Add(storage);
			}
			
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			if (_listsByType.TryGetValue(typeof(PrefabWrapper), out var prefabWrappers))
			{
				DeserializePrefabs(world, reader, (ComponentList<PrefabWrapper>)prefabWrappers);
			}
			
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			_singletons.Deserialize(reader);
			
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			CurrentEntity = reader.ReadInt32();
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		}
		
		private void DeserializePrefabs(World world, BinaryReader reader, ComponentList<PrefabWrapper> prefabs)
		{
			for (var i = 0; i < prefabs.Length; ++i)
			{
				var        entityIndex = reader.ReadInt32();
				float3     position    = new();
				quaternion rotation    = new();
				SerializersHelper.FromBytesStruct(ref position, reader);
				SerializersHelper.FromBytesStruct(ref rotation, reader);
				var    prefab   = prefabs.Value(entityIndex);
	
				SetupPrefabInstance(world, new(entityIndex), prefab, position, rotation).Forget();
			}
		}
		
		private async UniTaskVoid SetupPrefabInstance(World world, Entity entity, PrefabWrapper prefabWrapper,
			Vector3 position, Quaternion rotation)
		{
			var request  = Addressables.InstantiateAsync(prefabWrapper.prefabKey, position, rotation);
			var instance = await request.Task;
	
			RegisterEntity.SetupPrefabInstance(world, this, entity, request, instance, false);
		}
		#endregion Serialization
	
		#region DEBUG
		[Conditional("UNITY_EDITOR")]
		private void CheckComponentType(Type type)
		{
			var componentInterface = typeof(IComponent);
			if (!type.IsValueType)
			{
				throw new ConstraintException($"Type {type} is not value type, that violate the contract");
			}
	
			if (!componentInterface.IsAssignableFrom(type))
			{
				throw new ConstraintException($"Type {type} is not implement {nameof(IComponent)} interface");
			}
		}
	
		[Conditional("DEBUG")]
		// ReSharper disable once RedundantAssignment
		public void Name(Entity entity, ref string name)
		{
			#if DEBUG
			var _ = _debugNames.TryGetValue(entity, out name) || (name = entity.index.ToString()) != null;
			#endif
		}
		
		[Conditional("DEBUG")]
		public void SafetyCheck()
		{
			#if DEBUG
			_entityAllocator.AssertValidity(this);
			#endif
		}
		#endregion DEBUG
	}
}
