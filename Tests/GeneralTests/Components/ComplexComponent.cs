using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace KVD.ECS.GeneralTests.Tests.GeneralTests.Components
{
	public struct ComplexComponent : IComponent
	{
		public Circle circle;
		public float4x4 transformation;
		public NativeArray<Entity> entities;
		public NativeArray<Position> positions;
		public NativeArray<float> floats;
		public NativeArray<float4x4> matrices;
		public NativeArray<Circle> empty;
		
		public void Dispose()
		{
			if (entities.IsCreated)
			{
				entities.Dispose();
			}
			if (positions.IsCreated)
			{
				positions.Dispose();
			}
			if (floats.IsCreated)
			{
				floats.Dispose();
			}
			if (matrices.IsCreated)
			{
				matrices.Dispose();
			}
			if (empty.IsCreated)
			{
				empty.Dispose();
			}
		}
	}
}
