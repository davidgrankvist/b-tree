namespace BTrees.Lib
{
	internal class Node : IBTreeNode
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
