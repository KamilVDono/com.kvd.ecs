using System;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.ECS.UnityBridges;

namespace KVD.ECS.Generics
{
	[Serializable]
	public class WaitForAnimationExitSystem : IterationsSystemBase<World>
	{
#nullable disable
		ComponentsView<WaitForAnimationExit, AnimatorStateExit>[] _waitViews;
#nullable enable
		
		protected override void InitReferences()
		{
			var storages = World.AllComponentsStorages;
			_waitViews = new ComponentsView<WaitForAnimationExit, AnimatorStateExit>[storages.Count];
			for (var i = 0; i < _waitViews.Length; i++)
			{
				_waitViews[i] = new ComponentsViewBuilder(storages[i]).Build<WaitForAnimationExit, AnimatorStateExit>();
			}
		}
		
		protected override void Update()
		{
			for (var i = 0; i < _waitViews.Length; i++)
			{
				foreach (var iter in _waitViews[i])
				{
					var wait  = iter.Get0();
					var state = iter.Get1().stateInfo;
					if (wait != state)
					{
						continue;
					}
			
					var signalEntity = wait.entity;
					var storageKey   = wait.storageKey;
					
					iter.Remove0();
			
					World.Storage(storageKey).List<WaitSignal>().Remove(signalEntity);
				}
			}
		}
	}
}
