using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.Utils.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Profiling;

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public unsafe struct ComponentsView
	{
		readonly UnsafeArray<ComponentListPtrSoft> _hasComponents;
		readonly UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly bool _onlyWhenStructuralChanges;
		int _lastVersion;

		public uint Size{ get; private set; }

		public ComponentsView(UnsafeArray<ComponentListPtrSoft> hasComponents, UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges = false)
		{
			_lastVersion = -1;
			Size = 0;

			_hasComponents = hasComponents;
			_excludeComponents = excludeComponents;
			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _onlyWhenStructuralChanges,
				ref _lastVersion, out var entities);
			Size = entities.Length;
			return new(entities);
		}

		public ref struct ComponentsIterator
		{
			UnsafeArray<int> _entities;
			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities)
			{
				_entities = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public int Current => _entities.Ptr[_iteration];

			public bool MoveNext() => ++_iteration < _entities.Length;

			public void Dispose() => _entities.Dispose();
		}
	}

	// There is overhead associated with using these generic, for common case it should be negligible but
	// In worse case could be five times slower than accessing components by sparseList.Value(entity) (plain ComponentsView)

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public struct ComponentsView<T0>
		where T0 : unmanaged, IComponent
	{
		readonly UnsafeArray<ComponentListPtrSoft> _hasComponents;
		readonly UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly ComponentListPtrSoft<T0> _componentsList0;

		readonly bool _onlyWhenStructuralChanges;
		int _lastVersion;

		public uint Size{ get; private set; }

		public ComponentsView(ComponentListPtrSoft<T0> componentsList0,
			UnsafeArray<ComponentListPtrSoft> hasComponents, UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges = false)
		{
			_lastVersion = -1;
			Size = 0;

			_hasComponents     = hasComponents;
			_excludeComponents = excludeComponents;
			_componentsList0   = componentsList0;

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _onlyWhenStructuralChanges, ref _lastVersion,
				out var entities);
			Size = entities.Length;
			return new(entities, _componentsList0);
		}

		public unsafe ref struct ComponentsIterator
		{
			UnsafeArray<int> _entities;
			readonly ComponentListPtrSoft<T0> _componentsList0;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities, ComponentListPtrSoft<T0> componentsList0)
			{
				_componentsList0 = componentsList0;
				_entities = entities;

				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration], _componentsList0.ToListPtr());

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public ref struct IterationView
		{
			public readonly int entity;

			ComponentListPtr<T0> _componentsList0;

			public IterationView(int entity, ComponentListPtr<T0> componentsList0)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public readonly ref T0 Get0()
			{
				return ref _componentsList0.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.AsList().Remove(entity);
			}
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public struct ComponentsView<T0, T1>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
	{
		readonly UnsafeArray<ComponentListPtrSoft> _hasComponents;
		readonly UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		ComponentListPtrSoft<T0> _componentsList0;
		ComponentListPtrSoft<T1> _componentsList1;

		readonly bool _onlyWhenStructuralChanges;
		int _lastVersion;

		public uint Size{ get; private set; }

		public ComponentsView(ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
			UnsafeArray<ComponentListPtrSoft> hasComponents, UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges = false)
		{
			_lastVersion = -1;
			Size = 0;

			_hasComponents     = hasComponents;
			_excludeComponents = excludeComponents;

			_componentsList0 = componentsList0;
			_componentsList1 = componentsList1;

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _onlyWhenStructuralChanges, ref _lastVersion,
				out var entities);
			Size = entities.Length;
			return new(entities, _componentsList0, _componentsList1);
		}

		public unsafe ref struct ComponentsIterator
		{
			UnsafeArray<int> _entities;
			readonly ComponentListPtrSoft<T0> _componentsList0;
			readonly ComponentListPtrSoft<T1> _componentsList1;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities,
				ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;

				_entities  = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration],
				_componentsList0.ToListPtr(), _componentsList1.ToListPtr());

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public ref struct IterationView
		{
			public readonly int entity;

			ComponentListPtr<T0> _componentsList0;
			ComponentListPtr<T1> _componentsList1;

			public IterationView(int entity, ComponentListPtr<T0> componentsList0, ComponentListPtr<T1> componentsList1)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref _componentsList0.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref _componentsList1.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.AsList().Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1.AsList().Remove(entity);
			}
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public struct ComponentsView<T0, T1, T2>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
		where T2 : unmanaged, IComponent
	{
		readonly UnsafeArray<ComponentListPtrSoft> _hasComponents;
		readonly UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		ComponentListPtrSoft<T0> _componentsList0;
		ComponentListPtrSoft<T1> _componentsList1;
		ComponentListPtrSoft<T2> _componentsList2;

		readonly bool _onlyWhenStructuralChanges;
		int _lastVersion;

		public uint Size{ get; private set; }

		public ComponentsView(ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
			ComponentListPtrSoft<T2> componentsList2,
			UnsafeArray<ComponentListPtrSoft> hasComponents, UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges = false)
		{
			_lastVersion = -1;
			Size         = 0;

			_hasComponents     = hasComponents;
			_excludeComponents = excludeComponents;

			_componentsList0 = componentsList0;
			_componentsList1 = componentsList1;
			_componentsList2 = componentsList2;

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _onlyWhenStructuralChanges, ref _lastVersion,
				out var entities);
			Size = entities.Length;
			return new(entities, _componentsList0, _componentsList1, _componentsList2);
		}

		public unsafe ref struct ComponentsIterator
		{
			UnsafeArray<int> _entities;

			readonly ComponentListPtrSoft<T0> _componentsList0;
			readonly ComponentListPtrSoft<T1> _componentsList1;
			readonly ComponentListPtrSoft<T2> _componentsList2;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities,
				ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
				ComponentListPtrSoft<T2> componentsList2)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;

				_entities  = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration],
				_componentsList0.ToListPtr(), _componentsList1.ToListPtr(), _componentsList2.ToListPtr());

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public ref struct IterationView
		{
			public readonly int entity;

			ComponentListPtr<T0> _componentsList0;
			ComponentListPtr<T1> _componentsList1;
			ComponentListPtr<T2> _componentsList2;

			public IterationView(int entity, ComponentListPtr<T0> componentsList0, ComponentListPtr<T1> componentsList1,
				ComponentListPtr<T2> componentsList2)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref _componentsList0.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref _componentsList1.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T2 Get2()
			{
				return ref _componentsList2.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.AsList().Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1.AsList().Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove2()
			{
				_componentsList2.AsList().Remove(entity);
			}
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public struct ComponentsView<T0, T1, T2, T3>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
		where T2 : unmanaged, IComponent
		where T3 : unmanaged, IComponent
	{
		readonly UnsafeArray<ComponentListPtrSoft> _hasComponents;
		readonly UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		ComponentListPtrSoft<T0> _componentsList0;
		ComponentListPtrSoft<T1> _componentsList1;
		ComponentListPtrSoft<T2> _componentsList2;
		ComponentListPtrSoft<T3> _componentsList3;

		readonly bool _onlyWhenStructuralChanges;
		int _lastVersion;

		public uint Size{ get; private set; }

		public ComponentsView(ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
			ComponentListPtrSoft<T2> componentsList2, ComponentListPtrSoft<T3> componentsList3,
			UnsafeArray<ComponentListPtrSoft> hasComponents, UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges = false)
		{
			_lastVersion = -1;
			Size         = 0;

			_hasComponents     = hasComponents;
			_excludeComponents = excludeComponents;

			_componentsList0 = componentsList0;
			_componentsList1 = componentsList1;
			_componentsList2 = componentsList2;
			_componentsList3 = componentsList3;

			_onlyWhenStructuralChanges = onlyWhenStructuralChanges;
		}

		public ComponentsIterator GetEnumerator()
		{
			ComponentsViewHelper.Zip(_hasComponents, _excludeComponents, _onlyWhenStructuralChanges, ref _lastVersion,
				out var entities);
			Size = entities.Length;
			return new(entities, _componentsList0, _componentsList1, _componentsList2, _componentsList3);
		}

		public unsafe ref struct ComponentsIterator
		{
			UnsafeArray<int> _entities;

			readonly ComponentListPtrSoft<T0> _componentsList0;
			readonly ComponentListPtrSoft<T1> _componentsList1;
			readonly ComponentListPtrSoft<T2> _componentsList2;
			readonly ComponentListPtrSoft<T3> _componentsList3;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities,
				ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
				ComponentListPtrSoft<T2> componentsList2, ComponentListPtrSoft<T3> componentsList3)
			{
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
				_componentsList3 = componentsList3;

				_entities  = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration],
				_componentsList0.ToListPtr(), _componentsList1.ToListPtr(), _componentsList2.ToListPtr(),
				_componentsList3.ToListPtr());

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public ref struct IterationView
		{
			public readonly int entity;

			ComponentListPtr<T0> _componentsList0;
			ComponentListPtr<T1> _componentsList1;
			ComponentListPtr<T2> _componentsList2;
			ComponentListPtr<T3> _componentsList3;

			public IterationView(int entity, ComponentListPtr<T0> componentsList0, ComponentListPtr<T1> componentsList1,
				ComponentListPtr<T2> componentsList2, ComponentListPtr<T3> componentsList3)
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
				return ref _componentsList0.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref _componentsList1.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T2 Get2()
			{
				return ref _componentsList2.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T3 Get3()
			{
				return ref _componentsList3.AsList().Value(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0.AsList().Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1.AsList().Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove2()
			{
				_componentsList2.AsList().Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove3()
			{
				_componentsList3.AsList().Remove(entity);
			}
		}
	}

	[BurstCompile]
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	static class ComponentsViewHelper
	{
		static readonly ProfilerMarker ZipMarker = new("ComponentsIterator.Zip");

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Zip(in UnsafeArray<ComponentListPtrSoft> hasComponents, in UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges, ref int lastVersion, out UnsafeArray<int> entities)
		{
			using var marker = ZipMarker.Auto();

			var currentVersion = 0;
			for (var i = 0u; i < hasComponents.Length; i++)
			{
				if (!hasComponents[i].IsCreated)
				{
					continue;
				}
				unchecked
				{
					currentVersion += hasComponents[i].ToList().entitiesVersion;
				}
			}
			for (var i = 0u; i < excludeComponents.Length; i++)
			{
				if (!excludeComponents[i].IsCreated)
				{
					continue;
				}
				unchecked
				{
					currentVersion += excludeComponents[i].ToList().entitiesVersion;
				}
			}

			if ((lastVersion == currentVersion) & onlyWhenStructuralChanges)
			{
				entities = UnsafeArray<int>.Empty;
				return;
			}

			lastVersion = currentVersion;

			foreach (var list in hasComponents)
			{
				if (!list.IsCreated)
				{
					entities = UnsafeArray<int>.Empty;
					return;
				}
			}

			CollectExcludes(excludeComponents, out var excludedEntities);

			// Calculate bit mask
			var validEntities = new UnsafeBitmask(hasComponents[0].ToList().entitiesMask, Allocator.Temp);
			for (var i = 1u; i < hasComponents.Length; ++i)
			{
				validEntities.Intersect(hasComponents[i].ToList().entitiesMask);
			}
			validEntities.Exclude(excludedEntities);
			excludedEntities.Dispose();

			if (!validEntities.AnySet())
			{
				validEntities.Dispose();
				entities = UnsafeArray<int>.Empty;
				return;
			}

			validEntities.ToArray(Allocator.Temp, new EntityFromIndex(), out entities);
			validEntities.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void CollectExcludes(UnsafeArray<ComponentListPtrSoft> excludeComponents, out UnsafeBitmask excludedEntities)
		{
			if (excludeComponents.Length < 1)
			{
				excludedEntities = UnsafeBitmask.Empty;
				return;
			}

			var index = 0u;
			while (index < excludeComponents.Length && !excludeComponents[index].IsCreated)
			{
				index++;
			}
			if (index >= excludeComponents.Length)
			{
				excludedEntities = UnsafeBitmask.Empty;
				return;
			}
			excludedEntities = new UnsafeBitmask(excludeComponents[index].ToList().entitiesMask, Allocator.Temp);
			for (; index < excludeComponents.Length; index++)
			{
				var excludeComponent = excludeComponents[index];
				excludedEntities.Union(excludeComponent.ToList().entitiesMask);
			}
		}

		readonly struct EntityFromIndex : UnsafeBitmask.IConverter<int>
		{
			public int Convert(uint index)
			{
				return (int)index;
			}
		}
	}
}
