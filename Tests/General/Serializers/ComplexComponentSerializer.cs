using System;
using System.IO;
using KVD.ECS.GeneralTests.Components;
using KVD.ECS.Serializers;
using Unity.Mathematics;

namespace KVD.ECS.GeneralTests.Serializers
{
	public class ComplexComponentSerializer : IComponentSerializer<ComplexComponent>
	{
		private static readonly CircleSerializer CircleSerializer = new();

		
		public void WriteBytes(ComplexComponent target, BinaryWriter writer)
		{
			CircleSerializer.WriteBytes(target.circle, writer);
SerializersHelper.ToBytesStruct(target.transformation, writer);
SerializersHelper.ToBytesNativeArrayEntity(target.entities, writer);
SerializersHelper.ToBytesComponentNativeArrayComponent(target.positions, writer);
SerializersHelper.ToBytesNativeArray(target.floats, writer);
SerializersHelper.ToBytesNativeArray(target.matrices, writer);
SerializersHelper.ToBytesComponentNativeArrayComponent(target.empty, writer);

		}

		
		public ComplexComponent ReadBytes(BinaryReader reader)
		{
			var circle = CircleSerializer.ReadBytes(reader);
var transformation = SerializersHelper.FromMarshalBytes<float4x4>(reader);
var entities = SerializersHelper.FromBytesNativeArrayEntity(reader);
var positions = SerializersHelper.FromBytesNativeArrayComponent<Position>(reader);
var floats = SerializersHelper.FromBytesNativeArray<Single>(reader);
var matrices = SerializersHelper.FromBytesNativeArray<float4x4>(reader);
var empty = SerializersHelper.FromBytesNativeArrayComponent<Circle>(reader);


			var deserializedStruct = new ComplexComponent()
{
				circle = circle,
transformation = transformation,
entities = entities,
positions = positions,
floats = floats,
matrices = matrices,
empty = empty,

};

			return deserializedStruct;
		}

	}
}