using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;

#nullable enable

namespace KVD.ECS.Core.Entities
{
	public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>
	{
		public static readonly Entity Null = new(-1);
		
		public readonly int index;
		
		public Entity Next => new(index+1);

		public Entity(int index)
		{
			this.index = index;
		}

		public static implicit operator int(Entity e) => e.index;
		public static implicit operator Entity(int index) => new(index);

		public override string ToString()
		{
			return $"Entity {index}";
		}

		public void ToString(StringBuilder stringBuilder, ComponentsStorage storage)
		{
			stringBuilder.Append("Entity ");
			var name = new FixedString128Bytes(index.ToString());
			#if ENTITIES_NAMES
			storage.Name(this, ref name);
			#endif
			stringBuilder.Append(name);
			foreach (var sparseList in storage.AllLists)
			{
				if (!sparseList.IsCreated)
				{
					continue;
				}
				var list = sparseList.ToList();
				if (!list.Has(this))
				{
					continue;
				}
				// TODO: Do it somehow
				// if (list.Value(this) is not IDisplayable displayable)
				// {
				// 	continue;
				// }
				stringBuilder.Append(" ;-; ");
				stringBuilder.Append(list.typeInfo.typeHandle.Type.Name);
			}
		}

		#region Equality and comparasion
		public bool Equals(Entity other)
		{
			return index == other.index;
		}
		public override bool Equals(object? obj)
		{
			return obj is Entity other && Equals(other);
		}
		public override int GetHashCode()
		{
			return index;
		}
		public static bool operator ==(Entity left, Entity right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(Entity left, Entity right)
		{
			return !left.Equals(right);
		}
		public static bool operator >(Entity left, Entity right)
		{
			return left.index > right.index;
		}
		public static bool operator <(Entity left, Entity right)
		{
			return left.index < right.index;
		}
		public static bool operator >=(Entity left, Entity right)
		{
			return left.index >= right.index;
		}
		public static bool operator <=(Entity left, Entity right)
		{
			return left.index <= right.index;
		}

		public int CompareTo(Entity other)
		{
			return index.CompareTo(other.index);
		}

		public sealed class IndexEqualityComparer : IEqualityComparer<Entity>, IComparer<Entity>
		{
			public bool Equals(Entity x, Entity y)
			{
				return x.index == y.index;
			}
			public int GetHashCode(Entity obj)
			{
				return obj.index;
			}
			
			public int Compare(Entity x, Entity y)
			{
				return x.index.CompareTo(y.index);
			}
		}

		public static IndexEqualityComparer IndexComparer{ get; } = new();
		#endregion Equality and comparasion
	}
}
