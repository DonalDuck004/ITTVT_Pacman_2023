using System;
using System.Threading;
using System.Windows.Media;
using System.Diagnostics;

namespace PacManWPF.Utils
{
    class SoundEffect
    {
        public Func<bool>? Condition { get; private set; }
        public string Track { get; private set; }
        internal MediaPlayer player;
        private bool StopRequired;
        private bool ExtStopRequired = false;
        public bool PlayingTrack { get; private set; }
        private Action? DoneCbk = null;

        public SoundEffect(MediaPlayer player, Func<bool>? Condition, string Track) { 
            this.player = player;
            this.Condition = Condition;
            this.Track = Track;
            this.PlayingTrack = true;

            this.player.MediaEnded += (s, e) => {
                if (!this.StopRequired && !this.ExtStopRequired)
                {
                    this.player.Position = TimeSpan.Zero;
                    this.player.Play();
                }
                else
                {
                    if (this.DoneCbk is not null && !this.ExtStopRequired)
                        this.DoneCbk.Invoke();

                    SoundEffectsPlayer.INSTANCES.Remove(this);
                    this.PlayingTrack = false;
                }
            };

            if (this.Condition is not null)
            {
                this.StopRequired = false;
                new Thread(this.Hook).Start();
            }else
                this.StopRequired = true;


            this.player.Open(new Uri(Track, UriKind.Relative));
        }

        internal void Start()
        {
            this.player.Play();
        }

        private void Hook()
        {
            Debug.Assert(this.Condition is not null);
            while (this.ExtStopRequired || this.Condition.Invoke()) // External stop
                Thread.Sleep(1000);


            if (!this.ExtStopRequired)
                this.StopRequired = true;
            UIWindow.INSTANCE.Dispatcher.Invoke(() => this.player.Position = TimeSpan.FromMinutes(1)); // No 1 track is length more than 1 min
            // Doing this instead of:
            // MainWindow.INSTANCE.Dispatcher.Invoke(this.player.Stop);
            // MediaEnded will be dispatched 
        }

        public void RequireStop()
        {
            this.player.Volume = 0;
            this.ExtStopRequired = true;
            this.player.Stop();
        }

        public void OnDone(Action cbk)
        {
            this.DoneCbk = cbk;
        }
    }
}
