using System;

namespace PacManWPF.Utils
{
    class Config
    {
        public static readonly int[] Version = new[] { 4, 0 };

        public const string WORLD_DIR = "worlds";

        public const int CHUNK_WC = 33;
        public const int CHUNK_HC = 15;

        public const int CHUNK_CS = 32;
        public const int EXCPETED_WIDTH = CHUNK_CS * CHUNK_WC;
        public const int EXCPETED_HEIGHT = CHUNK_CS * CHUNK_HC;

        public const int NEW_LIFE_EVERY = 10000;
        public const int GHOST_EAT_POINTS = 500;
        public const int POWER_PELLET_WARN = 15;

        public const double PACMAN_MOVE_SPEED = 0.075;
        public const double PACMAN_DRUGGED_MOVE_SPEED = 0.15;

        public const int DRUG_TICKS = 1_000;
    }
}
