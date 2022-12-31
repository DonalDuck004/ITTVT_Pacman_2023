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

namespace PacManWPF.Game.PGs
{

    public class Ghost : Abs.BasePG
    {
        // public ImageBrush? reset_ceil = null;
        public ImageBrush Image;

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

        public Rectangle CeilObject { get; private set; }
        public System.Drawing.Point SpawnPoint { get; private set; }

        public System.Drawing.Point EffectivePosition => new(Grid.GetColumn(this.CeilObject), Grid.GetRow(this.CeilObject));

        private int RespawnTicks = 0;

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
            this.CeilObject = new Rectangle() { Tag = new Tags.GhostTag(this),
                                                Fill = this.Image};
            MainWindow.INSTANCE.game_grid.Children.Add(this.CeilObject);
        }

        public bool ShouldTick(GhostTickTypes tickType)
        {

            if (IsDied && tickType is GhostTickTypes.Died)
                return true;
            else if (Pacman.INSTANCE.IsDrugged && tickType is GhostTickTypes.Scaried)
                return true;
            else if (!IsDied && !Pacman.INSTANCE.IsDrugged && tickType is GhostTickTypes.Alive)
                return true;
            return false;
        }

        public void SetSchema(BaseGhostMover mover, System.Drawing.Point spawnPoint)
        {
            this.mover = mover;
            this.RespawnTicks = 0;
            this.IsDied = false;
            this.InGate = true;
            this.Initialized = false;
            this.NeedToGoToSpawn = false;
            this.SpawnPoint = spawnPoint;
            this.CeilObject.Fill = this.Image;
            Grid.SetZIndex(this.CeilObject, 3);

            Grid.SetColumn(this.CeilObject, spawnPoint.X);
            Grid.SetRow(this.CeilObject, spawnPoint.Y);
            Debug.WriteLine(this.Position);
        }

        public void Kill()
        {
            if (this.IsDied is false)
                this.RespawnTicks = 50;

            SoundEffectsPlayer.Play(SoundEffectsPlayer.CHOMP_FRUIT);

            this.IsDied = true;
            this.CeilObject.Fill = ResourcesLoader.GhostEyes;
            Grid.SetZIndex(this.CeilObject, 1);
        }

       
        public void Tick()
        {
            Debug.Assert(mover is not null);

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
                HandleCollisions();
                return;
            }


            if (this.IsDied)
            {
                this.RespawnTicks -= 1;
                if (this.RespawnTicks == 0)
                {
                    Grid.SetZIndex(this.CeilObject, 3);
                    this.IsDied = false;
                    this.CeilObject.Fill = this.Image;
                }
            }

            if (!this.IsDied && !Pacman.INSTANCE.IsDrugged && !ReferenceEquals(this.CeilObject.Fill, this.Image))
            {
                Grid.SetZIndex(this.CeilObject, 3);
                this.CeilObject.Fill = this.Image;
            }else if (Pacman.INSTANCE.IsDrugged && !this.IsDied && !ReferenceEquals(this.CeilObject.Fill, ResourcesLoader.ScaryGhost))
            {
                Grid.SetZIndex(this.CeilObject, 1);
                this.CeilObject.Fill = ResourcesLoader.ScaryGhost;
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

            HandleCollisions();
        }

        private void HandleCollisions()
        {
            System.Drawing.Point collision_ceil = this.EffectivePosition;

            if (!this.IsDied && !Pacman.INSTANCE.IsDrugged && Pacman.INSTANCE.IsAt(collision_ceil))
                PacmanGame.INSTANCE.GameOver = true;
            else if (!this.IsDied && Pacman.INSTANCE.IsDrugged && Pacman.INSTANCE.IsAt(collision_ceil))
                this.Kill();
        }
    }
}
