namespace WpfToolkit.Controls
{
    public struct ItemRange
    {
        public int StartIndex { get; }
        public int EndIndex { get; }

        public ItemRange(int startIndex, int endIndex) : this()
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public bool Contains(int itemIndex)
        {
            return itemIndex >= StartIndex && itemIndex <= EndIndex;
        }
    }
}
