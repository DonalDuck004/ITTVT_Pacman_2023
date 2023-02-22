using System.Drawing;

namespace WorldsBuilderWPF.Types
{
    public record class Tag(Tags tag)
    {
        public static Tag PAC_DOT = new(Tags.PacDot);
        public static Tag POWER_PELLET = new(Tags.PowerPellet);
        public static Tag EMPTY = new(Tags.Empty);
        public static Tag GATE = new(Tags.Gate);
        public static Tag UNSPAWNABLE = new(Tags.Unspawnable);
    }

    public record WallTag(Walls wall, Color color) : Tag(Tags.Wall);
}
