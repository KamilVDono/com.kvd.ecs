using System;
using System.Collections.Generic;

namespace KVD.ECS.Serializers
{
	public interface ISerializersProvider
	{
		void FillSerializers(Dictionary<Type, IComponentSerializer> serializers);
	}
}
