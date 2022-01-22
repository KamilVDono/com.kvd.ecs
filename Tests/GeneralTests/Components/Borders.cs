using KVD.ECS.Core.Components;
using Unity.Mathematics;

namespace KVD.ECS.GeneralTests.Tests.GeneralTests.Components
{
	public struct Borders : IComponent
	{
		public float2 xs;
		public float2 ys;
		
		public void Dispose()
		{
		}
	}
}
