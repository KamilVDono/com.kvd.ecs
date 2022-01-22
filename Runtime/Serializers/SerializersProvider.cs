using System;
using System.Collections.Generic;
using KVD.ECS.ComponentHelpers;
using UnityEngine.Scripting;

namespace KVD.ECS.Serializers
{
	[Preserve]
	public class SerializersProvider : ISerializersProvider
	{
		public void FillSerializers(Dictionary<Type, IComponentSerializer> serializers)
		{
			serializers[typeof(PrefabWrapper)] = new PrefabWrapperSerializer();
		}
	}
}
