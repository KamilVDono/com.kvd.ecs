using System;

#nullable enable

namespace KVD.ECS.Components
{
	public interface IConvertableMonoBehavior
	{
		Type ComponentType{ get; }
		object Component(World world, ComponentsStorage target);
		object? Restore(World world, ComponentsStorage target) => Component(world, target);
	}
	
	public interface IConvertableMonoBehavior<out T> : IConvertableMonoBehavior where T : struct, IComponent
	{
		Type IConvertableMonoBehavior.ComponentType => typeof(T);
	}
	
	public interface ISimpleConvertableMonoBehavior<out T> : IConvertableMonoBehavior<T> where T : struct, IComponent
	{
		public new T Component{ get; }
		object IConvertableMonoBehavior.Component(World world, ComponentsStorage target) => Component;
	}
	
	public interface IDefaultConvertableMonoBehavior<out T> : ISimpleConvertableMonoBehavior<T> where T : struct, IComponent
	{
		T ISimpleConvertableMonoBehavior<T>.Component => Component;
		public new T Component => default;
	}
}
