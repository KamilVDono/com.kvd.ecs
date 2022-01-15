using KVD.ECS.Systems;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

namespace KVD.ECS
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