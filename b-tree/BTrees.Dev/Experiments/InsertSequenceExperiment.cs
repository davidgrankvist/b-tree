using BTrees.Lib;

namespace BTrees.Dev.Experiments
{
	internal static class InsertSequenceExperiment
	{
		public static void InsertAscendingAndPrint(int size, int order = 3)
		{
			Console.WriteLine("############################");
			Console.WriteLine("##     NEW EXPERIMENT     ##");
			Console.WriteLine("############################");

			var btree = new BTree(order);
			for (var i = 0; i < size; i++)
			{
				btree.Insert(i, i);

				Console.WriteLine();
				Console.WriteLine($"Insert: {i}");
				Console.WriteLine("Result:");
				Console.WriteLine();

				BTreeDebugUtils.PrintTree(btree);

				if (i < size - 1)
				{
					Console.WriteLine("============================");
				}
			}
		}
	}
}
