namespace PacManWPF.Game.Tags
{
    class UnspawnableTag : BaseTag
    {
        public static UnspawnableTag INSTANCE { get; } = new UnspawnableTag();

        private UnspawnableTag()
        {
            this.IsAWall = false;
            this.IsFood = false;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.IsGate = false;
            this.IsUnspawnable = true;
        }
    }
}
