namespace BTrees.Lib
{
	public interface IBTreeNode
	{
		public IEnumerable<(int Key, int Value)> Entries { get; }

		public IEnumerable<IBTreeNode> Children { get; }

		public int Count { get; }

		public int? Find(int key);
	}
}