using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace KVD.ECS.Systems
{
	public interface ISystem
	{
		World World{ get; }
		string Name{ get; }
		IReadOnlyList<ISystem> InternalSystems{ get; }

		UniTask Init(World world);
		UniTask Restore(World world);
		void DoUpdate();
		UniTask Destroy();
	}
}
