using System;
using System.Windows.Media;

namespace VirtualizingWrapPanelSamples
{
    public class TestItem
    {
        public string Group { get; }
        public int Number { get; }
        public int Width { get; }
        public int Height { get; }
        public Color Background { get; }
        public DateTime CurrentDateTime => DateTime.Now;

        private static Random random = new Random();

        public TestItem(string group, int number)
        {
            Group = group;
            Number = number;
            Width = random.Next(20, 200);
            Height = random.Next(40, 80);
            Background = Color.FromRgb(RandomByte(), RandomByte(), RandomByte());
        }

        static byte RandomByte() => (byte)random.Next(0, byte.MaxValue);
    }
}
