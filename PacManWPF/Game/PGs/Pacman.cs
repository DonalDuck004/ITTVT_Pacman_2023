using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PacManWPF.Animations;
using PacManWPF.Game.Tags;
using PacManWPF.Utils;

using Point = System.Drawing.Point;

namespace PacManWPF.Game.PGs
{

    public class Pacman
    {
        public static Pacman INSTANCE = new();

        public int X { get; private set; }
        public int Y { get; private set; }
        public PacmanAnimation Animation { get; private set; } = new();

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

        public Rectangle CeilObject { get; private set; } = new Rectangle() { Tag = PacmanTag.INSTANCE };

#nullable disable
        private Pacman()
        {
            this.CeilObject.BeginAnimation(Rectangle.FillProperty, this.Animation);
            MainWindow.INSTANCE.game_grid.Children.Add(this.CeilObject);
        }
#nullable restore

        public void Initialize(int x, int y, int grad)
        {
            this.DrugTicks = 0;
            this.X = x;
            this.Y = y;

            this.Animation.Grad = grad;

            Grid.SetColumn(this.CeilObject, x);
            Grid.SetRow(this.CeilObject, y); 
            Grid.SetZIndex(this.CeilObject, 2);
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
            BaseTag ceil_type;
            foreach (var ceil in PacmanGame.INSTANCE.CeilsAt(x, y))
            {
                ceil_type = (BaseTag)ceil.Tag;

                if (ceil_type.IsGate || ceil_type.IsAWall)
                {
                    can_pass = false;
                    break;
                }
                else if (ceil_type.IsFood)
                {
                    switch(((FoodTag)ceil_type).FoodType)
                    {
                        case Enums.FoodTypes.PacDot:
                            SoundEffectsPlayer.Play(SoundEffectsPlayer.CHOMP);
                            PacmanGame.INSTANCE.PacDots++;
                            break;

                        case Enums.FoodTypes.PowerPellet:
                            this.DrugTicks += Config.DRUG_TICKS;
                            break;

                        default:
                            if (((FoodTag)ceil_type).animation is not null)
                                ((FoodTag)ceil_type).animation.Stop();
                            SoundEffectsPlayer.Play(SoundEffectsPlayer.CHOMP_FRUIT);
                            break;
                    }

                    PacmanGame.INSTANCE.Points += (int)((FoodTag)ceil_type).FoodType;

                    ceil.Fill = null;
                    ceil.Tag = EmptyTag.INSTANCE;
                }
                else if (ceil_type.IsAGhost)
                {
                    if (this.IsDrugged || ((GhostTag)ceil_type).ghost.IsDied)
                        ((GhostTag)ceil_type).ghost.Kill();
                    else
                    {
                        Grid.SetZIndex(this.CeilObject, 2);
                        PacmanGame.INSTANCE.GameOver = true;
                        break;
                    }
                }
                    
            }


            if (can_pass)
            {
                this.X = x;
                this.Y = y;

                Grid.SetColumn(this.CeilObject, x);
                Grid.SetRow(this.CeilObject, y);
            }

            this.Animation.Grad = grad;
        }


        public bool IsAt(Point point) => IsAt(point.X, point.Y);

        public bool IsAt(int x, int y) => x == X && y == Y;
    }
}
