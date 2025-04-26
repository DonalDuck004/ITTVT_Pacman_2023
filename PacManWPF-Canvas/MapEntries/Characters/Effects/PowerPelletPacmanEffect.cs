using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Media;
using PacManWPF.Utils;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.MapEntries.Static;
using PacManWPF_Canvas.Utils;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Characters.Effects
{
    public class PowerPelletPacmanEffect : PacmanEffect
    {
        private long DrugTicks = 0;
        public bool IsDrugged => DrugTicks != 0;
        public override bool IsActive => IsDrugged;
        public override bool CanEatGhosts => IsDrugged;

        private ScaleTransform TGTransform = new ScaleTransform(2, 2);

        public override double PatchSpeed()
        {
            return Config.PACMAN_DRUGGED_MOVE_SPEED;
        }


        public override void Transform(TransformGroup transfrom_group)
        {
            transfrom_group.AddUniqueRef(TGTransform);
        }

        public override void Tick(GameMapTransaction transaction, TransformGroup transfrom_group)
        {
            --DrugTicks;

            if (!IsDrugged)
                transfrom_group.RemoveRef(TGTransform);
        }

        public override void CheckTrigger(GameMapTransaction transaction, ReadOnlyCollection<IMapEntry> entries)
        {
            foreach (var item in entries)
            {
                if (item is FoodMapEntry food && food.Type is FoodTypes.PowerPellet)
                {
                    DrugTicks = Config.DRUG_TICKS;
                    break;
                }
            }
        }

        public PowerPelletPacmanEffect(PacmanMapEntry pacman) : base(pacman)
        {
        }
    }
}