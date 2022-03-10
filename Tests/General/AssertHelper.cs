using NUnit.Framework;
using Unity.Collections;
using static KVD.Utils.Extensions.NativeContainersExt;

namespace KVD.ECS.GeneralTests.Tests.GeneralTests
{
	public static class AssertHelper
	{
		public static void AreEqual<T>(NativeArray<T> expected, NativeArray<T> actual) where T : struct
		{
			var expectedAllocator = ExtractAllocator(expected); 
			var actualAllocator = ExtractAllocator(actual);
			
			if (expectedAllocator != actualAllocator)
			{
				throw new AssertionException($"Expected allocator: {expectedAllocator} but actual: {actualAllocator}");
			}
			
			if (expected.Length != actual.Length)
			{
				throw new AssertionException($"Expected length: {expected.Length} but actual: {actual.Length}");
			}

			for (var i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}
		
		public static void AreEqual<T>(T[] expected, T[] actual)
		{
			if (expected.Length != actual.Length)
			{
				throw new AssertionException($"Expected length: {expected.Length} but actual: {actual.Length}");
			}

			for (var i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}
	}
}
