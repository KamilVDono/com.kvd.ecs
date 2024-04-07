using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
#if SYSTEM_PROFILER_MARKERS
using Unity.Profiling;
using Unity.Profiling.LowLevel;
#endif

namespace KVD.ECS.Core.Systems
{
	public abstract class SystemBase : ISystem
	{
#if SYSTEM_PROFILER_MARKERS
		ProfilerMarker _updateMarker;
#endif

#nullable disable
		public World World{ get; private set; }
		public string Name{ get; }
#nullable enable

		public IReadOnlyList<ISystem> InternalSystems => Array.Empty<ISystem>();
		
		protected SystemBase()
		{
			Name          = GetType().Name;
#if SYSTEM_PROFILER_MARKERS
			_updateMarker = new(ProfilerCategory.Scripts, $"Update {Name}", MarkerFlags.Script);
#endif
		}

		public void Prepare()
		{
		}

		public async UniTask Init(World world)
		{
			CoreSetup(world);
			await InitialSetup();
		}

		public async UniTask Restore(World world)
		{
			CoreSetup(world);
			await RestoreSetup();
		}

		public void DoUpdate()
		{
#if SYSTEM_PROFILER_MARKERS
			_updateMarker.Begin();
#endif
			Update();
#if SYSTEM_PROFILER_MARKERS
			_updateMarker.End();
#endif
		}

		public async UniTask Destroy()
		{
			await TearDown();
		}
		
		protected abstract void Update();

		protected virtual UniTask InitialSetup()
		{
			return UniTask.CompletedTask;
		}
		
		protected virtual async UniTask RestoreSetup()
		{
			await InitialSetup();
		}
		
		protected virtual UniTask TearDown()
		{
			return UniTask.CompletedTask;
		}

		void CoreSetup(World world)
		{
			World = world;
		}
	}
	
	public abstract class SystemBase<TWorld> : SystemBase where TWorld : World
	{
		public new TWorld World => (TWorld)base.World;
	}
}
