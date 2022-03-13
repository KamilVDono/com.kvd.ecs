using System.Collections.Generic;
using System.IO;
using KVD.ECS.ComponentHelpers;
using KVD.ECS.Core.Helpers;
using NUnit.Framework;

namespace KVD.ECS.GeneralTests
{
	public class BigBitmaskTests
	{
		// Set
		[Test]
		public void Set_FromZero_ToTrue()
		{
			// Arrange
			var mask = new BigBitmask();
			
			// Act
			mask.Set(0, true);
			mask.Set(10, true);
			mask.Set(33, true);
			
			// Assert
			Assert.IsTrue(mask.Has(0));
			Assert.IsTrue(mask.Has(10));
			Assert.IsTrue(mask.Has(33));
			Assert.IsFalse(mask.Has(330));
			Assert.IsFalse(mask.Has(1));
			Assert.IsFalse(mask.Has(8));
		}
		
		[Test]
		public void Set_MultipleChanges()
		{
			// Arrange
			var mask = new BigBitmask();
			
			// Act
			mask.Set(0, true);
			mask.Set(10, true);
			mask.Set(33, true);
			mask.Set(10, false);
			mask.Set(1, false);
			mask.Set(1, true);
			mask.Set(2, false);
			mask.Set(1, false);

			// Assert
			Assert.IsTrue(mask.Has(0));
			Assert.IsTrue(mask.Has(33));
			Assert.IsFalse(mask.Has(10));
			Assert.IsFalse(mask.Has(330));
			Assert.IsFalse(mask.Has(1));
			Assert.IsFalse(mask.Has(2));
			Assert.IsFalse(mask.Has(8));
		}
		
		[Test]
		public void Zero_Empty()
		{
			// Arrange
			var mask = new BigBitmask();
			
			// Act
			mask.Zero();

			// Assert
			Assert.IsFalse(mask.Has(0));
			Assert.IsFalse(mask.Has(10));
			Assert.IsFalse(mask.Has(33));
			Assert.IsFalse(mask.Has(330));
		}
		
		[Test]
		public void Zero_NotEmpty()
		{
			// Arrange
			var mask = new BigBitmask();
			
			// Act
			mask.Set(0, true);
			mask.Set(10, true);
			mask.Set(33, true);
			mask.Zero();

			// Assert
			Assert.IsFalse(mask.Has(0));
			Assert.IsFalse(mask.Has(10));
			Assert.IsFalse(mask.Has(33));
			Assert.IsFalse(mask.Has(330));
		}
		
		[Test]
		public void All_Empty()
		{
			// Arrange
			var mask = new BigBitmask();
			
			// Act
			mask.All();

			// Assert
			Assert.IsTrue(mask.Has(0));
			Assert.IsTrue(mask.Has(10));
			Assert.IsTrue(mask.Has(33));
			Assert.IsFalse(mask.Has(330));
		}
		
		[Test]
		public void All_Resized()
		{
			// Arrange
			var mask = new BigBitmask();
			mask.EnsureCapacity(330);
			
			// Act
			mask.All();

			// Assert
			Assert.IsTrue(mask.Has(0));
			Assert.IsTrue(mask.Has(10));
			Assert.IsTrue(mask.Has(33));
			Assert.IsTrue(mask.Has(330));
		}
		
		[Test]
		public void Count_Zero()
		{
			// Arrange
			var mask = new BigBitmask();
			mask.EnsureCapacity(330);
			// Act&Assert
			Assert.Zero(mask.Count());
		}
		
		[Test]
		public void Count_SomeData()
		{
			// Arrange
			var mask = new BigBitmask();
			mask.Set(0, true);
			mask.Set(0, false);
			mask.Set(2, true);
			mask.Set(33, true);
			mask.Set(330, true);
			// Act&Assert
			Assert.AreEqual(3, mask.Count());
		}
		
		[Test]
		public void CopyFrom_EmptyToEmpty()
		{
			// Arrange
			var maskFrom = new BigBitmask();
			var maskTo = new BigBitmask();
			// Act
			maskTo.CopyFrom(maskFrom);
			// Assert
			Assert.AreEqual(maskFrom.Count(), maskTo.Count());
		}
		
		[Test]
		public void CopyFrom_FullBigToEmpty()
		{
			// Arrange
			var maskFrom = new BigBitmask();
			var maskTo   = new BigBitmask();
			maskFrom.EnsureCapacity(512);
			maskFrom.All();
			// Act
			maskTo.CopyFrom(maskFrom);
			// Assert
			Assert.AreEqual(maskFrom.Count(), maskTo.Count());
		}
		
		[Test]
		public void CopyFrom_EmptyToFullBig()
		{
			// Arrange
			var maskFrom = new BigBitmask();
			var maskTo   = new BigBitmask();
			maskTo.EnsureCapacity(512);
			maskTo.All();
			// Act
			maskTo.CopyFrom(maskFrom);
			// Assert
			Assert.AreEqual(maskFrom.Count(), maskTo.Count());
		}

		[Test, TestCase(true), TestCase(false),]
		public void Intersect_SmallerWithBigger(bool valueIfResize)
		{
			// Arrange
			var small = new BigBitmask(32);
			var big   = new BigBitmask(512);
			small.Set(0, true);
			small.Set(12, true);
			big.Set(12, true);
			big.Set(500, true);
			
			// Act
			small.Intersect(big, valueIfResize);
			
			// Assert
			Assert.IsFalse(small.Has(0));
			Assert.IsFalse(small.Has(1));
			Assert.IsTrue(small.Has(12));
			Assert.AreEqual(valueIfResize, small.Has(500));
			Assert.IsFalse(small.Has(510));
		}
		
		[Test, TestCase(true), TestCase(false),]
		public void Intersect_BiggerWithSmaller(bool valueIfResize)
		{
			// Arrange
			var small = new BigBitmask(32);
			var big   = new BigBitmask(512);
			small.Set(0, true);
			small.Set(12, true);
			big.Set(12, true);
			big.Set(500, true);
			
			// Act
			big.Intersect(small, valueIfResize);
			
			// Assert
			Assert.IsFalse(big.Has(0));
			Assert.IsFalse(big.Has(1));
			Assert.IsTrue(big.Has(12));
			Assert.IsFalse(big.Has(500));
			Assert.IsFalse(small.Has(510));
		}

		[Test]
		public void Union_SmallerWithBigger()
		{
			// Arrange
			var small = new BigBitmask(32);
			var big   = new BigBitmask(512);
			small.Set(0, true);
			small.Set(12, true);
			big.Set(12, true);
			big.Set(500, true);
			
			// Act
			small.Union(big);
			
			// Assert
			Assert.IsFalse(small.Has(1));
			Assert.IsTrue(small.Has(0));
			Assert.IsTrue(small.Has(12));
			Assert.IsTrue(small.Has(500));
			Assert.IsFalse(small.Has(510));
		}
		
		[Test]
		public void Union_BiggerWithSmaller()
		{
			// Arrange
			var small = new BigBitmask(32);
			var big   = new BigBitmask(512);
			small.Set(0, true);
			small.Set(12, true);
			big.Set(12, true);
			big.Set(500, true);
			
			// Act
			big.Union(small);
			
			// Assert
			Assert.IsFalse(big.Has(1));
			Assert.IsTrue(big.Has(0));
			Assert.IsTrue(big.Has(12));
			Assert.IsTrue(big.Has(500));
			Assert.IsFalse(big.Has(510));
		}
		
		[Test, TestCase(true), TestCase(false),]
		public void Exclude_SmallerWithBigger(bool valueIfResize)
		{
			// Arrange
			var small = new BigBitmask(32);
			var big   = new BigBitmask(512);
			small.Set(0, true);
			small.Set(12, true);
			big.Set(12, true);
			big.Set(500, true);
			
			// Act
			small.Exclude(big, valueIfResize);
			
			// Assert
			Assert.IsTrue(small.Has(0));
			Assert.IsFalse(small.Has(1));
			Assert.IsFalse(small.Has(12));
			Assert.IsFalse(small.Has(32));
			Assert.IsFalse(small.Has(500));
			Assert.AreEqual(valueIfResize, small.Has(64));
			Assert.AreEqual(valueIfResize, small.Has(128));
			Assert.AreEqual(valueIfResize, small.Has(512));
		}
		
		[Test, TestCase(true), TestCase(false),]
		public void Exclude_BiggerWithSmaller(bool valueIfResize)
		{
			// Arrange
			var small = new BigBitmask(32);
			var big   = new BigBitmask(512);
			small.Set(0, true);
			small.Set(12, true);
			big.Set(12, true);
			big.Set(500, true);
			
			// Act
			big.Exclude(small, valueIfResize);
			
			// Assert
			Assert.IsTrue(big.Has(500));
			Assert.IsFalse(big.Has(0));
			Assert.IsFalse(big.Has(1));
			Assert.IsFalse(big.Has(12));
			Assert.IsFalse(small.Has(510));
		}
		
		[Test]
		public void Iterate_Empty()
		{
			// Arrange
			var mask = new BigBitmask();
			var present = new List<int>(); 
			
			// Act&Assert
			foreach (var value in mask)
			{
				present.Add(value);
			}
			
			// Assert
			Assert.Zero(present.Count);
		}
		
		[Test]
		public void Iterate_NotEmpty()
		{
			// Arrange
			var mask    = new BigBitmask();
			mask.Set(1, true);
			mask.Set(12, true);
			mask.Set(26, true);
			var present = new List<int>(); 
			
			// Act
			foreach (var value in mask)
			{
				present.Add(value);
			}
			
			// Assert
			Assert.AreEqual(3, present.Count);
			Assert.AreEqual(1, present[0]);
			Assert.AreEqual(12, present[1]);
			Assert.AreEqual(26, present[2]);
		}
		
		[Test]
		public void SerializeDeserialize_Empty()
		{
			// Arrange&&Act
			var       mask   = new BigBitmask();
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);
			mask.Serialize(writer);
			stream.Flush();
			stream.Position = 0;
			using var reader       = new BinaryReader(stream);
			var       deserialized = new BigBitmask();
			deserialized.Deserialize(reader);
			
			// Assert
			Assert.Zero(deserialized.Count());
		}
		
		[Test]
		public void SerializeDeserialize_NotEmpty()
		{
			// Arrange
			var mask = new BigBitmask();
			mask.Set(1, true);
			mask.Set(12, true);
			mask.Set(26, true);
			
			// Arrange&Act
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);
			mask.Serialize(writer);
			stream.Flush();
			stream.Position = 0;
			using var reader       = new BinaryReader(stream);
			var       deserialized = new BigBitmask();
			deserialized.Deserialize(reader);
			
			// Assert
			Assert.AreEqual(3, deserialized.Count());
			Assert.IsTrue(deserialized.Has(1));
			Assert.IsTrue(deserialized.Has(12));
			Assert.IsTrue(deserialized.Has(26));
			Assert.IsFalse(deserialized.Has(27));
			Assert.IsFalse(deserialized.Has(0));
			Assert.IsFalse(deserialized.Has(16));
		}
	}
}
