using PacManWPF.Utils;

namespace PacManWPF.Game.Tags
{
    class WallTag : BaseTag
    {
        public Walls walls { get; init; }
        public WallTag(Walls walls)
        {
            this.IsAWall = true;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.walls = walls;
            this.IsGate = false;
        }
    }
}
