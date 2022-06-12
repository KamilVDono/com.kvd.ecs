using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
#if SYSTEM_PROFILER_MARKERS
using Unity.Profiling;
using Unity.Profiling.LowLevel;
#endif
using UnityEngine;

namespace KVD.ECS.Core.Systems
{
	[Serializable]
	public class SystemsGroup : ISystem
	{
		[SerializeField] private string _name;
		[SerializeReference, SubclassSelector,]
		private List<ISystem> _children = new();
		
#if SYSTEM_PROFILER_MARKERS
		private ProfilerMarker _updateMarker;
#endif
		private bool _executing;

#nullable disable
		public World World{ get; private set; }
		public string Name => _name;
#nullable enable

		public IReadOnlyList<ISystem> InternalSystems => _children;
		
		public SystemsGroup()
		{
			_name = nameof(SystemsGroup);
		}

		public SystemsGroup(string name, params ISystem[] systems)
		{
			_name = name;
			_children.AddRange(systems);
		}

		public void Prepare()
		{
#if SYSTEM_PROFILER_MARKERS
			_updateMarker = new(ProfilerCategory.Scripts, $"Update {Name}", MarkerFlags.Script);
#endif
			for (var i = 0; i < _children.Count; i++)
			{
				var child = _children[i];
				child.Prepare();
			}
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
#if SYSTEM_PROFILER_MARKERS
			_updateMarker.Begin();
#endif
			_executing = true;
			for (var i = 0; i < _children.Count; i++)
			{
				var child = _children[i];
				child.DoUpdate();
			}
			_executing = false;
#if SYSTEM_PROFILER_MARKERS
			_updateMarker.End();
#endif
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
