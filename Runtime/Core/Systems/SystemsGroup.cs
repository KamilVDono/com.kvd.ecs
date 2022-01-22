using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using Unity.Profiling.LowLevel;

namespace KVD.ECS.Core.Systems
{
	public class SystemsGroup : ISystem
	{
		private ProfilerMarker _updateMarker;
		
		private bool _executing;
		private readonly List<ISystem> _children = new();

#nullable disable
		public World World{ get; private set; }
		public string Name{ get; }
#nullable enable

		public IReadOnlyList<ISystem> InternalSystems => _children;

		public SystemsGroup(string name)
		{
			_updateMarker = new(ProfilerCategory.Scripts, $"Update {name}", MarkerFlags.Script);
			Name          = name;
		}
		
		public SystemsGroup(string name, params ISystem[] systems) : this(name)
		{
			_children.AddRange(systems);
		}

		public UniTask Init(World world)
		{
			World = world;
			
			var initTasks = new UniTask[_children.Count];
			for (var i = 0; i < _children.Count; i++)
			{
				initTasks[i] = _children[i].Init(World);
			}
			return UniTask.WhenAll(initTasks);
		}
		
		public UniTask Restore(World world)
		{
			World = world;
			
			var restoreTasks = new UniTask[_children.Count];
			for (var i = 0; i < _children.Count; i++)
			{
				restoreTasks[i] = _children[i].Restore(World);
			}
			return UniTask.WhenAll(restoreTasks);
		}
		
		public void DoUpdate()
		{
			_updateMarker.Begin();
			_executing = true;
			foreach (var child in _children)
			{
				child.DoUpdate();
			}
			_executing = false;
			_updateMarker.End();
		}
		
		public UniTask Destroy()
		{
			var destroyTasks = new UniTask[_children.Count];
			for (var i = 0; i < _children.Count; i++)
			{
				destroyTasks[i] = _children[i].Destroy();
			}
			_children.Clear();
			return UniTask.WhenAll(destroyTasks);
		}

		public UniTask Add(ISystem system)
		{
			CheckIfCanAdd();
			_children.Add(system);
			return system.Init(World);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("DEBUG"),]
		private void CheckIfCanAdd()
		{
			if (_executing)
			{
				throw new ApplicationException("Cannot add new system to group when group is executing.");
			}
		}
	}
}
