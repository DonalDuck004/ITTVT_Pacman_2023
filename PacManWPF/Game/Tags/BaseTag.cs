namespace PacManWPF.Game.Tags
{
    class BaseTag
    {
        public bool IsAWall { get; init; }
        public bool IsFood { get; init; }
        public bool IsAGhost { get; init; }
        public bool IsPacman { get; init; }
        public bool IsUnspawnable { get; init; }
        public bool IsGate { get; init; }

        protected BaseTag() { 
        }
    }
}
