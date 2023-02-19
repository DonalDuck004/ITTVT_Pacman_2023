using System;
using System.Diagnostics;
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

        public DoubleAnimation? CurrentAnimation { get; private set; } = null;
        public int X { get; private set; }
        public int Y { get; private set; }
        public Point Position => new (X, Y);
        public Point SpawnPoint { get; private set; }
        public int SpawnGrad { get; private set; }
        public int Grad = 0;

        public int ComboKill { get; set; } = 0;
        private int _drug_frames = 0;
        public int DrugTicks
        {
            get => _drug_frames;
            set
            {
                Debug.Assert(GamePage.Current is not null);
                if (value == 0)
                {
                    GamePage.Current!.drug_wrap.Visibility = Visibility.Hidden;
                    ComboKill = 0;
                }
                else
                {
                    GamePage.Current!.drug_wrap.Visibility = Visibility.Visible;

                    GamePage.Current!.drug_ticks_label.Content = value;
                }

                if (value <= Config.POWER_PELLET_WARN)
                    GamePage.Current!.warn_lbl.Visibility = value % 2 == 0 ? Visibility.Hidden : Visibility.Visible;
                else if (GamePage.Current!.warn_lbl.Visibility != Visibility.Hidden)
                    GamePage.Current!.warn_lbl.Visibility = Visibility.Hidden;

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
            GamePage.CurrentGrid!.Children.Add(Pacman.INSTANCE.CeilObject);
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


        public bool MoveTo(int x, int y, int grad, ref bool PacmanHitted)
        {
            var dest = new Point(x, y).Fix();

            bool can_pass = true;
            BaseTag ceil_type;
            Image? food_ceil = null;

            foreach (var ceil in PacmanGame.INSTANCE.CeilsAt(dest.X, dest.Y))
            {
                ceil_type = (BaseTag)ceil.Tag;

                if (ceil_type.IsGate || ceil_type.IsAWall)
                {
                    can_pass = false;
                    break;
                }
                else if (ceil_type.IsFood)
                    food_ceil = ceil;
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

            if (food_ceil is not null)
            {
                switch (((FoodTag)food_ceil.Tag).FoodType)
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
                        ((FoodTag)food_ceil.Tag).animation?.Stop();
                        SoundEffectsPlayer.Play(SoundEffectsPlayer.CHOMP_FRUIT);
                        break;
                }

                PacmanGame.INSTANCE.Points += (int)((FoodTag)food_ceil.Tag).FoodType;

                food_ceil.Source = ResourcesLoader.EmptyImage;
                food_ceil.Tag = EmptyTag.INSTANCE;
            }

            this.UpdateLayout(grad);

            if (can_pass)
            {
                this.X = dest.X;
                this.Y = dest.Y;

                Grid.SetColumn(this.CeilObject, dest.X);
                Grid.SetRow(this.CeilObject, dest.Y);
                this.HandleAnimation(grad);
            }

            return can_pass;
        }


        private void HandleAnimation(int grad)
        {
            if (!RuntimeSettingsHandler.AnimationsEnabled)
                return;

            TimeSpan duration = new((long)(TimeSpan.TicksPerSecond / (this.IsDrugged ? Config.PACMAN_PP_MOVE_DIV : Config.PACMAN_MOVE_DIV)));
            TranslateTransform trans = new();
            TransformGroup group = new();
            if (Pacman.INSTANCE.IsDrugged)
            {
                group.Children.Add(new ScaleTransform(2, 2));
                this.CeilObject.RenderTransformOrigin = new(0.5, 0.5);
            }else
                this.CeilObject.RenderTransformOrigin = new(0, 0);
            group.Children.Add(trans);

            this.CeilObject.RenderTransform = group;


            if (grad == 90) // Down
            {
                CurrentAnimation = new(-this.CeilObject.ActualHeight, 0, duration);
                trans.BeginAnimation(TranslateTransform.YProperty, CurrentAnimation);
            } else if (grad == 180) // <-
            {
                CurrentAnimation = new(this.CeilObject.ActualWidth, 0, duration);
                trans.BeginAnimation(TranslateTransform.XProperty, CurrentAnimation);
            } else if (grad == 270) // Up
            {
                CurrentAnimation = new(this.CeilObject.ActualHeight, 0, duration);
                trans.BeginAnimation(TranslateTransform.YProperty, CurrentAnimation);
            } else // ->
            {
                CurrentAnimation = new(-this.CeilObject.ActualWidth, 0, duration);
                trans.BeginAnimation(TranslateTransform.XProperty, CurrentAnimation);
            }
        }

        public bool IsAt(Point point) => IsAt(point.X, point.Y);

        public bool IsAt(int x, int y) => x == X && y == Y;
    }
}
