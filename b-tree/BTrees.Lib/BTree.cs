namespace BTrees.Lib
{
	/*
	 * Work in progress B-tree. This is implemented as a TDD code kata,
	 * starting with a linked-list structure and incrementing from there.
	 *
	 * The current implementation details may look strange :)
	 */
	public class BTree
	{
		public const int DEFAULT_ORDER = 3;

		public int Order { get; private set; }
		private Node? root = null;

		public BTree()
		{
			Order = DEFAULT_ORDER;
		}

		public BTree(int order)
		{
			Order = order;
		}

		public int? Find(int key)
		{
			if (root == null)
			{
				return null;
			}

			// naive traversal search
			foreach (var node in Traverse())
			{
				var found = node.Find(key);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}

		public void Insert(int key, int val)
		{
			if (root == null)
			{
				root = new Node(key, val);
				return;
			}

			InsertInto(key, val, root);
		}

		private void InsertInto(int key, int val, Node node)
		{
			if (node.IsLeaf() && HasSlots(node))
			{
				node.Insert(key, val);
			}
			else if (node.IsLeaf())
			{
				// split

				// dummy implementation - just add a new node for now
				node.children.Add(new Node(key, val));
			}
			else
			{
				// find the subtree to insert into

				// dummy implementation - always pick the first one
				InsertInto(key, val, node.children.First());
			}
		}

		private bool HasSlots(Node node)
		{
			return node.EntryCount < Order - 1;
		}

		public void Delete(int key)
		{
			if (root == null)
			{
				return;
			}

			foreach (Node node in Traverse())
			{
				var found = node.Find(key);
				if (found != null)
				{
					node.Remove(key);
					return;
				}
			}
		}

		public IBTreeNode GetRoot()
		{
			if (root == null)
			{
				throw new InvalidOperationException();
			}
			return root;
		}

		public IEnumerable<IBTreeNode> Traverse()
		{
			return IBTreeUtils.TraverseNode(GetRoot());
		}

		private class Node : IBTreeNode
		{
			public List<(int Key, int Value)> entries = new List<(int Key, int Value)>();
			public List<Node> children = new List<Node>();

			public IEnumerable<(int Key, int Value)> Entries => entries;
			public IEnumerable<IBTreeNode> Children => children;

			public int Count { get => children.Count; }
			public int EntryCount { get => entries.Count; }

			public Node(int key, int val)
			{
				entries.Add((key, val));
			}

			public int? Find(int key)
			{
				foreach (var entry in entries)
				{
					if (entry.Key == key)
					{
						return entry.Value;
					}
				}
				return null;
			}

			public void Insert(int key, int val)
			{
				var iAdd = entries.FindLastIndex((x) => key > x.Key);
				entries.Insert(iAdd + 1, (key, val));
			}

			public void Remove(int key)
			{
				var iRemove = entries.FindIndex((x) => x.Key == key);
				if (iRemove != -1)
				{
					entries.RemoveAt(iRemove);
				}
			}

			public bool IsLeaf()
			{
				return Count == 0;
			}
		}
	}
}
