using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace KVD.ECS.Core.Systems
{
	public interface ISystem
	{
		World World{ get; }
		string Name{ get; }
		IReadOnlyList<ISystem> InternalSystems{ get; }

		void Prepare();

		UniTask Init(World world);
		UniTask Restore(World world);
		void DoUpdate();
		UniTask Destroy();
	}
}
