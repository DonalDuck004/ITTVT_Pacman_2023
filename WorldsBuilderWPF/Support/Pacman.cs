using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace WorldsBuilderWPF
{

    public partial class MainWindow : Window
    {

        private void PacmanApplyChanges(object sender, RoutedEventArgs e)
        {
            if (pacman_x_txt.Text == "" || pacman_y_txt.Text == "")
                return;

            this.SetPacman(int.Parse(pacman_x_txt.Text),
                           int.Parse(pacman_y_txt.Text),
                           pacman_rotation_combo_box.SelectedIndex,
                           from_e: true);
        }

        private void SetPacman(int x, int y, int rt, bool from_e = false)
        {
            if (rt <= 3)
                rt *= 90;

            var transform = Matrix.Identity;
            transform.RotateAt(rt, 0.5, 0.5);
            this.PacmanCeil.LayoutTransform = new MatrixTransform(transform);

            Grid.SetColumn(this.PacmanCeil, x);
            Grid.SetRow(this.PacmanCeil, y);

            if (from_e is false)
            {
                this.pacman_rotation_combo_box.SelectedIndex = rt / 90;
                this.pacman_x_txt.Text = x.ToString();
                this.pacman_y_txt.Text = y.ToString();
            }
        }
    }
}
