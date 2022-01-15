using Cysharp.Threading.Tasks;

namespace KVD.ECS.Systems
{
	public abstract class IterationsSystemBase<TWorld> : ComponentsSystemBase<TWorld> where TWorld : World
	{
		protected override UniTask InitialSetup()
		{
			InitReferences();
			return base.InitialSetup();
		}

		protected abstract void InitReferences();
	}
}
