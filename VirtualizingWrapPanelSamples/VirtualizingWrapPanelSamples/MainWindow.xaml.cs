using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VirtualizingWrapPanelSamples {

    public partial class MainWindow : Window {

        public class TestItem {

            public int Number { get; set; }
            public Brush Background { get; set; }

            private static Random random = new Random();

            public TestItem(int number) {
                Number = number;
                byte[] randomBytes = new byte[3]; 
                random.NextBytes(randomBytes);
                Background = new SolidColorBrush(Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]));
            }

        }

        public ObservableCollection<TestItem> Items { get; }

        public MainWindow() {
            DataContext = this;
            Items = new ObservableCollection<TestItem>();
            for (int i = 0; i < 1000; i++) {
                Items.Add(new TestItem(i + 1));
            }
            InitializeComponent();
        }

    }

}
