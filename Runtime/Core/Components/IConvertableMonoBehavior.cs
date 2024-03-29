using System;
using KVD.ECS.Core.Entities;

#nullable enable

namespace KVD.ECS.Core.Components
{
	public interface IConvertableMonoBehavior
	{
		void Register(Entity entity, World world, ComponentsStorage target);
		void Restore(Entity entity, World world, ComponentsStorage target) => Register(entity, world, target);
	}
	
	public interface ISingleConvertableMonoBehavior : IConvertableMonoBehavior
	{
		Type ComponentType{ get; }
		object Component(World world, ComponentsStorage target);
		
		void IConvertableMonoBehavior.Register(Entity entity, World world, ComponentsStorage target)
		{
			// TODO: do it from ground zero
			// var convertableStorage = target.List(ComponentType);
			// var component          = Component(world, target);
			// convertableStorage.AddByObject(entity, component);
		}
	}
	
	public interface ISingleConvertableMonoBehavior<out T> : ISingleConvertableMonoBehavior where T : struct, IComponent
	{
		Type ISingleConvertableMonoBehavior.ComponentType => typeof(T);
	}
	
	public interface ISimpleSingleConvertableMonoBehavior<out T> : ISingleConvertableMonoBehavior<T> where T : struct, IComponent
	{
		public new T Component{ get; }
		
		object ISingleConvertableMonoBehavior.Component(World world, ComponentsStorage target) => Component;
	}
	
	public interface IDefaultSingleConvertableMonoBehavior<out T> : ISimpleSingleConvertableMonoBehavior<T> where T : struct, IComponent
	{
		T ISimpleSingleConvertableMonoBehavior<T>.Component => Component;
		public new T Component => default;
	}
}
