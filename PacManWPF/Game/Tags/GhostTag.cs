namespace PacManWPF.Game.Tags
{
    class GhostTag : BaseTag
    {
        public PGs.Ghost ghost { get; init; }
        public GhostTag(PGs.Ghost ghost)
        {
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = true;
            this.IsGate = false;
            this.IsPacman = false;
            this.ghost = ghost;
            this.IsUnspawnable = false;
        }
    }
}
