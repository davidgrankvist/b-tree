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

		[TestMethod]
		public void TestCreateTreeWithOrder()
		{
			var order = 5;
			var btree = new BTree(order);

			Assert.AreEqual(order, btree.Order);
		}

		[TestMethod]
		public void TestValuesInserted()
		{
			var entries = GetInsertionTestData();
			var btree = CreateTree(entries);

			foreach (var (Key, Value) in entries)
			{
				var retrieved = btree.Find(Key);

				Assert.IsNotNull(retrieved);
				Assert.AreEqual(Value, retrieved);
			}
		}

		public static IEnumerable<(int Key, int Value)> GetInsertionTestData(int size = 10)
		{
			for (int i = 0; i < size; i++)
			{
				yield return (i, i + 10);
			}
		}

		public static BTree CreateTree(IEnumerable<(int Key, int Value)> entries, int order = BTree.DEFAULT_ORDER)
		{
			var btree = new BTree(order);
			foreach (var (Key, Value) in entries)
			{
				btree.Insert(Key, Value);
			}

			return btree;
		}

		[TestMethod]
		public void TestMissingKeyNotFound()
		{
			var btree = new BTree();
			var retrieved = btree.Find(0);

			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void TestInsertedElementsWereDeleted()
		{
			var entries = GetInsertionTestData();
			var btree = CreateTree(entries);

			foreach (var (Key, _) in entries)
			{
				btree.Delete(Key);

				var retrieved = btree.Find(Key);
				Assert.IsNull(retrieved);
			}
		}

		[TestMethod]
		public void TestOnlyDeletedEntryWasDeleted()
		{
			var entries = GetInsertionTestData().ToList();
			var btree = CreateTree(entries);

			var first = entries.First();
			btree.Delete(first.Key);

			var numEntries = TraverseEntries(btree).Count();
			Assert.AreEqual(entries.Count - 1, numEntries);
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
		[TestMethod]
		public void TestNumberOfChildrenDoesNotExceedOrder()
		{
			var order = 5;
			var entries = GetInsertionTestData();
			var btree = CreateTree(entries, order);

			var nodes = btree.Traverse();
			foreach (var node in nodes)
			{
				Assert.IsTrue(node.Count <= btree.Order);
			}
		}

		[TestMethod]
		public void TestTraversedNodesContainAllEntries()
		{
			var order = 5;
			var entries = GetInsertionTestData();
			var btree = CreateTree(entries, order);

			var expectedVisitCount = GetVisitCount(entries);

			var addedEntries = TraverseEntries(btree);
			var actualVisitCount = GetVisitCount(addedEntries);

			foreach (var countEntry in expectedVisitCount)
			{
				Assert.IsTrue(actualVisitCount.ContainsKey(countEntry.Key));
				Assert.AreEqual(countEntry.Value, actualVisitCount[countEntry.Key]);
			}
		}

		private static Dictionary<(int, int), int> GetVisitCount(IEnumerable<(int, int)> entries)
		{
			var visitCount = new Dictionary<(int, int), int>();
			foreach (var entry in entries)
			{
				if (!visitCount.ContainsKey(entry))
				{
					visitCount.Add((entry), 0);
				}
				visitCount[entry]++;
			}

			return visitCount;
		}

		private static IEnumerable<(int, int)> TraverseEntries(BTree btree)
		{
			return btree.Traverse().SelectMany(node => node.Entries);
		}

		// Knuth's definition part 3 - The root node has at least two children unless it is a leaf
		//
		// case: leaf
		[TestMethod]
		public void TestAllEntriesInRootWhenFewerThanOrder()
		{
			var order = 5;
			var btree = CreateTree(GetInsertionTestData(order - 1), order);

			var root = btree.GetRoot();
			var isLeaf = root.Count == 0;

			Assert.IsTrue(isLeaf);
		}

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