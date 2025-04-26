using System.Windows.Controls;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.Utils;

namespace PacManWPF_Canvas.MapEntries.Static
{
    public class FoodMapEntry : CenteredMapEntry
    {
        public override int ZIndex { get; protected set; } = -1;
        public FoodTypes Type { get; init; }
        protected override int Size { get; } = 32;


        private static FoodTypes GetRandomFoodType()
        {
            var values = (FoodTypes[])Enum.GetValues(typeof(FoodTypes));

            return values[Random.Shared.Next(2, values.Length)];
        }

        public FoodMapEntry(FoodTypes? type = null) : base(new Image() { Source = ResourcesLoader.GetImage(type ??= GetRandomFoodType()) })
        {
            this.Type = type.Value;
        }
    }
}