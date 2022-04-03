using System;
using System.IO;

namespace KVD.ECS.Core.Entities.Allocators
{
	public sealed class CircularBufferEntitiesAllocator : IEntityAllocator
	{
		private int _size;
		private int _nextEntity;

		public CircularBufferEntitiesAllocator(int size = 128)
		{
			_size = size;
		}

		public Entity Allocate()
		{
			var entity = _nextEntity;
			_nextEntity = (_nextEntity+1)%_size;
			return entity;
		}
		
		public void Return(Entity _) {}
		
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(_size);
			writer.Write(_nextEntity);
		}
		
		public void Deserialize(BinaryReader reader)
		{
			_size       = reader.ReadInt32();
			_nextEntity = reader.ReadInt32();
		}

		public void AssertValidity(ComponentsStorage storage)
		{
			#if !DEBUG
			return;
			#endif
			var nextEntity = _nextEntity;
			if (storage.IsAlive(nextEntity))
			{
				throw new ApplicationException($"Next entity in {nameof(CircularBufferEntitiesAllocator)} for {storage} is still in usage");
			}
		}
	}
}
