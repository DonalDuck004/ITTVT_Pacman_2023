using PacManWPF.Game;
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

namespace PacManWPF
{
    /// <summary>
    /// Logica di interazione per WonPage.xaml
    /// </summary>
    public partial class WonPage : UserControl
    {
        public WonPage(TimeSpan time, int points)
        {
            InitializeComponent();
            this.ellapsed_time_label.Content = (new DateTime() + time).ToString("HH:mm:ss");
            this.points_final_label.Content = points;
        }
    }
}
