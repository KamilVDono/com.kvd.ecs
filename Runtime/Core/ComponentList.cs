using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;
using KVD.ECS.Serializers;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Profiling;
using UnityEngine.Assertions;

#nullable enable

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public abstract class ComponentListConstants
	{
		public const int InitialCapacity = 128;
		
#if DEBUG
		public static readonly ProfilerMarker EnsureSizeMarker = new("SparseList.EnsureSize");
		public static readonly ProfilerMarker AddMarker = new("SparseList.Add");
		public static readonly ProfilerMarker BulkAddMarker = new("SparseList.BulkAdd");
		public static readonly ProfilerMarker RemoveMarker = new("SparseList.Remove");
#endif
	}
	
	public interface IReadonlyComponentList
	{
		public BigBitmask EntitiesMask{ get; }
		public int Length{ get; }
		public int EntitiesVersion{ get; }
	}
	
	public interface IReadonlyComponentListView<T> : IReadonlyComponentList where T : struct, IComponent
	{
		public IReadonlyComponentListView<T> Sync();
	}

	public interface IComponentList : IReadonlyComponentList
	{
		public int[] IndexByEntity{ get; }
		public int[] EntityByIndex{ get; }
		public int Capacity{ get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(Entity entity);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public object ValueAsObject(Entity entity);
		public void AddByObject(Entity entity, object value);
		public void AddOrReplaceAsObject(Entity entity, object value);
		public void BulkAdd(RentedArray<Entity> entities);
		public bool Remove(int entity);
		public void ClearSingleFrameEntities();
		public void Destroy();
		public void Serialize(BinaryWriter writer);
	}

	public interface IComponentList<T> : IComponentList, IReadonlyComponentListView<T> where T : struct, IComponent
	{
		IReadonlyComponentListView<T> IReadonlyComponentListView<T>.Sync()
		{
			return this;
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentList<T> : IComponentList<T> where T : struct, IComponent
	{
		private const char ControlCharacter = 'c';
		private static T _dummyRefReturn;

		private readonly BigBitmask _entitiesMask;
		private readonly IComponentSerializer<T>? _serializer;
		private readonly List<int> _singleFrameComponents = new(12);
		
		private int[] _indexByEntity;
		private int[] _entityByIndex;
		private int _length;
		private int _capacity;
		private T[] _values;
		private int _entitiesVersion;

		public BigBitmask EntitiesMask => _entitiesMask;
		public int EntitiesVersion => _entitiesVersion;
		
		public int[] IndexByEntity => _indexByEntity;
		public int[] EntityByIndex => _entityByIndex;
		public int Capacity => _capacity;
		public int Length => _length;
		public T[] DenseArray => _values;
		public ArraySegment<T> ValidDenseArray => new(_values, 0, _length);

		public ComponentList(int capacity = ComponentListConstants.InitialCapacity)
		{
			_capacity      = Math.Max(capacity, 64);
			_length        = 0;
			_indexByEntity       = new int[_capacity];
			Array.Fill(_indexByEntity, -1);
			_entityByIndex = new int[_capacity];
			_entitiesMask  = new(_capacity);
			_values        = new T[_capacity];
			_serializer    = SerializersLibrary.Serializer<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(Entity entity) => Index(entity) >= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Value(int entity) => ref _values[_indexByEntity[entity]];
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryValue(int entity, out bool has)
		{
			var index = Index(entity);
			has = index >= 0;
			if (has)
			{
				return ref _values[index];
			}
			return ref _dummyRefReturn;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use with care and as last choice", false),]
		public object ValueAsObject(Entity entity) => _values[_indexByEntity[entity]];

		public void BulkAdd(RentedArray<Entity> entities) => BulkAdd(entities, default);

		public void BulkAdd(RentedArray<Entity> entities, T value)
		{
#if DEBUG
			using var marker = ComponentListConstants.BulkAddMarker.Auto();
#endif
			
			var startSize = _length;
			_length += entities.Length;
			EnsureSize(entities[^1]);
			
			for (var i = 0; i < entities.Length; i++)
			{
				var index  = startSize+i;
				var entityIndex = entities[i].index;
				_indexByEntity[entityIndex] = index;
				_entityByIndex[index] = entities[i];
				_values[index]   = value;
				_entitiesMask.Set(entityIndex, true);
			}

			_entitiesVersion++;
		}

		[Obsolete("Use with care and as last choice", false)]
		public void AddByObject(Entity entity, object value) => Add(entity, (T)value);

		public void Add(Entity e, T value)
		{
#if DEBUG
			using var marker = ComponentListConstants.AddMarker.Auto();
			if (Has(e))
			{
				throw new ArgumentException($"Entity [{e}] already present in SparseList");
			}
#endif
			InternalAddSafe(e, value);
		}

		[Obsolete("Use with care and as last choice", false)]
		public void AddOrReplaceAsObject(Entity entity, object value)
		{
			AddOrReplace(entity, (T)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrReplace(Entity entity, T value)
		{
			// Just replace value
			var index = Index(entity);
			if (index > -1)
			{
				_values[index] = value;
			}
			// Create new value
			else
			{
				InternalAddSafe(entity, value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddSingleFrame(Entity e, T value)
		{
			Add(e, value);
			_singleFrameComponents.Add(e);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearSingleFrameEntities()
		{
			foreach (var singleFrameComponent in _singleFrameComponents)
			{
				Remove(singleFrameComponent);
			}
			_singleFrameComponents.Clear();
		}

		public bool Remove(int entity)
		{
#if DEBUG
			using var marker = ComponentListConstants.RemoveMarker.Auto();
#endif
			var index = _indexByEntity[entity];
			if (index <= -1)
			{
				// The entity is not present in array
				return false;
			}
			_values[index].Dispose();
			
			_entitiesVersion++;
			
			--_length;
			if (_length <= 0)
			{
				_indexByEntity[entity] = -1;
				_entitiesMask.Set(entity, false);
				return true;
			}
			// _length was decreased so it's pointing to last element\
			// Swap last value with removed one
			_values[index]   = _values[_length];
			// Remove entity from list
			_entitiesMask.Set(entity, false);

			var entityIndexToSwap = _entityByIndex[_length];
			_indexByEntity[entityIndexToSwap] = index;
			_entityByIndex[index]       = entityIndexToSwap;
			
			_indexByEntity[entity] = -1;
			return true;
		}

		public void Destroy()
		{
			for (var i = 0; i < _length; ++i)
			{
				_values[i].Dispose();
			}
			
			_length   = 0;
			_capacity = 0;
		}

		public void EnsureCapacity(int capacity, Entity lastEntity)
		{
			IncreaseSizeDense(capacity);
			IncreaseSizeEntities(lastEntity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Index(Entity e)
		{
			return Index(e.index);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Index(int e)
		{
			return e < _indexByEntity.Length ? _indexByEntity[e]: -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InternalAddSafe(Entity e, T value)
		{
			++_length;
			EnsureSize(e);
			var entityIndex = e.index;
			_indexByEntity[entityIndex] = _length-1;
			_entityByIndex[_length-1]           = entityIndex;
			_values[_length-1]                  = value;
			_entitiesMask.Set(entityIndex, true);
			
			_entitiesVersion++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureSize(Entity lastEntity)
		{
#if DEBUG
			using var marker = ComponentListConstants.EnsureSizeMarker.Auto();
#endif

			IncreaseSizeDense(_length);
			IncreaseSizeEntities(lastEntity);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseSizeDense(int requireSize)
		{
			if (requireSize <= _capacity)
			{
				return;
			}
			
			while (_capacity < requireSize)
			{
				_capacity <<= 2;
			}
			
			Array.Resize(ref _values, _capacity);
			Array.Resize(ref _entityByIndex, _capacity);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncreaseSizeEntities(Entity lastEntity)
		{
			var sizeRequest = lastEntity.index+1;
			var oldSize     = _indexByEntity.Length;
			
			if (sizeRequest <= oldSize)
			{
				return;
			}

			--sizeRequest;
			sizeRequest <<= 2;
			while (sizeRequest < oldSize)
			{
				sizeRequest <<= 2;
			}

			Array.Resize(ref _indexByEntity, sizeRequest);
			Array.Fill(_indexByEntity, -1, oldSize, sizeRequest-oldSize);
			_entitiesMask.EnsureCapacity(sizeRequest);
		}
		
		#region Serialization
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(ControlCharacter);
			writer.Write(_capacity);
			writer.Write(_length);
			
			if (_serializer == null)
			{
				return;
			}
			
			writer.Write(ControlCharacter);
			_entitiesMask.Serialize(writer);
			writer.Write(ControlCharacter);
			foreach (var index in _indexByEntity)
			{
				writer.Write(index);
			}
			writer.Write(ControlCharacter);
			for (var i = 0; i < _length; i++)
			{
				_serializer.WriteBytes(_values[i], writer);
			}
			writer.Write(ControlCharacter);
		}

		public static ComponentList<T>? Deserialize(BinaryReader reader)
		{
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			var capacity = reader.ReadInt32();
			var length   = reader.ReadInt32();
			var list     = new ComponentList<T>(capacity);
			list.Deserialize(reader, length);
			return list._serializer != null ? list : null;
		}
		
		private void Deserialize(BinaryReader reader, int length)
		{
			if (_serializer == null)
			{
				Destroy();
				return;
			}
			
			_length = length;

			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			_entitiesMask.Deserialize(reader);
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			for (var i = 0; i < _capacity; i++)
			{
				var index = reader.ReadInt32();
				_indexByEntity[i] = index;
			}
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			for (var i = 0; i < _length; i++)
			{
				_values[i] = _serializer.ReadBytes(reader);
			}
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		}
		#endregion Serialization
	}
}
