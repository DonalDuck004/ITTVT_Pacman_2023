using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Media;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Characters.Effects
{
    public abstract class PacmanEffect
    {
        public abstract bool CanEatGhosts { get; }
        protected PacmanMapEntry Pacman;
        public virtual bool IsActive { get; }

        public abstract double PatchSpeed();
        public abstract void Transform(TransformGroup transfrom_group);
        public abstract void Tick(GameMapTransaction transaction, TransformGroup transfrom_group);

        public PacmanEffect(PacmanMapEntry pacman)
        {
            Pacman = pacman;
        }

        public abstract void CheckTrigger(GameMapTransaction transaction, ReadOnlyCollection<IMapEntry> entries);
    }
}