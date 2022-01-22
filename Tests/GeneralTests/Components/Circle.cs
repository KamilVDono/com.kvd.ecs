using KVD.ECS.Core.Components;

namespace KVD.ECS.GeneralTests.Tests.GeneralTests.Components
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
