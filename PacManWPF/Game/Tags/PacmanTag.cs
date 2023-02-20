namespace PacManWPF.Game.Tags
{
    class PacmanTag : BaseTag
    {
        public static PacmanTag INSTANCE { get; } = new PacmanTag();

        private PacmanTag() { 
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsGate = false;
            this.IsPacman = true;
            this.IsUnspawnable = false;
        }
    }
}
