using System;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Profiling;

#nullable enable

namespace KVD.ECS.Core
{
	public interface IComponentsView : IDisposable
	{
		int Size{ get; }
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentsView : IComponentsView
	{
		private readonly IReadonlyComponentList[] _hasComponents;
		private readonly IReadonlyComponentList[] _excludeComponents;
	
		private readonly BigBitmask _validEntities = new(ComponentsViewHelper.PreallocationSize);
		private readonly BigBitmask _excludedEntities = new(ComponentsViewHelper.PreallocationSize);
	
		private RentedArray<int> _entities = new(0);
	
		private readonly bool _onlyWhenStructuralChanges;
		private int _lastVersion;
		private int _lastVersionExcluded;
	
		public int Size => _entities.Length;
	
		public ComponentsView(ComponentsStorage storage,
			Type[] hasComponents, Type[]? excludeComponents = null, 
			bool onlyWhenStructuralChanges = false)
		{
			_hasComponents = new IReadonlyComponentList[hasComponents.Length];
			for (var i = 0; i < hasComponents.Length; i++)
			{
				_hasComponents[i] = storage.ReadonlyListView(hasComponents[i]);
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<IReadonlyComponentList>();
			}
			else
			{
				_excludeComponents = new IReadonlyComponentList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.ReadonlyListView(excludeComponents[i]);
				}
			}
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}
		
		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities);
		}
	
		public void Dispose()
		{
			_entities.Dispose();
		}
	
		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly int _length;
		
			private int _iteration;
	
			public ComponentsIterator(RentedArray<int> entities)
			{
				_entities = entities.array;
				_length   = entities.Length;
	
				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public int Current => _entities[_iteration];
	
			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
	}
	
	// There is overhead associated with using these generic, for common case it should be negligible but
	// In worse case could be five times slower than accessing components by sparseList.Value(entity) (plain ComponentsView)
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentsView<T0> : IComponentsView 
		where T0 : struct, IComponent
	{
		private readonly IReadonlyComponentList[] _hasComponents;
		private readonly IReadonlyComponentList[] _excludeComponents;
	
		private readonly BigBitmask _validEntities = new(ComponentsViewHelper.PreallocationSize);
		private readonly BigBitmask _excludedEntities = new(ComponentsViewHelper.PreallocationSize);
		
		private IReadonlyComponentListView<T0> _componentsList0;
	
		private RentedArray<int> _entities = new(0);
	
		private readonly bool _onlyWhenStructuralChanges;
		private int _lastVersion;
		private int _lastVersionExcluded;
	
		public int Size => _entities.Length;
		
		public ComponentsView(ComponentsStorage storage,
			Type[]? hasComponents = null, Type[]? excludeComponents = null, 
			bool onlyWhenStructuralChanges = false)
		{
			_componentsList0 = storage.ReadonlyListView<T0>();
			
			if (hasComponents == null)
			{
				_hasComponents = new IReadonlyComponentList[] { _componentsList0, };
			}
			else
			{
				_hasComponents    = new IReadonlyComponentList[1 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+1] = storage.ReadonlyListView(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<IReadonlyComponentList>();
			}
			else
			{
				_excludeComponents = new IReadonlyComponentList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.ReadonlyListView(excludeComponents[i]);
				}
			}
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}
	
		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0 = _componentsList0.Sync());
		}
	
		public void Dispose()
		{
			_entities.Dispose();
		}
	
		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly IReadonlyComponentListView<T0> _componentsList0;
			
			private readonly int _length;
			private int _iteration;
	
			public ComponentsIterator(RentedArray<int> entities, IReadonlyComponentListView<T0> componentsList0)
			{
				_componentsList0 = componentsList0;
				_entities        = entities.array;
				_length          = entities.Length;
	
				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities[_iteration], (ComponentList<T0>)_componentsList0);
	
			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly ComponentList<T0> _componentsList0;
	
			public IterationView(int entity, ComponentList<T0> componentsList0)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
			}
	
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref _componentsList0.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.Remove(entity);
			}
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentsView<T0, T1> : IComponentsView 
		where T0 : struct, IComponent
		where T1 : struct, IComponent
	{
		private readonly IReadonlyComponentList[] _hasComponents;
		private readonly IReadonlyComponentList[] _excludeComponents;
	
		private IReadonlyComponentListView<T0> _componentsList0;
		private IReadonlyComponentListView<T1> _componentsList1;
	
		private readonly BigBitmask _validEntities = new(ComponentsViewHelper.PreallocationSize);
		private readonly BigBitmask _excludedEntities = new(ComponentsViewHelper.PreallocationSize);
	
		private RentedArray<int> _entities = new(0);
	
		private readonly bool _onlyWhenStructuralChanges;
		private int _lastVersion;
		private int _lastVersionExcluded;
	
		public int Size => _entities.Length;
		
		public ComponentsView(ComponentsStorage storage,
			Type[]? hasComponents = null, Type[]? excludeComponents = null,
			bool onlyWhenStructuralChanges = false)
		{
			_componentsList0 = storage.ReadonlyListView<T0>();
			_componentsList1 = storage.ReadonlyListView<T1>();
			
			if (hasComponents == null)
			{
				_hasComponents = new IReadonlyComponentList[] { _componentsList0, _componentsList1, };
			}
			else
			{
				_hasComponents    = new IReadonlyComponentList[2 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				_hasComponents[1] = _componentsList1;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+2] = storage.ReadonlyListView(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<IReadonlyComponentList>();
			}
			else
			{
				_excludeComponents = new IReadonlyComponentList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.ReadonlyListView(excludeComponents[i]);
				}
			}
	
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}
		
		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0 = _componentsList0.Sync(), _componentsList1 = _componentsList1.Sync());
		}
	
		public void Dispose()
		{
			_entities.Dispose();
		}
	
		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly IReadonlyComponentListView<T0> _componentsList0;
			private readonly IReadonlyComponentListView<T1> _componentsList1;
			
			private readonly int _length;
	
			private int _iteration;
	
			public ComponentsIterator(RentedArray<int> entities, 
				IReadonlyComponentListView<T0> componentsList0, IReadonlyComponentListView<T1> componentsList1)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_entities        = entities.array;
				_length          = entities.Length;
	
				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities[_iteration], 
				(ComponentList<T0>)_componentsList0, 
				(ComponentList<T1>)_componentsList1);
	
			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly ComponentList<T0> _componentsList0;
			private readonly ComponentList<T1> _componentsList1;
	
			public IterationView(int entity, ComponentList<T0> componentsList0, ComponentList<T1> componentsList1)
			{
				this.entity           = entity;
				_componentsList0      = componentsList0;
				_componentsList1 = componentsList1;
			}
	
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref _componentsList0.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref _componentsList1.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.Remove(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1.Remove(entity);
			}
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentsView<T0, T1, T2> : IComponentsView 
		where T0 : struct, IComponent
		where T1 : struct, IComponent
		where T2 : struct, IComponent
	{
		private readonly IReadonlyComponentList[] _hasComponents;
		private readonly IReadonlyComponentList[] _excludeComponents;
		
		private IReadonlyComponentListView<T0> _componentsList0;
		private IReadonlyComponentListView<T1> _componentsList1;
		private IReadonlyComponentListView<T2> _componentsList2;
		
		private readonly BigBitmask _validEntities = new(ComponentsViewHelper.PreallocationSize);
		private readonly BigBitmask _excludedEntities = new(ComponentsViewHelper.PreallocationSize);
	
		private RentedArray<int> _entities = new(0);
	
		private readonly bool _onlyWhenStructuralChanges;
		private int _lastVersion;
		private int _lastVersionExcluded;
	
		public int Size => _entities.Length;
		
		public ComponentsView(ComponentsStorage storage,
			Type[]? hasComponents = null, Type[]? excludeComponents = null,
			bool onlyWhenStructuralChanges = false)
		{
			_componentsList0 = storage.ReadonlyListView<T0>();
			_componentsList1 = storage.ReadonlyListView<T1>();
			_componentsList2 = storage.ReadonlyListView<T2>();
			
			if (hasComponents == null)
			{
				_hasComponents = new IReadonlyComponentList[]
				{
					_componentsList0, _componentsList1, _componentsList2,
				};
			}
			else
			{
				_hasComponents    = new IReadonlyComponentList[3 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				_hasComponents[1] = _componentsList1;
				_hasComponents[2] = _componentsList2;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+3] = storage.ReadonlyListView(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<IReadonlyComponentList>();
			}
			else
			{
				_excludeComponents = new IReadonlyComponentList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.ReadonlyListView(excludeComponents[i]);
				}
			}
	
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}
	
		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0 = _componentsList0.Sync(), 
				_componentsList1 = _componentsList1.Sync(), _componentsList2 = _componentsList2.Sync());
		}
	
		public void Dispose()
		{
			_entities.Dispose();
		}
	
		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
	
			private readonly IReadonlyComponentListView<T0> _componentsList0;
			private readonly IReadonlyComponentListView<T1> _componentsList1;
			private readonly IReadonlyComponentListView<T2> _componentsList2;
			
			private readonly int _length;
		
			private int _iteration;
	
			public ComponentsIterator(RentedArray<int> entities, 
				IReadonlyComponentListView<T0> componentsList0, IReadonlyComponentListView<T1> componentsList1, 
				IReadonlyComponentListView<T2> componentsList2)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
				_entities        = entities.array;
				_length          = entities.Length;
	
				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities[_iteration], 
				(ComponentList<T0>)_componentsList0,
				(ComponentList<T1>)_componentsList1,
				(ComponentList<T2>)_componentsList2);
	
			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly ComponentList<T0> _componentsList0;
			private readonly ComponentList<T1> _componentsList1;
			private readonly ComponentList<T2> _componentsList2;
	
			public IterationView(int entity, ComponentList<T0> componentsList0, ComponentList<T1> componentsList1, ComponentList<T2> componentsList2)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
			}
	
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref _componentsList0.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref _componentsList1.Value(entity);
			}
	
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T2 Get2()
			{
				return ref _componentsList2.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.Remove(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1.Remove(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove2()
			{
				_componentsList2.Remove(entity);
			}
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentsView<T0, T1, T2, T3> : IComponentsView 
		where T0 : struct, IComponent
		where T1 : struct, IComponent
		where T2 : struct, IComponent
		where T3 : struct, IComponent
	{
		private readonly IReadonlyComponentList[] _hasComponents;
		private readonly IReadonlyComponentList[] _excludeComponents;
	
		private IReadonlyComponentListView<T0> _componentsList0;
		private IReadonlyComponentListView<T1> _componentsList1;
		private IReadonlyComponentListView<T2> _componentsList2;
		private IReadonlyComponentListView<T3> _componentsList3;
	
		private readonly BigBitmask _validEntities = new(ComponentsViewHelper.PreallocationSize);
		private readonly BigBitmask _excludedEntities = new(ComponentsViewHelper.PreallocationSize);
	
		private RentedArray<int> _entities = new(0);
	
		private readonly bool _onlyWhenStructuralChanges;
		private int _lastVersion;
		private int _lastVersionExcluded;
	
		public int Size => _entities.Length;
		
		public ComponentsView(ComponentsStorage storage,
			Type[]? hasComponents = null, Type[]? excludeComponents = null,
			bool onlyWhenStructuralChanges = false)
		{
			_componentsList0 = storage.ReadonlyListView<T0>();
			_componentsList1 = storage.ReadonlyListView<T1>();
			_componentsList2 = storage.ReadonlyListView<T2>();
			_componentsList3 = storage.ReadonlyListView<T3>();
			
			if (hasComponents == null)
			{
				_hasComponents = new IReadonlyComponentList[]
				{
					_componentsList0, _componentsList1, _componentsList2, _componentsList3,
				};
			}
			else
			{
				_hasComponents    = new IReadonlyComponentList[4 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				_hasComponents[1] = _componentsList1;
				_hasComponents[2] = _componentsList2;
				_hasComponents[3] = _componentsList3;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+4] = storage.ReadonlyListView(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<IReadonlyComponentList>();
			}
			else
			{
				_excludeComponents = new IReadonlyComponentList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.ReadonlyListView(excludeComponents[i]);
				}
			}
	
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}
	
		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0 = _componentsList0.Sync(),
				_componentsList1 = _componentsList1.Sync(), _componentsList2 = _componentsList2.Sync(),
				_componentsList3 = _componentsList3.Sync());
		}
	
		public void Dispose()
		{
			_entities.Dispose();
		}
	
		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
	
			private readonly IReadonlyComponentListView<T0> _componentsList0;
			private readonly IReadonlyComponentListView<T1> _componentsList1;
			private readonly IReadonlyComponentListView<T2> _componentsList2;
			private readonly IReadonlyComponentListView<T3> _componentsList3;
			
			private readonly int _length;
		
			private int _iteration;
	
			public ComponentsIterator(RentedArray<int> entities, IReadonlyComponentListView<T0> componentsList0,
				IReadonlyComponentListView<T1> componentsList1, IReadonlyComponentListView<T2> componentsList2,
				IReadonlyComponentListView<T3> componentsList3)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
				_componentsList3 = componentsList3;
				_entities        = entities.array;
				_length          = entities.Length;
	
				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities[_iteration], 
				(ComponentList<T0>)_componentsList0,
				(ComponentList<T1>)_componentsList1,
				(ComponentList<T2>)_componentsList2,
				(ComponentList<T3>)_componentsList3);
	
			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly ComponentList<T0> _componentsList0;
			private readonly ComponentList<T1> _componentsList1;
			private readonly ComponentList<T2> _componentsList2;
			private readonly ComponentList<T3> _componentsList3;
	
			public IterationView(int entity, ComponentList<T0> componentsList0, ComponentList<T1> componentsList1, 
				ComponentList<T2> componentsList2, ComponentList<T3> componentsList3)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
				_componentsList3 = componentsList3;
			}
	
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref _componentsList0.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref _componentsList1.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T2 Get2()
			{
				return ref _componentsList2.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T3 Get3()
			{
				return ref _componentsList3.Value(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.Remove(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1.Remove(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove2()
			{
				_componentsList2.Remove(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove3()
			{
				_componentsList3.Remove(entity);
			}
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	internal static class ComponentsViewHelper
	{
		internal const int PreallocationSize = 128;
		private static readonly ProfilerMarker ZipMarker = new("ComponentsIterator.Zip");
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Zip(IReadonlyComponentList[] hasComponents, IReadonlyComponentList[] excludeComponents,
			BigBitmask validEntities, BigBitmask excludedEntities, bool onlyWhenStructuralChanges,
			ref RentedArray<int> entities, ref int lastVersion, ref int lastVersionExcluded)
		{
			using var marker = ZipMarker.Auto();
	
			var currentVersion = 0;
			for (var i = 0; i < hasComponents.Length; i++)
			{
				var list = hasComponents[i] = hasComponents[i].Sync();
				unchecked
				{
					currentVersion += list.EntitiesVersion;
				}
			}
			for (var i = 0; i < excludeComponents.Length; i++)
			{
				var list = excludeComponents[i] = excludeComponents[i].Sync();
				unchecked
				{
					currentVersion += list.EntitiesVersion;
				}
			}
	
			if (lastVersion == currentVersion)
			{
				if (onlyWhenStructuralChanges)
				{
					entities.Dispose();
				}
				return;
			}
	
			lastVersion = currentVersion;
			
			// TODO: Try reuse
			entities.Dispose();
			
			foreach (var list in hasComponents)
			{
				if (list.Length < 1)
				{
					return;
				}
			}
	
			CollectExcludes(excludeComponents, excludedEntities, ref lastVersionExcluded);
	
			// Calculate bit mask
			validEntities.CopyFrom(hasComponents[0].EntitiesMask);
			for (var i = 1; i < hasComponents.Length; ++i)
			{
				validEntities.Intersect(hasComponents[i].EntitiesMask, false);
			}
			validEntities.Exclude(excludedEntities, false);
	
			var validEntitiesCount = validEntities.Count();
			if (validEntitiesCount < 1)
			{
				return;
			}
	
			entities = new(validEntitiesCount);
			var entitiesArray = entities.array;
			
			var entityIndex = 0;
			foreach (var validEntityIndex in validEntities)
			{
				var entity = new Entity(validEntityIndex);
				entitiesArray[entityIndex] = entity;
				++entityIndex;
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CollectExcludes(IReadonlyComponentList[] excludeComponents, 
			BigBitmask excludedEntities, ref int lastVersionExcluded)
		{
			if (excludeComponents.Length < 1)
			{
				return;
			}
			
			var currentVersion = 0;
			foreach (var list in excludeComponents)
			{
				unchecked
				{
					currentVersion += list.EntitiesVersion;
				}
			}
			
			if (lastVersionExcluded == currentVersion)
			{
				return;
			}
	
			lastVersionExcluded = currentVersion;
			
			excludedEntities.CopyFrom(excludeComponents[0].EntitiesMask);
			for (var index = 1; index < excludeComponents.Length; index++)
			{
				var excludeComponent = excludeComponents[index];
				excludedEntities.Union(excludeComponent.EntitiesMask);
			}
		}
	}
}
