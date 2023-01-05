using PacManWPF.Animations;
using PacManWPF.Game.PGs.Enums;
using System;

namespace PacManWPF.Game.Tags
{
    class FoodTag : BaseTag
    {
        public FoodTypes FoodType { get; init; }
        public SpecialFoodAnimation? animation { get; set; }
        public Guid? guid { get; set; }

        public FoodTag(FoodTypes type, SpecialFoodAnimation? animation = null, Guid? guid = null)
        {
            this.IsAWall = false;
            this.IsFood = true;
            this.IsAGhost = false;
            this.IsPacman = false;
            this.IsGate = false;
            this.FoodType = type;
            this.animation = animation;
            this.guid = guid;
        }
    }
}
