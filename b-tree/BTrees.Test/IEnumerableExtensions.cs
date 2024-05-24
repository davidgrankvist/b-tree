namespace BTrees.Test
{
	internal static class IEnumerableExtensions
	{
		public static bool IsSortedAsc<T>(this IEnumerable<T> items)
		{
			var comparer = Comparer<T>.Default;
			var prev = items.First();

			foreach ( var item in items )
			{
				if (comparer.Compare(prev, item) > 0)
				{
					return false;
				}

				prev = item;
			}
			return true;
		}
	}
}
