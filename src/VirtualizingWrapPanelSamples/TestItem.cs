using System;
using System.Windows.Media;

namespace VirtualizingWrapPanelSamples
{
    public class TestItem
    {
        public string Group { get; }
        public int Number { get; }
        public Color Background { get; }
        public int Width { get; }
        public int Height { get; }
        public DateTime CurrentDateTime => DateTime.Now;

        private static Random random = new Random();

        public TestItem(string group, int number)
        {
            Group = group;
            Number = number;
            byte[] randomBytes = new byte[3];
            random.NextBytes(randomBytes);
            Background = Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]);
            Width = random.Next(20, 200);
            Height = random.Next(40, 80);
        }
    }
}
