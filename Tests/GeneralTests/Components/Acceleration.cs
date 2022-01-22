﻿using KVD.ECS.Core.Components;

namespace KVD.ECS.GeneralTests.Tests.GeneralTests.Components
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
