﻿namespace BTrees.Lib
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

		public bool IsEmpty => root == null;

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
				throw new InvalidOperationException("Duplicate keys are not allowed in this B-tree implementation.");
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

				// insert beforehand and let the split correct this
				node.Insert(key, val);
				SplitNode(node);
			}
			else
			{
				// find the subtree to insert into

				// dummy implementation - always pick the first one
				InsertInto(key, val, node.children.First());
			}
		}

		private void SplitNode(Node node)
		{
			// Phase 2 - Split a node that has exceeded its capacity
			// If needed, split parents recursively.

			var mid = FindMid(node);
			var (left, right) = SplitEntries(node, mid.Key);
			var (leftChildren, rightChildren) = SplitChildren(node.children, mid.Key);
			left.children = leftChildren;
			right.children = rightChildren;

			if (node.parent == null)
			{
				// if we split the root, then we need a new root
				var newRoot = new Node();

				// lift the mid entry
				newRoot.Insert(mid.Key, mid.Value);

				// replace current node with the left/right parts
				newRoot.InsertChild(left);
				newRoot.InsertChild(right);
				left.parent = newRoot;
				right.parent = newRoot;

				// no further splitting is required as the new root has exactly two children
				root = newRoot;
			}
			else
			{
				var parent = node.parent;

				// lift the mid entry
				parent.Insert(mid.Key, mid.Value);

				// replace current node with the left/right parts
				parent.RemoveChild(node);
				parent.InsertChild(left);
				parent.InsertChild(right);
				left.parent = parent;
				right.parent = parent;

				// If the parent now has too many entries or children,
				// then this is corrected with a split
				if (IsOverfull(parent))
				{
					SplitNode(parent);
				}
			}
		}

		private static (int Key, int Value) FindMid(Node node)
		{
			return node.entries[node.entries.Count / 2];
		}

		private static (Node Left, Node Right) SplitEntries(Node node, int midKey)
		{
			// split by mid
			var left = new Node(); // no parent node defined for now
			var right = new Node();
			foreach (var entry in node.entries)
			{
				if (entry.Key < midKey)
				{
					left.Insert(entry.Key, entry.Value);
				}
				else if (entry.Key > midKey)
				{
					right.Insert(entry.Key, entry.Value);
				}
				// skip mid as it will be inserted into a parent node
			}

			return (left, right);
		}

		private static (List<Node> LeftChildren, List<Node> RightChildren) SplitChildren(List<Node> children, int midKey)
		{
			var leftChildren = new List<Node>();
			var rightChildren = new List<Node>();

			foreach (var child in children)
			{
				var max = child.entries.Last().Key;

				if (max <= midKey)
				{
					leftChildren.Add(child);
				}
				else
				{
					rightChildren.Add(child);
				}
			}

			return (leftChildren, rightChildren);
		}


		private bool HasSlots(Node node)
		{
			return node.EntryCount < Order - 1;
		}

		private bool IsOverfull(Node node)
		{
			return node.EntryCount > Order - 1;
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
				throw new InvalidOperationException("Unable to retrieve root node. The tree is empty.");
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
