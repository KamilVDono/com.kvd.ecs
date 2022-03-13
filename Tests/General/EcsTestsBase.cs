using System;
using System.Threading.Tasks;
using KVD.ECS.Core;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests
{
	[TestFixture]
	public abstract class EcsTestsBase
	{
#nullable disable
		protected World world;
#nullable enable

		[SetUp]
		public void SetUp()
		{
			CreateWorld().Wait();
			OnSetup().Wait();
		}

		private async Task CreateWorld()
		{
			world = new World(Array.Empty<IBootstrapable>());
			await world.Initialize();
		}
		
		protected virtual Task OnSetup()
		{
			return Task.CompletedTask;
		}

		[TearDown]
		public void TearDown()
		{
			OnTearDown().Wait();
			world.Destroy();
		}
		protected virtual Task OnTearDown()
		{
			return Task.CompletedTask;
		}
	}
}
