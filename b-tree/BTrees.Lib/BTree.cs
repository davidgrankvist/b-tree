namespace BTrees.Lib
{
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
			return FindWithNode(key)?.Value;
		}

		private (Node Node, int Value)? FindWithNode(int key)
		{
			var current = root;
			while (current != null)
			{
				var found = current.Find(key);
				if (found != null)
				{
					return (current, found.Value);
				}

				if (current.IsLeaf())
				{
					break;
				}
				current = FindTargetChild(current, key);
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

			BottomUpInsert(key, val, root);
		}

		private void BottomUpInsert(int key, int val, Node node)
		{
			// Phase 1 - Find a leaf to insert the entry into
			if (node.IsLeaf())
			{
				node.Insert(key, val);

				if (IsOverfull(node))
				{
					SplitNode(node);
				}
			}
			else
			{
				BottomUpInsert(key, val, FindTargetChild(node, key));
			}
		}

		private static Node FindTargetChild(Node node, int key)
		{
			// Find the entry which has the target child
			var iTargetEntry = node.entries.FindIndex(x => key < x.Key);
			if (iTargetEntry == -1)
			{
				iTargetEntry = node.entries.Count - 1;
			}
			var targetEntryKey = node.entries[iTargetEntry].Key;

			// Pick left/right child of the entry as target
			var isLeft = key < targetEntryKey;
			var iTargetNode = isLeft ? iTargetEntry : iTargetEntry + 1;

			// TODO: Temporary fix while deletion does not rebalance the tree.
			if (iTargetNode >= node.children.Count)
			{
				iTargetNode--;
			}

			return node.children[iTargetNode];
		}

		private void SplitNode(Node node)
		{
			// Phase 2 - Split a node that has exceeded its capacity
			// If needed, split parents recursively.

			var mid = FindMid(node);
			var (left, right) = SplitEntries(node, mid.Key);
			SplitChildren(node.children, mid.Key, left, right);

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

		private static void SplitChildren(List<Node> children, int midKey, Node left, Node right)
		{
			var leftChildren = new List<Node>();
			var rightChildren = new List<Node>();

			foreach (var child in children)
			{
				var max = child.entries.Last().Key;

				if (max <= midKey)
				{
					child.parent = left;
					leftChildren.Add(child);
				}
				else
				{
					child.parent = right;
					rightChildren.Add(child);
				}
			}

			left.children = leftChildren;
			right.children = rightChildren;
		}

		private bool IsOverfull(Node node)
		{
			return node.EntryCount > Order - 1;
		}

		public void Delete(int key)
		{
			var found = FindWithNode(key);
			if (found == null || !found.HasValue)
			{
				return;
			}

			var node = found.Value.Node;
			if (node.IsLeaf() || node.EntryCount == 1)
			{
				if (node == root)
				{
					root = null;
					return;
				}

				node.parent.RemoveChild(node);
				foreach (var child in node.children)
				{
					child.parent = node.parent;
				}
			}
			else
			{
				node.Remove(key);
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
	}
}
