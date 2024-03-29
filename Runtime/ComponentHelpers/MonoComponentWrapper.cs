using System;
using KVD.ECS.Core.Components;
using UnityEngine;
using Object = UnityEngine.Object;

#nullable enable

namespace KVD.ECS.ComponentHelpers
{
	public readonly struct MonoComponentWrapper<T> : IEquatable<MonoComponentWrapper<T>>, IMonoComponent
		where T : Component
	{
		private static readonly bool CallDestroy = typeof(T) != typeof(Transform);
		
		public readonly int instanceId;

		public TK As<TK>() where TK : T => (TK)Value;

		public T Value => (T)Resources.InstanceIDToObject(instanceId);
		
		public MonoComponentWrapper(T value)
		{
			this.instanceId = value.GetHashCode();
		}

		public MonoComponentWrapper(int instanceId)
		{
			this.instanceId = instanceId;
		}

		public bool Equals(MonoComponentWrapper<T> other)
		{
			return instanceId == other.instanceId;
		}
		public override bool Equals(object? obj)
		{
			return obj is MonoComponentWrapper<T> other && Equals(other);
		}
		public override int GetHashCode()
		{
			return instanceId;
		}
		public static bool operator ==(MonoComponentWrapper<T> left, MonoComponentWrapper<T> right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(MonoComponentWrapper<T> left, MonoComponentWrapper<T> right)
		{
			return !left.Equals(right);
		}
		
		public void Dispose()
		{
			var value = Value;
			if (value && CallDestroy)
			{
				Object.Destroy(value);
			}
		}
	}
}
