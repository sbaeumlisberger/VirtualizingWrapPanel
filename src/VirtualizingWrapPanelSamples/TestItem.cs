using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VirtualizingWrapPanelSamples
{
    public class TestItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Group { get; }

        public int Number { get; }

        public Color Background { get; }

        public Size Size
        {
            get
            {
                if (size == new Size(0, 0))
                {
                    Task.Delay(10).ContinueWith((_) =>
                    {
                        size = new Size(Width, Height);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
                    });
                }
                return size;
            }
        }

        public DateTime CurrentDateTime => DateTime.Now;

        private static Random random = new Random();

        private Size size = new Size(0, 0);

        public int Width { get; } = random.Next(80, 200);
        public int Height { get; } = random.Next(40, 100);

        public TestItem(string group, int number)
        {
            Group = group;
            Number = number;
            byte[] randomBytes = new byte[3];
            random.NextBytes(randomBytes);
            Background = Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]);
            Width = random.Next(80, 200);
            Height = random.Next(40, 100);
        }
    }
}
