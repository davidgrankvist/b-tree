using BTrees.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTrees.Test
{
	public static class TestDataHelpers
	{
		public const int DEFAULT_SIZE = 10;
		public const int DEFAULT_SEED = 10;
		public static readonly IEnumerable<int> DEFAULT_ORDERS = new int[] { 3, 5, 10 };

		public static IEnumerable<object[]> GetDefaultTestDataSetsWithOrders()
		{
			return GetTestDataSetsWithOrders(DEFAULT_ORDERS);
		}

		public static IEnumerable<object[]> GetTestDataSetsWithOrders(IEnumerable<int> orders, int size = DEFAULT_SIZE, int seed = DEFAULT_SEED)
		{
			var dataSets = GetTestDataSets(size, seed);

			foreach (var order in orders)
			{
				foreach (var dataSet in dataSets)
				{
					yield return new object[] { order, dataSet[0] };
				}
			}
		}

		public static IEnumerable<object[]> GetDefaultTestDataSets()
		{
			return GetTestDataSets();
		}

		public static IEnumerable<object[]> GetTestDataSets(int size = DEFAULT_SIZE, int seed = DEFAULT_SEED)
		{
			var ascending = GetAscendingTestData(size);
			var descending = GetDescendingTestData(size);
			var randomized = GetRandomTestData(size, seed);

			yield return new object[] { ascending };
			yield return new object[] { descending };
			yield return new object[] { randomized };
		}

		public static IEnumerable<(int Key, int Value)> GetAscendingTestData(int size = DEFAULT_SIZE)
		{
			for (int i = 0; i < size; i++)
			{
				yield return (i, i + 10);
			}
		}

		public static IEnumerable<(int Key, int Value)> GetDescendingTestData(int size = DEFAULT_SIZE)
		{
			for (int i = size - 1; i >= 0; i--)
			{
				yield return (i, i + 10);
			}
		}

		public static IEnumerable<(int Key, int Value)> GetRandomTestData(int size = DEFAULT_SIZE, int seed = DEFAULT_SEED)
		{
			var arr = new (int, int)[size];

			for (var i = 0; i < size; i++)
			{
				arr[i] = (i, i + 10);
			}

			// shuffle
			var random = new Random(seed);
			for (var i = 0; i < size; i++)
			{
				var iSwap = random.Next(size);

				var tmp = arr[i];
				arr[i] = arr[iSwap];
				arr[iSwap] = tmp;
			}

			return arr;
		}

		public static BTree CreateTreeWithData(IEnumerable<(int Key, int Value)> entries, int order = BTree.DEFAULT_ORDER)
		{
			var btree = new BTree(order);
			foreach (var (Key, Value) in entries)
			{
				btree.Insert(Key, Value);
			}

			return btree;
		}
		public static Dictionary<(int, int), int> GetVisitCount(IEnumerable<(int, int)> entries)
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


		public static IEnumerable<(int, int)> TraverseEntries(BTree btree)
		{
			return btree.Traverse().SelectMany(node => node.Entries);
		}
	}
}
