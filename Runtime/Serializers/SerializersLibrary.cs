using System;
using System.Collections.Generic;
using System.Linq;
using KVD.ECS.Components;
using KVD.Utils.Extensions;

namespace KVD.ECS.Serializers
{
	public static class SerializersLibrary
	{
		private static readonly Type ProviderInterface = typeof(ISerializersProvider);
		private static readonly Dictionary<Type, IComponentSerializer> Serializers = new();

		static SerializersLibrary()
		{
			AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(t => ProviderInterface.IsAssignableFrom(t) && t.IsClass)
				.Select(t => (ISerializersProvider)Activator.CreateInstance(t))
				.ForEachSlow(p => p.FillSerializers(Serializers));
		}

		public static void RegisterSerializer<T>(IComponentSerializer<T> serializer) where T : struct, IComponent
		{
			Serializers[typeof(T)] = serializer;
		}

		public static IComponentSerializer<T> Serializer<T>() where T : struct, IComponent
		{
			Serializers.TryGetValue(typeof(T), out var serializer);
			return (IComponentSerializer<T>)serializer;
		}
	}
}
