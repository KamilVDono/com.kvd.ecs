using Cysharp.Threading.Tasks;

namespace KVD.ECS
{
	public interface IBootstrapable
	{
		public UniTask Init(World world);
		public UniTask Restore(World world);
	}
}
