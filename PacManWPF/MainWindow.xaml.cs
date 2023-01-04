﻿using System;
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

namespace PacManWPF
{


    public partial class MainWindow : Window
    {
        private static MainWindow? _INSTANCE = null;

        public static MainWindow INSTANCE => _INSTANCE ?? throw new Exception("No instance was found");


        public int total_points;

        private DateTime last_call = DateTime.Now;

        private DispatcherTimer game_ticker = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = new (TimeSpan.TicksPerSecond / 12),
        };

        private int tick_seq = 0;
        

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

            if (!PacmanGame.INSTANCE.Initizialized)
                return;

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

            bool PacmanHitted = false;
            Pacman.INSTANCE.MoveTo(dest_x, dest_y, angular, ref PacmanHitted);

            if (PacmanGame.INSTANCE.GameOver)
                this.GameOver();
            else if (PacmanHitted)
                PacmanGame.INSTANCE.Respawn();
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

        GhostTickTypes[] seq = { GhostTickTypes.Alive, GhostTickTypes.Died, GhostTickTypes.Scaried, GhostTickTypes.Alive, GhostTickTypes.Died, GhostTickTypes.Died };
        private void OnGameTick(object? sender, EventArgs e)
        {
            if (PacmanGame.INSTANCE.Frozen || !PacmanGame.INSTANCE.Initizialized)
                return;


            if (!this.game_ticker.IsEnabled)
                return;

            bool was_drugged = Pacman.INSTANCE.IsDrugged;


            PacmanGame.INSTANCE.Tick(seq[++tick_seq % seq.Length]);


            if (PacmanGame.INSTANCE.GameOver)
            {
                this.GameOver();
                return;
            }

            if (was_drugged && --Pacman.INSTANCE.DrugTicks == 0)
                Pacman.INSTANCE.UpdateLayout();

            if (PacmanGame.INSTANCE.Won)
                this.Won();

            PacmanGame.INSTANCE.SpawnFood();

        }

        public void Won()
        {
            this.FreezeGame();

            this.ellapsed_time_label.Content = (new DateTime() + TimeSpan.FromSeconds(PacmanGame.INSTANCE.Seconds)).ToString("HH:mm:ss");
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

            this.FreezeGame();
            this.world_label.Content = WorldLoader.Worlds[this.worlds_box.SelectedIndex].Name;
            this.game_won_label.Content = this.world_label.Content;
            WorldLoader.Worlds[this.worlds_box.SelectedIndex].Apply();
            this.game_tab.IsSelected = true;
            this.CloseMenu();
            GC.Collect(2, GCCollectionMode.Aggressive, true, true);
        }

    }
}
