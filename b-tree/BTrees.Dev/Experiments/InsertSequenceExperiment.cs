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
			PrintInteractiveInstructions();

			var btree = new BTree(order);

			while (true)
			{
				var keys = InsertInputPrompt();
				if (keys == null)
				{
					break;
				}

				foreach (var key in keys)
				{
					btree.Insert(key, key);

					PrintInsert(key);
					PrintResult(btree, true);
				}
			}
		}

		private static int[]? InsertInputPrompt()
		{
			Console.Write("Input: ");
			var input = Console.ReadLine();
			if (input == null || input == string.Empty)
			{
				Console.WriteLine();
				Console.WriteLine("Exiting.");

				return null;
			}

			var inputs = input.Split(",");
			var keys = new int[inputs.Length];

			for (var i = 0; i < inputs.Length; i++)
			{
				var trimmedInput = inputs[i].Trim();
				var didParse = int.TryParse(trimmedInput, out var key);

				if (!didParse)
				{
					Console.WriteLine("Unable to parse input. Exiting.");

					return null;
				}

				keys[i] = key;
			}

			return keys;
		}

		private static void PrintInteractiveInstructions()
		{
			Console.WriteLine();
			Console.WriteLine("Instructions:");
			Console.WriteLine("- Write the key to insert, followed by enter");
			Console.WriteLine("- Insert multiple keys by separating with commas");
			Console.WriteLine("- To exit, press enter or hit Ctrl+C");
			Console.WriteLine();
		}
	}
}
