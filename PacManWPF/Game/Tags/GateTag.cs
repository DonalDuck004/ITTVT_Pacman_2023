namespace PacManWPF.Game.Tags
{
    class GateTag : BaseTag
    {
        public GateTag()
        {
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.IsGate = true;
        }
    }
}
