using KVD.ECS.Core.Systems;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

namespace KVD.ECS.Core.Helpers
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public class OneFrameComponentsSystem : SystemBase
	{
		protected override void Update()
		{
			var allStorages = World.AllComponentsStorages;
			for (var i = 0; i < allStorages.Count; i++)
			{
				allStorages[i].ClearSingleFrameEntities();
			}
		}
	}
}