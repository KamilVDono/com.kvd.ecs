using KVD.ECS.Core.Components;

namespace KVD.ECS.Core.Entities
{
	public struct RichEntity : IComponent
	{
		public Entity entity;
		public ComponentsStorageKey key;

		public RichEntity(Entity entity, ComponentsStorageKey key)
		{
			this.entity = entity;
			this.key    = key;
		}

		public void Dispose() {}
	}
}
