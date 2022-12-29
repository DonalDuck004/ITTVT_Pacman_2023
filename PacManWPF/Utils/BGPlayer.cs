using System;
using System.Windows.Media;

namespace PacManWPF.Utils
{
    public static class SoundEffectsPlayer
    {
        public static string CHOMP = "Sounds/chomp.wav";
        public static string START = "Sounds/start.wav";

        private static MediaPlayer soundPlayer = new MediaPlayer();

        static SoundEffectsPlayer()
        {
            soundPlayer.MediaOpened += OnOpened;
            soundPlayer.Volume = 1;
        }

        private static void OnOpened(object? sender, EventArgs e)
        {
            soundPlayer.Stop();
            soundPlayer.Play();
        }

        public static void Play(string track)
        {
            soundPlayer.Open(new Uri(track, UriKind.Relative));
        }

        public static void Stop()
        {
            soundPlayer.Stop();
        }

        public static void SetVolume(double value)
        {
            soundPlayer.Volume = value / 100;
        }
    }
}
