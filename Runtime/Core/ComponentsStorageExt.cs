using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public static class ComponentsStorageExt
	{
		#region Add
		public static Entity Add<T>(this ComponentsStorage storage, T component) where T : struct, IComponent
		{
			var entity = storage.NextEntity();
			storage.List<T>().Add(entity, component);
			return entity;
		}
	
		public static Entity Add<T0, T1>(this ComponentsStorage storage, T0 component0, T1 component1)
			where T0 : struct, IComponent
			where T1 : struct, IComponent
		{
			var entity = storage.NextEntity();
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			return entity;
		}
	
		public static Entity Add<T0, T1, T2>(
			this ComponentsStorage storage, T0 component0, T1 component1, T2 component2)
			where T0 : struct, IComponent
			where T1 : struct, IComponent
			where T2 : struct, IComponent
		{
			var entity = storage.NextEntity();
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			storage.List<T2>().Add(entity, component2);
			return entity;
		}
	
		public static Entity Add<T>(this ComponentsStorage storage, Entity entity, T component)
			where T : struct, IComponent
		{
			storage.List<T>().Add(entity, component);
			return entity;
		}
	
		public static Entity Add<T0, T1>(this ComponentsStorage storage, Entity entity, T0 component0, T1 component1)
			where T0 : struct, IComponent
			where T1 : struct, IComponent
		{
			storage.List<T0>().Add(entity, component0);
			storage.List<T1>().Add(entity, component1);
			return entity;
		}
	
		public static Entity Add<T0, T1, T2>(
			this ComponentsStorage storage, Entity entity, T0 component0, T1 component1, T2 component2)
			where T0 : struct, IComponent
			where T1 : struct, IComponent
			where T2 : struct, IComponent
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
			for (var i = 0; i < lists.Count; i++)
			{
				if (lists[i].Has(entity))
				{
					lists[i].Remove(entity);
				}
			}
			storage.ReturnEntity(entity);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlive(this ComponentsStorage storage, Entity entity)
		{
			var isAlive = false;
			var i       = 0;
			var lists   = storage.AllLists;
			
			while (!isAlive && i < lists.Count)
			{
				var list = lists[i];
				isAlive = list.Has(entity);
				++i;
			}
			return isAlive;
		}
	
		public static RentedArray<Entity> NextEntitiesBulk(this ComponentsStorage storage, int length)
		{
			var entities = new RentedArray<Entity>(length);
			for (var i = 0; i < length; i++)
			{
				entities[i] = storage.NextEntity();
			}
			return entities;
		}
		
		public static RentedArray<Entity> AddToAllBulk(this ComponentsStorage storage, int length)
		{
			var entities = new RentedArray<Entity>(length);
			for (var i = 0; i < length; i++)
			{
				entities[i] = storage.NextEntity();
			}
			foreach (var components in storage.AllLists)
			{
				components.BulkAdd(entities);
			}
			return entities;
		}
		
		public static void ReturnBulkAddedEntities(this ComponentsStorage _, RentedArray<Entity> entities)
		{
			entities.Dispose();
		}
	
		public static void Remove<T>(this ComponentsStorage storage, Entity entity) where T : struct, IComponent
		{
			storage.List<T>().Remove(entity);
		}
	}
}
