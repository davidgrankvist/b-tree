using BTrees.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace BTrees.Test
{
	[TestClass]
	public class BTreeTest
	{
		[TestMethod]
		public void TestCreateTreeWithDefaultOrder()
		{
			var btree = new BTree();

			Assert.AreEqual(BTree.DEFAULT_ORDER, btree.Order);
		}

		[TestMethod]
		public void TestCreateTreeWithOrder()
		{
			var order = 5;
			var btree = new BTree(order);

			Assert.AreEqual(order, btree.Order);
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestValuesInserted(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);

			foreach (var (Key, Value) in entries)
			{
				var retrieved = btree.Find(Key);

				Assert.IsNotNull(retrieved);
				Assert.AreEqual(Value, retrieved);
			}
		}

		[TestMethod]
		public void TestMissingKeyNotFound()
		{
			var btree = new BTree();
			var retrieved = btree.Find(0);

			Assert.IsNull(retrieved);
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestInsertedElementsWereDeleted(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = new BTree();

			foreach (var (Key, Value) in entries)
			{
				btree.Insert(Key, Value);

				btree.Delete(Key);

				var retrieved = btree.Find(Key);
				Assert.IsNull(retrieved);
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestOnlyDeletedEntryWasDeleted(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);

			var first = entries.First();
			btree.Delete(first.Key);

			var numEntries = TestDataHelpers.TraverseEntries(btree).Count();
			Assert.AreEqual(entries.Count() - 1, numEntries);
		}

		[TestMethod]
		public void TestGetRootNodeThrowsIfEmpty()
		{
			var btree = new BTree();
			Assert.ThrowsException<InvalidOperationException>(() => btree.GetRoot());
		}

		[TestMethod]
		public void TestGetRootNodeHasInsertedEntry()
		{
			var btree = new BTree();
			var key = 0;
			var val = 1;
			btree.Insert(key, val);

			var root = btree.GetRoot();
			var received = root.Find(key);

			Assert.AreEqual(val, received);
		}

		// Knuth's definition part 1 - Every node has at most m children
		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestNumberOfChildrenDoesNotExceedOrder(IEnumerable<(int Key, int Value)> entries)
		{
			var order = 5;
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var nodes = btree.Traverse();
			foreach (var node in nodes)
			{
				Assert.IsTrue(node.Count <= btree.Order);
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestTraversedNodesContainAllEntries(IEnumerable<(int Key, int Value)> entries)
		{
			var order = 5;
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var expectedVisitCount = TestDataHelpers.GetVisitCount(entries);

			var addedEntries = TestDataHelpers.TraverseEntries(btree);
			var actualVisitCount = TestDataHelpers.GetVisitCount(addedEntries);

			foreach (var countEntry in expectedVisitCount)
			{
				Assert.IsTrue(actualVisitCount.ContainsKey(countEntry.Key));
				Assert.AreEqual(countEntry.Value, actualVisitCount[countEntry.Key]);
			}
		}

		// Knuth's definition part 3 - The root node has at least two children unless it is a leaf
		//
		// case: leaf
		[DataTestMethod]
		[DynamicData(nameof(GetData_TestAllEntriesInRootWhenFewerThanOrder), DynamicDataSourceType.Method)]
		public void TestAllEntriesInRootWhenFewerThanOrder(int order, IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var root = btree.GetRoot();
			var isLeaf = root.Count == 0;

			Assert.IsTrue(isLeaf);
		}

		public static IEnumerable<object[]> GetData_TestAllEntriesInRootWhenFewerThanOrder()
		{
			var order = 5;
			var size = order - 1;

			var dataSets = TestDataHelpers.GetTestDataSets(size);
			return dataSets.Select(ds => new object[] { order, ds[0] });
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestNodeEntriesAreSortedAsc(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);

			foreach (var node in btree.Traverse())
			{
				Assert.IsTrue(node.Entries.IsSortedAsc());
			}
		}

		// Knuth's definition part 3 - The root node has at least two children unless it is a leaf
		//
		// case: not a leaf
		//[DataTestMethod]
		//[DynamicData(nameof(GetData_TestRootHasAtLeastTwoChildrenWhenNumberOfEntriesExceedsOrder), DynamicDataSourceType.Method)]
		//public void TestRootHasAtLeastTwoChildrenWhenNumberOfEntriesExceedsOrder(int order, IEnumerable<(int Key, int Value)> entries)
		//{
		//	var btree = TestDataHelpers.CreateTreeWithData(entries, order);

		//	var root = btree.GetRoot();

		//	Assert.IsTrue(root.Count >= 2);
		//}

		//public static IEnumerable<object[]> GetData_TestRootHasAtLeastTwoChildrenWhenNumberOfEntriesExceedsOrder()
		//{
		//	var order = 5;
		//	var size = order + 1;

		//	var dataSets = TestDataHelpers.GetTestDataSets(size);
		//	return dataSets.Select(ds => new object[] { order, ds[0] });
		//}

		// Knuth's definition part 2 - Every node, except for the root and the leaves, has at least ceil(m/2) children
		//[TestMethod]
		//public void TestInnerNodesDegreesSatisfyOrder()
		//{
		//	var order = 5;
		//	var entries = GetInsertionTestData();
		//	var btree = CreateTree(entries, order);

		//	var childrenLowerBound = order / 2 + (order % 2);

		//	var nodes = btree.Traverse().Skip(1);
		//	foreach(var node in nodes)
		//	{
		//		var isLeaf = node.Count == 0;
		//		if (isLeaf)
		//		{
		//			continue;
		//		}
		//		Assert.IsTrue(node.Count >= childrenLowerBound);
		//	}
		//}
	}
}