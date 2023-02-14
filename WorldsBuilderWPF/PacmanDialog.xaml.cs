using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WorldsBuilderWPF
{
    /// <summary>
    /// Logica di interazione per Pacman.xaml
    /// </summary>
    public partial class PacmanDialog : Window
    {
        public int? X;
        public int? Y;
        public int D => this.rotation.SelectedIndex;

        public static PacmanDialog? SINGLETON { get; private set; } = null;
        private Action cbk;

        public PacmanDialog(Action cbk)
        {
            if (PacmanDialog.SINGLETON is not null)
                throw new Exception("Instance is not null");

            InitializeComponent();
            PacmanDialog.SINGLETON = this;
            this.cbk = cbk;
            this.rotation.SelectedIndex = 0;
        }

        public void SetPos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;

            wp_X.Text = X.ToString();
            wp_Y.Text = Y.ToString();
            cbk.Invoke();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            var n = int.Parse(((TextBox)sender).Text + e.Text);
            e.Handled = !(0 <= n && n <= (((TextBox)sender).Name == "wp_Y" ? 14 : 31));
        }

        private void OnCloseButton(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public void Reload()
        {
            wp_Y.Text = Y.ToString();
            wp_X.Text = X.ToString();
            this.Show();
            this.Activate();
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            if (wp_X.Text != "")
                X = int.Parse(wp_X.Text);
            else
                X = null;

            if (wp_Y.Text != "")
                Y = int.Parse(wp_Y.Text);
            else
                Y = null;

            cbk.Invoke();
            this.Hide();
            e.Cancel = true;
        }

        private void OnKeydown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
                this.Close();
        }
    }
}
