namespace PacManWPF.Game.Tags
{
    class EmptyTag : BaseTag
    {
        public static EmptyTag INSTANCE { get; }  = new EmptyTag();

        private EmptyTag()
        {
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.IsGate = false;
        }
    }
}
