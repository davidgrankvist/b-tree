using System.Text;

namespace BTrees.Lib
{
	public class BTreeDebugUtils
	{
		private const string BRANCH_STR = "|__";
		private const string EMPTY_NODE_STR = "X";
		private const string ENTRY_SEPARATOR = ",";

		public static void PrintTree(BTree tree)
		{
			Console.WriteLine(StringifyTree(tree));
		}

		public static void PrintTree(IBTreeNode node)
		{
			Console.WriteLine(StringifyTree(node));
		}

		public static string StringifyTree(BTree btree)
		{
			if (btree.IsEmpty)
			{
				return string.Empty;
			}
			return StringifyTree(btree.GetRoot());
		}


		public static string StringifyTree(IBTreeNode node)
		{
			var sb = new StringBuilder();
			StringifySubtree(node, sb, 0);

			return sb.ToString();
		}

		private static void StringifySubtree(IBTreeNode node, StringBuilder sb, int indentation)
		{
			var nodeStr = StringifyNode(node);
			sb.AppendLine(nodeStr);

			foreach (var child in node.Children)
			{
				var branchStr = BRANCH_STR.PadLeft(indentation);
				sb.Append(branchStr);
				StringifySubtree(child, sb, indentation + BRANCH_STR.Length);
			}
		}

		private static string StringifyNode(IBTreeNode node)
		{
			if (!node.Entries.Any())
			{
				return EMPTY_NODE_STR;
			}

			var keys = node.Entries.Select(x => x.Key);
			return string.Join(ENTRY_SEPARATOR, keys);
		}
	}
}
