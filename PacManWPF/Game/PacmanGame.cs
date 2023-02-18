using System;
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
using System.Threading;

namespace PacManWPF.Game
{
    public class PacmanGame : Singleton<PacmanGame>
    {
        public Image[] CeilsAt(Point point) => CeilsAt(point.X, point.Y);
        public Image[] CeilsAt(int x, int y) => GamePage.CurrentGrid!.Children.OfType<Image>().Where(i => Grid.GetRow(i) == y && Grid.GetColumn(i) == x).ToArray();

        private Random rnd = new();

        public readonly List<Point> FreeAreas = new();
        public bool Initizialized { get; private set; } = false;

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

        private int _points = 0;
        public int Points
        {
            get => _points;
            set
            {
                Debug.Assert(GamePage.Current is not null);
                GamePage.Current!.points_label.Content = value.ToString().ZFill(3);
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

                Debug.Assert(GamePage.Current is not null);
                Image[] lifes_wp = GamePage.Current!.lifes_wp.Children.OfType<Image>().ToArray();

                for (int i = 0; i < this._lifes; i++)
                    lifes_wp[i].Visibility = Visibility.Visible;

                for (int i = this._lifes; i < lifes_wp.Length; i++)
                    lifes_wp[i].Visibility = Visibility.Hidden;

            }
        }

        private DispatcherTimer clock;
        public int Seconds { get; private set; } = 0;

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

            clock.Tick += (s, e) =>
            {
                if (!this.Initizialized)
                    return;

                Debug.Assert(GamePage.Current is not null);
                GamePage.Current!.time_label.Content = (new DateTime() + TimeSpan.FromSeconds(++this.Seconds)).ToString("HH:mm:ss");
            };
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
            this.Seconds = 0;
            GamePage.Current!.time_label.Content = "00:00:00";
            SoundEffectsPlayer.StopAll();
            this.Initizialized = false;
            SoundEffectsPlayer.Play(SoundEffectsPlayer.START).OnDone(() => {
                this.Initizialized = true;
                SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.GHOST_SIREN, () => !Pacman.INSTANCE.IsDrugged);
            });

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
            clock.Start();
            Pacman.INSTANCE.Initialize(pacman_x, pacman_y, pacman_grad);
        }

  

        public void Respawn()
        {
            SoundEffectsPlayer.StopAll();
            this.Initizialized = false;
            Pacman.INSTANCE.DrugTicks = 0;
            Pacman.INSTANCE.UpdateLayout();

            SoundEffectsPlayer.Play(SoundEffectsPlayer.GAME_OVER).OnDone(() =>
                {
                    Pacman.INSTANCE.Respawn();
                    for (int i = 0; i < Ghost.INSTANCES.Length; i++)
                        Ghost.INSTANCES[i].Respawn();
                    
                    SoundEffectsPlayer.Play(SoundEffectsPlayer.START).OnDone(() =>
                    {
                        this.Initizialized = true;
                        SoundEffectsPlayer.PlayWhile(SoundEffectsPlayer.GHOST_SIREN, () => !Pacman.INSTANCE.IsDrugged);
                    });
                }
            );


        }
    }
}
