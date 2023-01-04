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
using WpfAnimatedGif;
using Point = System.Drawing.Point;

namespace PacManWPF.Game.PGs
{

    public class Pacman
    {
        public static Pacman INSTANCE = new();

        public int X { get; private set; }
        public int Y { get; private set; }
        public Point Position => new (X, Y);
        public Point SpawnPoint { get; private set; }
        public int SpawnGrad { get; private set; }
        public int Grad = 0;

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

                _drug_frames = value;
            }
        }

        public bool IsDrugged => DrugTicks > 0;
        // TODO Property Grad, CeilObjectRotate
        public Image CeilObject { get; private set; } = new () { Tag = PacmanTag.INSTANCE, ClipToBounds = true };

#nullable disable
        private Pacman()
        {
            ImageBehavior.SetAnimatedSource(this.CeilObject, ResourcesLoader.PacMan);
            MainWindow.INSTANCE.game_grid.Children.Add(this.CeilObject);
            // this.CeilObject.BeginAnimation(Image.SourceProperty, this.Animation);
        }
#nullable restore

        public void UpdateLayout(int? Grad = null)
        {
            Grad ??= this.Grad;
            var transform = Matrix.Identity;
            transform.RotateAt(Grad.Value, 0.5, 0.5);
            this.CeilObject.LayoutTransform = new MatrixTransform(transform);

            if (Pacman.INSTANCE.IsDrugged)
            {
                this.CeilObject.RenderTransform = new ScaleTransform(2, 2);
                this.CeilObject.RenderTransformOrigin = new(0.5, 0.5);
            }
            else
            {
                this.CeilObject.RenderTransform = null;
                this.CeilObject.RenderTransformOrigin = new(0, 0);
            }

            this.Grad = Grad.Value;
        }

        public void Respawn()
        {
            this.DrugTicks = 0;
            this.UpdateLayout(this.SpawnGrad);

            this.X = this.SpawnPoint.X;
            this.Y = this.SpawnPoint.Y;
            Grid.SetColumn(this.CeilObject, this.SpawnPoint.X);
            Grid.SetRow(this.CeilObject, this.SpawnPoint.Y);
            Grid.SetZIndex(this.CeilObject, 2);
        }

        public void Initialize(int x, int y, int grad)
        {
            this.DrugTicks = 0;
            this.X = x;
            this.Y = y;
            this.SpawnPoint = this.Position;
            this.SpawnGrad = grad;

            this.UpdateLayout(this.SpawnGrad);

            Grid.SetColumn(this.CeilObject, x);
            Grid.SetRow(this.CeilObject, y); 
            Grid.SetZIndex(this.CeilObject, 2);
        }


        public void MoveTo(int x, int y, int grad, ref bool PacmanHitted)
        {
            var dest = new Point(x, y).Fix();

            bool can_pass = true;
            BaseTag ceil_type;
            foreach (var ceil in PacmanGame.INSTANCE.CeilsAt(dest.X, dest.Y))
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
                            SoundEffectsPlayer.PlayNoOverlap(SoundEffectsPlayer.CHOMP);
                            PacmanGame.INSTANCE.PacDots++;
                            break;

                        case Enums.FoodTypes.PowerPellet:
                            var play = !this.IsDrugged;

                            this.DrugTicks += Config.DRUG_TICKS;
                            if (play)
                                SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.POWER_PELLET, () => this.IsDrugged).OnDone(() => SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.GHOST_SIREN, () => !this.IsDrugged));
                            break;

                        default:
                            if (((FoodTag)ceil_type).animation is not null)
                                ((FoodTag)ceil_type).animation.Stop();
                            SoundEffectsPlayer.Play(SoundEffectsPlayer.CHOMP_FRUIT);
                            break;
                    }

                    PacmanGame.INSTANCE.Points += (int)((FoodTag)ceil_type).FoodType;

                    ceil.Source = null;
                    ceil.Tag = EmptyTag.INSTANCE;
                }
                else if (ceil_type.IsAGhost)
                {
                    if (this.IsDrugged || ((GhostTag)ceil_type).ghost.IsDied)
                        ((GhostTag)ceil_type).ghost.Kill();
                    else
                    {
                        Grid.SetZIndex(this.CeilObject, 2);
                        PacmanGame.INSTANCE.Lifes--;
                        PacmanHitted = true;
                        break;
                    }
                }
            }


            if (can_pass)
            {
                this.X = dest.X;
                this.Y = dest.Y;

                Grid.SetColumn(this.CeilObject, dest.X);
                Grid.SetRow(this.CeilObject, dest.Y);
            }

            this.UpdateLayout(grad);
        }


        public bool IsAt(Point point) => IsAt(point.X, point.Y);

        public bool IsAt(int x, int y) => x == X && y == Y;
    }
}
