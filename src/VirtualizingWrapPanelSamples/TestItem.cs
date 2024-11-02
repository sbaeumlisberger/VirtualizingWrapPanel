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

        public string Group { get; }

        public int Number { get; }

        public Color Background { get; }

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

        public Size Size { get; }

        private static Random random = new Random();

        private Size sizeLazy = Size.Empty;

        public TestItem(string group, int number)
        {
            Group = group;
            Number = number;
            byte[] randomBytes = new byte[3];
            random.NextBytes(randomBytes);
            Background = Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]);
            var width = random.Next(MinWidth, MaxWidth);
            var height = random.Next(MinHeight, MaxHeight);
            Size = new Size(width, height);
        }

        public void Reset() 
        {
            sizeLazy = Size.Empty;
        }

        override public string ToString()
        {
            return $"TestItem({Number})";
        }
    }
}
