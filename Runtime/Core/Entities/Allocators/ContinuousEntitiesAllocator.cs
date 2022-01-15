namespace KVD.ECS.Entities
{
	public class ContinuousEntitiesAllocator : IEntityAllocator
	{
		private Entity _lastEntity = Entity.Null;
		
		public Entity Allocate()
		{
			_lastEntity = _lastEntity.Next;
			return _lastEntity;
		}

		public void Return(Entity _) {}
	}
}
