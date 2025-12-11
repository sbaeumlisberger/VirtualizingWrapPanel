using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VirtualizingWrapPanelSamples
{
    public class TestItem : INotifyPropertyChanged
    {
        private static readonly int MinWidth = 100;
        private static readonly int MaxWidth = 200;
        private static readonly int MinHeight = 80;
        private static readonly int MaxHeight = 160;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Group { get; }

        public int Number { get; }

        public Color Background
        {
            get
            {
                if (background == default)
                {
                    byte[] randomBytes = new byte[3];
                    Random.NextBytes(randomBytes);
                    background = Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]);
                }
                return background;
            }
        }

        public Size Size { get; }

        public Size SizeLazy
        {
            get
            {
                if (sizeLazy == Size.Empty)
                {
                    Task.Delay(1000).ContinueWith((_) =>
                    {
                        sizeLazy = Size;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SizeLazy)));
                    });
                    return new Size(MinWidth, MinHeight);
                }
                return sizeLazy;
            }
        }

        public DateTime CurrentDateTime => DateTime.Now;

        private static readonly Random Random = new Random();

        private Size sizeLazy = Size.Empty;

        private Color background = default;

        public TestItem(int group, int number)
        {
            Group = group;
            Number = number;
            var width = Random.Next(MinWidth, MaxWidth);
            var height = Random.Next(MinHeight, MaxHeight);
            Size = new Size(width, height);
        }

        public void Reset()
        {
            sizeLazy = Size.Empty;
        }

        public override string ToString()
        {
            return $"TestItem({Number})";
        }
    }
}
