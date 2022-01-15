using System.IO;
using KVD.ECS.Serializers;
using KVD.ECS.Tests.Components;

namespace KVD.ECS.Tests.Serializers
{
	public class RadiusSerializer : IComponentSerializer<Radius>
	{
		
		
		public void WriteBytes(Radius target, BinaryWriter writer)
		{
			writer.Write(target.r);

		}

		
		public Radius ReadBytes(BinaryReader reader)
		{
			var r = reader.ReadSingle();


			var deserializedStruct = new Radius()
{
				r = r,

};

			return deserializedStruct;
		}

	}
}