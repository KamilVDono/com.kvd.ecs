using System.IO;
using KVD.ECS.GeneralTests.Components;
using KVD.ECS.Serializers;

namespace KVD.ECS.GeneralTests.Serializers
{
	public class CircleSerializer : IComponentSerializer<Circle>
	{
		private static readonly RadiusSerializer RadiusSerializer = new();
private static readonly PositionSerializer PositionSerializer = new();

		
		public void WriteBytes(Circle target, BinaryWriter writer)
		{
			writer.Write(target.id);
RadiusSerializer.WriteBytes(target.radius, writer);
PositionSerializer.WriteBytes(target.position, writer);

		}

		
		public Circle ReadBytes(BinaryReader reader)
		{
			var id = reader.ReadByte();
var radius = RadiusSerializer.ReadBytes(reader);
var position = PositionSerializer.ReadBytes(reader);


			var deserializedStruct = new Circle()
{
				id = id,
radius = radius,
position = position,

};

			return deserializedStruct;
		}

	}
}