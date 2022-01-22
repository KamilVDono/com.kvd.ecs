using Cysharp.Threading.Tasks;

namespace KVD.ECS.Core
{
	public interface IBootstrapable
	{
		public UniTask Init(World world);
		public UniTask Restore(World world);
	}
}
