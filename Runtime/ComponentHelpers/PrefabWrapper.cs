using KVD.ECS.Core.Components;

namespace KVD.ECS.ComponentHelpers
{
	public struct PrefabWrapper : IComponent
	{
		public string prefabKey;
		
		public void Dispose()
		{
		}
	}
}
