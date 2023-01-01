using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using PacManWPF.Game.Worlds;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Utils;
using PacManWPF.Animations;

using Point = System.Drawing.Point;


namespace PacManWPF
{

    public class PacmanGame : Singleton<PacmanGame>
    {
        public Rectangle[] CeilsAt(Point point) => CeilsAt(point.X, point.Y);
        public Rectangle[] CeilsAt(int x, int y) => MainWindow.INSTANCE.game_grid.Children.OfType<Rectangle>().Where(i => Grid.GetRow(i) == y && Grid.GetColumn(i) == x).ToArray();

        private Random rnd = new();

        public readonly List<Point> FreeAreas = new();

        private bool _frozen;

        public bool Frozen { 
            get => _frozen; 
            set { 
                this._frozen = value; // TODO Stop animations
                if (this._frozen)
                    this.clock.Stop();
                else
                    this.clock.Start();
            } 
        }

        public bool GameOver { get; set;} = false;
        public bool Won { get; set;} = false;
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

        private DispatcherTimer clock;

        private PacmanGame()
        {
            Ghost.INSTANCES[0] = new(GhostColors.Red);
            Ghost.INSTANCES[1] = new(GhostColors.Pink);
            Ghost.INSTANCES[2] = new(GhostColors.Cyan);
            Ghost.INSTANCES[3] = new(GhostColors.Orange);

            this.clock = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            this.clock.Tick += (s, e) => MainWindow.INSTANCE.time_label.Content = new DateTime((DateTime.Now - this.StartDate).Ticks).ToString("HH:mm:ss");
        }

        public void Tick(GhostTickTypes type)
        {
            if (WorldLoader.CurrentWorld is null)
                throw new Exception("No world selected");

            for (int i = 0; i < Ghost.INSTANCES.Length; i++)
            {
                if (Ghost.INSTANCES[i].ShouldTick(type))
                {
                    if (!Ghost.INSTANCES[i].Initialized)
                    {
                        Ghost.INSTANCES[i].Tick();
                        break;
                    }

                    Ghost.INSTANCES[i].Tick();

                    if (this.GameOver)
                        return;
                }
            }


            if (this.PacDots == WorldLoader.CurrentWorld.PacDotCount)
                this.Won = true;
        }

        public void InitGame(int pacman_x, int pacman_y, int pacman_grad)
        {
            Debug.Assert(WorldLoader.CurrentWorld is not null);
            
            this.FreeAreas.Clear();
            this.Points = 0;
            this.PacDots = 0;
            this.Frozen = false;
            this.GameOver = false;
            this.Won = false;
            this.StartDate = DateTime.Now;
            this.clock.Start();

            Pacman.INSTANCE.Initialize(pacman_x, pacman_y, pacman_grad);
        }

        public void SpawnFood()
        {
            if (rnd.Next(100) == 0)
            {
                Debug.Assert(WorldLoader.CurrentWorld is not null);
                var world = WorldLoader.CurrentWorld;
                var values = ((FoodTypes[])Enum.GetValues(typeof(FoodTypes)));
                var ceils = MainWindow.INSTANCE.game_grid.Children.OfType<Rectangle>().Where(x => ((Game.Tags.BaseTag)x.Tag).GetType() == typeof(Game.Tags.EmptyTag)).Where(x => !Pacman.INSTANCE.IsAt(Grid.GetColumn(x), Grid.GetRow(x))).Where(x => !world.IsInSpawnArea(Grid.GetColumn(x), Grid.GetRow(x))).ToArray();
                if (ceils.Length == 0)
                    return;

                var ceil = ceils[rnd.Next(ceils.Length)];
                var food = values[rnd.Next(2, values.Length)];
                ceil.Fill = ResourcesLoader.GetImage(food);
                Guid guid = Guid.NewGuid();
                var animation = new SpecialFoodAnimation();
                animation.Id = guid;
                ceil.Tag = new Game.Tags.FoodTag(food, animation);
               
                animation.Completed += (s, e) => {
                    Debug.WriteLine("Ao");
                    if (ceil.Tag.GetType() != typeof(Game.Tags.FoodTag) || ((Game.Tags.FoodTag)ceil.Tag).animation is null)
                        return;
                   Guid ID = ((Game.Tags.FoodTag)ceil.Tag).animation.Id;

                    if (ID == guid)
                    {
                        ceil.Tag = Game.Tags.EmptyTag.INSTANCE;
                        ceil.Fill = null;
                    }
                };

                ceil.BeginAnimation(Rectangle.OpacityProperty, animation);
            }
        }
    }


    public partial class MainWindow : Window
    {
        private static MainWindow? _INSTANCE = null;

        public static MainWindow INSTANCE { get => _INSTANCE ?? throw new Exception("No instance was found"); }


        public int total_points;

        private Semaphore mutex = new Semaphore(1, 1);
        
        private DateTime last_call = DateTime.Now;

        private DispatcherTimer game_ticker = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = new TimeSpan(TimeSpan.TicksPerSecond / 8),

        };

        private GhostTickTypes tick_seq = GhostTickTypes.Scaried;
        


        public MainWindow()
        {
            if (MainWindow._INSTANCE is null)
                MainWindow._INSTANCE = this;
            InitializeComponent();
            this.AdaptToSize();
            this.game_ticker.Tick += new EventHandler(OnGameTick);
        }
       
        public void DispatchKey(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key is Key.Escape)
            {
                if (this.pause_menu_tab.IsSelected)
                {
                    if (WorldLoader.CurrentWorld is null)
                    {
                        this.start_game_tab.IsSelected = true;
                        return;
                    }

                    this.ResumeGame();
                    this.game_tab.IsSelected = true;
                }
                else
                {
                    this.FreezeGame();
                    this.app_pages.SelectedIndex = this.pause_menu_tab.TabIndex;
                }
                CloseMenu();
                return;
            }

            int dest_x = Pacman.INSTANCE.X;
            int dest_y = Pacman.INSTANCE.Y;
            int angular;

            if (e.Key is Key.Right)
            {
                dest_x++;
                angular = 0;
            }
            else if (e.Key is Key.Left)
            {
                dest_x--;
                angular = 180;
            }
            else if (e.Key is Key.Up)
            {
                dest_y--;
                angular = 270;
            }
            else if (e.Key is Key.Down)
            {
                dest_y++;
                angular = 90;
            }
            else
            {
                e.Handled = false;
                return;
            }

            if (PacmanGame.INSTANCE.Frozen)
                return;

            if (!Pacman.INSTANCE.IsDrugged && DateTime.Now - this.last_call < new TimeSpan(TimeSpan.TicksPerSecond / 10))
                return;

            this.last_call = DateTime.Now;

            try
            {
                this.mutex.WaitOne();
                Pacman.INSTANCE.MoveTo(dest_x, dest_y, angular);
                if (PacmanGame.INSTANCE.GameOver)
                    this.GameOver();
            }
            finally
            {
                this.mutex.Release();
            }
            
        }

        public void CloseMenu()
        {
            if (this.pause_menu_tab.IsSelected)
            {
                this.worlds_box.Items.Clear();
                this.FillWorldsBox();
            }
            else
                this.ResumeGame();

        }


        private void OnGameTick(object? sender, EventArgs e)
        {
            if (PacmanGame.INSTANCE.Frozen)
                return;


            this.mutex.WaitOne();
            if (!this.game_ticker.IsEnabled)
                return;

            try
            {
                bool was_drugged = Pacman.INSTANCE.IsDrugged;


                this.tick_seq = (GhostTickTypes)(((int)tick_seq + 1) % 3);
                
                PacmanGame.INSTANCE.Tick(this.tick_seq);
                if (was_drugged)
                    Pacman.INSTANCE.DrugTicks--;

                if (PacmanGame.INSTANCE.GameOver)
                {
                    this.GameOver();
                    return;
                }


                if (PacmanGame.INSTANCE.Won)
                    this.Won();

                PacmanGame.INSTANCE.SpawnFood();

            }
            finally
            {
                this.mutex.Release();
            }
        }

        public void Won()
        {
            this.FreezeGame();

            this.ellapsed_time_label.Content = new DateTime((DateTime.Now - PacmanGame.INSTANCE.StartDate).Ticks).ToString("HH:mm:ss");
            this.game_won_tab.IsSelected = true;
        }

        public void GameOver()
        {
            this.FreezeGame();
            SoundEffectsPlayer.Play(SoundEffectsPlayer.GAME_OVER);
            PacmanGame.INSTANCE.GameOver = true;
            this.UpdateLayout();
            MessageBox.Show("Game Over");
        }

        private void FreezeGame()
        {
            if (this.game_ticker.IsEnabled)
                this.game_ticker.Stop();

            PacmanGame.INSTANCE.Frozen = true;
        }


        private void ResumeGame()
        {
            if (!this.game_ticker.IsEnabled)
                this.game_ticker.Start();

            PacmanGame.INSTANCE.Frozen = false;
        }


        private void OnWorldSelected(object sender, SelectionChangedEventArgs e)
        {
            if (this.worlds_box.SelectedIndex == -1) 
                return;

            this.mutex.WaitOne();
            this.FreezeGame();
            this.world_label.Content = WorldLoader.Worlds[this.worlds_box.SelectedIndex].Name;
            this.game_won_label.Content = this.world_label.Content;
            WorldLoader.Worlds[this.worlds_box.SelectedIndex].Apply();
            this.game_tab.IsSelected = true;
            this.CloseMenu();
            GC.Collect();
            this.mutex.Release();
        }

    }
}
