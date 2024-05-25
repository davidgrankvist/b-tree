using BTrees.Lib;

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
	}
}