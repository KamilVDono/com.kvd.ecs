using System;
using KVD.ECS.Core.Components;

#nullable enable

namespace KVD.ECS.Core
{
	public sealed class ViewDescriptor
	{
		public IReadonlyComponentList[] HasComponents{ get; }
		public IReadonlyComponentList[] ExcludeComponents{ get; }

		// === Constructors
		private ViewDescriptor(ComponentsStorage storage, Type[] hasComponents, Type[]? excludeComponents = null)
		{
			HasComponents = new IReadonlyComponentList[hasComponents.Length];
			for (var i = 0; i < hasComponents.Length; i++)
			{
				HasComponents[i] = storage.List(hasComponents[i]);
			}
			
			if (excludeComponents == null)
			{
				ExcludeComponents = Array.Empty<IReadonlyComponentList>();
			}
			else
			{
				ExcludeComponents = new IReadonlyComponentList[excludeComponents.Length];
				for (var i = 0; i < excludeComponents.Length; i++)
				{
					ExcludeComponents[i] = storage.List(excludeComponents[i]);
				}
			}
		}

		// === Static constructors
		public static ViewDescriptor New<T1>(ComponentsStorage storage, Type[]? excludeComponents = null)
			where T1 : struct, IComponent
		{
			var hasComponents = new[]
			{
				typeof(T1),
			};

			return new(storage, hasComponents, excludeComponents);
		}
		
		public static ViewDescriptor New<T1, T2>(ComponentsStorage storage, Type[]? excludeComponents = null)
			where T1 : struct, IComponent
			where T2 : struct, IComponent
		{
			var hasComponents = new[]
			{
				typeof(T1), typeof(T2),
			};

			return new(storage, hasComponents, excludeComponents);
		}
		
		public static ViewDescriptor New<T1, T2, T3>(ComponentsStorage storage, Type[]? excludeComponents = null)
			where T1 : struct, IComponent
			where T2 : struct, IComponent
			where T3 : struct, IComponent
		{
			var hasComponents = new[]
			{
				typeof(T1), typeof(T2), typeof(T3),
			};

			return new(storage, hasComponents, excludeComponents);
		}
		
		public static ViewDescriptor New<T1, T2, T3, T4>(ComponentsStorage storage, Type[]? excludeComponents = null)
			where T1 : struct, IComponent
			where T2 : struct, IComponent
			where T3 : struct, IComponent
			where T4 : struct, IComponent
		{
			var hasComponents = new[]
			{
				typeof(T1), typeof(T2), typeof(T3), typeof(T4),
			};

			return new(storage, hasComponents, excludeComponents);
		}
		
		public static ViewDescriptor New<T1, T2, T3, T4, T5>(ComponentsStorage storage, Type[]? excludeComponents = null)
			where T1 : struct, IComponent
			where T2 : struct, IComponent
			where T3 : struct, IComponent
			where T4 : struct, IComponent
			where T5 : struct, IComponent
		{
			var hasComponents = new[]
			{
				typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5),
			};

			return new(storage, hasComponents, excludeComponents);
		}
		
		public static ViewDescriptor New<T1, T2, T3, T4, T5, T6>(ComponentsStorage storage, Type[]? excludeComponents = null)
			where T1 : struct, IComponent
			where T2 : struct, IComponent
			where T3 : struct, IComponent
			where T4 : struct, IComponent
			where T5 : struct, IComponent
			where T6 : struct, IComponent
		{
			var hasComponents = new[]
			{
				typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6),
			};

			return new(storage, hasComponents, excludeComponents);
		}
	}
}
