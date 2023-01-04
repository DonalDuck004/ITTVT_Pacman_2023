using System.Collections.Generic;
using System.Windows.Media;
using System;
using System.Windows;
using System.Windows.Shapes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Utils;
using PacManWPF.Game;
using PacManWPF.Game.PGs.Movers.Abs;
using PacManWPF.Game.PGs.Movers;
using PacManWPF.Game.Worlds;
using System.Windows.Media.Animation;
using PacManWPF.Animations;

namespace PacManWPF.Game.PGs
{

    public class Ghost : Abs.BasePG
    {
        // public ImageBrush? reset_ceil = null;
        public BitmapImage Image;

        public GhostColors Type { get; private set; }
        private BaseGhostMover? mover = null;


        public static Ghost[] INSTANCES = new Ghost[4];
        public static Ghost RedGhost { get => Ghost.INSTANCES[0]; }
        public static Ghost PinkGhost { get => Ghost.INSTANCES[1]; }
        public static Ghost CyanGhost { get => Ghost.INSTANCES[2]; }
        public static Ghost OrangeGhost { get => Ghost.INSTANCES[3]; }

        public bool Initialized { get; private set; } = false;
        public bool NeedToGoToSpawn { get; private set; } = false;
        public bool InGate { get; private set; } = false;

        public Image CeilObject { get; private set; }

        public System.Drawing.Point SpawnPoint { get; private set; }

        public System.Drawing.Point EffectivePosition => new(Grid.GetColumn(this.CeilObject), Grid.GetRow(this.CeilObject));

        public override System.Drawing.Point Position
        {
            get
            {
                Debug.Assert(mover is not null);
                return mover.GetPos();
            }
        }

        public Ghost(GhostColors type)
        {
            this.Type = type;
            this.Image = ResourcesLoader.GetImage(type is GhostColors.Cyan ? ResourcesLoader.CyanGhost :
                                                  type is GhostColors.Pink ? ResourcesLoader.PinkGhost :
                                                  type is GhostColors.Red ?  ResourcesLoader.RedGhost : 
                                                                             ResourcesLoader.OrangeGhost);
            this.CeilObject = new Image() { Tag = new Tags.GhostTag(this),
                                            Source = this.Image};
            MainWindow.INSTANCE.game_grid.Children.Add(this.CeilObject);
        }

        public bool ShouldTick(GhostTickTypes tickType)
        {

            if (this.IsDied && tickType is GhostTickTypes.Died)
                return true;
            else if (Pacman.INSTANCE.IsDrugged && tickType is GhostTickTypes.Scaried)
                return true;
            else if (!this.IsDied && !Pacman.INSTANCE.IsDrugged && tickType is GhostTickTypes.Alive)
                return true;
            return false;
        }

        public void SetSchema(BaseGhostMover mover, System.Drawing.Point spawnPoint)
        {
            this.mover = mover;
            this.mover.Loaded();
            this.IsDied = false;
            this.InGate = true;
            this.Initialized = false;
            this.NeedToGoToSpawn = false;
            this.SpawnPoint = spawnPoint;
            this.CeilObject.Source = this.Image;
            this.CeilObject.RenderTransform = null;
            Grid.SetZIndex(this.CeilObject, 3);

            Grid.SetColumn(this.CeilObject, spawnPoint.X);
            Grid.SetRow(this.CeilObject, spawnPoint.Y);
        }

        public void Kill()
        {
            var controller = SoundEffectsPlayer.Play(SoundEffectsPlayer.EAT_GHOST);

            if (this.IsDied is false)
                controller.OnDone(() => SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.GHOST_GO_BACK, () => this.IsDied));


            this.IsDied = true;
            this.CeilObject.Source = ResourcesLoader.GhostEyes;
            Grid.SetZIndex(this.CeilObject, 1);
        }

#if ANIMATION_DEBUG
        private bool AnimationDebugLock = false;
#endif
        public void Tick(ref bool PacmanHitted)
        {
#if ANIMATION_DEBUG
            if (AnimationDebugLock)
                return;
#endif
            Debug.Assert(this.mover is not null);
            System.Drawing.Point position = this.EffectivePosition;

            if (this.InGate)
            {
                Debug.Assert(WorldLoader.CurrentWorld is not null);
                var pos = new System.Drawing.Point(Grid.GetColumn(this.CeilObject), Grid.GetRow(this.CeilObject));
                if (WorldLoader.CurrentWorld.SpawnGate == pos)
                {
                    Grid.SetRow(this.CeilObject, pos.Y - 1);
                    this.InGate = false;
                }
                else if(WorldLoader.CurrentWorld.SpawnGate.Y + 1 == pos.Y && WorldLoader.CurrentWorld.SpawnGate.X == pos.X)
                    Grid.SetRow(this.CeilObject, pos.Y - 1);
                else if(WorldLoader.CurrentWorld.SpawnGate.X - 1 == pos.X && WorldLoader.CurrentWorld.SpawnGate.Y + 1 == pos.Y)
                    Grid.SetColumn(this.CeilObject, pos.X + 1);
                else if(WorldLoader.CurrentWorld.SpawnGate.X + 1 == pos.X && WorldLoader.CurrentWorld.SpawnGate.Y + 1 == pos.Y)
                    Grid.SetColumn(this.CeilObject, pos.X - 1);
                else
                    Debug.Assert(false);

                this.HandleAnimation(position, this.EffectivePosition);
                this.HandleCollisions(ref PacmanHitted);
                return;
            }

            if (!this.IsDied && !Pacman.INSTANCE.IsDrugged && !ReferenceEquals(this.CeilObject.Source, this.Image))
            {
                Grid.SetZIndex(this.CeilObject, 3);
                this.CeilObject.Source = this.Image;
            }else if (Pacman.INSTANCE.IsDrugged && !this.IsDied && !ReferenceEquals(this.CeilObject.Source, ResourcesLoader.ScaryGhost))
            {
                Grid.SetZIndex(this.CeilObject, 1);
                this.CeilObject.Source = ResourcesLoader.ScaryGhost;
            }


            if (mover.NextFrame())
            {
                Grid.SetColumn(this.CeilObject, this.Position.X);
                Grid.SetRow(this.CeilObject, this.Position.Y);
            }
            else if (this.mover.GetStartPoint() == this.EffectivePosition)
            {
                this.Initialized = true;
                this.NeedToGoToSpawn = false;
            }
            else if (this.IsDied is true && this.SpawnPoint == this.EffectivePosition)
            {
                this.IsDied = false;
                this.NeedToGoToSpawn = true;
            }

            else if (this.CeilObject.RenderTransform is not null)
                this.CeilObject.RenderTransform = null;

            this.HandleAnimation(position, this.EffectivePosition);
            this.HandleCollisions(ref PacmanHitted);
        }

        public void HandleAnimation(System.Drawing.Point from, System.Drawing.Point to)
        {
            if (this.IsDied)
                return;

            TimeSpan duration = new TimeSpan(TimeSpan.TicksPerSecond / 12 * (!Pacman.INSTANCE.IsDrugged ?  3 : 5));
            if (from.X - to.X == 1 && from.Y == to.Y) // <-
            {
                TranslateTransform trans = new TranslateTransform();
                this.CeilObject.RenderTransform = trans;
                DoubleAnimation anim2 = new DoubleAnimation(this.CeilObject.ActualWidth, 0, duration);
#if ANIMATION_DEBUG
                    this.AnimationDebugLock = true;
                    anim2.Completed += (s, e) => { this.AnimationDebugLock = false; };
#endif
                trans.BeginAnimation(TranslateTransform.XProperty, anim2);
            }
            else if (from.X - to.X == -1 && from.Y == to.Y) // ->
            {
                TranslateTransform trans = new TranslateTransform();
                this.CeilObject.RenderTransform = trans;
                DoubleAnimation anim2 = new DoubleAnimation(-this.CeilObject.ActualWidth, 0, duration);
#if ANIMATION_DEBUG
                    this.AnimationDebugLock = true;
                    anim2.Completed += (s, e) => { this.AnimationDebugLock = false; };
#endif
                trans.BeginAnimation(TranslateTransform.XProperty, anim2);
            }
            else if (from.Y - to.Y == 1 && from.X == to.X) // Up
            {
                TranslateTransform trans = new TranslateTransform();
                this.CeilObject.RenderTransform = trans;
                DoubleAnimation anim2 = new DoubleAnimation(this.CeilObject.ActualHeight, 0, duration);
#if ANIMATION_DEBUG
                    this.AnimationDebugLock = true;
                    anim2.Completed += (s, e) => { this.AnimationDebugLock = false; };
#endif
                trans.BeginAnimation(TranslateTransform.YProperty, anim2);
            }
            else if (from.Y - to.Y == -1 && from.X == to.X) // Down
            {
                TranslateTransform trans = new TranslateTransform();
                this.CeilObject.RenderTransform = trans;
                DoubleAnimation anim2 = new DoubleAnimation(-this.CeilObject.ActualHeight, 0, duration);
#if ANIMATION_DEBUG
                    this.AnimationDebugLock = true;
                    anim2.Completed += (s, e) => { this.AnimationDebugLock = false; };
#endif
                trans.BeginAnimation(TranslateTransform.YProperty, anim2);
            }
        }

        private void HandleCollisions(ref bool PacmanHitted)
        {
            System.Drawing.Point collision_ceil = this.EffectivePosition;

            if (!this.IsDied && !Pacman.INSTANCE.IsDrugged && Pacman.INSTANCE.IsAt(collision_ceil))
            {
                PacmanHitted = true;
                PacmanGame.INSTANCE.Lifes--;
            }
            else if (!this.IsDied && Pacman.INSTANCE.IsDrugged && Pacman.INSTANCE.IsAt(collision_ceil))
                this.Kill();
        }

        public void Respawn()
        {
            Debug.Assert(this.mover is not null);
            this.SetSchema(this.mover, this.SpawnPoint);
        }
    }
}
