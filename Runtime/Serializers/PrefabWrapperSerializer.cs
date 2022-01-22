using System.IO;
using KVD.ECS.ComponentHelpers;

namespace KVD.ECS.Serializers
{
	public class PrefabWrapperSerializer : IComponentSerializer<PrefabWrapper>
	{
		public void WriteBytes(PrefabWrapper target, BinaryWriter writer)
		{
			writer.Write(target.prefabKey);
		}
		
		public PrefabWrapper ReadBytes(BinaryReader reader)
		{
			var prefabKey = reader.ReadString();
			return new() { prefabKey = prefabKey, };
		}
	}
}
