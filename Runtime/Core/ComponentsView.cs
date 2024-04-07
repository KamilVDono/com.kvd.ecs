using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.Utils.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Profiling;

namespace KVD.ECS.Core
{
	// TODO: Code generation for ComponentsView
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public unsafe struct ComponentsView
	{
		UnsafeArray<ComponentListPtrSoft> _hasComponents;
		UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly byte _onlyWhenStructuralChanges;
		int _lastVersion;

		public uint Size{ get; private set; }

		public ComponentsView(UnsafeArray<ComponentListPtrSoft> hasComponents, UnsafeArray<ComponentListPtrSoft> excludeComponents,
			bool onlyWhenStructuralChanges = false)
		{
			_lastVersion = -1;
			Size = 0;

			_hasComponents = hasComponents;
			_excludeComponents = excludeComponents;
			_onlyWhenStructuralChanges = (byte)(onlyWhenStructuralChanges ? 1 : 0);
		}

		public void Dispose()
		{
			_hasComponents.Dispose();
			_excludeComponents.Dispose();
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
		UnsafeArray<ComponentListPtrSoft> _hasComponents;
		UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly ComponentListPtrSoft<T0> _componentsList0;

		readonly byte _onlyWhenStructuralChanges;
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

			_onlyWhenStructuralChanges = (byte)(onlyWhenStructuralChanges ? 1 : 0);
		}

		public void Dispose()
		{
			_hasComponents.Dispose();
			_excludeComponents.Dispose();
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
			readonly ComponentList<T0>* _componentsList0;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities, ComponentListPtrSoft<T0> componentsList0)
			{
				if (entities.Length > 0)
				{
					_componentsList0 = componentsList0.ToListPtr().ptr;
				}
				else
				{
					_componentsList0 = default;
				}
				_entities = entities;

				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration], _componentsList0);

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public readonly unsafe ref struct IterationView
		{
			public readonly int entity;

			readonly ComponentList<T0>* _componentsList0;

			public IterationView(int entity, ComponentList<T0>* componentsList0)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RW(out T0* component)
			{
				component = _componentsList0->ValuePtr(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO(out T0 component)
			{
				component = *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO<TU>(out TU component) where TU : unmanaged
			{
				component = *(TU*)_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0->Remove(entity);
			}
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public struct ComponentsView<T0, T1>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
	{
		UnsafeArray<ComponentListPtrSoft> _hasComponents;
		UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly ComponentListPtrSoft<T0> _componentsList0;
		readonly ComponentListPtrSoft<T1> _componentsList1;

		readonly byte _onlyWhenStructuralChanges;
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

			_onlyWhenStructuralChanges = (byte)(onlyWhenStructuralChanges ? 1 : 0);
		}

		public void Dispose()
		{
			_hasComponents.Dispose();
			_excludeComponents.Dispose();
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
			readonly ComponentList<T0>* _componentsList0;
			readonly ComponentList<T1>* _componentsList1;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities,
				ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1)
			{
				if (entities.Length > 0)
				{
					_componentsList0 = componentsList0.ToListPtr().ptr;
					_componentsList1 = componentsList1.ToListPtr().ptr;
				}
				else
				{
					_componentsList0 = default;
					_componentsList1 = default;
				}

				_entities  = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration],
				_componentsList0, _componentsList1);

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public readonly unsafe ref struct IterationView
		{
			public readonly int entity;

			readonly ComponentList<T0>* _componentsList0;
			readonly ComponentList<T1>* _componentsList1;

			public IterationView(int entity, ComponentList<T0>* componentsList0, ComponentList<T1>* componentsList1)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO(out T0 component)
			{
				component = *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO<TU>(out TU component) where TU : unmanaged
			{
				component = *(TU*)_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref *_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RO(out T1 component)
			{
				component = *_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RO<TU>(out TU component) where TU : unmanaged
			{
				component = *(TU*)_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0->Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1->Remove(entity);
			}
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public struct ComponentsView<T0, T1, T2>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
		where T2 : unmanaged, IComponent
	{
		UnsafeArray<ComponentListPtrSoft> _hasComponents;
		UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly ComponentListPtrSoft<T0> _componentsList0;
		readonly ComponentListPtrSoft<T1> _componentsList1;
		readonly ComponentListPtrSoft<T2> _componentsList2;

		readonly byte _onlyWhenStructuralChanges;
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

			_onlyWhenStructuralChanges = (byte)(onlyWhenStructuralChanges ? 1 : 0);
		}

		public void Dispose()
		{
			_hasComponents.Dispose();
			_excludeComponents.Dispose();
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

			readonly ComponentList<T0>* _componentsList0;
			readonly ComponentList<T1>* _componentsList1;
			readonly ComponentList<T2>* _componentsList2;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities,
				ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
				ComponentListPtrSoft<T2> componentsList2)
			{
				if (entities.Length > 0)
				{
					_componentsList0 = componentsList0.ToListPtr().ptr;
					_componentsList1 = componentsList1.ToListPtr().ptr;
					_componentsList2 = componentsList2.ToListPtr().ptr;
				}
				else
				{
					_componentsList0 = default;
					_componentsList1 = default;
					_componentsList2 = default;
				}

				_entities  = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration], _componentsList0, _componentsList1, _componentsList2);

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public readonly unsafe ref struct IterationView
		{
			public readonly int entity;

			readonly ComponentList<T0>* _componentsList0;
			readonly ComponentList<T1>* _componentsList1;
			readonly ComponentList<T2>* _componentsList2;

			public IterationView(int entity, ComponentList<T0>* componentsList0, ComponentList<T1>* componentsList1,
				ComponentList<T2>* componentsList2)
			{
				this.entity      = entity;
				_componentsList0 = componentsList0;
				_componentsList1 = componentsList1;
				_componentsList2 = componentsList2;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T0 Get0()
			{
				return ref *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RW(out T0* component)
			{
				component = _componentsList0->ValuePtr(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO(out T0 component)
			{
				component = *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO<TU>(out TU component) where TU : unmanaged
			{
				component = *(TU*)_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref *_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RW(out T1* component)
			{
				component = _componentsList1->ValuePtr(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RO(out T1 component)
			{
				component =  *_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RO<TU>(out TU component) where TU : unmanaged
			{
				component = *(TU*)_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T2 Get2()
			{
				return ref *_componentsList2->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get2RW(out T2* component)
			{
				component = _componentsList2->ValuePtr(entity);
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get2RO(out T2 component)
			{
				component = *_componentsList2->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get2RO<TU>(out TU component) where TU : unmanaged
			{
				component = *(TU*)_componentsList2->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0->Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1->Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove2()
			{
				_componentsList2->Remove(entity);
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
		UnsafeArray<ComponentListPtrSoft> _hasComponents;
		UnsafeArray<ComponentListPtrSoft> _excludeComponents;

		readonly ComponentListPtrSoft<T0> _componentsList0;
		readonly ComponentListPtrSoft<T1> _componentsList1;
		readonly ComponentListPtrSoft<T2> _componentsList2;
		readonly ComponentListPtrSoft<T3> _componentsList3;

		readonly byte _onlyWhenStructuralChanges;
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

			_onlyWhenStructuralChanges = (byte)(onlyWhenStructuralChanges ? 1 : 0);
		}

		public void Dispose()
		{
			_hasComponents.Dispose();
			_excludeComponents.Dispose();
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

			readonly ComponentList<T0>* _componentsList0;
			readonly ComponentList<T1>* _componentsList1;
			readonly ComponentList<T2>* _componentsList2;
			readonly ComponentList<T3>* _componentsList3;

			int _iteration;

			public ComponentsIterator(UnsafeArray<int> entities,
				ComponentListPtrSoft<T0> componentsList0, ComponentListPtrSoft<T1> componentsList1,
				ComponentListPtrSoft<T2> componentsList2, ComponentListPtrSoft<T3> componentsList3)
			{
				if (entities.Length > 0)
				{
					_componentsList0 = componentsList0.ToListPtr().ptr;
					_componentsList1 = componentsList1.ToListPtr().ptr;
					_componentsList2 = componentsList2.ToListPtr().ptr;
					_componentsList3 = componentsList3.ToListPtr().ptr;
				}
				else
				{
					_componentsList0 = default;
					_componentsList1 = default;
					_componentsList2 = default;
					_componentsList3 = default;
				}


				_entities  = entities;
				_iteration = -1;
			}

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public IterationView Current => new(_entities.Ptr[_iteration],
				_componentsList0, _componentsList1, _componentsList2, _componentsList3);

			public bool MoveNext() => ++_iteration < _entities.Length;
			public void Dispose() => _entities.Dispose();
		}

		public readonly unsafe ref struct IterationView
		{
			public readonly int entity;

			readonly ComponentList<T0>* _componentsList0;
			readonly ComponentList<T1>* _componentsList1;
			readonly ComponentList<T2>* _componentsList2;
			readonly ComponentList<T3>* _componentsList3;

			public IterationView(int entity, ComponentList<T0>* componentsList0, ComponentList<T1>* componentsList1,
				ComponentList<T2>* componentsList2, ComponentList<T3>* componentsList3)
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
				return ref *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RW(out T0* component)
			{
				component = _componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get0RO(out T0 component)
			{
				component = *_componentsList0->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T1 Get1()
			{
				return ref *_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RW(out T1* component)
			{
				component = _componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get1RO(out T1 component)
			{
				component = *_componentsList1->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T2 Get2()
			{
				return ref *_componentsList2->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get2RW(out T2* component)
			{
				component = _componentsList2->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get2RO(out T2 component)
			{
				component = *_componentsList2->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref T3 Get3()
			{
				return ref *_componentsList3->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get3RW(out T3* component)
			{
				component = _componentsList3->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Get3RO(out T3 component)
			{
				component = *_componentsList3->ValuePtr(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove0()
			{
				_componentsList0->Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove1()
			{
				_componentsList1->Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove2()
			{
				_componentsList2->Remove(entity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove3()
			{
				_componentsList3->Remove(entity);
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
			byte onlyWhenStructuralChanges, ref int lastVersion, out UnsafeArray<int> entities)
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

			if ((lastVersion == currentVersion) & onlyWhenStructuralChanges != 0)
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
