using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.Utils.DataStructures;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public static class ComponentsStorageExt
	{
		#region Add
		public static Entity Add<T>(this ComponentsStorage storage, T component) where T : unmanaged, IComponent
		{
			var entity = storage.NextEntity();
			storage.List<T>().Add(entity, component);
			return entity;
		}
	
		public static Entity Add<T0, T1>(this ComponentsStorage storage, T0 component0, T1 component1)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
		{
			var entity = storage.NextEntity();
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			return entity;
		}
	
		public static Entity Add<T0, T1, T2>(
			this ComponentsStorage storage, T0 component0, T1 component1, T2 component2)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
		{
			var entity = storage.NextEntity();
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			storage.List<T2>().Add(entity, component2);
			return entity;
		}
	
		public static Entity Add<T>(this ComponentsStorage storage, Entity entity, T component)
			where T : unmanaged, IComponent
		{
			storage.List<T>().Add(entity, component);
			return entity;
		}
	
		public static Entity Add<T0, T1>(this ComponentsStorage storage, Entity entity, T0 component0, T1 component1)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
		{
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			return entity;
		}
	
		public static Entity Add<T0, T1, T2>(
			this ComponentsStorage storage, Entity entity, T0 component0, T1 component1, T2 component2)
			where T0 : unmanaged, IComponent
			where T1 : unmanaged, IComponent
			where T2 : unmanaged, IComponent
		{
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			storage.List<T2>().Add(entity, component2);
			return entity;
		}
		#endregion Add
		
		public static void RemoveEntity(this ComponentsStorage storage, int entity)
		{
			var lists = storage.AllLists;
			for (var i = 0u; i < lists.Length; i++)
			{
				if (lists[i].IsCreated)
				{
					var list = lists[i].AsList();
					if (list.Has(entity))
					{
						list.Remove(entity);
					}
				}
			}
			storage.ReturnEntity(entity);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlive(this ComponentsStorage storage, Entity entity)
		{
			var isAlive = false;
			var i       = 0u;
			var lists   = storage.AllLists;
			
			while (!isAlive && i < lists.Length)
			{
				var list = lists[i];
				isAlive = list.IsCreated && list.AsList().Has(entity);
				++i;
			}
			return isAlive;
		}
	
		public static UnsafeArray<Entity> NextEntitiesBulk(this ComponentsStorage storage, uint length, Allocator allocator)
		{
			var entities = new UnsafeArray<Entity>(length, allocator);
			for (var i = 0u; i < length; i++)
			{
				entities[i] = storage.NextEntity();
			}
			return entities;
		}
		
		public static UnsafeArray<Entity> AddToAllBulk(this ComponentsStorage storage, uint length, Allocator allocator)
		{
			var entities = new UnsafeArray<Entity>(length, allocator);
			for (var i = 0u; i < length; i++)
			{
				entities[i] = storage.NextEntity();
			}
			foreach (var components in storage.AllLists)
			{
				if (components.IsCreated)
				{
					components.AsList().BulkAdd(entities);
				}
			}
			return entities;
		}
	
		public static void Remove<T>(this ComponentsStorage storage, Entity entity) where T : unmanaged, IComponent
		{
			storage.List<T>().Remove(entity);
		}
	}
}
