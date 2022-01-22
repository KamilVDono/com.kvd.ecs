using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KVD.Utils.Extensions;

#nullable enable

namespace KVD.ECS.Core
{
	[Serializable]
	public struct ComponentsStorageKey : IComponentsStorageKeyProvider, IEquatable<ComponentsStorageKey>
	{
		#region All values
#if DEBUG
		private static ComponentsStorageKey[]? _allValues;
		private static string[]? _allNames;

		public static ComponentsStorageKey[] AllValues
		{
			get
			{
				if (_allValues == null)
				{
					CollectValues();
				}
				return _allValues!;
			}
		}

		public static string[] AllNames
		{
			get
			{
				if (_allNames == null)
				{
					CollectValues();
				}
				return _allNames!;
			}
		}

		public static string Name(ComponentsStorageKey key)
		{
			var valueIndex = Array.IndexOf(AllValues, key);
			return AllNames[valueIndex];
		}

		private static void CollectValues()
		{
			var fields       = typeof(IComponentsStorageKeyProvider).SubClasses().SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static));
			var fieldsValues = fields.Select(f => ((ComponentsStorageKey)f.GetValue(null), f)).Where(p => p.Item1 != default).ToList();
			_allValues = fieldsValues.Select(fv => fv.Item1).ToArray();
			_allNames  = fieldsValues.Select(fv => fv.f.Name).ToArray();
		}
#endif
		#endregion All values
		
		public static readonly ComponentsStorageKey Default = new(nameof(Default));
		
		private readonly int _hash;

		public int Value => _hash;

		public ComponentsStorageKey(string name)
		{
			_hash = name.GetHashCode();
		}
		
		public ComponentsStorageKey(int hash)
		{
			_hash = hash;
		}

		#region Equality
		public bool Equals(ComponentsStorageKey other)
		{
			return _hash == other._hash;
		}
		public override bool Equals(object? obj)
		{
			return obj is ComponentsStorageKey other && Equals(other);
		}
		public override int GetHashCode()
		{
			return _hash;
		}
		public static bool operator ==(ComponentsStorageKey? left, ComponentsStorageKey? right)
		{
			return Equals(left, right);
		}
		public static bool operator !=(ComponentsStorageKey? left, ComponentsStorageKey? right)
		{
			return !Equals(left, right);
		}

		private sealed class HashEqualityComparer : IEqualityComparer<ComponentsStorageKey>
		{
			public bool Equals(ComponentsStorageKey x, ComponentsStorageKey y)
			{
				return x._hash == y._hash;
			}
			public int GetHashCode(ComponentsStorageKey obj)
			{
				return obj._hash;
			}
		}

		public static IEqualityComparer<ComponentsStorageKey> ComponentStorageKeyComparer{ get; } = new HashEqualityComparer();
		#endregion Equality
	}

	public interface IComponentsStorageKeyProvider
	{
	}
}