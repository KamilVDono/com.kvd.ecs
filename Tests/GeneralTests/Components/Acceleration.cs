using KVD.ECS.Components;

namespace KVD.ECS.Tests.Components
{
	public struct Acceleration : IComponent
	{
		public float x;
		public float y;
		public float z;
		
		public void Dispose()
		{
		}
	}
}
