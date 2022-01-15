using KVD.ECS.Components;
using Unity.Mathematics;

namespace KVD.ECS.Tests.Components
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
