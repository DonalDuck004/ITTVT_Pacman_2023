using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using PacManWPF.Game.Worlds;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Utils;

using Point = System.Drawing.Point;
using System.Drawing.Drawing2D;

namespace PacManWPF
{

    public class PacmanGame : Singleton<PacmanGame>
    {
        public Rectangle[] CeilsAt(Point point) => CeilsAt(point.X, point.Y);
        public Rectangle[] CeilsAt(int x, int y) => MainWindow.INSTANCE.game_grid.Children.OfType<Rectangle>().Where(i => Grid.GetRow(i) == y && Grid.GetColumn(i) == x).ToArray();

        public readonly List<Point> FreeAreas = new();
        public bool Frozen { get; set;} = false;
        public bool GameOver { get; set;} = false;
        public bool Won { get; set;} = false;


        private int _points = 0;
        public int Points
        {
            get
            {
                return _points;
            }
            set
            {
                MainWindow.INSTANCE.points_label.Content = value.ToString().ZFill(3);
                _points = value;
            }
        }


        private PacmanGame()
        {
            Ghost.INSTANCES[0] = new(GhostColors.Cyan);
            Ghost.INSTANCES[1] = new(GhostColors.Pink);
            Ghost.INSTANCES[2] = new(GhostColors.Red);
            Ghost.INSTANCES[3] = new(GhostColors.Orange);
        }

        public void Tick(GhostTickTypes type)
        {
            if (WorldLoader.CurrentWorld is null)
                throw new Exception("No world selected");

            for (int i = 0; i < Ghost.INSTANCES.Length; i++)
                if (Ghost.INSTANCES[i].ShouldTick(type))
                {
                    Ghost.INSTANCES[i].Tick();

                    if (this.GameOver)
                        return;
                }

            if (this.Points == WorldLoader.CurrentWorld.TotalPoints)
                this.Won = true;
        }

        public void InitGame(int pacman_x, int pacman_y, int pacman_grad)
        {
            Pacman.INSTANCE.Initialize(pacman_x, pacman_y, pacman_grad);
            this.FreeAreas.Clear();
            this.Points = 0;
            this.Frozen = false;
            this.GameOver = false;
            this.Won = false;
        }
    }


    public partial class MainWindow : Window
    {
        private static MainWindow? _INSTANCE = null;

        public static MainWindow INSTANCE { get => _INSTANCE ?? throw new Exception("No instance was found"); }


        public int total_points;

        private Semaphore mutex = new Semaphore(1, 1);

        private DateTime start_time;
        
        private DateTime last_call = DateTime.Now;

        private DispatcherTimer game_ticker = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = new TimeSpan(TimeSpan.TicksPerSecond / 8),

        };


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
                if (this.pause_menu.IsSelected)
                {
                    this.ResumeGame();
                    this.game_tab.IsSelected = true;
                }
                else
                {
                    this.FreezeGame();
                    this.app_pages.SelectedIndex = this.pause_menu.TabIndex;
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

        public void ClosePauseMenu(object sender, EventArgs e)
        {
            this.game_tab.IsSelected = true;
            CloseMenu();
        }

        public void CloseMenu()
        {
            if (this.pause_menu.IsSelected)
            {
                foreach (World world in WorldLoader.Worlds)
                    this.worlds_box.Items.Add(world.Name);
            }
            else
            {
                this.worlds_box.Items.Clear();
                this.ResumeGame();
            }

        }

        GhostTickTypes tick_seq = GhostTickTypes.Scaried;

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
                if (was_drugged)
                    Pacman.INSTANCE.DrugTicks--;

                this.tick_seq = (GhostTickTypes)(((int)tick_seq + 1) % 3);
                Debug.WriteLine(this.tick_seq);
                
                PacmanGame.INSTANCE.Tick(this.tick_seq);

                if (PacmanGame.INSTANCE.GameOver)
                {
                    this.GameOver();
                    return;
                }

                if (tick_seq is not GhostTickTypes.Died)
                    Pacman.INSTANCE.Animate();


                if (PacmanGame.INSTANCE.Won)
                    this.Won();

            }
            finally
            {
                this.mutex.Release();
            }
        }

        public void Won()
        {
            var tmp = Pacman.INSTANCE.CeilObject.Fill.RelativeTransform;
            Pacman.INSTANCE.CeilObject.Fill = ResourcesLoader.PacMan;
            Pacman.INSTANCE.CeilObject.Fill.RelativeTransform = tmp;
            this.FreezeGame();

            this.ellapsed_time_label.Content = (new DateTime((DateTime.Now - this.start_time).Ticks)).ToString("HH:mm:ss");
            this.game_won_tab.IsSelected = true;
        }

        public void GameOver()
        {
            this.FreezeGame();
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
            FreezeGame();
            this.world_label.Content = WorldLoader.Worlds[this.worlds_box.SelectedIndex].Name;
            this.game_won_label.Content = this.world_label.Content;
            WorldLoader.Worlds[this.worlds_box.SelectedIndex].Apply(this);
            this.game_tab.IsSelected = true;
            this.CloseMenu();
            this.start_time = DateTime.Now;
            GC.Collect();
            this.mutex.Release();
        }
    }
}
