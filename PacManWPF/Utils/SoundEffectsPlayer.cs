using System;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Media;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Windows.Threading;

namespace PacManWPF.Utils
{

    class SoundEffectsPlayer
    {
        public static string CHOMP = "Sounds/chomp.wav";
        public static string CHOMP_FRUIT = "Sounds/eatfruit.wav";
        public static string GAME_OVER = "Sounds/gameover.wav";
        public static string START = "Sounds/start.wav";
        public static string EAT_GHOST = "Sounds/eatghost.wav";
        public static string POWER_PELLET = "Sounds/pacman_power_pellet.wav";
        public static string GHOST_GO_BACK = "Sounds/ghost_go_back.wav";
        public static string GHOST_SIREN = "Sounds/ghost_siren.wav";

        internal static List<SoundEffect> INSTANCES = new();
        public static double Volume { get; private set; } = 1; //  Game.RuntimeSettingsHandler.INSTANCE.Volume;

        private static Dictionary<string, SoundEffect> Tracks = new();


        private SoundEffectsPlayer()
        {
        }
        

        public static void PlayNoOverlap(string Track)
        {
            var player = new MediaPlayer();
            player.Volume = SoundEffectsPlayer.Volume;
            var rt = new SoundEffect(player, null, Track);
            SoundEffect? value;

            if (SoundEffectsPlayer.Tracks.TryGetValue(Track, out value))
                value.RequireStop();

            SoundEffectsPlayer.Tracks[Track] = new SoundEffect(player, null, Track); ;
            SoundEffectsPlayer.Tracks[Track].Start();
        }


        public static SoundEffect Play(string Track)
        {

            var player = new MediaPlayer();
            player.Volume = SoundEffectsPlayer.Volume;
            var rt = new SoundEffect(player, null, Track);
            rt.Start();
            SoundEffectsPlayer.INSTANCES.Add(rt);
            return rt;
        }

        public static SoundEffect PlayWhile(string Track, Func<bool> Condition)
        {

            var player = new MediaPlayer();
            player.Volume = SoundEffectsPlayer.Volume;
            var rt = new SoundEffect(player, Condition, Track);
            rt.Start();
            SoundEffectsPlayer.INSTANCES.Add(rt);
            return rt;
        }

        public static void SetVolume(double value)
        {
            SoundEffectsPlayer.Volume = value > 1 ? value / 100 : value;
            
            foreach (var item in SoundEffectsPlayer.INSTANCES)
                item.player.Volume = SoundEffectsPlayer.Volume;

        }


        public static void StopAll()
        {
            foreach (var item in SoundEffectsPlayer.INSTANCES)
                item.RequireStop();
        }


        public static void PauseAll()
        {
            foreach (var item in SoundEffectsPlayer.INSTANCES)
                item.player.Pause();
        }

        public static void ResumeAll()
        {
            foreach (var item in SoundEffectsPlayer.INSTANCES)
                item.player.Play();
        }
    }
}
