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
using Debug = UnityEngine.Debug;

// Unity serialization and nullables arent working together
#nullable disable

namespace KVD.ECS.Core.Systems
{
	[Serializable]
	public class SystemsGroup : ISystem
	{
		[SerializeField] private string _name;
		[SerializeReference, SubclassSelector,]
		private ISystem[] _children = Array.Empty<ISystem>();
		
#if SYSTEM_PROFILER_MARKERS
		private ProfilerMarker _updateMarker;
#endif
		private bool _executing;

		public World World{ get; private set; }
		public string Name => _name;

		public IReadOnlyList<ISystem> InternalSystems => _children;
		
		public SystemsGroup()
		{
			_name = nameof(SystemsGroup);
		}

		public SystemsGroup(string name, params ISystem[] systems)
		{
			_name = name;
			_children = systems;
		}

		public void Prepare()
		{
#if SYSTEM_PROFILER_MARKERS
			_updateMarker = new(ProfilerCategory.Scripts, $"Update {Name}", MarkerFlags.Script);
#endif
			var length = _children.Length;
			for (var i = 0; i < length; i++)
			{
				var child = _children[i];

				if (child == null)
				{
					Debug.LogError($"System {i} in group {Name} is null.");
					Array.Copy(_children, i+1, _children, i, length-i-1);
					--i;
					--length;
					continue;
				}

				child.Prepare();
			}

			if (length != _children.Length)
			{
				Array.Resize(ref _children, length);
			}
		}

		public UniTask Init(World world)
		{
			World = world;
			
			var initTasks = new UniTask[_children.Length];
			for (var i = 0; i < _children.Length; i++)
			{
				initTasks[i] = _children[i].Init(World);
			}
			return UniTask.WhenAll(initTasks);
		}
		
		public UniTask Restore(World world)
		{
			World = world;
			
			var restoreTasks = new UniTask[_children.Length];
			for (var i = 0; i < _children.Length; i++)
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
			for (var i = 0; i < _children.Length; i++)
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
			var destroyTasks = new UniTask[_children.Length];
			for (var i = 0; i < _children.Length; i++)
			{
				destroyTasks[i] = _children[i].Destroy();
			}
			_children = Array.Empty<ISystem>();
			return UniTask.WhenAll(destroyTasks);
		}

		public UniTask Add(ISystem system)
		{
			CheckIfCanAdd();
			Array.Resize(ref _children, _children.Length+1);
			_children[^1] = system;
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
