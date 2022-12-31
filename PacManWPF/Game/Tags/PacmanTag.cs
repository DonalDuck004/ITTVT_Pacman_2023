namespace PacManWPF.Game.Tags
{
    class PacmanTag : BaseTag
    {
        public PacmanTag() { 
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsGate = false;
            this.IsPacman = true;
        }
    }
}
