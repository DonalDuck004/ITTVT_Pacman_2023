using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PacManWPF.Game.Worlds;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Utils;
using PacManWPF.Game;
using System.Linq;

namespace PacManWPF
{


    public partial class MainWindow : Window
    {
        private static MainWindow? _INSTANCE = null;

        public static MainWindow INSTANCE => _INSTANCE ?? throw new Exception("No instance was found");


        public int total_points;

        private Semaphore mutex = new Semaphore(1, 1);

        private DateTime last_call = DateTime.Now;

        private DispatcherTimer game_ticker = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = new TimeSpan(TimeSpan.TicksPerSecond / 12),
        };

        private GhostTickTypes tick_seq = GhostTickTypes.Scaried;
        


        public MainWindow()
        {
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

            if (!Pacman.INSTANCE.IsDrugged && DateTime.Now - this.last_call < new TimeSpan(TimeSpan.TicksPerSecond / 12))
                return;

            this.last_call = DateTime.Now;

            try
            {
                this.mutex.WaitOne();
                bool PacmanHitted = false;
                Pacman.INSTANCE.MoveTo(dest_x, dest_y, angular, ref PacmanHitted);

                if (PacmanGame.INSTANCE.GameOver)
                    this.GameOver();
                else if (PacmanHitted)
                    PacmanGame.INSTANCE.Respawn();
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


                if (PacmanGame.INSTANCE.GameOver)
                {
                    this.GameOver();
                    return;
                }

                if (was_drugged)
                    Pacman.INSTANCE.DrugTicks--;

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
            this.points_final_label.Content = PacmanGame.INSTANCE.Points;
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
