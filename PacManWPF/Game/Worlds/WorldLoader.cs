using System.IO;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PacManWPF.Utils;
using System.Runtime.CompilerServices;

namespace PacManWPF.Game.Worlds
{

    static class WorldLoader
    {
        private static List<World>? _cache = null;
        public static World? CurrentWorld { internal set; get; } = null;
        public static List<World> Worlds
        {
            get
            {
                if (_cache is null)
                {
                    _cache = new List<World>();

                    foreach (string file in Directory.GetFiles(Config.WORLD_DIR))
                        _cache.Add(new World(file));

                }

                return _cache;
            }
        }

        public static void DropCache()
        {
            _cache = null;
        }
    }
}
