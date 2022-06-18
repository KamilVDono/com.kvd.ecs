using Cysharp.Threading.Tasks;

namespace KVD.ECS.Core.Systems
{
	public abstract class IterationsSystemBase<TWorld> : SystemBase<TWorld> where TWorld : World
	{
		protected override UniTask InitialSetup()
		{
			InitReferences();
			return base.InitialSetup();
		}

		protected abstract void InitReferences();
	}
}
