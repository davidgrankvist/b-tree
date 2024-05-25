namespace BTrees.Lib
{
	internal class BTreeUtils
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
	}
}
