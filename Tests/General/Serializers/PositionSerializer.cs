using System.IO;
using KVD.ECS.GeneralTests.Components;
using KVD.ECS.Serializers;

namespace KVD.ECS.GeneralTests.Serializers
{
	public class PositionSerializer : IComponentSerializer<Position>
	{
		
		
		public void WriteBytes(Position target, BinaryWriter writer)
		{
			writer.Write(target.x);
writer.Write(target.y);
writer.Write(target.z);

		}

		
		public Position ReadBytes(BinaryReader reader)
		{
			var x = reader.ReadSingle();
var y = reader.ReadSingle();
var z = reader.ReadSingle();


			var deserializedStruct = new Position()
{
				x = x,
y = y,
z = z,

};

			return deserializedStruct;
		}

	}
}