using System;
using System.Runtime.CompilerServices;
using KVD.ECS.Components;
using KVD.ECS.Entities;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Profiling;

#nullable enable

namespace KVD.ECS
{
	public interface IComponentsView : IDisposable
	{
		int Size{ get; }
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class ComponentsView : IComponentsView
	{
		private readonly ISparseList[] _hasComponents;
		private readonly ISparseList[] _excludeComponents;

		private readonly BigBitmask _validEntities = new(ComponentsViewHelper.PreallocationSize);
		private readonly BigBitmask _excludedEntities = new(ComponentsViewHelper.PreallocationSize);

		private RentedArray<int> _entities = new(0);

		private readonly bool _onlyWhenStructuralChanges;
		private int _lastVersion;
		private int _lastVersionExcluded;

		public int Size => _entities.Length;

		public ComponentsView(ViewDescriptor viewDescriptor, bool onlyWhenStructuralChanges = false)
		{
			_hasComponents             = viewDescriptor.HasComponents;
			_excludeComponents         = viewDescriptor.ExcludeComponents;
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
		private readonly ISparseList[] _hasComponents;
		private readonly ISparseList[] _excludeComponents;
		
		private readonly SparseList<T0> _componentsList0;

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
			_componentsList0 = storage.List<T0>();
			
			if (hasComponents == null)
			{
				_hasComponents = new ISparseList[] { _componentsList0, };
			}
			else
			{
				_hasComponents    = new ISparseList[1 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+1] = storage.List(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<ISparseList>();
			}
			else
			{
				_excludeComponents = new ISparseList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.List(excludeComponents[i]);
				}
			}
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0);
		}

		public void Dispose()
		{
			_entities.Dispose();
		}

		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly int _length;
			
			private readonly SparseList<T0> _componentsList0;
		
			private int _iteration;

			public ComponentsIterator(RentedArray<int> entities, SparseList<T0> componentsList0)
			{
				_componentsList0 = componentsList0;
				_entities        = entities.array;
				_length          = entities.Length;

				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities[_iteration], _componentsList0);

			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly SparseList<T0> _componentsList0;

			public IterationView(int entity, SparseList<T0> componentsList0)
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
		private readonly ISparseList[] _hasComponents;
		private readonly ISparseList[] _excludeComponents;

		private readonly SparseList<T0> _componentsList0;
		private readonly SparseList<T1> _componentsList1;

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
			_componentsList0 = storage.List<T0>();
			_componentsList1 = storage.List<T1>();
			
			if (hasComponents == null)
			{
				_hasComponents = new ISparseList[] { _componentsList0, _componentsList1, };
			}
			else
			{
				_hasComponents    = new ISparseList[2 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				_hasComponents[1] = _componentsList1;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+2] = storage.List(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<ISparseList>();
			}
			else
			{
				_excludeComponents = new ISparseList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.List(excludeComponents[i]);
				}
			}

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}
		
		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0, _componentsList1);
		}

		public void Dispose()
		{
			_entities.Dispose();
		}

		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly int _length;
			
			private readonly SparseList<T0> _componentsList0;
			private readonly SparseList<T1> _componentsList1;
		
			private int _iteration;

			public ComponentsIterator(RentedArray<int> entities, SparseList<T0> componentsList0, SparseList<T1> componentsList1)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_entities        = entities.array;
				_length          = entities.Length;

				_iteration = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities[_iteration], _componentsList0, _componentsList1);

			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly SparseList<T0> _componentsList0;
			private readonly SparseList<T1> _componentsList1;

			public IterationView(int entity, SparseList<T0> componentsList0, SparseList<T1> componentsList1)
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
		private readonly ISparseList[] _hasComponents;
		private readonly ISparseList[] _excludeComponents;

		private readonly SparseList<T0> _componentsList0;
		private readonly SparseList<T1> _componentsList1;
		private readonly SparseList<T2> _componentsList2;

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
			_componentsList0 = storage.List<T0>();
			_componentsList1 = storage.List<T1>();
			_componentsList2 = storage.List<T2>();
			
			if (hasComponents == null)
			{
				_hasComponents = new ISparseList[]
				{
					_componentsList0, _componentsList1, _componentsList2,
				};
			}
			else
			{
				_hasComponents    = new ISparseList[3 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				_hasComponents[1] = _componentsList1;
				_hasComponents[2] = _componentsList2;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+3] = storage.List(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<ISparseList>();
			}
			else
			{
				_excludeComponents = new ISparseList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.List(excludeComponents[i]);
				}
			}

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0, _componentsList1, _componentsList2);
		}

		public void Dispose()
		{
			_entities.Dispose();
		}

		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly int _length;
			
			private readonly SparseList<T0> _componentsList0;
			private readonly SparseList<T1> _componentsList1;
			private readonly SparseList<T2> _componentsList2;
		
			private int _iteration;

			public ComponentsIterator(RentedArray<int> entities, 
				SparseList<T0> componentsList0, SparseList<T1> componentsList1, SparseList<T2> componentsList2)
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
				_componentsList0, _componentsList1, _componentsList2);

			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly SparseList<T0> _componentsList0;
			private readonly SparseList<T1> _componentsList1;
			private readonly SparseList<T2> _componentsList2;

			public IterationView(int entity, SparseList<T0> componentsList0, SparseList<T1> componentsList1, SparseList<T2> componentsList2)
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
		private readonly ISparseList[] _hasComponents;
		private readonly ISparseList[] _excludeComponents;

		private readonly SparseList<T0> _componentsList0;
		private readonly SparseList<T1> _componentsList1;
		private readonly SparseList<T2> _componentsList2;
		private readonly SparseList<T3> _componentsList3;

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
			_componentsList0 = storage.List<T0>();
			_componentsList1 = storage.List<T1>();
			_componentsList2 = storage.List<T2>();
			_componentsList3 = storage.List<T3>();
			
			if (hasComponents == null)
			{
				_hasComponents = new ISparseList[]
				{
					_componentsList0, _componentsList1, _componentsList2, _componentsList3,
				};
			}
			else
			{
				_hasComponents    = new ISparseList[4 + hasComponents.Length];
				_hasComponents[0] = _componentsList0;
				_hasComponents[1] = _componentsList1;
				_hasComponents[2] = _componentsList2;
				_hasComponents[3] = _componentsList3;
				for (var i = 0; i < hasComponents.Length; i++)
				{
					_hasComponents[i+4] = storage.List(hasComponents[i]);
				}
			}
			
			if (excludeComponents == null)
			{
				_excludeComponents = Array.Empty<ISparseList>();
			}
			else
			{
				_excludeComponents = new ISparseList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					_excludeComponents[i] = storage.List(excludeComponents[i]);
				}
			}

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _validEntities, _excludedEntities,
				_onlyWhenStructuralChanges, ref _entities, ref _lastVersion, ref _lastVersionExcluded);
			return new(_entities, _componentsList0, _componentsList1, _componentsList2, _componentsList3);
		}

		public void Dispose()
		{
			_entities.Dispose();
		}

		public ref struct ComponentsIterator
		{
			private readonly int[] _entities;
			private readonly int _length;
			
			private readonly SparseList<T0> _componentsList0;
			private readonly SparseList<T1> _componentsList1;
			private readonly SparseList<T2> _componentsList2;
			private readonly SparseList<T3> _componentsList3;
		
			private int _iteration;

			public ComponentsIterator(RentedArray<int> entities, SparseList<T0> componentsList0, 
				SparseList<T1> componentsList1, SparseList<T2> componentsList2, SparseList<T3> componentsList3)
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
				_componentsList0, _componentsList1, _componentsList2, _componentsList3);

			public bool MoveNext()
			{
				return ++_iteration < _length;
			}
		}
		
		public readonly ref struct IterationView
		{
			public readonly int entity;
			
			private readonly SparseList<T0> _componentsList0;
			private readonly SparseList<T1> _componentsList1;
			private readonly SparseList<T2> _componentsList2;
			private readonly SparseList<T3> _componentsList3;

			public IterationView(int entity, SparseList<T0> componentsList0, SparseList<T1> componentsList1, 
				SparseList<T2> componentsList2, SparseList<T3> componentsList3)
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
		public static void Zip(ISparseList[] hasComponents, ISparseList[] excludeComponents,
			BigBitmask validEntities, BigBitmask excludedEntities, bool onlyWhenStructuralChanges,
			ref RentedArray<int> entities, ref int lastVersion, ref int lastVersionExcluded)
		{
			using var marker = ZipMarker.Auto();

			var currentVersion = 0;
			foreach (var list in hasComponents)
			{
				unchecked
				{
					currentVersion += list.EntitiesVersion;
				}
			}
			foreach (var list in excludeComponents)
			{
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
			validEntities.Copy(hasComponents[0].EntitiesMask);
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
		private static void CollectExcludes(ISparseList[] excludeComponents, BigBitmask excludedEntities, 
			ref int lastVersionExcluded)
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
			
			excludedEntities.Copy(excludeComponents[0].EntitiesMask);
			for (var index = 1; index < excludeComponents.Length; index++)
			{
				var excludeComponent = excludeComponents[index];
				excludedEntities.Union(excludeComponent.EntitiesMask);
			}
		}
	}
}
