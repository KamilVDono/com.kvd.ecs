using System.IO;
using KVD.ECS.Components;

namespace KVD.ECS.Serializers
{
	public interface IComponentSerializer
	{
	}

	public interface IComponentSerializer<T> : IComponentSerializer where T : struct, IComponent
	{
		public void WriteBytes(T target, BinaryWriter writer);
		
		public T ReadBytes(BinaryReader reader);
	}
}
