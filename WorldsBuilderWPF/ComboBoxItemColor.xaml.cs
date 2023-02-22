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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Logica di interazione per ComboBoxItemColor.xaml
    /// </summary>
    public partial class ComboBoxItemColor : UserControl
    {
        private static DependencyProperty ColorProperty;
        private static DependencyProperty LabelContentProperty;

        static ComboBoxItemColor()
        {
            ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(ComboBoxItemColor));
            LabelContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(ComboBoxItemColor));
        }
        
        public ComboBoxItemColor()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public SolidColorBrush ColorFill
        {
            get => (SolidColorBrush)base.GetValue(ColorProperty);
            set => base.SetValue(ColorProperty, value);
        }

        public new string Content
        {
            get => (string)base.GetValue(LabelContentProperty);
            set => base.SetValue(LabelContentProperty, value);
        }

    }
}
