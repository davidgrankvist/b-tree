using BTrees.Lib;

namespace BTrees.Dev.Experiments
{
	internal static class InsertSequenceExperiment
	{
		public static void InsertAscendingAndPrint(int size, int order = 3)
		{
			ExperimentUtils.PrintHeader();

			var btree = new BTree(order);
			for (var i = 0; i < size; i++)
			{
				btree.Insert(i, i);

				ExperimentUtils.PrintInsert(i);
				ExperimentUtils.PrintResult(btree, i < size - 1);
			}
		}

		public static void InsertPresetAndPrint(int[] keys, int order = 3)
		{
			ExperimentUtils.PrintHeader();

			var btree = new BTree(order);
			for (var i = 0; i < keys.Length; i++)
			{
				var key = keys[i];
				btree.Insert(key, key);

				ExperimentUtils.PrintInsert(key);
				ExperimentUtils.PrintResult(btree, i < keys.Length - 1);
			}
		}
	}
}
