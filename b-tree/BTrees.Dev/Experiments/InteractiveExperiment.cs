using BTrees.Lib;

namespace BTrees.Dev.Experiments
{
	internal static class InteractiveExperiment
	{
		private enum TreeOperation
		{
			Unknown,
			Insert,
			Delete,
		}

		public static void RunExperiment(int order = 3)
		{
			ExperimentUtils.PrintHeader();
			PrintInstructions();

			var btree = new BTree(order);

			while (true)
			{
				var (operation, opShouldExit) = OperationPrompt();
				if (opShouldExit)
				{
					break;
				}

				var (keys, keyShouldExit) = KeyPrompt(operation);
				if (keyShouldExit)
				{
					break;
				}

				foreach (var key in keys)
				{
					ExecuteOperation(key, btree, operation);
					PrintOperation(key, operation);

					ExperimentUtils.PrintResult(btree, true);
				}
			}
		}

		private static void ExecuteOperation(int key, BTree btree, TreeOperation operation)
		{
			switch (operation)
			{
				case TreeOperation.Insert:
					btree.Insert(key, key);
					break;
				case TreeOperation.Delete:
					btree.Delete(key);
					break;
			}
		}

		private static void PrintOperation(int key, TreeOperation operation)
		{
			Console.WriteLine($"{operation}: {key}");
		}

		private static (TreeOperation Operation, bool ShouldExit) OperationPrompt()
		{
			Console.Write("Pick an operation (i/d): ");
			var input = Console.ReadLine();
			var result = ParseOperation(input);

			if (result == TreeOperation.Unknown)
			{
				Console.WriteLine();
				Console.WriteLine("Unkown operation. Exiting.");
				return (TreeOperation.Unknown, true);
			}

			return (result, false);
		}

		private static TreeOperation ParseOperation(string? input)
		{
			if (input == "i")
			{
				return TreeOperation.Insert;
			}
			else if (input == "d")
			{
				return TreeOperation.Delete;
			}
			else
			{
				return TreeOperation.Unknown;
			}
		}

		private static (int[] Keys, bool ShouldExit) KeyPrompt(TreeOperation operation)
		{
			Console.Write($"Enter keys to {operation.ToString().ToLower()}: ");
			var input = Console.ReadLine();
			Console.WriteLine();

			var result = ParseInput(input);
			if (result.Length == 0)
			{
                Console.WriteLine();
                Console.WriteLine("Unable to parse keys. Exiting.");
				return (result, true);
            }
			return (result, false);
		}

		private static int[] ParseInput(string? input)
		{
			var defaultResult = Array.Empty<int>();
			if (input == null || input == string.Empty)
			{
				return defaultResult;
			}

			var inputs = input.Split(",");
			var keys = new int[inputs.Length];

			for (var i = 0; i < inputs.Length; i++)
			{
				var trimmedInput = inputs[i].Trim();
				var didParse = int.TryParse(trimmedInput, out var key);

				if (!didParse)
				{
					return defaultResult;
				}

				keys[i] = key;
			}

			return keys;
		}

		private static void PrintInstructions()
		{
			var instructions = @"
Instructions:
- Pick the operation to execute (insert/delete)
- Write the key to insert, followed by enter
- Enter multiple keys by separating with commas
- To exit, press enter or hit Ctrl+C

";
			Console.Write(instructions);
		}
	}
}
