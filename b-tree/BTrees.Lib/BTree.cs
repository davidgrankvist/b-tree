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

				if (current.IsLeaf)
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
			if (node.IsLeaf)
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
			if (node.IsLeaf || node.EntryCount == 1)
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

		// TODO: replace delete with this when it works
		public void BalancedDelete(int key)
		{
			var found = FindWithNode(key);
			if (found == null || !found.HasValue)
			{
				return;
			}

			var node = found.Value.Node;
			var iKey = node.entries.FindIndex(x => x.Key == key);
			node.Remove(key);

			if (node.IsLeaf)
			{
				if (node == root && node.Count == 0)
				{
					root = null;
				}
				else if (IsUnderflown(node))
				{
					Rebalance(node, iKey);
				}
			}
			else
			{
				var leaf = PromoteLeaf(node, iKey);
				if (IsUnderflown(leaf))
				{
					Rebalance(leaf, iKey);
				}
			}
		}

		private static Node PromoteLeaf(Node node, int iKey)
		{
			var leftSubtree = node.children[iKey];
			var rightSubtree = node.children[iKey + 1];

			var leftLeaf = FindRightmostLeaf(leftSubtree);
			var rightLeaf = FindLeftmostLeaf(rightSubtree);

			if (leftLeaf.EntryCount > rightLeaf.EntryCount)
			{
				var max = leftLeaf.entries.Last();
				leftLeaf.Remove(max.Key);
				node.Insert(max.Key, max.Value);

				return leftLeaf;
			}
			else
			{
				var min = rightLeaf.entries.First();
				rightLeaf.Remove(min.Key);
				node.Insert(min.Key, min.Value);

				return rightLeaf;
			}
		}

		private static Node FindRightmostLeaf(Node node)
		{
			var current = node;
			while (!current.IsLeaf)
			{
				current = current.children.Last();
			}
			return current;
		}

		private static Node FindLeftmostLeaf(Node node)
		{
			var current = node;
			while (!current.IsLeaf)
			{
				current = current.children.First();
			}
			return current;
		}

		private bool IsUnderflown(Node node)
		{
			return node.EntryCount < GetMinimumNumberOfKeys(node);
		}

		private int GetMinimumNumberOfKeys(Node node)
		{
			if (node.IsLeaf)
			{
				if (node == root)
				{
					return 1;
				}
				else
				{
					// minimum number of keys is ceil(m/2) - 1
					return Order / 2 + (Order % 2) - 1;
				}
			}
			else
			{
				return node.Count - 1;
			}
		}

		private void Rebalance(Node node, int iKey)
		{
			// The root has no siblings or parent, so we can't rotate/merge.
			// Promote a leaf instead and rebalance it if needed.
			if (node == root)
			{
				var leaf = PromoteLeaf(node, iKey);
				if (IsUnderflown(leaf))
				{
					Rebalance(leaf, iKey);
				}
				return;
			}

			// otherwise rotate/merge
			var didRotate = TryRotate(node);
			if (!didRotate)
			{
				var iRemovedParentKey = Merge(node);
				if (IsUnderflown(node.parent))
				{
					// TODO: check which second argument makes sense here
					// remember that children are merged and indices may change
					Rebalance(node.parent, iRemovedParentKey);
				}
			}
		}

		private bool TryRotate(Node node)
		{
			// Rotate largest key in left sibling or smallest key in right sibling.
			// Take from the one with the most keys.
			//
			// Also rotate children. If left is rotated, bring the largest subtree.
			// If right is rotated, bring the smallest subtree

			var leftSibling = new Node(); // dummy sibling values
			var rightSibling = new Node();
			var parent = node.parent;

			var iNode = parent.children.FindIndex(x => x == node);
			if (iNode > 0)
			{
				leftSibling = parent.children[iNode - 1];
			}
			if (iNode < parent.children.Count - 1)
			{
				rightSibling = parent.children[iNode + 1];
			}

			// Check that we are able to rotate at all
			if (leftSibling.EntryCount <= GetMinimumNumberOfKeys(leftSibling) && rightSibling.EntryCount <= GetMinimumNumberOfKeys(rightSibling))
			{
				return false;
			}

			// Rotate a key from the sibling which has the most keys
			if (leftSibling.EntryCount > rightSibling.EntryCount)
			{
				/* Rotate max entry from left sibling
				 *
				 *     2              1
				 *    / \            / \
				 *  0,1 target  ->  0   2
				 *     \               /
				 *     subtree       subtree
				 */
				var iParentKey = iNode - 1;
				var iLeftMax = leftSibling.EntryCount - 1;
				var leftMax = leftSibling.entries.Last();

				// rotate entry
				var parentEntry = parent.entries[iParentKey];
				parent.entries[iParentKey] = leftMax;
				leftSibling.Remove(leftMax.Key);
				node.Insert(parentEntry.Key, parentEntry.Value);

				if (!node.IsLeaf)
				{
					// move subtree
					var leftSiblingMaxChild = leftSibling.children[iLeftMax + 1];
					leftSibling.RemoveChild(leftSiblingMaxChild);
					node.InsertChild(leftSiblingMaxChild);
					leftSiblingMaxChild.parent = node;
				}
			}
			else
			{
				/* Rotate min entry from right sibling
				 *
				 *       2            3
				 *      / \          / \
				 * target  3,4  ->  2   4
				 *        /          \
				 *     subtree       subtree
				 */
				var iParentKey = iNode;
				var iRightMin = 0;
				var rightMin = rightSibling.entries.First();

				// rotate entry
				var parentEntry = parent.entries[iParentKey];
				parent.entries[iParentKey] = rightMin;
				rightSibling.Remove(rightMin.Key);
				node.Insert(parentEntry.Key, parentEntry.Value);

				if (!node.IsLeaf)
				{
					// move subtree
					var rightSiblingMinChild = leftSibling.children[iRightMin];
					rightSibling.RemoveChild(rightSiblingMinChild);
					node.InsertChild(rightSiblingMinChild);
					rightSiblingMinChild.parent = node;
				}
			}

			return true;
		}

		private int Merge(Node node)
		{
			throw new NotImplementedException();
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
