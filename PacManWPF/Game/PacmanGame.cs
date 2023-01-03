﻿using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using PacManWPF.Game.Worlds;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Utils;
using PacManWPF.Animations;

using Point = System.Drawing.Point;


namespace PacManWPF.Game
{
    public class PacmanGame : Singleton<PacmanGame>
    {
        public Rectangle[] CeilsAt(Point point) => CeilsAt(point.X, point.Y);
        public Rectangle[] CeilsAt(int x, int y) => MainWindow.INSTANCE.game_grid.Children.OfType<Rectangle>().Where(i => Grid.GetRow(i) == y && Grid.GetColumn(i) == x).ToArray();

        private Random rnd = new();

        public readonly List<Point> FreeAreas = new();

        private bool _frozen;

        public bool Frozen
        {
            get => this._frozen;
            set
            {
                this._frozen = value; // TODO Stop animations
                if (this._frozen)
                {
                    this.clock.Stop();
                    SoundEffectsPlayer.PauseAll();
                }
                else
                {
                    this.clock.Start();
                    SoundEffectsPlayer.ResumeAll();
                }
            }
        }

        public bool GameOver { get; set; } = false;
        public bool Won { get; set; } = false;
        public int PacDots { get; set; } = 0;
        public DateTime StartDate { get; set; }

        private int _points = 0;
        public int Points
        {
            get => _points;
            set
            {
                MainWindow.INSTANCE.points_label.Content = value.ToString().ZFill(3);
                _points = value;
            }
        }

        private int _lifes = 3;

        public int Lifes
        {
            get => _lifes;
            set
            {
                if (value <= 0)
                {
                    this.GameOver = true;
                    this._lifes = 0;
                }
                else
                    this._lifes = value;

                Image[] lifes_wp = MainWindow.INSTANCE.lifes_wp.Children.OfType<Image>().ToArray();

                for (int i = 0; i < this._lifes; i++)
                    lifes_wp[i].Visibility = Visibility.Visible;

                for (int i = this._lifes; i < lifes_wp.Length; i++)
                    lifes_wp[i].Visibility = Visibility.Hidden;

            }
        }

        private DispatcherTimer clock;

        private PacmanGame()
        {
            Ghost.INSTANCES[0] = new(GhostColors.Red);
            Ghost.INSTANCES[1] = new(GhostColors.Pink);
            Ghost.INSTANCES[2] = new(GhostColors.Cyan);
            Ghost.INSTANCES[3] = new(GhostColors.Orange);

            clock = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            clock.Tick += (s, e) => MainWindow.INSTANCE.time_label.Content = new DateTime((DateTime.Now - StartDate).Ticks).ToString("HH:mm:ss");
        }

        private List<Animations.Abs.IInterruptable> PendingAnimations = new();

        public void AddPendingAnimation(Animations.Abs.IInterruptable animation) => PendingAnimations.Add(animation);

        public void Tick(GhostTickTypes type)
        {
            if (WorldLoader.CurrentWorld is null)
                throw new Exception("No world selected");

            if (this.PacDots == WorldLoader.CurrentWorld.PacDotCount)
            {
                this.Won = true;
                return;
            }

            bool PacmanHitted = false;

            for (int i = 0; i < Ghost.INSTANCES.Length; i++)
            {
                if (Ghost.INSTANCES[i].ShouldTick(type))
                {
                    Ghost.INSTANCES[i].Tick(ref PacmanHitted);

                    if (this.GameOver)
                        return;
                    else if (PacmanHitted)
                    {
                        this.Respawn();
                        return;
                    }
                    if (!Ghost.INSTANCES[i].Initialized)
                        break;
                }
            }

        }

        public void InitGame(int pacman_x, int pacman_y, int pacman_grad)
        {
            Debug.Assert(WorldLoader.CurrentWorld is not null);
            SoundEffectsPlayer.StopAll();
            SoundEffectsPlayer.Play(SoundEffectsPlayer.START).OnDone(() => SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.GHOST_SIREN, () => !Pacman.INSTANCE.IsDrugged));
            foreach (var animation in PendingAnimations)
                animation.Interrupt();

            PendingAnimations.Clear();
            this.Lifes = 3;

            FreeAreas.Clear();
            Points = 0;
            PacDots = 0;
            Frozen = false;
            GameOver = false;
            Won = false;
            StartDate = DateTime.Now;
            clock.Start();
            Pacman.INSTANCE.Initialize(pacman_x, pacman_y, pacman_grad);
        }

        public void SpawnFood()
        {
            if (rnd.Next(200) == 0)
            {
                Debug.Assert(WorldLoader.CurrentWorld is not null);
                var world = WorldLoader.CurrentWorld;
                var values = (FoodTypes[])Enum.GetValues(typeof(FoodTypes));
                var ceils = MainWindow.INSTANCE.game_grid.Children.OfType<Rectangle>().Where(x => ((Tags.BaseTag)x.Tag).GetType() == typeof(Tags.EmptyTag)).Where(x => !Pacman.INSTANCE.IsAt(Grid.GetColumn(x), Grid.GetRow(x))).Where(x => !world.IsInSpawnArea(Grid.GetColumn(x), Grid.GetRow(x))).ToArray();
                if (ceils.Length == 0)
                    return;

                var ceil = ceils[rnd.Next(ceils.Length)];
                var food = values[rnd.Next(2, values.Length)];
                ceil.Fill = ResourcesLoader.GetImage(food);
                Guid guid = Guid.NewGuid();
                var animation = new SpecialFoodAnimation();
                animation.Id = guid;
                ceil.Tag = new Tags.FoodTag(food, animation);

                animation.Completed += (s, e) =>
                {

                    if (ceil.Tag.GetType() != typeof(Tags.FoodTag) || ((Tags.FoodTag)ceil.Tag).animation is null)
                        return;
                    Guid ID = ((Tags.FoodTag)ceil.Tag).animation.Id;
                    ceil.Opacity = 1.0;

                    if (ID == guid)
                    {
                        ceil.Tag = Tags.EmptyTag.INSTANCE;
                        ceil.Fill = null;
                    }
                };


                ceil.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }

        public void Respawn()
        {
            SoundEffectsPlayer.StopAll();
            SoundEffectsPlayer.Play(SoundEffectsPlayer.START).OnDone(() => SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.GHOST_SIREN, () => !Pacman.INSTANCE.IsDrugged));
            Pacman.INSTANCE.Respawn();
            for (int i = 0; i < Ghost.INSTANCES.Length; i++)
                Ghost.INSTANCES[i].Respawn();
        }
    }
}