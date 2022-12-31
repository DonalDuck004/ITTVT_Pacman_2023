using PacManWPF.Game.PGs.Enums;

namespace PacManWPF.Game.Tags
{
    class FoodTag : BaseTag
    {
        public FoodTypes FoodType { get; init; }
        public FoodTag(FoodTypes type)
        {
            this.IsAWall = false;
            this.IsFood = true;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.IsGate = false;
            this.FoodType = type;
        }
    }
}
