using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Media;
using PacManWPF.Utils;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.MapEntries.Characters.Effects;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Characters.Effects.Abs
{
    public class BasePacmanEffect : PacmanEffect
    {
        public override bool CanEatGhosts { get; } = false;
        public override bool IsActive => true;

        public override double PatchSpeed()
        {
            return Config.PACMAN_MOVE_SPEED;
        }

        public override void Transform(TransformGroup transfrom_group)
        {
        }

        public override void Tick(GameMapTransaction transaction, TransformGroup transfrom_group)
        {
        }

        public override void CheckTrigger(GameMapTransaction transaction, ReadOnlyCollection<IMapEntry> entries)
        {
        }

        public BasePacmanEffect(PacmanMapEntry pacman) : base(pacman)
        {
        }
    }
}