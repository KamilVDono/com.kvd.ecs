using System;
using System.Collections.Generic;
using KVD.ECS.Serializers;
using KVD.ECS.Tests.Components;
using UnityEngine.Scripting;

namespace KVD.ECS.Tests.Serializers
{
	[Preserve]
	public class SerializersProvider : ISerializersProvider
	{
		public void FillSerializers(Dictionary<Type, IComponentSerializer> serializers)
		{
			serializers[typeof(Circle)] = new CircleSerializer();
serializers[typeof(ComplexComponent)] = new ComplexComponentSerializer();
serializers[typeof(Acceleration)] = new AccelerationSerializer();
serializers[typeof(Position)] = new PositionSerializer();
serializers[typeof(Radius)] = new RadiusSerializer();

		}
	}
}