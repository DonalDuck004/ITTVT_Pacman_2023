using PacManWPF.Utils;
using System.Collections.Generic;
using System.Windows.Automation;

namespace PacManWPF.Game.Tags
{
    class WallTag : BaseTag
    {
        private static Dictionary<Walls, WallTag> _cache = new Dictionary<Walls, WallTag>();

        public Walls walls { get; init; }

        private WallTag(Walls walls)
        {
            this.IsAWall = true;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.walls = walls;
            this.IsGate = false;
            this.IsUnspawnable = false;
        }

        public static WallTag New(Walls walls)
        {
            _cache.TryGetValue(walls, out var wt);
            if (wt is not null)
                return wt;

            return _cache[walls] = new(walls);
        }
    }
}
