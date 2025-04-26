using System.Collections.Immutable;
using System.Collections.ObjectModel;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Points.Abs;
using PacManWPF_Canvas.Utils;

namespace PacManWPF_Canvas.Worlds
{
    public class GameMapTransaction
    {
        enum GameMapTransactionOP
        {
            Add,
            Drop
        }

        private List<(GameMapTransactionOP op_code, IMapEntry entry)> Operations = new List<(GameMapTransactionOP op_code, IMapEntry entry)>();

        private GameMap Map;
        public KeyboardStatus Status { get; init; }
        public GameEntitiesStatus Entities { get; init; }
        public Direction? SuggestedFallback { get; set; } = null;

        public IReadOnlyDictionary<ReadonlyBasePoint<int>, ReadOnlyCollection<IMapEntry>> MapCeils { get; init; }
        public IReadOnlyDictionary<IntPoint, ImmutableArray<ISpawnPointMapEntry>> SpawnPoints { get; init; }

        internal GameMapTransaction(GameMap map, 
                                    KeyboardStatus status,
                                    GameEntitiesStatus entities,
                                    IReadOnlyDictionary<IntPoint, ImmutableArray<ISpawnPointMapEntry>> spawn_points)
        {
            SpawnPoints = spawn_points;
            Map = map;
            Status = status;
            Entities = entities;

            MapCeils = map.MapState.ToDictionary(k => (ReadonlyBasePoint<int>)k.Key, v => v.Value.AsReadOnly()).AsReadOnly();
            
            var ghosts = Entities.GhostMapEntries;
            var processed = new List<GhostColor>();

            var c = ghosts.Count(x => x.IsInSpawnArea(SpawnPoints));

            foreach (var entry in ghosts)
            {
                entry.Configure(Entities.Pacman, SpawnPoints, ref c);
                
                if (processed.Contains(entry.Color))
                    continue;

                if (entry.MoverHandler.Mover is IMassiveContextRefresh x)
                {
                    var refreshed = x.MassiveRefresh(this, ghosts.Where(x => !processed.Contains(x.Color)).ToImmutableArray());

                    processed.AddRange(refreshed.Select(x => x.Color));
                }
            }

            foreach (var entry in ghosts.Where(x => !processed.Contains(x.Color)))
                entry.RefreshContext(this);
        }

        public ReadOnlyCollection<IMapEntry> At(IntPoint point)
        {
            return MapCeils[point];
        }

        public void Add(IMapEntry entry)
        {
            Operations.Add((GameMapTransactionOP.Add, entry));
        }

        public void Drop(IMapEntry entry)
        {
            Operations.Add((GameMapTransactionOP.Drop, entry));
        }

        public void Flush()
        {
            foreach ((var op_code, var entry) in Operations)
            {
                var k = entry.Point.ToIntFloorPoint().Normalize();
                if (op_code == GameMapTransactionOP.Add)
                {
                    Map.Entries.Add(entry);
                    Map.MapState[k].Add(entry);
                    entry.Init(Map.Canvas);
                }
                else if (op_code == GameMapTransactionOP.Drop)
                {
                    Map.Entries.Remove(entry);
                    Map.MapState[k].Remove(entry);
                    entry.Deinit(Map.Canvas);
                }
                else
                    throw new Exception();
            }

            Operations.Clear();
        }
    }
}