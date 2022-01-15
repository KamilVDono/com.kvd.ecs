using KVD.ECS.Components;

namespace KVD.ECS.Tests.Components
{
	public struct Circle : IComponent
	{
		public byte id;
		public Radius radius;
		public Position position;
		
		public void Dispose()
		{
		}
	}
}
