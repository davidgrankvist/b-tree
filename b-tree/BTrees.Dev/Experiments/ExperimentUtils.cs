using BTrees.Lib;

namespace BTrees.Dev.Experiments
{
	internal static class ExperimentUtils
	{
		public static void PrintHeader()
		{
			Console.WriteLine("############################");
			Console.WriteLine("##     NEW EXPERIMENT     ##");
			Console.WriteLine("############################");
		}

		public static void PrintInsert(int key)
		{
			Console.WriteLine();
			Console.WriteLine($"Insert: {key}");
		}

		public static void PrintResult(BTree btree, bool withSeparator)
		{
			Console.WriteLine("Result:");
			Console.WriteLine();

			BTreeDebugUtils.PrintTree(btree);

			if (withSeparator)
			{
				Console.WriteLine("============================");
			}
		}
	}
}
