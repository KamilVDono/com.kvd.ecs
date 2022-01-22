using System;
using System.Collections.Generic;
using KVD.ECS.Core.Components;
using UnityEngine;
using Object = UnityEngine.Object;

#nullable enable

namespace KVD.ECS.ComponentHelpers
{
	public readonly struct MonoComponentWrapper<T> : IEquatable<MonoComponentWrapper<T>>, IMonoComponent where T : Component
	{
		private static readonly bool CallDestroy = typeof(T) != typeof(Transform);
		
		public readonly T value;

		public TK As<TK>() where TK : T => (TK)value;
		
		public MonoComponentWrapper(T value)
		{
			this.value = value;
		}

		public bool Equals(MonoComponentWrapper<T> other)
		{
			return EqualityComparer<T>.Default.Equals(value, other.value);
		}
		public override bool Equals(object? obj)
		{
			return obj is MonoComponentWrapper<T> other && Equals(other);
		}
		public override int GetHashCode()
		{
			return value.GetHashCode();
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
			if (value && CallDestroy)
			{
				Object.Destroy(value);
			}
		}
	}
}
