using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using Unity.Profiling.LowLevel;

namespace KVD.ECS.Systems
{
	public abstract class SystemBase : ISystem
	{
		private ProfilerMarker _updateMarker;
		private readonly List<IComponentsView> _componentsViews = new(1);

#nullable disable
		public World World{ get; private set; }
		public string Name{ get; }
#nullable enable

		public IReadOnlyList<ISystem> InternalSystems => Array.Empty<ISystem>();
		
		protected SystemBase()
		{
			Name          = GetType().Name;
			_updateMarker = new(ProfilerCategory.Scripts, $"Update {Name}", MarkerFlags.Script);
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
			_updateMarker.Begin();
			Update();
			_updateMarker.End();
		}

		public async UniTask Destroy()
		{
			for (var i = 0; i < _componentsViews.Count; i++)
			{
				var componentsView = _componentsViews[i];
				componentsView.Dispose();
			}
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

		private void CoreSetup(World world)
		{
			World = world;
		}

		protected ComponentsView RegisterComponentsView(ViewDescriptor descriptor)
		{
			var view = new ComponentsView(descriptor);
			_componentsViews.Add(view);
			return view;
		}
		
		protected IComponentsView RegisterComponentsView(IComponentsView view)
		{
			_componentsViews.Add(view);
			return view;
		}
	}
}
