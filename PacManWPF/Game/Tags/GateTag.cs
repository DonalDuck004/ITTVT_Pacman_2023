namespace PacManWPF.Game.Tags
{
    class GateTag : BaseTag
    {
        public static GateTag INSTANCE { get; } = new GateTag();
        private GateTag()
        {
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.IsGate = true;
        }
    }
}
