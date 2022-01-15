namespace KVD.ECS.Entities
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
