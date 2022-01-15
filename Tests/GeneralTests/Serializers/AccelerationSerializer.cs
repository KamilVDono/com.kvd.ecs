using System.IO;
using KVD.ECS.Serializers;
using KVD.ECS.Tests.Components;

namespace KVD.ECS.Tests.Serializers
{
	public class AccelerationSerializer : IComponentSerializer<Acceleration>
	{
		
		
		public void WriteBytes(Acceleration target, BinaryWriter writer)
		{
			writer.Write(target.x);
writer.Write(target.y);
writer.Write(target.z);

		}

		
		public Acceleration ReadBytes(BinaryReader reader)
		{
			var x = reader.ReadSingle();
var y = reader.ReadSingle();
var z = reader.ReadSingle();


			var deserializedStruct = new Acceleration()
{
				x = x,
y = y,
z = z,

};

			return deserializedStruct;
		}

	}
}