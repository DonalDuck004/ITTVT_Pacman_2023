using System.Collections.Immutable;
using System.Diagnostics;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.Points;

namespace PacManWPF_Canvas.Worlds
{
    public class GameMapTransactionFactory
    {
        private GameEntitiesStatus Entities { get; init; }
        private GameMap Map { get; init; }
        private IReadOnlyDictionary<IntPoint, ImmutableArray<ISpawnPointMapEntry>> SpawnPoints { get; init; }

        public GameMapTransactionFactory(GameMap Map)
        {
            this.Map = Map;
            var tmp = this.Map.MapState.ToDictionary(k => k.Key, v => v.Value.OfType<ISpawnPointMapEntry>().ToImmutableArray()).Where(x => x.Value.Length != 0);

            this.SpawnPoints = tmp.ToImmutableDictionary();
            this.Entities = new GameEntitiesStatus(Map.Entries.OfType<PacmanMapEntry>().First(), Map.Entries.OfType<GhostMapEntry>().ToList());
        }

        public GameMapTransaction build(KeyboardStatus status)
        {
            var tmp = this.Map.MapState.ToDictionary(k => k.Key, v => v.Value.OfType<ISpawnPointMapEntry>().ToImmutableArray()).Where(x => x.Value.Length != 0).ToImmutableDictionary();
            this.ValidateMapState();

            return new GameMapTransaction(Map, status, Entities, SpawnPoints);
        }


        [Conditional("DEBUG")]
        private void ValidateMapState()
        {
            var got = false;
            foreach (var entry in Map.Entries)
            {
                if (entry is PacmanMapEntry)
                {
                    if (got) throw new Exception("Multiple Pacman entries found.");
                    got = true;
                }
            }

            if (!got) throw new Exception("No Pacman entry found.");
        }

    }
}