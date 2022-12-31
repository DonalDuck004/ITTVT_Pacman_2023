using System;
using System.Windows.Media;

namespace PacManWPF.Utils
{
    public static class SoundEffectsPlayer
    {
        public static string CHOMP = "Sounds/chomp.wav";
        public static string CHOMP_FRUIT = "Sounds/eatfruit.wav";
        public static string GAME_OVER = "Sounds/gameover.wav";
        public static string START = "Sounds/start.wav";
        public static string EAT_GHOST = "Sounds/eatghost.wav";

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
