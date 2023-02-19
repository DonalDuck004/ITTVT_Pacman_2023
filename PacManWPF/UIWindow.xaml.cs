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
using System.Diagnostics;
using System.Xml.Linq;
using PacManWPF.Game.PGs.Movers.Abs;

namespace PacManWPF
{
    public partial class UIWindow : Window
    {
        private static UIWindow? _INSTANCE = null;
        // todo public static List<Thread>
        internal Thread? KeyListener = null;

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
            BaseGhostMover.Initialize();
            InitializeComponent();
            this.game_ticker.Tick += new EventHandler(OnGameTick);

            if (RuntimeSettingsHandler.MaximizedStartup)
                this.WindowState = WindowState.Maximized;

            this.Content = StartPage.INSTANCE;
        }

        public void SetPage(UserControl page)
        {
            this.Content = page;
            this.UpdateLayout();
            if (page.ActualWidth > this.ActualWidth)
                this.Width = page.ActualWidth;

            if ((page.ActualHeight + 80) > this.ActualHeight)
                this.Height = page.ActualHeight + 80;
        }

        internal Key? last_key = null;
        internal void MovementListener()
        {
            Key? key;
            int dest_x, dest_y, angular;
            TimeSpan wait;
            TimeSpan diff;

            try
            {
                while (true) {// (this._stop_required)
                    wait = new((long)(TimeSpan.TicksPerSecond / (Pacman.INSTANCE.IsDrugged ? Config.PACMAN_PP_MOVE_DIV : Config.PACMAN_MOVE_DIV)));
                    diff = this.last_call + wait - DateTime.Now;
                    if (diff.Ticks > 0)
                        Thread.Sleep(diff);
                    if (!PacmanGame.INSTANCE.Initizialized || PacmanGame.INSTANCE.Frozen)
                        continue;

                    do
                    {
                        key = this.Dispatcher.Invoke<Key?>(() => Keyboard.IsKeyDown(Key.Right) ? Key.Right : Keyboard.IsKeyDown(Key.Left) ? Key.Left : Keyboard.IsKeyDown(Key.Up) ? Key.Up : Keyboard.IsKeyDown(Key.Down) ? Key.Down : null, (DispatcherPriority)5);
                        if (RuntimeSettingsHandler.LegacyMode)
                        {
                            if (last_key is null && key is null)
                                Thread.Sleep(25);
                            else if (key is null)
                            {
                                key = last_key;
                                break;
                            }
                            else
                            {
                                last_key = key;
                                break;
                            }
                        }
                        else
                        {
                            if (key is null)
                                Thread.Sleep(25);
                            else
                                break;
                        }
                    } while (true);


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
                        if (!PacmanGame.INSTANCE.Initizialized || PacmanGame.INSTANCE.GameOver)
                            return;

                        bool PacmanHitted = false;
                        if (!Pacman.INSTANCE.MoveTo(dest_x, dest_y, angular, ref PacmanHitted))
                            return;

                        this.last_call = DateTime.Now;

                        if (PacmanGame.INSTANCE.GameOver)
                            this.GameOver();
                        else if (PacmanHitted)
                        {
                            PacmanGame.INSTANCE.Respawn();
                        }
                    }, DispatcherPriority.Render);

                    Thread.Yield();
                }                
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
            }
        }

        public void DispatchKey(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key is Key.Escape)
            {
                if (this.Content is PausePage CurrentPausePage)
                {
                    CurrentPausePage.Close();
                    return;
                }

                PausePage.Open();
            }

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

            GamePage.Current!.SpawnFood();
        }

        public void Won()
        {
            this.FreezeGame();
            this.SetPage(new WonPage(TimeSpan.FromSeconds(PacmanGame.INSTANCE.Seconds), PacmanGame.INSTANCE.Points));

        }

        public void GameOver()
        {
            if (PacmanGame.INSTANCE.GameOver)
                return;

            Pacman.INSTANCE.DrugTicks = 0;
            Pacman.INSTANCE.UpdateLayout();
            this.FreezeGame();
            SoundEffectsPlayer.Play(SoundEffectsPlayer.GAME_OVER);
            PacmanGame.INSTANCE.GameOver = true;
            this.UpdateLayout();
            MessageBox.Show("Game Over");
        }

        public void FreezeGame()
        {
            if (this.game_ticker.IsEnabled)
                this.game_ticker.Stop();

            PacmanGame.INSTANCE.Frozen = true;
        }


        internal void ResumeGame()
        {
            if (!this.game_ticker.IsEnabled)
                this.game_ticker.Start();

            Debug.Assert(GamePage.Current is not null);
            UIWindow.INSTANCE.SetPage(GamePage.Current!);
            PacmanGame.INSTANCE.Frozen = false;
        }
    }
}
