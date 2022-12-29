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

namespace PacManWPF.Game.PGs
{

    public class Ghost : Abs.BasePG
    {
        // public ImageBrush? reset_ceil = null;
        public ImageBrush Image;

        public GhostColors Type { get; private set; }
        private BaseGhostMover? mover = null;

        private bool initialized = false;

        public static Ghost[] INSTANCES = new Ghost[4];
        public static Ghost CyanGhost { get => Ghost.INSTANCES[0]; }
        public static Ghost PinkGhost { get => Ghost.INSTANCES[1]; }
        public static Ghost RedGhost { get => Ghost.INSTANCES[2]; }
        public static Ghost OrangeGhost { get => Ghost.INSTANCES[3]; }

        public Rectangle CeilObject { get; private set; }

        private int RespawnTicks = 0;

        public override System.Drawing.Point Position
        {
            get
            {
                if (!initialized) throw new Exception($"SetSchema for ghost {Type} not called!");

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
            this.CeilObject = new Rectangle();
            this.CeilObject.Fill = this.Image;
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

        public void SetSchema(BaseGhostMover mover)
        {
            initialized = true;
            this.mover = mover;
            this.RespawnTicks = 0;
            this.IsDied = false;
            Grid.SetZIndex(this.CeilObject, 3);
        }

        public void Kill()
        {
            if (this.IsDied is false)
                this.RespawnTicks = 50;

            IsDied = true;

            this.CeilObject.Fill = ResourcesLoader.GhostEyes;
            Grid.SetZIndex(this.CeilObject, 1);
        }

       
        public void Tick()
        {
            if (!initialized)
                throw new Exception($"SetSchema for ghost {Type} not called!");

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

            mover.NextFrame(this);

            var tmp = this.Position;
            Grid.SetColumn(this.CeilObject, tmp.X);
            Grid.SetRow(this.CeilObject, tmp.Y);

            if (!this.IsDied && !Pacman.INSTANCE.IsDrugged && Pacman.INSTANCE.IsAt(tmp))
                PacmanGame.INSTANCE.GameOver = true;
            else if (!this.IsDied && Pacman.INSTANCE.IsDrugged && Pacman.INSTANCE.IsAt(tmp))
                this.Kill();

        }
    }
}
