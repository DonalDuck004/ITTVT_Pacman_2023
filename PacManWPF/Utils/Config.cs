﻿using System;

namespace PacManWPF.Utils
{
    class Config
    {
        public const string WORLD_DIR = "worlds";

        public const int CHUNK_WC = 33;
        public const int CHUNK_HC = 15;

        public const int PACMAN_MOVE_DIV = 5;
        public const int PACMAN_PP_MOVE_DIV = 15;

        public const long PACMAN_MOVE_DIV_TICKS = TimeSpan.TicksPerMillisecond * PACMAN_MOVE_DIV;
        public const long PACMAN_PP_MOVE_DIV_TICKS = TimeSpan.TicksPerMillisecond * PACMAN_PP_MOVE_DIV;

        public const long GAME_TICK = TimeSpan.TicksPerSecond / 14;

        private const double CHUNK_WCD = CHUNK_WC;
        private const double CHUNK_HCD = CHUNK_HC + 1;
        // 16 Because there is the gamebar

        public const int DRUG_TICKS = 30;

        public static double[][] Sizes = { new[] { CHUNK_WCD * 32, CHUNK_HCD * 32 },  // 32
                                        new[] { CHUNK_WCD * 36, CHUNK_HCD * 36 },  // 36
                                        new[] { CHUNK_WCD * 40, CHUNK_HCD * 40 },  // 40
                                        new[] { CHUNK_WCD * 44, CHUNK_HCD * 44 },  // 44
                                        new[] { CHUNK_WCD * 48, CHUNK_HCD * 48 },  // 48
                                        new[] { CHUNK_WCD * 52, CHUNK_HCD * 52 },  // 52
                                        new[] { CHUNK_WCD * 56, CHUNK_HCD * 56 },  // 56
                                        new[] { CHUNK_WCD * 64, CHUNK_HCD * 64 },  // 64
                                        new[] { CHUNK_WCD * 68, CHUNK_HCD * 68 },  // 68
                                        new[] { CHUNK_WCD * 72, CHUNK_HCD * 72 },  // 72
                                        new[] { CHUNK_WCD * 76, CHUNK_HCD * 76 },  // 76
                                        new[] { CHUNK_WCD * 80, CHUNK_HCD * 80 },  // 80
                                        new[] { CHUNK_WCD * 84, CHUNK_HCD * 84 },  // 84
                                        new[] { CHUNK_WCD * 88, CHUNK_HCD * 88 },  // 88
                                        new[] { CHUNK_WCD * 92, CHUNK_HCD * 92 },  // 92
                                        new[] { CHUNK_WCD * 96, CHUNK_HCD * 96 },  // 96
                                        new[] { CHUNK_WCD * 100, CHUNK_HCD * 100 },  // 100
                                        new[] { CHUNK_WCD * 104, CHUNK_HCD * 104 },  // 104
                                        new[] { CHUNK_WCD * 108, CHUNK_HCD * 108 },  // 108
                                        new[] { CHUNK_WCD * 112, CHUNK_HCD * 112 },  // 112
                                        new[] { CHUNK_WCD * 116, CHUNK_HCD * 116 },  // 116
                                        new[] { CHUNK_WCD * 120, CHUNK_HCD * 120 },  // 120
                                        new[] { CHUNK_WCD * 124, CHUNK_HCD * 124 },  // 124
                                        new[] { CHUNK_WCD * 128, CHUNK_HCD * 128 },  // 128
        };
    }
}
