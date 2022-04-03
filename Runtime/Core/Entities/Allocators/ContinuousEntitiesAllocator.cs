using System.IO;

namespace KVD.ECS.Core.Entities.Allocators
{
	public sealed class ContinuousEntitiesAllocator : IEntityAllocator
	{
		private Entity _lastEntity = Entity.Null;
		
		public Entity Allocate()
		{
			_lastEntity = _lastEntity.Next;
			return _lastEntity;
		}

		public void Return(Entity _) {}
		
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(_lastEntity.index);
		}
		
		public void Deserialize(BinaryReader reader)
		{
			_lastEntity = reader.ReadInt32();
		}
	}
}
