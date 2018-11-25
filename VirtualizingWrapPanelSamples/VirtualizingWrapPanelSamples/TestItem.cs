using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VirtualizingWrapPanelSamples {

    public class TestItem {

        public int Number { get; }

        public Color Background { get; set; }

        private static Random random = new Random();

        public TestItem(int number) {
            Number = number;
            byte[] randomBytes = new byte[3];
            random.NextBytes(randomBytes);
            Background = Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]);
        }

    }

}