using System.Diagnostics;

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

			if (Find(key) != null)
			{
				throw new InvalidOperationException();
			}

			InsertInto(key, val, root);
		}

		private void InsertInto(int key, int val, Node node)
		{
			// Phase 1 - Find a leaf to insert the entry into
			if (node.IsLeaf() && HasSlots(node))
			{
				node.Insert(key, val);
			}
			else if (node.IsLeaf())
			{
				// the leaf has no more slots, so we need to split it
				SplitInsert(key, val, node);
			}
			else
			{
				// find the subtree to insert into

				// dummy implementation - always pick the first one
				InsertInto(key, val, node.children.First());
			}
		}

		private void SplitInsert(int key, int val, Node node)
		{
			// Phase 2 - Splitting
			//
			// 1. Split a node by the mid entry
			// 2. Insert the mid into the parent.
			//     If needed, keep splitting up the tree.

			// We performed a split and the parent had room for a new entry
			if (HasSlots(node))
			{
				node.Insert(key, val);
				return;
			}

			// if we split the root, then we need a new root
			if (node == root)
			{
				var newRoot = new Node();
				root = newRoot;
				node.parent = newRoot;
			}
			var parent = node.parent;

			// create two splits by the mid value
			node.Insert(key, val); // temporary insert to include new entry when looking for mid
			var mid = node.entries[node.entries.Count / 2];
			var left = new Node(parent);
			var right = new Node(parent);
			// split the node entries
			foreach (var entry in node.entries)
			{
				if (entry.Key <= mid.Key)
				{
					left.Insert(entry.Key, entry.Value);
				}
				else
				{
					right.Insert(entry.Key, entry.Value);
				}
			}
			// remove at least one mid as it will be inserted into the parent
			left.Remove(mid.Key);

			// existing children are pushed down the new left/right subtrees
			foreach (var child in node.children)
			{
				var max = child.entries.Last().Key;

				if (max <= mid.Key)
				{
					left.InsertChild(child);
				}
				else
				{
					right.InsertChild(child);
				}
			}

			// replace the node with its splits
			parent.RemoveChild(node);
			parent.InsertChild(left);
			parent.InsertChild(right);

			// finally insert the mid into the parent
			SplitInsert(mid.Key, mid.Value, parent);
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
			return BTreeUtils.TraverseNode(GetRoot());
		}

		private class Node : IBTreeNode
		{
			public Node parent;

			public List<(int Key, int Value)> entries = new List<(int Key, int Value)>();
			public List<Node> children = new List<Node>();

			public IEnumerable<(int Key, int Value)> Entries => entries;
			public IEnumerable<IBTreeNode> Children => children;

			public int Count { get => children.Count; }
			public int EntryCount { get => entries.Count; }

			public Node()
			{

			}

			public Node(int key, int val)
			{
				entries.Add((key, val));
			}

			public Node(Node parent)
			{
				this.parent = parent;
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

			public void InsertChild(Node node)
			{
				var max = node.EntryCount == 0 ? -1 : node.entries.Last().Key;
				var iAdd = children.FindLastIndex((x) => max > x.entries.Last().Key);
				children.Insert(iAdd + 1, node);
			}

			public void RemoveChild(Node node)
			{
				children.Remove(node);
			}
		}
	}
}
