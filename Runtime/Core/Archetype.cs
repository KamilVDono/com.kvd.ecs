using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public readonly struct Archetype<T0>
		where T0 : unmanaged, IComponent
	{
		readonly ComponentsStorage _storage;
		readonly ComponentListPtrSoft<T0> _list;

		public int Length => ArchetypeHelpers.Length(_list);
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list    = storage.ListPtrSoft<T0>();
		}
	
		public bool Has(Entity entity)
		{
			ArchetypeHelpers.Has(entity, _list, out var has);
			return has;
		}
	
		public Entity Create(in T0 component)
		{
			var entity = _storage.NextEntity();
			ArchetypeHelpers.Add(entity, _list, component);
			return entity;
		}
		
		public void Update(Entity entity, in T0 component)
		{
			ArchetypeHelpers.Update(entity, _list, component);
		}

		public void Remove(Entity entity)
		{
			ArchetypeHelpers.Remove(entity, _list);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public readonly struct Archetype<T0, T1>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
	{
		readonly ComponentsStorage _storage;
		readonly ComponentListPtrSoft<T0> _list0;
		readonly ComponentListPtrSoft<T1> _list1;
		
		public int Length => math.max(ArchetypeHelpers.Length(_list0), ArchetypeHelpers.Length(_list1));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0   = storage.ListPtrSoft<T0>();
			_list1   = storage.ListPtrSoft<T1>();
		}
		
		public bool Has(Entity entity)
		{
			ArchetypeHelpers.Has(entity, _list0, out var has0);
			ArchetypeHelpers.Has(entity, _list1, out var has1);
			return has0 && has1;
		}

		public Entity Create(in T0 component0 = default, in T1 component1 = default)
		{
			var entity = _storage.NextEntity();
			ArchetypeHelpers.Add(entity, _list0, component0);
			ArchetypeHelpers.Add(entity, _list1, component1);
			return entity;
		}
		
		public void Update(Entity entity, in T0 component)
		{
			ArchetypeHelpers.Update(entity, _list0, component);
		}
		
		public void Update(Entity entity, in T1 component)
		{
			ArchetypeHelpers.Update(entity, _list1, component);
		}
		
		public void Update(Entity entity, in T0 component0, in T1 component1)
		{
			ArchetypeHelpers.Update(entity, _list0, component0);
			ArchetypeHelpers.Update(entity, _list1, component1);
		}
		
		public void Remove(Entity entity)
		{
			ArchetypeHelpers.Remove(entity, _list0);
			ArchetypeHelpers.Remove(entity, _list1);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public readonly struct Archetype<T0, T1, T2>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
		where T2 : unmanaged, IComponent
	{
		readonly ComponentsStorage _storage;
		readonly ComponentListPtrSoft<T0> _list0;
		readonly ComponentListPtrSoft<T1> _list1;
		readonly ComponentListPtrSoft<T2> _list2;

		public int Length => math.max(ArchetypeHelpers.Length(_list0),
			math.max(ArchetypeHelpers.Length(_list1), ArchetypeHelpers.Length(_list2)));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0   = storage.ListPtrSoft<T0>();
			_list1   = storage.ListPtrSoft<T1>();
			_list2   = storage.ListPtrSoft<T2>();
		}
		
		public bool Has(Entity entity)
		{
			ArchetypeHelpers.Has(entity, _list0, out var has0);
			ArchetypeHelpers.Has(entity, _list1, out var has1);
			ArchetypeHelpers.Has(entity, _list2, out var has2);
			return has0 & has1 & has2;
		}
	
		public Entity Create(in T0 component0 = default, in T1 component1 = default, in T2 component2 = default)
		{
			var entity = _storage.NextEntity();
			ArchetypeHelpers.Add(entity, _list0, component0);
			ArchetypeHelpers.Add(entity, _list1, component1);
			ArchetypeHelpers.Add(entity, _list2, component2);
			return entity;
		}
		
		public void Update(Entity entity, in T0 component)
		{
			ArchetypeHelpers.Update(entity, _list0, component);
		}

		public void Update(Entity entity, in T1 component)
		{
			ArchetypeHelpers.Update(entity, _list1, component);
		}
		
		public void Update(Entity entity, in T2 component)
		{
			ArchetypeHelpers.Update(entity, _list2, component);
		}
		
		public void Update(Entity entity, in T0 component0, in T1 component1, in T2 component2)
		{
			ArchetypeHelpers.Update(entity, _list0, component0);
			ArchetypeHelpers.Update(entity, _list1, component1);
			ArchetypeHelpers.Update(entity, _list2, component2);
		}
		
		public void Remove(Entity entity)
		{
			ArchetypeHelpers.Remove(entity, _list0);
			ArchetypeHelpers.Remove(entity, _list1);
			ArchetypeHelpers.Remove(entity, _list2);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public readonly struct Archetype<T0, T1, T2, T3>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
		where T2 : unmanaged, IComponent
		where T3 : unmanaged, IComponent
	{
		readonly ComponentsStorage _storage;
		readonly ComponentListPtrSoft<T0> _list0;
		readonly ComponentListPtrSoft<T1> _list1;
		readonly ComponentListPtrSoft<T2> _list2;
		readonly ComponentListPtrSoft<T3> _list3;

		public int Length => math.max(ArchetypeHelpers.Length(_list0),
			math.max(ArchetypeHelpers.Length(_list1),
				math.max(ArchetypeHelpers.Length(_list2), ArchetypeHelpers.Length(_list3))));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0 = storage.ListPtrSoft<T0>();
			_list1 = storage.ListPtrSoft<T1>();
			_list2 = storage.ListPtrSoft<T2>();
			_list3 = storage.ListPtrSoft<T3>();
		}
		
		public bool Has(Entity entity)
		{
			ArchetypeHelpers.Has(entity, _list0, out var has0);
			ArchetypeHelpers.Has(entity, _list1, out var has1);
			ArchetypeHelpers.Has(entity, _list2, out var has2);
			ArchetypeHelpers.Has(entity, _list3, out var has3);
			return has0 & has1 & has2 & has3;
		}
	
		public Entity Create(in T0 component0 = default, in T1 component1 = default, in T2 component2 = default,
			in T3 component3 = default)
		{
			var entity = _storage.NextEntity();
			ArchetypeHelpers.Add(entity, _list0, component0);
			ArchetypeHelpers.Add(entity, _list1, component1);
			ArchetypeHelpers.Add(entity, _list2, component2);
			ArchetypeHelpers.Add(entity, _list3, component3);
			return entity;
		}
		
		public void Update(Entity entity, in T0 component)
		{
			ArchetypeHelpers.Update(entity, _list0, component);
		}

		public void Update(Entity entity, in T1 component)
		{
			ArchetypeHelpers.Update(entity, _list1, component);
		}

		public void Update(Entity entity, in T2 component)
		{
			ArchetypeHelpers.Update(entity, _list2, component);
		}

		public void Update(Entity entity, in T3 component)
		{
			ArchetypeHelpers.Update(entity, _list3, component);
		}
		
		public void Update(Entity entity, in T0 component0, in T1 component1, in T2 component2, in T3 component3)
		{
			ArchetypeHelpers.Update(entity, _list0, component0);
			ArchetypeHelpers.Update(entity, _list1, component1);
			ArchetypeHelpers.Update(entity, _list2, component2);
			ArchetypeHelpers.Update(entity, _list3, component3);
		}
		
		public void Remove(Entity entity)
		{
			ArchetypeHelpers.Remove(entity, _list0);
			ArchetypeHelpers.Remove(entity, _list1);
			ArchetypeHelpers.Remove(entity, _list2);
			ArchetypeHelpers.Remove(entity, _list3);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public readonly struct Archetype<T0, T1, T2, T3, T4>
		where T0 : unmanaged, IComponent
		where T1 : unmanaged, IComponent
		where T2 : unmanaged, IComponent
		where T3 : unmanaged, IComponent
		where T4 : unmanaged, IComponent
	{
		readonly ComponentsStorage _storage;
		readonly ComponentListPtrSoft<T0> _list0;
		readonly ComponentListPtrSoft<T1> _list1;
		readonly ComponentListPtrSoft<T2> _list2;
		readonly ComponentListPtrSoft<T3> _list3;
		readonly ComponentListPtrSoft<T4> _list4;

		public int Length => math.max(ArchetypeHelpers.Length(_list0),
			math.max(ArchetypeHelpers.Length(_list1),
				math.max(ArchetypeHelpers.Length(_list2),
					math.max(ArchetypeHelpers.Length(_list3), ArchetypeHelpers.Length(_list4)))));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0 = storage.ListPtrSoft<T0>();
			_list1 = storage.ListPtrSoft<T1>();
			_list2 = storage.ListPtrSoft<T2>();
			_list3 = storage.ListPtrSoft<T3>();
			_list4 = storage.ListPtrSoft<T4>();
		}
		
		public bool Has(Entity entity)
		{
			ArchetypeHelpers.Has(entity, _list0, out var has0);
			ArchetypeHelpers.Has(entity, _list1, out var has1);
			ArchetypeHelpers.Has(entity, _list2, out var has2);
			ArchetypeHelpers.Has(entity, _list3, out var has3);
			ArchetypeHelpers.Has(entity, _list4, out var has4);
			return has0 & has1 & has2 & has3 & has4;
		}

		public Entity Create(in T0 component0 = default, in T1 component1 = default, in T2 component2 = default,
			in T3 component3 = default, in T4 component4 = default)
		{
			var entity = _storage.NextEntity();
			ArchetypeHelpers.Add(entity, _list0, component0);
			ArchetypeHelpers.Add(entity, _list1, component1);
			ArchetypeHelpers.Add(entity, _list2, component2);
			ArchetypeHelpers.Add(entity, _list3, component3);
			ArchetypeHelpers.Add(entity, _list4, component4);
			return entity;
		}
	
		public void Update(Entity entity, in T0 component)
		{
			ArchetypeHelpers.Update(entity, _list0, component);
		}

		public void Update(Entity entity, in T1 component)
		{
			ArchetypeHelpers.Update(entity, _list1, component);
		}

		public void Update(Entity entity, in T2 component)
		{
			ArchetypeHelpers.Update(entity, _list2, component);
		}

		public void Update(Entity entity, in T3 component)
		{
			ArchetypeHelpers.Update(entity, _list3, component);
		}
		
		public void Update(Entity entity, in T4 component)
		{
			ArchetypeHelpers.Update(entity, _list4, component);
		}
		
		public void Update(Entity entity, in T0 component0, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
		{
			ArchetypeHelpers.Update(entity, _list0, component0);
			ArchetypeHelpers.Update(entity, _list1, component1);
			ArchetypeHelpers.Update(entity, _list2, component2);
			ArchetypeHelpers.Update(entity, _list3, component3);
			ArchetypeHelpers.Update(entity, _list4, component4);
		}
		
		public void Remove(Entity entity)
		{
			ArchetypeHelpers.Remove(entity, _list0);
			ArchetypeHelpers.Remove(entity, _list1);
			ArchetypeHelpers.Remove(entity, _list2);
			ArchetypeHelpers.Remove(entity, _list3);
			ArchetypeHelpers.Remove(entity, _list4);
		}
	}

	static class ArchetypeHelpers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Length<T>(ComponentListPtrSoft<T> list)
			where T : unmanaged, IComponent
		{
			return list.IsCreated ? list.ToList().Length : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Has<T>(Entity entity, ComponentListPtrSoft<T> list, out bool has)
			where T : unmanaged, IComponent
		{
			has = list.IsCreated & list.ToList().Has(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Add<T>(Entity entity, ComponentListPtrSoft<T> list, in T component)
			where T : unmanaged, IComponent
		{
			list.ToList().Add(entity, component);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Update<T>(Entity entity, ComponentListPtrSoft<T> list, in T component)
			where T : unmanaged, IComponent
		{
			list.ToList().AddOrReplace(entity, component);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(Entity entity, ComponentListPtrSoft<T> list)
			where T : unmanaged, IComponent
		{
			list.ToList().Remove(entity);
		}
	}
}
