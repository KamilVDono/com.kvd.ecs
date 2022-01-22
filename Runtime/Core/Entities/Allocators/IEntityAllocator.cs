namespace KVD.ECS.Core.Entities.Allocators
{
	public interface IEntityAllocator
	{
		public Entity Allocate();
		public void Return(Entity e);

		public void AssertValidity(ComponentsStorage storage)
		{
			return;
		}
	}
}
