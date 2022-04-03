using System.IO;

namespace KVD.ECS.Core.Entities.Allocators
{
	public interface IEntityAllocator
	{
		public Entity Allocate();
		public void Return(Entity e);
		public void Serialize(BinaryWriter writer);
		public void Deserialize(BinaryReader reader);

		public void AssertValidity(ComponentsStorage storage)
		{
		}
	}
}
