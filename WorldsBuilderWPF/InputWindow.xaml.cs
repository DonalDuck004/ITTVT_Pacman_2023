using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Logica di interazione per Window1.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public bool Acquired { get; private set; } = false;
        public string InTitle => this.title_box.Text;
        public string[] InTags => this.tags_box.Text.Split("; ");

        public InputWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Acquired = true;
            this.Close();
        }
    }
}
