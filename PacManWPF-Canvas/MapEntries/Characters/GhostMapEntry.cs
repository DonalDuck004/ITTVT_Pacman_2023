using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PacManWPF.Utils;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Static;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Points.Abs;
using PacManWPF_Canvas.Utils;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas.MapEntries.Characters
{

    public class BaseGhostMoverContext
    {
        public virtual Type? GetContextInitType()
        {
            return null;
        }
        
        public virtual void InitializeFromStream(BinaryReader stream)
        {

        }

        protected virtual void _Initialize(object value)
        {
        }


        public virtual void Initialize(object? value)
        {
            var type = GetContextInitType();
            if (type is null && value is null)
                return;

            if (!type!.IsAssignableFrom(value!.GetType()))
                throw new InvalidOperationException($"{type.FullName} is not compatible with {value!.GetType().FullName}");

            this._Initialize(value!);
        }
    }

    public abstract class NonGenericBaseGhostMover
    {
        public abstract Type? GetContextType();
    }

    interface IContextLess
    {
        public void Refresh(GameMapTransaction transaction, GhostMapEntry ghost);
        public void Process(GameMapTransaction transaction, GhostMapEntry ghost, double speed);
    }

    interface IWithContext
    {
        public object CreateContext();

        public object Refresh(object last_ctx, GameMapTransaction transaction, GhostMapEntry ghost);
        public void Process(object last_ctx, GameMapTransaction transaction, GhostMapEntry ghost, double speed);
    }


    public abstract class ContextLessBaseGhostMover<T> : NonGenericBaseGhostMover, IContextLess where T : class, new()
    {
        public static T INSTANCE { get; } = new T();

        public override Type? GetContextType()
        {
            return null;
        }
   
        protected ContextLessBaseGhostMover() { }

        public abstract void Refresh(GameMapTransaction transaction, GhostMapEntry ghost);
        public abstract void Process(GameMapTransaction transaction, GhostMapEntry ghost, double speed);
    }


    public abstract class BaseGhostMover<ContextT, T> : NonGenericBaseGhostMover, IWithContext where ContextT : BaseGhostMoverContext where T : class, new()
    {
        public static T INSTANCE { get; } = new T();

        public override Type GetContextType()
        {
            return typeof(ContextT);
        }

        public ContextT CreateContext()
        {
            return Activator.CreateInstance<ContextT>();
        }

        object IWithContext.CreateContext() => CreateContext();

        protected BaseGhostMover() { }

        public abstract ContextT Refresh(ContextT last_ctx, GameMapTransaction transaction, GhostMapEntry ghost);

        public object Refresh(object last_ctx, GameMapTransaction transaction, GhostMapEntry ghost)
        {
            if (last_ctx is not ContextT x)
                throw new Exception();

            return this.Refresh(x, transaction, ghost);
        }

        public abstract void Process(ContextT last_ctx, GameMapTransaction transaction, GhostMapEntry ghost, double speed);
      
        public void Process(object last_ctx, GameMapTransaction transaction, GhostMapEntry ghost, double speed)

        {
            if (last_ctx is not ContextT x)
                throw new Exception();

            this.Process(x, transaction, ghost, speed);
        }

    }

    public interface IMassiveContextRefresh
    {
        public GhostMapEntry[] MassiveRefresh(GameMapTransaction transaction, ImmutableArray<GhostMapEntry> unrefreshed_entries);
    }

    public class AutoFollowerGhostMoverContext: BaseGhostMoverContext
    {
        public List<ReadonlyBasePoint<int>> Path { get; protected set; }

        public override Type? GetContextInitType()
        {
            return typeof(List<ReadonlyBasePoint<int>>);
        }

        protected override void _Initialize(object value)
        {
            this.Path = (List<ReadonlyBasePoint<int>>)value;
        }
    }

    public class AutoFollowerGhostMover : BaseGhostMover<AutoFollowerGhostMoverContext, AutoFollowerGhostMover>, IMassiveContextRefresh
    {
        public override AutoFollowerGhostMoverContext Refresh(AutoFollowerGhostMoverContext ctx, 
                                                              GameMapTransaction transaction, 
                                                              GhostMapEntry ghost)
        {
            this.MassiveRefresh(transaction, [ghost]);
            return ctx;
        }

        public override void Process(AutoFollowerGhostMoverContext ctx,
                                     GameMapTransaction transaction,
                                     GhostMapEntry ghost,
                                     double speed)
        {
            if (ctx.Path is null || ctx.Path.Count == 0)
                return;

            if (ctx.Path.Count > 20)
            {
                var tmp = ctx.Path.Slice(0, 20);
                ctx.Path.Clear();
                ctx.Path.AddRange(tmp);   
            }

            var from_point = ghost.Point.ToIntFloorPoint();
            var to_point = ctx.Path[0];
            var diffs_by = from_point.X == to_point.X ? 'Y' : 'X';

            var align_diff = diffs_by == 'Y' ? Math.Round(ghost.Point.X - (from_point.X + 0.5), 5) :
                                               Math.Round(ghost.Point.Y - (from_point.Y + 0.5), 5);

            if (align_diff != 0)
            {
                var fix = Math.Min(Math.Abs(align_diff), speed);
                Debug.Assert(fix >= 0);
                Debug.Assert(fix <= speed);
                
                if (diffs_by == 'Y')
                    ghost.Point.SetX(ghost.Point.X + (align_diff < 0 ? fix : -fix));
                else
                    ghost.Point.SetY(ghost.Point.Y + (align_diff < 0 ? fix : -fix));
                
                speed -= fix;
                if (speed == 0)
                    return;
            }


            bool move_on;
            if (diffs_by == 'Y')
            {
                var diff = from_point.Y - to_point.Y;
                var going_to_bottom = diff == -1 || diff == Config.CHUNK_HC - 1;

                ghost.Point.SetY(ghost.Point.Y + (going_to_bottom ? speed : -speed));
                ghost.Point.Normalize();

                move_on = Math.Floor(ghost.Point.Y) != from_point.Y;
            } else {
                var diff = from_point.X - to_point.X;
                var going_to_right = diff == -1 || diff == Config.CHUNK_WC - 1;

                ghost.Point.SetX(ghost.Point.X + (going_to_right ? speed : -speed));
                ghost.Point.Normalize();

                move_on = Math.Floor(ghost.Point.X) != from_point.X;
            }

            if (move_on)
                ctx.Path.RemoveAt(0);

            ghost.Point.Normalize();
        }

        protected virtual IEnumerable<GhostMapEntry> BuildMassiveRefreshLinq(ImmutableArray<GhostMapEntry> unrefreshed_entries)
        {
            // TODO in cached auto follower edit this
            return from x in unrefreshed_entries where x.MoverHandler.Mover is AutoFollowerGhostMover t select x;
        }

        [Conditional("DEBUG")]
        private static void _check_mapping(LinkedPoints<ReadonlyBasePoint<int>>[] map_states)
        {
            foreach (var map_state in map_states)
            {
                if (map_state.Right is not null)
                {
                    Debug.Assert(map_state.Right.Point.X == 0 || map_state.Right.Point.X - map_state.Point.X == 1);
                    Debug.Assert(map_state.Right.Point.Y == map_state.Point.Y);
                    Debug.Assert(map_state.Right.Left is not null);
                    Debug.Assert(object.ReferenceEquals(map_state.Right.Left!.Point, map_state.Point));
                    Debug.Assert(object.ReferenceEquals(map_state.Right.Left, map_state));
                }
                if (map_state.Left is not null)
                {
                    Debug.Assert(map_state.Left.Point.X == Config.CHUNK_WC - 1 || map_state.Left.Point.X - map_state.Point.X == -1);
                    Debug.Assert(map_state.Left.Point.Y == map_state.Point.Y);

                    Debug.Assert(map_state.Left.Right is not null);
                    Debug.Assert(object.ReferenceEquals(map_state.Left.Right!.Point, map_state.Point));
                    Debug.Assert(object.ReferenceEquals(map_state.Left.Right, map_state));
                }
                if (map_state.Top is not null)
                {
                    Debug.Assert(map_state.Top.Point.Y == Config.CHUNK_HC - 1 || map_state.Top.Point.Y - map_state.Point.Y == -1);
                    Debug.Assert(map_state.Top.Point.X == map_state.Point.X);

                    Debug.Assert(map_state.Top.Bottom is not null);
                    Debug.Assert(object.ReferenceEquals(map_state.Top.Bottom!.Point, map_state.Point));
                    Debug.Assert(object.ReferenceEquals(map_state.Top.Bottom, map_state));
                }
                if (map_state.Bottom is not null)
                {
                    Debug.Assert(map_state.Bottom.Point.Y == 0 || map_state.Bottom.Point.Y - map_state.Point.Y == 1);
                    Debug.Assert(map_state.Bottom.Point.X == map_state.Point.X);

                    Debug.Assert(map_state.Bottom.Top is not null);
                    Debug.Assert(object.ReferenceEquals(map_state.Bottom.Top!.Point, map_state.Point));
                    Debug.Assert(object.ReferenceEquals(map_state.Bottom.Top, map_state));
                }

                var x = 0;
                if (map_state.Bottom is not null)
                    ++x;
                if (map_state.Top is not null)
                    ++x;
                if (map_state.Left is not null)
                    ++x;
                if (map_state.Right is not null)
                    ++x;

                // Debug.Assert(x >= 2);
            }

        }

        [Conditional("DEBUG")]
        private static void _check_explore(List<ReadonlyBasePoint<int>> path)
        {
            ReadonlyBasePoint<int> last;
            ReadonlyBasePoint<int> current;
            for (int i = 1; i < path.Count; ++i)
            {
                last = path[i - 1];
                current = path[i];

                var diff = Math.Abs(last.X - current.X + last.Y - current.Y);
                Debug.Assert((diff == 1 || diff == 0) || diff == Config.CHUNK_WC - 1 || diff == Config.CHUNK_HC - 1);
            }
        }

        public GhostMapEntry[] MassiveRefresh(GameMapTransaction transaction, ImmutableArray<GhostMapEntry> unrefreshed_entries) // This maybe is more "static" method but rip
        {
            var dc = this.BuildMassiveRefreshLinq(unrefreshed_entries);
            var update_pending = dc.Where(x => (((AutoFollowerGhostMoverContext)x.MoverHandler.MoverContext)?.Path?.Count ?? 0) == 0);
            if (update_pending.Count() == 0)
                return dc.ToArray();

            var rev_roots = new Dictionary<ReadonlyBasePoint<int>, List<GhostMapEntry>>();
            foreach (var item in update_pending)
            {
                var k = item.Point.ToIntFloorPoint().AsReadonlyMask();
                rev_roots.TryGetValue(k, out var tmp);
                if (tmp is null)
                    rev_roots.Add(k, tmp = new List<GhostMapEntry>());
                tmp.Add(item);
            }

            var roots = new Dictionary<GhostMapEntry, LinkedPoints<ReadonlyBasePoint<int>>>();
            var built = new Dictionary<ReadonlyBasePoint<int>, LinkedPoints<ReadonlyBasePoint<int>>>();
            var to_check = new List<(ReadonlyBasePoint<int> Point, LinkedPoints<ReadonlyBasePoint<int>> Node, char Direction)>();

            ReadonlyBasePoint<int>? previus = null;
            var previus_row = new LinkedPoints<ReadonlyBasePoint<int>>?[Config.CHUNK_WC];
            LinkedPoints<ReadonlyBasePoint<int>> node;
            LinkedPoints<ReadonlyBasePoint<int>>? tmp_node;

            var overlapped_ghosts = new List<List<GhostMapEntry>>();

            foreach (var item in transaction.MapCeils)
            {
                // TODO Verificare why non linka
                if (item.Value.Any(x => x is IUnwalkable)) {
                    previus_row[item.Key.X] = null;
                    continue;
                }

                node = built[item.Key] = new LinkedPoints<ReadonlyBasePoint<int>>() { Point = item.Key };

                if (item.Key.X == 0)
                    to_check.Add((new IntPoint(Config.CHUNK_WC - 1, item.Key.Y), node, 'L'));
                else if ((tmp_node = previus_row[item.Key.X - 1]) is not null)
                {
                    Debug.Assert(node.Left is null);
                    tmp_node.Right = node;
                    node.Left = tmp_node;
                }

                if (item.Key.Y == 0)
                    to_check.Add((new IntPoint(item.Key.X, Config.CHUNK_HC - 1), node, 'T'));
                else if ((tmp_node = previus_row[item.Key.X]) is not null)
                {
                    Debug.Assert(node.Top is null);
                    tmp_node.Bottom = node;
                    node.Top = tmp_node;
                }

                previus_row[item.Key.X] = node;
                previus = item.Key;

                if (rev_roots.TryGetValue(item.Key, out var tmp))
                {
                    foreach (var g in tmp)
                        roots[g] = node;

                    if (tmp.Count != 1)
                        overlapped_ghosts.Add(tmp);
                }
            }

            foreach (var item in to_check)
            {
                if (built.TryGetValue(item.Point, out var tmp))
                {
                    if (item.Direction == 'T')
                    {
                        Debug.Assert(item.Node.Top is null);
                        item.Node.Top = tmp;
                        tmp.Bottom = item.Node;
                    }
                    else if (item.Direction == 'L')
                    {
                        Debug.Assert(item.Node.Left is null);
                        item.Node.Left = tmp;
                        tmp.Right = item.Node;
                    }
                    else
                        Debug.Assert(false);
                }
            }


            _check_mapping(built.Values.ToArray());
            var pacman_pos = transaction.Entities.Pacman.Point.ToIntFloorPoint().AsReadonlyMask();

            roots.Where(x =>
                !overlapped_ghosts.Any(y => 
                y.Any(z => 
                ReferenceEquals(z, x.Key)
                ))).AsParallel().ForAll(root => {
                if (root.Key.MoverHandler.MoverContext is AutoFollowerGhostMoverContext tmp)
                {
                    var effective_point = root.Key.State == GhostState.GOING_TO_SPAWN ? 
                                            root.Key.SpawnPoint.Point.ToIntFloorPoint().AsReadonlyMask() :
                                            pacman_pos;

                    tmp.Initialize(Explore(effective_point, root.Value));
                }
                else
                    Debug.Assert(false);
            });

            overlapped_ghosts.AsParallel().ForAll(
                overlapping => {
                    var banned_paths = new HashSet<int>(overlapping.Count - 1);
                    List<ReadonlyBasePoint<int>>? last_path = null;

                    foreach (var item in overlapping)
                    {
                        var ghost_pos = roots[item];

                        if (item.MoverHandler.MoverContext is AutoFollowerGhostMoverContext tmp)
                        {
                            var path = Explore(pacman_pos, ghost_pos, banned_paths);
                            if (path.Count == 0)
                            {
                                if (last_path is null)
                                    return;

                                path = last_path;
                            } else
                                last_path = path;
                            tmp.Initialize(path);
                        }
                        else
                            Debug.Assert(false);

                    }
                }
            );



            return dc.ToArray();


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool _check(ReadonlyBasePoint<int> pacman_pos,
                               LinkedPoints<ReadonlyBasePoint<int>> start_pos, 
                               ref List<ReadonlyBasePoint<int>> result,
                               List<ReadonlyBasePoint<int>> building,
                               HashSet<ReadonlyBasePoint<int>> banned_points, 
                               List<ExploreContext> long_explore_ctx,
                               int depth,
                               HashSet<int>? banned_paths)
            {
                if (depth <= 0)
                {
                    long_explore_ctx.Add(new ExploreContext(start_pos, building, banned_points));
                    return true;
                }

                if (result.Count > 0 && building.Count >= result.Count)
                    return true;

                if (pacman_pos == start_pos.Point)
                {
                    if (building.Count > 0 && building[^1] != start_pos.Point)
                        building.Add(start_pos.Point);

                    
                    if (result.Count == 0 || building.Count < result.Count)
                    {
                        if (banned_paths is not null)
                        {
                            var hash = HashCode.Combine(string.Join(';', building.Select(i => i.GetHashCode())));
                            if (banned_paths.Contains(hash))
                                return true;
                        }

                        result = building;
                    }
                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool _check_banned(LinkedPoints<ReadonlyBasePoint<int>> point, HashSet< ReadonlyBasePoint<int>> banned_points)
            {
                return banned_points.Contains(point.Point);
            }

            static void _Explore(ReadonlyBasePoint<int> pacman_pos,
                                 LinkedPoints<ReadonlyBasePoint<int>> start_pos, 
                                 ref List<ReadonlyBasePoint<int>> result,
                                 List<ReadonlyBasePoint<int>> building,
                                 HashSet<ReadonlyBasePoint<int>> banned_points,
                                 List<ExploreContext> long_explore_ctx,
                                 int depth,
                                 HashSet<int>? banned_paths)
            {
                if (_check(pacman_pos, start_pos, ref result, building, banned_points, long_explore_ctx, depth, banned_paths))
                    return;

                banned_points.Add(start_pos.Point);
                building.Add(start_pos.Point);

                var src_start_pos = start_pos;
                --depth;

                if (building.Count >= 2)
                {
                    ReadonlyBasePoint<int>? tmp_point = building[building.Count - 2];
                    LinkedPoints<ReadonlyBasePoint<int>>? tmp ;

                    while ((tmp = start_pos.GetTheOnlyNear(tmp_point)) is not null)
                    {
                        tmp_point = start_pos.Point;
                        start_pos = tmp;
                        banned_points.Add(start_pos.Point);
                        building.Add(start_pos.Point);

                        if (_check(pacman_pos, start_pos, ref result, building, banned_points, long_explore_ctx, --depth, banned_paths))
                            return;
                    }
                }

                if (start_pos.Left is not null && !_check_banned(start_pos.Left, banned_points))
                {
                    Debug.Assert(!object.ReferenceEquals(building.ToList(), building));
                    _Explore(pacman_pos, start_pos.Left, ref result, building.ToList(), banned_points.ToHashSet(), long_explore_ctx, depth, banned_paths);
                }

                if (start_pos.Right is not null && !_check_banned(start_pos.Right, banned_points))
                {
                    _Explore(pacman_pos, start_pos.Right, ref result, building.ToList(), banned_points.ToHashSet(), long_explore_ctx, depth, banned_paths);
                }

                if (start_pos.Bottom is not null && !_check_banned(start_pos.Bottom, banned_points))
                {
                    _Explore(pacman_pos, start_pos.Bottom, ref result, building.ToList(), banned_points.ToHashSet(), long_explore_ctx, depth, banned_paths);
                }

                if (start_pos.Top is not null && !_check_banned(start_pos.Top, banned_points))
                {
                    _Explore(pacman_pos, start_pos.Top, ref result, building.ToList() , banned_points.ToHashSet(), long_explore_ctx, depth, banned_paths);
                }
            }


            static List<ReadonlyBasePoint<int>> Explore(ReadonlyBasePoint<int> pacman_pos,
                                                        LinkedPoints<ReadonlyBasePoint<int>> ghost_pos,
                                                        HashSet<int>? banned_paths = null)
            {
                if (pacman_pos == ghost_pos.Point)
                    return [];
                
                var rt = new List<ReadonlyBasePoint<int>>();
                var banned_points = new HashSet<ReadonlyBasePoint<int>>();
                var long_explore_ctx = new List<ExploreContext>();


                _Explore(pacman_pos, ghost_pos, ref rt, new List<ReadonlyBasePoint<int>>(), banned_points, long_explore_ctx, 32, banned_paths);

                if (rt.Count == 0)
                {
                    var depth = new int[] { 16, 32, 999 };

                    foreach (var d in depth)
                    {
                        var tmp = long_explore_ctx.ToArray();
                        long_explore_ctx.Clear();
                        foreach (var item in tmp) // Copy
                            _Explore(pacman_pos, item.start_pos, ref rt, item.building, item.banned_points, long_explore_ctx, d, banned_paths);

                        if (rt.Count != 0)
                            break;
                    }
                }

                _check_explore(rt);


                if (banned_paths is not null)
                    banned_paths.Add(HashCode.Combine(string.Join(';', rt.Select(i => i.GetHashCode()))));

                if (rt.Count > 1 && rt[^1] == rt[^2])
                {
                    Debug.Assert(false);
                    rt.RemoveAt(rt.Count - 1);
                }

                if (rt.Count != 0)
                    rt.RemoveAt(0); // ODIO, Ma sticazz non dovrebbe essere una lista enorme 😩

                return rt;
            }
        }
    }

    record ExploreContext(LinkedPoints<ReadonlyBasePoint<int>> start_pos, List<ReadonlyBasePoint<int>> building, HashSet<ReadonlyBasePoint<int>> banned_points);

    public class FixedGhostMover : ContextLessBaseGhostMover<FixedGhostMover>
    {
        public override void Refresh(GameMapTransaction transaction, GhostMapEntry ghost)
        {
        }

        public override void Process(GameMapTransaction transaction, GhostMapEntry ghost, double speed)
        {
        }
    }

    public enum GhostState
    {
        GOING_TO_SPAWN, // Appena stato mangiato, sta tornadno alo spawn
        GOT_EATEN, // È stato mangiato, è respawnato quindi può mangiare pacman (anche se drogato)
        NORMAL, // È in uno stato normale, in cui pacman non è drogato
        SCARIED, // Pacman è drogato, e il fantasma non è stato mangiato
        SPAWNING, // Il fantasma sta uscendo dallo spawn per la prima volta
        RESPAWNING, // Il fantasma è stato mangiato e sta respawnando
    }

    public enum GhostColor
    {
        Cyan,
        Red,
        Pink,
        Orange,
    }

    public class MoverHandler
    {
        public NonGenericBaseGhostMover Mover { get; protected set; }

        public object? MoverContext { get; protected set; }
        
        public MoverHandler(NonGenericBaseGhostMover mover)
        {
            this.Mover = mover;

            if (mover is IWithContext x)
                this.MoverContext = x.CreateContext();
            else
                this.MoverContext = null;
        }

        public void Process(GameMapTransaction transaction, GhostMapEntry ghost, double speed)
        {
            if (this.Mover is IContextLess y)
                y.Process(transaction, ghost, speed);
            else if (this.Mover is IWithContext z)
                z.Process(this.MoverContext!, transaction, ghost, speed);
            else
                throw new InvalidOperationException();
        }

        public void Refresh(GameMapTransaction transaction, GhostMapEntry ghost)
        {
            if (this.Mover is IContextLess x)
                x.Refresh(transaction, ghost);
            else if (this.Mover is IWithContext y)
                this.MoverContext = y.Refresh(this.MoverContext!, transaction, ghost);
            else
                throw new InvalidOperationException();
        }
    }

    public class GhostImageSourceProvider
    {
        public required GhostColor Color { get; init; }

        private int c = 0;
        public BitmapImage Get(GhostState state, Direction direction)
        {
            if (state is GhostState.SCARIED)
                return ResourcesLoader.GetImage($@"Assets/Images/dead_ghost.png");
            if (state is GhostState.GOING_TO_SPAWN)
                return ResourcesLoader.GetImage($@"Assets/Images/eyes.png");

            c = ++c % 2;
            var tmp = this.Color.ToString().ToLower();
            var tmp2 = direction.ToString().ToLower();

            return ResourcesLoader.GetImage($@"Assets/Images/{tmp}_{c}_{tmp2}.png");
        }
    }


    public class GhostMapEntry : CenteredMapEntry, IDynamicDraw
    {
        public override int ZIndex { get; protected set; } = 3;
        public GhostState State { get; protected set; } = GhostState.SPAWNING;
        public GhostColor Color { get; protected init; }

        public ISpawnPointMapEntry SpawnPoint { get; protected init; }

        private GhostImageSourceProvider ImageProvider;
        public MoverHandler MoverHandler { get; protected set; }

        private long RespawnTicks = 0;
        private int StrikeGotKillCount = 0;

        public override void Init(Canvas canvas)
        {
            base.Init(canvas);

            this.Element.Width = Config.CHUNK_CS;
            this.Element.Height = Config.CHUNK_CS;
            this.Element.MinHeight = Config.CHUNK_CS;
            this.Element.MinWidth = Config.CHUNK_CS;

            Canvas.SetBottom(this.Element, 1000);
        }

        public double GetSpeed()
        {
            if (this.State is GhostState.NORMAL || this.State is GhostState.GOT_EATEN || this.State is GhostState.SPAWNING || this.State is GhostState.RESPAWNING)
                return 0.08D;
            if (this.State is GhostState.GOING_TO_SPAWN)
                return 0.085D * Math.Min(this.StrikeGotKillCount, 3);
            if (this.State is GhostState.SCARIED)
                return 0.05D;

            throw new InvalidOperationException();
        }

        public GhostMapEntry(GhostColor Color, ISpawnPointMapEntry SpawnPoint) : base(new Image())
        {
            this.Color = Color;
            this.ImageProvider = new GhostImageSourceProvider() { Color = Color};

            this.SpawnPoint = SpawnPoint;
            var tmp = SpawnPoint.Point.ToIntFloorPoint();
            this.Point.SetX(tmp.X + 0.5);
            this.Point.SetY(tmp.Y + 0.5);

            this.MoverHandler = new MoverHandler(AutoFollowerGhostMover.INSTANCE);
        }

        public override void Process(GameMapTransaction transaction)
        {

            if (this.RespawnTicks != 0)
                return;

#if DEBUG
            var debug_p = this.Point.Clone();
#endif
            this.MoverHandler.Process(transaction, this, this.GetSpeed());
#if DEBUG
            var diff = Math.Abs(debug_p.X - this.Point.X + debug_p.Y - this.Point.Y);
            Debug.Assert((diff == 1 || diff == 0) || true);
#endif
        }

        private DoublePoint? last_point = null;
        public void Draw(Canvas canvas, double scale)
        {
            Direction tmp;
            if (last_point is null)
            {
                tmp = Direction.Right;
            }
            else if (last_point.X < this.Point.X)
            {
                tmp = Direction.Right;
            }
            else if (last_point.X > this.Point.X)
            {
                tmp = Direction.Left;
            }
            else if (last_point.Y < this.Point.Y)
            {
                tmp = Direction.Down;
            } else
            {
                tmp = Direction.Up;
            }

            last_point = this.Point.Clone();
            ((Image)this.Element).Source = this.ImageProvider.Get(this.State, tmp);
        }

        [Conditional("DEBUG")]
        private void CheckKillValidateState(PacmanMapEntry entry)
        {
            if (entry.CanEatGhosts)
            {
                Debug.Assert(this.State is not GhostState.NORMAL);
            } else if (!entry.CanEatGhosts)
            {
                Debug.Assert(this.State is not GhostState.SCARIED && this.State is not GhostState.GOT_EATEN);
            } else
            {
                Debug.Assert(this.State is GhostState.SPAWNING || this.State is GhostState.RESPAWNING);
            }
        }

        private void Die()
        {
            this.State = GhostState.GOING_TO_SPAWN;
        }

        public bool CheckKill(PacmanMapEntry entry) // returns true if pacman should survive
        {
            this.CheckKillValidateState(entry);
            
            if (entry.CanEatGhosts && this.State is GhostState.SCARIED)
            {
                this.StrikeGotKillCount = 1;
                entry.AddPoints(1000);
                this.Die();
                return true;
            }

            if (entry.CanEatGhosts && this.State is GhostState.GOING_TO_SPAWN)
            {
                entry.AddPoints(1000 * Math.Pow(2, ++StrikeGotKillCount - 1));
                return true;
            }

            if (!entry.CanEatGhosts && this.State is GhostState.GOING_TO_SPAWN)
                return true;

            if (!entry.CanEatGhosts && this.State is GhostState.NORMAL)
                return false;

            if (entry.CanEatGhosts && this.State is GhostState.GOT_EATEN)
                return false;

            throw new Exception($"Unhandled case {this.State} {entry.CanEatGhosts}!");
        }

        internal bool IsInSpawnArea(IReadOnlyDictionary<IntPoint, ImmutableArray<ISpawnPointMapEntry>> spawn_points)
        {
            return spawn_points.ContainsKey(this.Point.ToIntFloorPoint().Normalize());
        }

        internal bool IsAtSpawnPoint()
        {
            if (this.Point.ToIntFloorPoint() == this.SpawnPoint.Point.ToIntFloorPoint())
                return true;
            // TODO ALGIMENT
            return false;
        }


        private bool PMWasAbleToEat = false;
        public void Configure(PacmanMapEntry entry,
                              IReadOnlyDictionary<IntPoint, ImmutableArray<ISpawnPointMapEntry>> spawn_points,
                              ref int in_spawn_points_no)
        {
            var is_in_spawn_area = this.IsInSpawnArea(spawn_points);
            if (is_in_spawn_area && this.RespawnTicks == 0)
            {
                if (this.State is GhostState.SPAWNING || this.State is GhostState.RESPAWNING)
                    this.RespawnTicks = (in_spawn_points_no--) * 100L;
            }

            if (!is_in_spawn_area && entry.CanEatGhosts && this.State is GhostState.NORMAL) // Se i fantasmi non sono tutti usciti, uno di loro viene ucciso e pacman è ancora drogato
                this.State = GhostState.SCARIED;

            if (this.State is GhostState.SPAWNING)
            {
                if (--this.RespawnTicks == 0)
                    this.State = GhostState.NORMAL;
            }
            else if (this.State is GhostState.RESPAWNING)
            {
                if (--this.RespawnTicks == 0)
                    this.State = GhostState.GOT_EATEN;
            }
            else if (this.State is GhostState.GOING_TO_SPAWN && !is_in_spawn_area)
            {
                ;
            } 
            else if (this.State is GhostState.GOING_TO_SPAWN && this.IsAtSpawnPoint())
            {
                this.State = GhostState.RESPAWNING;
            }
            else if (!this.PMWasAbleToEat && entry.CanEatGhosts) // Se non mi poteva mangiare ed ora può
            {
                if (this.State is GhostState.NORMAL)
                    this.State = GhostState.SCARIED;
            }
            else if (this.PMWasAbleToEat && !entry.CanEatGhosts) { // Se mi poteva mangiare ed ora non può
                Debug.Assert(this.State is not GhostState.NORMAL);
                if (this.State is GhostState.SCARIED || this.State is GhostState.GOT_EATEN)
                    this.State = GhostState.NORMAL;
            }

            if (this.RespawnTicks == 0)
                ;


            this.PMWasAbleToEat = entry.CanEatGhosts;
        }

        public override int GetHashCode()
        {
            return this.Color.GetHashCode();
        }

        public void RefreshContext(GameMapTransaction transaction)
        {
            this.MoverHandler.Refresh(transaction, this);
        }
    }
}