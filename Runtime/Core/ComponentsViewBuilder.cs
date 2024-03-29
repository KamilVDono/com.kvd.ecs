using KVD.ECS.Core.Components;
using KVD.Utils.DataStructures;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace KVD.ECS.Core
{
	public struct ComponentsViewBuilder
	{
		internal readonly ComponentsStorage _storage;
		internal bool _onlyWhenStructuralChanges;

		internal UnsafeList<ComponentListPtr> _hasComponents;
		internal UnsafeList<ComponentListPtr> _excludeComponents;

		#region Create
		public static ComponentsView<T0> Create<T0>(ComponentsStorage storage,
			Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
		{
			return new ComponentsViewBuilder(storage).Build<T0>(allocator, disposeWithStorage);
		}

		public static ComponentsView<T0, T1> Create<T0, T1>(ComponentsStorage storage,
			Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
		{
			return new ComponentsViewBuilder(storage).Build<T0, T1>(allocator, disposeWithStorage);
		}

		public static ComponentsView<T0, T1, T2> Create<T0, T1, T2>(ComponentsStorage storage,
			Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
		{
			return new ComponentsViewBuilder(storage).Build<T0, T1, T2>(allocator, disposeWithStorage);
		}

		public static ComponentsView<T0, T1, T2, T3> Create<T0, T1, T2, T3>(ComponentsStorage storage,
			Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
			where T3 : unmanaged, IComponent
		{
			return new ComponentsViewBuilder(storage).Build<T0, T1, T2, T3>(allocator, disposeWithStorage);
		}
		#endregion Create

		public ComponentsViewBuilder(ComponentsStorage storage, Allocator allocator = Allocator.Temp)
		{
			_storage                   = storage;
			_onlyWhenStructuralChanges = false;

			_hasComponents     = new(8, allocator);
			_excludeComponents = new(8, allocator);
		}

		public ComponentsView Build(Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
		{
			var hasComponents     = UnsafeArray<ComponentListPtr>.Move(ref _hasComponents, allocator);
			var excludeComponents = UnsafeArray<ComponentListPtr>.Move(ref _excludeComponents, allocator);
			if (disposeWithStorage)
			{
				_storage.AddAllocation(hasComponents.AsAllocation());
				_storage.AddAllocation(excludeComponents.AsAllocation());
			}
			return new(hasComponents, excludeComponents, _onlyWhenStructuralChanges);
		}

		public ComponentsView<T0> Build<T0>(Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
		{
			var list0 = _storage.ListPtrSoft<T0>();
			if (!_hasComponents.Contains(list0.TypeLess()))
			{
				_hasComponents.Add(list0.TypeLess());
			}
			var hasComponents     = UnsafeArray<ComponentListPtr>.Move(ref _hasComponents, allocator);
			var excludeComponents = UnsafeArray<ComponentListPtr>.Move(ref _excludeComponents, allocator);
			if (disposeWithStorage)
			{
				_storage.AddAllocation(hasComponents.AsAllocation());
				_storage.AddAllocation(excludeComponents.AsAllocation());
			}
			return new(list0, hasComponents, excludeComponents, _onlyWhenStructuralChanges);
		}

		public ComponentsView<T0, T1> Build<T0, T1>(Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
		{
			var list0 = _storage.ListPtrSoft<T0>();
			if (!_hasComponents.Contains(list0.TypeLess()))
			{
				_hasComponents.Add(list0.TypeLess());
			}
			var list1 = _storage.ListPtrSoft<T1>();
			if (!_hasComponents.Contains(list1.TypeLess()))
			{
				_hasComponents.Add(list1.TypeLess());
			}
			var hasComponents     = UnsafeArray<ComponentListPtr>.Move(ref _hasComponents, allocator);
			var excludeComponents = UnsafeArray<ComponentListPtr>.Move(ref _excludeComponents, allocator);
			if (disposeWithStorage)
			{
				_storage.AddAllocation(hasComponents.AsAllocation());
				_storage.AddAllocation(excludeComponents.AsAllocation());
			}
			return new(list0, list1, hasComponents, excludeComponents, _onlyWhenStructuralChanges);
		}

		public ComponentsView<T0, T1, T2> Build<T0, T1, T2>(Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
		{
			var list0 = _storage.ListPtrSoft<T0>();
			if (!_hasComponents.Contains(list0.TypeLess()))
			{
				_hasComponents.Add(list0.TypeLess());
			}
			var list1 = _storage.ListPtrSoft<T1>();
			if (!_hasComponents.Contains(list1.TypeLess()))
			{
				_hasComponents.Add(list1.TypeLess());
			}
			var list2 = _storage.ListPtrSoft<T2>();
			if (!_hasComponents.Contains(list2.TypeLess()))
			{
				_hasComponents.Add(list2.TypeLess());
			}
			var hasComponents     = UnsafeArray<ComponentListPtr>.Move(ref _hasComponents, allocator);
			var excludeComponents = UnsafeArray<ComponentListPtr>.Move(ref _excludeComponents, allocator);
			if (disposeWithStorage)
			{
				_storage.AddAllocation(hasComponents.AsAllocation());
				_storage.AddAllocation(excludeComponents.AsAllocation());
			}
			return new(list0, list1, list2, hasComponents, excludeComponents, _onlyWhenStructuralChanges);
		}

		public ComponentsView<T0, T1, T2, T3> Build<T0, T1, T2, T3>(Allocator allocator = Allocator.Persistent, bool disposeWithStorage = true)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
			where T3 : unmanaged, IComponent
		{
			var list0 = _storage.ListPtrSoft<T0>();
			if (!_hasComponents.Contains(list0.TypeLess()))
			{
				_hasComponents.Add(list0.TypeLess());
			}
			var list1 = _storage.ListPtrSoft<T1>();
			if (!_hasComponents.Contains(list1.TypeLess()))
			{
				_hasComponents.Add(list1.TypeLess());
			}
			var list2 = _storage.ListPtrSoft<T2>();
			if (!_hasComponents.Contains(list2.TypeLess()))
			{
				_hasComponents.Add(list2.TypeLess());
			}
			var list3 = _storage.ListPtrSoft<T3>();
			if (!_hasComponents.Contains(list3.TypeLess()))
			{
				_hasComponents.Add(list3.TypeLess());
			}
			var hasComponents     = UnsafeArray<ComponentListPtr>.Move(ref _hasComponents, allocator);
			var excludeComponents = UnsafeArray<ComponentListPtr>.Move(ref _excludeComponents, allocator);
			if (disposeWithStorage)
			{
				_storage.AddAllocation(hasComponents.AsAllocation());
				_storage.AddAllocation(excludeComponents.AsAllocation());
			}
			return new(list0, list1, list2, list3, hasComponents, excludeComponents, _onlyWhenStructuralChanges);
		}
	}

	public static class ComponentsViewBuilderExt
	{
		public static ref ComponentsViewBuilder OnlyWhenStructuralChanges(this ref ComponentsViewBuilder builder)
		{
			builder._onlyWhenStructuralChanges = true;
			return ref builder;
		}

		public static ref ComponentsViewBuilder With<T>(this ref ComponentsViewBuilder builder) where T : unmanaged, IComponent
		{
			var listPtr = builder._storage.ListPtrSoft<T>().TypeLess();
			if (!builder._hasComponents.Contains(listPtr))
			{
				builder._hasComponents.Add(listPtr);
			}
			return ref builder;
		}

		public static ref ComponentsViewBuilder Exclude<T>(this ref ComponentsViewBuilder builder) where T : unmanaged, IComponent
		{
			var listPtr = builder._storage.ListPtrSoft<T>().TypeLess();
			if (!builder._excludeComponents.Contains(listPtr))
			{
				builder._excludeComponents.Add(listPtr);
			}
			return ref builder;
		}
	}
}
