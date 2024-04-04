using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;

#nullable enable

namespace KVD.ECS.Core.Components
{
	public interface IConvertableMonoBehavior
	{
		void Register(Entity entity, World world, ComponentsStorage target);
		void Restore(Entity entity, World world, ComponentsStorage target) => Register(entity, world, target);
	}
	
	public interface ISingleConvertableMonoBehavior<T> : IConvertableMonoBehavior where T : unmanaged, IComponent
	{
		void Component(World world, ComponentsStorage target, out T component);

		unsafe void IConvertableMonoBehavior.Register(Entity entity, World world, ComponentsStorage target)
		{
			var convertableStorage = target.ListPtr(ComponentTypeHandle.From<T>());
			Component(world, target, out var component);
			convertableStorage.AsList().Add(entity, &component);
		}
	}

	public interface ISimpleSingleConvertableMonoBehavior<T> : ISingleConvertableMonoBehavior<T> where T : unmanaged, IComponent
	{
		new T Component{ get; }

		void ISingleConvertableMonoBehavior<T>.Component(World world, ComponentsStorage target, out T component)
		{
			component = Component;
		}
	}

	public interface IDefaultSingleConvertableMonoBehavior<T> : ISimpleSingleConvertableMonoBehavior<T> where T : unmanaged, IComponent
	{
		T ISimpleSingleConvertableMonoBehavior<T>.Component => default;
	}
}
