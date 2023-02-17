using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PacManWPF.Game.Worlds;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Utils;
using PacManWPF.Game;
using System.Threading;

namespace PacManWPF
{


    public partial class UIWindow : Window
    {
        private static UIWindow? _INSTANCE = null;
        // todo public static List<Thread>
        private Thread? KeyListener = null;

        public static UIWindow INSTANCE => _INSTANCE ?? throw new Exception("No instance was found");

        public static GhostTickTypes[] Seq = { GhostTickTypes.Alive, 
                                               GhostTickTypes.Died,
                                               GhostTickTypes.Scaried,
                                               GhostTickTypes.Alive, 
                                               GhostTickTypes.Died, 
                                               GhostTickTypes.Died
        };

        public int total_points;

        private DateTime last_call = DateTime.Now;

        private DispatcherTimer game_ticker = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = new TimeSpan(Config.GAME_TICK)
        };

        private int tick_seq = 0;
        

        public UIWindow()
        {
            UIWindow._INSTANCE = this;
            InitializeComponent();
            this.game_ticker.Tick += new EventHandler(OnGameTick);

            if (RuntimeSettingsHandler.MaximizedStartup)
                this.WindowState = WindowState.Maximized;
        }

        public void MovementListener()
        {
            Key? key;
            int dest_x, dest_y, angular;
            try
            {
                while (true) {// (this._stop_required)

                    key = this.Dispatcher.Invoke<Key?>(() => Keyboard.IsKeyDown(Key.Right) ? Key.Right : Keyboard.IsKeyDown(Key.Left) ? Key.Left : Keyboard.IsKeyDown(Key.Up) ? Key.Up : Keyboard.IsKeyDown(Key.Down) ? Key.Down : null, (DispatcherPriority)5);


                if (!PacmanGame.INSTANCE.Initizialized || PacmanGame.INSTANCE.Frozen || key is null)
                    continue;

                if (DateTime.Now - this.last_call < new TimeSpan(TimeSpan.TicksPerSecond / (Pacman.INSTANCE.IsDrugged ? Config.PACMAN_PP_MOVE_DIV : Config.PACMAN_MOVE_DIV)))
                    continue;

                dest_x = Pacman.INSTANCE.X;
                dest_y = Pacman.INSTANCE.Y;

                if (key is Key.Right)
                {
                    dest_x++;
                    angular = 0;
                }
                else if (key is Key.Left)
                {
                    dest_x--;
                    angular = 180;
                }
                else if (key is Key.Up)
                {
                    dest_y--;
                    angular = 270;
                }
                else // if (key is Key.Down)
                {
                    dest_y++;
                    angular = 90;
                }

                this.Dispatcher.Invoke(() =>
                {
                    bool PacmanHitted = false;
                    if (!Pacman.INSTANCE.MoveTo(dest_x, dest_y, angular, ref PacmanHitted))
                        return;

                    this.last_call = DateTime.Now;

                    if (PacmanGame.INSTANCE.GameOver)
                        this.GameOver();
                    else if (PacmanHitted)
                        PacmanGame.INSTANCE.Respawn();
                }, DispatcherPriority.Render);

                Thread.Yield();
            }                }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                return;
            }
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
            if (PacmanGame.INSTANCE.Frozen || !PacmanGame.INSTANCE.Initizialized)
                return;


            if (!this.game_ticker.IsEnabled)
                return;

            bool was_drugged = Pacman.INSTANCE.IsDrugged;


            PacmanGame.INSTANCE.Tick(Seq[++tick_seq % Seq.Length]);


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
            if (this.KeyListener is null)
            {
                this.KeyListener = new Thread(MovementListener);
                this.KeyListener.Start();
            }
            this.CloseMenu();
            GC.Collect(2, GCCollectionMode.Aggressive, true, true);
        }
    }
}
