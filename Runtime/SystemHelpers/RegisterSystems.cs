using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using UnityEngine;

namespace KVD.ECS.SystemHelpers
{
	public class RegisterSystems : MonoBehaviour, IBootstrapable
	{
		[SerializeReference, SubclassSelector,]
		private ISystem[] _systemReferences;
		
		public UniTask Init(World world)
		{
			return Register(world);
		}
		
		public UniTask Restore(World world)
		{
			return Register(world);
		}
		
		private UniTask Register(World world)
		{
			var group = new SystemsGroup(name, _systemReferences);
			return world.RegisterSystem(group);
		}
	}
}
