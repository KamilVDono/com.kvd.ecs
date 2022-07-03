using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Mathematics;

#nullable enable

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class Archetype<T0>
		where T0 : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
		private readonly LazyComponentList<T0> _list;
		
		public int Length => _list.Length;
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list    = new(_storage);
		}
	
		public bool Has(Entity entity)
		{
			return _list.ReadonlyList.Has(entity);
		}
	
		public Entity Create(T0 component)
		{
			var entity = _storage.NextEntity();
			_list.List.Add(entity, component);
			return entity;
		}
		
		public void Update(Entity entity, T0 component)
		{
			_list.List.AddOrReplace(entity, component);
		}

		public void Remove(Entity entity)
		{
			_list.List.Remove(entity);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class Archetype<T0, T1>
		where T0 : struct, IComponent
		where T1 : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
		private readonly LazyComponentList<T0> _list0;
		private readonly LazyComponentList<T1> _list1;
		
		public int Length => math.max(_list0.Length, _list1.Length);
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0   = new(_storage);
			_list1   = new(_storage);
		}
		
		public bool Has(Entity entity)
		{
			return _list0.ReadonlyList.Has(entity) && _list1.ReadonlyList.Has(entity);
		}
		
		public Entity Create(T0 component0)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			return entity;
		}
		
		public Entity Create(T1 component1)
		{
			var entity = _storage.NextEntity();
			_list1.List.Add(entity, component1);
			return entity;
		}
	
		public Entity Create(T0 component0, T1 component1)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			_list1.List.Add(entity, component1);
			return entity;
		}
		
		public void Update(Entity entity, T0 component)
		{
			_list0.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T1 component)
		{
			_list1.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T0 component0, T1 component1)
		{
			_list0.List.AddOrReplace(entity, component0);
			_list1.List.AddOrReplace(entity, component1);
		}
		
		public void Remove(Entity entity)
		{
			_list0.List.Remove(entity);
			_list1.List.Remove(entity);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class Archetype<T0, T1, T2>
		where T0 : struct, IComponent
		where T1 : struct, IComponent
		where T2 : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
		private readonly LazyComponentList<T0> _list0;
		private readonly LazyComponentList<T1> _list1;
		private readonly LazyComponentList<T2> _list2;
		
		public int Length => math.max(_list0.Length, math.max(_list1.Length, _list2.Length));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0   = new(_storage);
			_list1   = new(_storage);
			_list2   = new(_storage);
		}
		
		public bool Has(Entity entity)
		{
			return _list0.ReadonlyList.Has(entity) && _list1.ReadonlyList.Has(entity) && _list2.ReadonlyList.Has(entity);
		}
		
		public Entity Create(T0 component0)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			return entity;
		}
		
		public Entity Create(T1 component1)
		{
			var entity = _storage.NextEntity();
			_list1.List.Add(entity, component1);
			return entity;
		}
		
		public Entity Create(T2 component2)
		{
			var entity = _storage.NextEntity();
			_list2.List.Add(entity, component2);
			return entity;
		}
	
		public Entity Create(T0 component0, T1 component1, T2 component2)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			_list1.List.Add(entity, component1);
			_list2.List.Add(entity, component2);
			return entity;
		}
		
		public void Update(Entity entity, T0 component)
		{
			_list0.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T1 component)
		{
			_list1.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T2 component)
		{
			_list2.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T0 component0, T1 component1, T2 component2)
		{
			_list0.List.AddOrReplace(entity, component0);
			_list1.List.AddOrReplace(entity, component1);
			_list2.List.AddOrReplace(entity, component2);
		}
		
		public void Remove(Entity entity)
		{
			_list0.List.Remove(entity);
			_list1.List.Remove(entity);
			_list2.List.Remove(entity);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class Archetype<T0, T1, T2, T3>
		where T0 : struct, IComponent
		where T1 : struct, IComponent
		where T2 : struct, IComponent
		where T3 : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
		private readonly LazyComponentList<T0> _list0;
		private readonly LazyComponentList<T1> _list1;
		private readonly LazyComponentList<T2> _list2;
		private readonly LazyComponentList<T3> _list3;
		
		public int Length => math.max(_list0.Length, math.max(_list1.Length, math.max(_list2.Length, _list3.Length)));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0   = new(_storage);
			_list1   = new(_storage);
			_list2   = new(_storage);
			_list3   = new(_storage);
		}
		
		public bool Has(Entity entity)
		{
			return _list0.ReadonlyList.Has(entity) && _list1.ReadonlyList.Has(entity) && _list2.ReadonlyList.Has(entity) && _list3.ReadonlyList.Has(entity);
		}
		
		public Entity Create(T0 component0)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			return entity;
		}
		
		public Entity Create(T1 component1)
		{
			var entity = _storage.NextEntity();
			_list1.List.Add(entity, component1);
			return entity;
		}
		
		public Entity Create(T2 component2)
		{
			var entity = _storage.NextEntity();
			_list2.List.Add(entity, component2);
			return entity;
		}
		
		public Entity Create(T3 component3)
		{
			var entity = _storage.NextEntity();
			_list3.List.Add(entity, component3);
			return entity;
		}
	
		public Entity Create(T0 component0, T1 component1, T2 component2, T3 component3)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			_list1.List.Add(entity, component1);
			_list2.List.Add(entity, component2);
			_list3.List.Add(entity, component3);
			return entity;
		}
		
		public void Update(Entity entity, T0 component)
		{
			_list0.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T1 component)
		{
			_list1.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T2 component)
		{
			_list2.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T3 component)
		{
			_list3.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T0 component0, T1 component1, T2 component2, T3 component3)
		{
			_list0.List.AddOrReplace(entity, component0);
			_list1.List.AddOrReplace(entity, component1);
			_list2.List.AddOrReplace(entity, component2);
			_list3.List.AddOrReplace(entity, component3);
		}
		
		public void Remove(Entity entity)
		{
			_list0.List.Remove(entity);
			_list1.List.Remove(entity);
			_list2.List.Remove(entity);
			_list3.List.Remove(entity);
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class Archetype<T0, T1, T2, T3, T4>
		where T0 : struct, IComponent
		where T1 : struct, IComponent
		where T2 : struct, IComponent
		where T3 : struct, IComponent
		where T4 : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
		private readonly LazyComponentList<T0> _list0;
		private readonly LazyComponentList<T1> _list1;
		private readonly LazyComponentList<T2> _list2;
		private readonly LazyComponentList<T3> _list3;
		private readonly LazyComponentList<T4> _list4;
	
		public int Length => math.max(_list0.Length,
			math.max(_list1.Length, math.max(_list2.Length, math.max(_list3.Length, _list4.Length))));
	
		public Archetype(ComponentsStorage storage)
		{
			_storage = storage;
			_list0   = new(_storage);
			_list1   = new(_storage);
			_list2   = new(_storage);
			_list3   = new(_storage);
			_list4   = new(_storage);
		}
		
		public bool Has(Entity entity)
		{
			return _list0.ReadonlyList.Has(entity) &&
			       _list1.ReadonlyList.Has(entity) &&
			       _list2.ReadonlyList.Has(entity) &&
			       _list3.ReadonlyList.Has(entity) &&
			       _list4.ReadonlyList.Has(entity);
		}
		
		public Entity Create(T0 component0)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			return entity;
		}
		
		public Entity Create(T1 component1)
		{
			var entity = _storage.NextEntity();
			_list1.List.Add(entity, component1);
			return entity;
		}
		
		public Entity Create(T2 component2)
		{
			var entity = _storage.NextEntity();
			_list2.List.Add(entity, component2);
			return entity;
		}
		
		public Entity Create(T3 component3)
		{
			var entity = _storage.NextEntity();
			_list3.List.Add(entity, component3);
			return entity;
		}
		
		public Entity Create(T4 component4)
		{
			var entity = _storage.NextEntity();
			_list4.List.Add(entity, component4);
			return entity;
		}
	
		public Entity Create(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4)
		{
			var entity = _storage.NextEntity();
			_list0.List.Add(entity, component0);
			_list1.List.Add(entity, component1);
			_list2.List.Add(entity, component2);
			_list3.List.Add(entity, component3);
			_list4.List.Add(entity, component4);
			return entity;
		}
	
		public void Update(Entity entity, T0 component)
		{
			_list0.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T1 component)
		{
			_list1.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T2 component)
		{
			_list2.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T3 component)
		{
			_list3.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T4 component)
		{
			_list4.List.AddOrReplace(entity, component);
		}
		
		public void Update(Entity entity, T0 component0, T1 component1, T2 component2, T3 component3, T4 component4)
		{
			_list0.List.AddOrReplace(entity, component0);
			_list1.List.AddOrReplace(entity, component1);
			_list2.List.AddOrReplace(entity, component2);
			_list3.List.AddOrReplace(entity, component3);
			_list4.List.AddOrReplace(entity, component4);
		}
		
		public void Remove(Entity entity)
		{
			_list0.List.Remove(entity);
			_list1.List.Remove(entity);
			_list2.List.Remove(entity);
			_list3.List.Remove(entity);
			_list4.List.Remove(entity);
		}
	}
	
	internal class LazyComponentList<T> where T : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
	
		private ComponentList<T>? _list;
		private IReadonlyComponentListView<T> _readonlyList;
		
		public ComponentList<T> List => _list ??= _storage.List<T>();
		public IReadonlyComponentListView<T> ReadonlyList => _list ?? (_readonlyList = _readonlyList.Sync());
	
		public int Length => ReadonlyList.Length;
	
		public LazyComponentList(ComponentsStorage storage)
		{
			_storage      = storage;
			_readonlyList = _storage.ReadonlyListView<T>();
		}
	}
}
