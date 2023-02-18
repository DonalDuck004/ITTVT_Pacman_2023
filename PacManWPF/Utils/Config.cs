using System;

namespace PacManWPF.Utils
{
    class Config
    {
        public static readonly int[] Version = new[] { 1, 50 };

        public const string WORLD_DIR = "worlds";

        public const int CHUNK_WC = 33;
        public const int CHUNK_HC = 15;

        public const int PACMAN_MOVE_DIV = 5;
        public const int PACMAN_PP_MOVE_DIV = 10;

        public const long PACMAN_MOVE_DIV_TICKS = TimeSpan.TicksPerMillisecond * PACMAN_MOVE_DIV;
        public const long PACMAN_PP_MOVE_DIV_TICKS = TimeSpan.TicksPerMillisecond * PACMAN_PP_MOVE_DIV;

        public const long GAME_TICK = TimeSpan.TicksPerSecond / 14;

        public const int DRUG_TICKS = 30;
    }
}
