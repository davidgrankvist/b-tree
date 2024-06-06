using BTrees.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultOrders), typeof(TestDataHelpers), DynamicDataSourceType.Method)]

		public void TestCreateTreeWithOrder(int order)
		{
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

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestNoNodeIsEmpty(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);

			foreach (var node in btree.Traverse())
			{
				Assert.IsTrue(node.Entries.Count() > 0);
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestNoEntryIsDuplicated(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);

			var visitCount = TestDataHelpers.GetVisitCount(TestDataHelpers.TraverseEntries(btree));
			foreach (var countEntry in visitCount)
			{
				Assert.AreEqual(1, countEntry.Value, $"Found duplicate entry {countEntry.Key}");
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestNodeChildrenAreSortedAsc(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);
			var nonLeafNodes = btree.Traverse().Where(x => x.Children.Any());

			foreach (var node in nonLeafNodes)
			{
				var childrenFirstEntries = node.Children.Select(x => x.Entries.First());
				Assert.IsTrue(childrenFirstEntries.IsSortedAsc());
			}
		}

		/*
		 * If the order is m and m values are inserted, then the root
		 * should be split.
		 *
		 * For example, when the order is 3, then inserting 1, 2, 3 should give
		 *
		 *   1
		 *  / \
		 * 2   3
		 *
		 */
		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultOrders), typeof(TestDataHelpers), DynamicDataSourceType.Method)]

		public void TestRootShouldSplitWhenExceedingEntryLimit(int order)
		{
			var entries = TestDataHelpers.GetAscendingTestData(order);
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var root = btree.GetRoot();
			var didSplit = root.Count == 2;

			Assert.IsTrue(didSplit);
		}

		[TestMethod]
		public void TestThrowsIfDuplicateKeyIsInserted()
		{
			var btree = new BTree();
			btree.Insert(0, 0);

			Assert.ThrowsException<InvalidOperationException>(() => btree.Insert(0, 0));
		}

		[TestMethod]
		public void TestIsEmptyWhenNothingIsInserted()
		{
			var btree = new BTree();

			Assert.IsTrue(btree.IsEmpty);
		}

		/*
		 * Make sure that new entries are inserted into the correct leaf.
		 *
		 * Example:
		 *
		 * If we insert 0 it should be the first entry in the left leaf
		 *
		 *    1           1
		 *   / \    ->   / \
		 *  2   3      0,2  3
		 *
		 */
		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestEntriesOnTheSameLevelAreSorted(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);
			var nonLeafNodes = btree.Traverse().Where(x => x.Children.Any());

			foreach (var node in nonLeafNodes)
			{
				var allEntriesInLevel = node.Children.SelectMany(child => child.Entries);
				Assert.IsTrue(allEntriesInLevel.IsSortedAsc());
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSets), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestTreeIsEmptyIfAllEntriesAreRemoved(IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries);

			foreach (var (key, _) in entries)
			{
				btree.Delete(key);
			}

			Assert.IsTrue(btree.IsEmpty);
		}

		/*
		 * Let the order m be an odd number. If we insert m elements we first get a split. If we then delete
		 * one entry, that should undo the split.
		 *
		 * Since m is odd, each child will have exactly the minimum number of keys and we can't rotate.
		 *
		 * Example:
		 *
		 * Tree of order 3. Insert 1,2,3 and remove 1.
		 *
		 *   2
		 *  / \   ->  2,3
		 * 1   3
		 */
		[DataTestMethod]
		[DataRow(3)]
		[DataRow(5)]
		[DataRow(9)]
		public void TestRootShouldMergeAfterDeleteIfOddOrder(int order)
		{
			var entries = TestDataHelpers.GetAscendingTestData(order);
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var firstKey = entries.First().Key;
			btree.Delete(firstKey);

			var rootIsLeaf = btree.GetRoot().Count == 0;

			Assert.IsTrue(rootIsLeaf);
		}
	}
}