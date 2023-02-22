using System;

namespace PacManWPF.Utils
{
    class Config
    {
        public static readonly int[] Version = new[] { 1, 90 };

        public const string WORLD_DIR = "worlds";

        public const int CHUNK_WC = 33;
        public const int CHUNK_HC = 15;

        public const int NEW_LIFE_EVERY = 10000;
        public const int GHOST_EAT_POINTS = 500;
        public const int POWER_PELLET_WARN = 15;

        public const double PACMAN_MOVE_DIV = 4.25;
        public const double PACMAN_PP_MOVE_DIV = 8.75;

        public const long PACMAN_MOVE_DIV_TICKS = (long)(TimeSpan.TicksPerMillisecond * PACMAN_MOVE_DIV);
        public const long PACMAN_PP_MOVE_DIV_TICKS = (long)(TimeSpan.TicksPerMillisecond * PACMAN_PP_MOVE_DIV);

        public const long GAME_TICK = TimeSpan.TicksPerSecond / 14;

        public const int DRUG_TICKS = 30;
    }
}
