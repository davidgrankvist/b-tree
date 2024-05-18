


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
			var current = root;
			while (current != null)
			{
				var found = current.Find(key);
				if (found != null)
				{
					return found;
				}
				current = current.Next;
			}
			return null;
		}

		public void Insert(int key, int val)
		{
			var node = new Node(key, val);
			if (root == null)
			{
				root = node;
				return;
			}

			var prev = root;
			var current = root;
			var didInsert = false;
			while (current != null)
			{
				if (current.EntryCount < Order - 1)
				{
					current.Insert(key, val);
					didInsert = true;
					break;
				}
				prev = current;
				current = current.Next;
			}

			if (!didInsert)
			{
				prev.Next = node;
			}
		}

		public void Delete(int key)
		{
			if (root == null)
			{
				return;
			}

			Node? prev = null;
			var current = root;

			while (current != null)
			{
				var found = current.Find(key);
				if (found != null)
				{
					if (current.EntryCount > 1)
					{
						current.Remove(key);
					}
					else if (prev == null)
					{
						root = current.Next;
					}
					else
					{
						prev.Next = current.Next;
					}
				}

				prev = current;
				current = current.Next;
			}
		}

		private class Node : IBTreeNode
		{
			public Node? Next;
			private List<(int Key, int Value)> entries = new List<(int Key, int Value)>();

			public IEnumerable<(int Key, int Value)> Entries => entries;
			public IEnumerable<IBTreeNode> Children => GetChildren();

			public int Count { get => Children.Count(); }
			public int EntryCount { get => entries.Count(); }

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

			private IEnumerable<IBTreeNode> GetChildren()
			{
				if (Next == null)
				{
					yield break;
				}
				yield return Next;
			}

			public void Insert(int key, int val)
			{
				entries.Add((key, val));
			}

			public void Remove(int key)
			{
				var iRemove = entries.FindIndex((x) => x.Key == key);
				if (iRemove != -1)
				{
					entries.RemoveAt(iRemove);
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

		public interface IBTreeNode
		{
			public IEnumerable<(int Key, int Value)> Entries { get; }
			public IEnumerable<IBTreeNode> Children { get; }
			public int Count { get; }

			public int? Find(int key);
		}

		public IEnumerable<IBTreeNode> Traverse()
		{
			return TraverseNode(GetRoot());
		}

		private static IEnumerable<IBTreeNode> TraverseNode(IBTreeNode node)
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
