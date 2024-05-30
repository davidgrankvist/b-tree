namespace BTrees.Lib
{
	public class BTreeUtils
	{
		public static IEnumerable<IBTreeNode> TraverseNode(IBTreeNode node)
		{
			yield return node;

			foreach (var child in node.Children)
			{
				var subtree = TraverseNode(child);
				foreach (var subtreeNode in subtree)
				{
					yield return subtreeNode;
				}
			}
		}

		public static IEnumerable<(IBTreeNode Node, int Depth)> TraverseNodeWithDepth(IBTreeNode node, int depth = 0)
		{
			yield return (node, depth);

			foreach (var child in node.Children)
			{
				var subtree = TraverseNodeWithDepth(child, depth + 1);
				foreach (var subtreeNode in subtree)
				{
					yield return subtreeNode;
				}
			}
		}
	}
}
