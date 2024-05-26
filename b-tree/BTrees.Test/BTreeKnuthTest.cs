using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BTrees.Test
{
	/// <summary>
	/// These tests make use of Knuth's definition of a B-tree to make sure that the test data
	/// satisfies the right properties.
	///
	/// <br /><br />
	/// See https://en.wikipedia.org/wiki/B-tree#Definition
	/// </summary>
	[TestClass]
	public class BTreeKnuthTest
	{
		// Knuth's definition part 1 - Every node has at most m children
		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSetsWithOrders), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestNumberOfChildrenDoesNotExceedOrder(int order, IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var nodes = btree.Traverse();
			foreach (var node in nodes)
			{
				Assert.IsTrue(node.Count <= btree.Order);
			}
		}

		// Knuth's definition part 2 - Every node, except for the root and the leaves, has at least ceil(m/2) children
		[DataTestMethod]
		[DynamicData(nameof(TestDataHelpers.GetDefaultTestDataSetsWithOrders), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
		public void TestInnerNodesDegreesSatisfyOrder(int order, IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var childrenLowerBound = order / 2 + (order % 2);

			var nodes = btree.Traverse().Skip(1);
			foreach (var node in nodes)
			{
				var isLeaf = node.Count == 0;
				if (isLeaf)
				{
					continue;
				}
				Assert.IsTrue(node.Count >= childrenLowerBound);
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
			var orders = TestDataHelpers.DEFAULT_ORDERS;
			foreach (var order in orders)
			{
				var size = order - 1;
				var dataSets = TestDataHelpers.GetTestDataSets(size);
				foreach (var dataSet in dataSets)
				{
					yield return new object[] { order, dataSet[0] };
				}
			}
		}

		// Knuth's definition part 3 - The root node has at least two children unless it is a leaf
		//
		// case: not a leaf
		[DataTestMethod]
		[DynamicData(nameof(GetData_TestRootHasAtLeastTwoChildrenWhenNumberOfEntriesExceedsOrder), DynamicDataSourceType.Method)]
		public void TestRootHasAtLeastTwoChildrenWhenNumberOfEntriesExceedsOrder(int order, IEnumerable<(int Key, int Value)> entries)
		{
			var btree = TestDataHelpers.CreateTreeWithData(entries, order);

			var root = btree.GetRoot();

			Assert.IsTrue(root.Count >= 2);
		}

		public static IEnumerable<object[]> GetData_TestRootHasAtLeastTwoChildrenWhenNumberOfEntriesExceedsOrder()
		{
			var orders = TestDataHelpers.DEFAULT_ORDERS;
			foreach (var order in orders)
			{
				var size = order + 1;
				var dataSets = TestDataHelpers.GetTestDataSets(size);
				foreach (var dataSet in dataSets)
				{
					yield return new object[] { order, dataSet[0] };
				}
			}
		}

		// Knuth's definition part 4 - All leaves appear on the same level

		// Knuth's definition part 5 - A non-leaf node with k children contains k - 1 keys
	}
}
