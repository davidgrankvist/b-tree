using BTrees.Lib;

namespace BTrees.Dev.Experiments
{
	internal static class InsertSequenceExperiment
	{
		public static void InsertAscendingAndPrint(int size, int order = 3)
		{
			PrintHeader();

			var btree = new BTree(order);
			for (var i = 0; i < size; i++)
			{
				btree.Insert(i, i);

				PrintInsert(i);
				PrintResult(btree, i < size - 1);
			}
		}

		private static void PrintHeader()
		{
			Console.WriteLine("############################");
			Console.WriteLine("##     NEW EXPERIMENT     ##");
			Console.WriteLine("############################");
		}

		private static void PrintInsert(int key)
		{
			Console.WriteLine();
			Console.WriteLine($"Insert: {key}");
		}

		private static void PrintResult(BTree btree, bool withSeparator)
		{
			Console.WriteLine("Result:");
			Console.WriteLine();

			BTreeDebugUtils.PrintTree(btree);

			if (withSeparator)
			{
				Console.WriteLine("============================");
			}
		}

		public static void InsertInteractiveAndPrint(int order = 3)
		{
			PrintHeader();
			Console.WriteLine("Write the key to insert, followed by enter. To exit, press enter or hit Ctrl+C");
			Console.WriteLine();

			var btree = new BTree(order);

			while (true)
			{
				var key = InsertInputPrompt();
				if (!key.HasValue)
				{
					break;
				}

				var k = key.Value;
				btree.Insert(k, k);

				PrintInsert(k);
				PrintResult(btree, true);
			}
		}

		private static int? InsertInputPrompt()
		{
			Console.Write("Input: ");
			var input = Console.ReadLine();
			if (input == null || input == string.Empty)
			{
				Console.WriteLine();
				Console.WriteLine("Exiting.");

				return null;
			}

			var didParse = int.TryParse(input, out var key);

			if (!didParse)
			{
				Console.WriteLine("Unable to parse integer. Exiting.");

				return null;
			}

			return key;
		}
	}
}
