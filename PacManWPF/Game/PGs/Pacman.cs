using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PacManWPF.Utils;

using Point = System.Drawing.Point;

namespace PacManWPF.Game.PGs
{

    public class Pacman
    {
        public static Pacman INSTANCE = new();

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Grad { get; private set; }
        private int animation_count;

        public Point Position => new (X, Y);


        private int _drug_frames = 0;
        public int DrugTicks
        {
            get => _drug_frames;
            set
            {
                if (value == 0)
                    MainWindow.INSTANCE.drug_wrap.Visibility = Visibility.Hidden;
                else
                {
                    MainWindow.INSTANCE.drug_wrap.Visibility = Visibility.Visible;

                    MainWindow.INSTANCE.drug_ticks_label.Content = value.ToString().ZFill(2) + " ticks";
                }

                _drug_frames = value; // + 100;
            }
        }

        public bool IsDrugged => DrugTicks > 0;

        public Rectangle CeilObject { get; private set; } = new Rectangle();

#nullable disable
        private Pacman()
        {
            MainWindow.INSTANCE.game_grid.Children.Add(this.CeilObject);
        }
#nullable restore

        public void Initialize(int x, int y, int grad)
        {
            this.DrugTicks = 0;
            this.X = x;
            this.Y = y;
            this.Grad = grad;

            this.Animate();


            Grid.SetColumn(this.CeilObject, x);
            Grid.SetRow(this.CeilObject, y); 
            Grid.SetZIndex(this.CeilObject, 2);
        }


        public MatrixTransform GetTransform(int? grad = null)
        {
            var transform = Matrix.Identity;
            transform.RotateAt(grad ?? Grad, 0.5, 0.5);

            if (IsDrugged)
                transform.ScaleAt(2, 2, 0.5, 0.5);
            else
                transform.ScaleAt(1, 1, 0.5, 0.5);

            return new (transform);
        }


        public void MoveTo(int x, int y, int grad)
        {
            if (x == -1)
                x = Config.CHUNK_WC - 1;
            else if (x == Config.CHUNK_WC)
                x = 0;

            if (y == -1)
                y = Config.CHUNK_HC - 1;
            else if (y == Config.CHUNK_HC)
                y = 0;


            bool can_pass = true;
            foreach (var ceil in PacmanGame.INSTANCE.CeilsAt(x, y))
            {

                if (ceil.IsWall())
                {
                    can_pass = false;
                    break;
                }
                else if (ceil.IsPoint())
                {
                    SoundEffectsPlayer.Play(SoundEffectsPlayer.CHOMP);
                    PacmanGame.INSTANCE.Points++;
                    ceil.Fill = null;
                }
                else if (ceil.IsDrug())
                {
                    DrugTicks += Config.DRUG_TICKS;
                    ceil.Fill = null;
                }
                else if (ceil.Fill is not null)
                {

                    foreach (Ghost killer in Ghost.INSTANCES)
                    {
                        if (object.ReferenceEquals(ceil, killer.CeilObject))
                        {
                            if (IsDrugged || killer.IsDied)
                                killer.Kill();
                            else
                            {
                                PacmanGame.INSTANCE.GameOver = true;
                                break;
                            }
                        }
                    }

                    if (PacmanGame.INSTANCE.GameOver)
                        break;
                }
                    
            }


            if (can_pass)
            {
                this.X = x;
                this.Y = y;

                Grid.SetColumn(this.CeilObject, x);
                Grid.SetRow(this.CeilObject, y);
            }

            this.Grad = grad;
            var img = GetImage(); // Without this var, it may result laggy
            img.RelativeTransform = GetTransform(grad);
            this.CeilObject.Fill = img;
        }

        public void Animate()
        {
            var tmp = this.GetImage();
            tmp.RelativeTransform = this.GetTransform();
            this.CeilObject.Fill = tmp;
        }

        public bool IsAt(Point point) => IsAt(point.X, point.Y);

        public bool IsAt(int x, int y) => x == X && y == Y;
        public ImageBrush GetImage() => ResourcesLoader.GetImage(ResourcesLoader.PacManAnimationPaths[++animation_count % 3]);
    }
}
