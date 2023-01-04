using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PacManWPF.Utils;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Diagnostics;

namespace PacManWPF.Game.PGs.Movers.Abs
{

    public interface IGhostMover
    {
        Point GetPos();
        bool NextFrame();
    }

    public class ChainNode
    {
        public required Point Point;
        public required ChainNode? Previus;

        private int? _CachedCount = null;
        public int CountBefore
        {
            get
            {
                lock (this)
                {
                    if (_CachedCount is null)
                    {
                        var C = this;
                        this._CachedCount = 0;
                        while (C.Previus is not null)
                        {
                            this._CachedCount++;
                            C = C.Previus;
                        }
                    }
                  return _CachedCount.Value;
                }
            }
        }

        public bool Contains(Point point)
        {
            var C = this;
            while (C.Previus is not null)
            {
                if (C.Point == point)
                    return true;
                C = C.Previus;
            }

            return false;
        }


        public ChainNode GetFirstNode()
        {
            var C = this;
            while (C.Previus is not null)
                C = C.Previus;

            return C;
        }

        public ChainNode PopFirstNode()
        {
            if (this._CachedCount is not null)
                this._CachedCount--;
            var C = this;
            ChainNode tmp = C;

            while (C.Previus is not null)
            {
                tmp = C;
                C = C.Previus;
                if (C._CachedCount is not null)
                    C._CachedCount--;
            }

            tmp.Previus = null;

            return C;
        }
    }

    public abstract class BaseGhostMover : IGhostMover
    {
        protected record GhostState(bool Died, bool PacmanDrugged, bool InGate, bool initialized, bool needToGoToSpawn);

        protected Ghost ghost;
        protected GhostState? OldState = null;
        protected GhostState? CurrentState = null;

        public BaseGhostMover(Ghost self) { 
            this.ghost = self;
        }

        public abstract Point GetPos();
        public abstract Point GetStartPoint();

        protected ChainNode? goto_way = null;

        protected virtual void NearPoints(ChainNode from, 
                                          Point to,
                                          List<ChainNode> done)
        {
            Queue<ChainNode> q = new();
            q.Enqueue(from);
            ChainNode? item;
            Point tmp;
            List<Point> near = new();

            while (q.TryDequeue(out item))
            {
                if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(item.Point.X - 1, item.Point.Y).Fix()) && !item.Contains(tmp))
                    near.Add(tmp);

                if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(item.Point.X + 1, item.Point.Y).Fix()) && !item.Contains(tmp))
                    near.Add(tmp);

                if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(item.Point.X, item.Point.Y - 1).Fix()) && !item.Contains(tmp))
                    near.Add(tmp);

                if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(item.Point.X, item.Point.Y + 1).Fix()) && !item.Contains(tmp))
                    near.Add(tmp);

                foreach (var x in near)
                {
                    if (x == to)
                    {
                        lock (done)
                            done.Add(new ChainNode() { Point = x, Previus = item });
                        return;
                    }
                    else
                        q.Enqueue(new ChainNode() { Point = x, Previus = item });
                }

                near.Clear();
            }
        }


        protected virtual ChainNode GetWay(Point from, Point to)
        {
            if (from == to)
                return new ChainNode() { Point = from, Previus = null };

            Point tmp;
            List<Point> routes = new();

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X - 1, from.Y).Fix()))
                routes.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X + 1, from.Y).Fix()))
                routes.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X, from.Y - 1).Fix()))
                routes.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X, from.Y + 1).Fix()))
                routes.Add(tmp);


            foreach (var route in routes)
                if (route == to)
                    return new ChainNode() { Point = route, Previus = null };

            List<Thread> threads = new();
            List<ChainNode> done = new();

            Thread th;
            foreach (var route in routes)
            {
#nullable disable
                th = new(new ParameterizedThreadStart((node) => NearPoints((ChainNode)node, to, done)));
#nullable restore
                th.Start(new ChainNode() { Point = route, Previus = null });
                threads.Add(th);
            }
                

            foreach (var thread in threads)
                thread.Join();

            return done.OrderBy(x => x.CountBefore).First();
        }

        protected virtual void CalculateWay(Point to)
        {
            var from = ghost.EffectivePosition;

            if (this.goto_way is null)
                this.goto_way = GetWay(from, to);


            this.ghost.HandleAnimation(this.ghost.EffectivePosition, to);
            var Node = this.goto_way.PopFirstNode();
            Grid.SetColumn(this.ghost.CeilObject, Node.Point.X);
            Grid.SetRow(this.ghost.CeilObject, Node.Point.Y);
        }

        public virtual bool NextFrame() // False no read, True read .GetPos() result
        {
            this.CurrentState = new GhostState(ghost.IsDied, Pacman.INSTANCE.IsDrugged, ghost.InGate, ghost.Initialized, ghost.NeedToGoToSpawn);
            if (this.CurrentState != this.OldState && this.goto_way is not null)
                this.goto_way = null;

            this.OldState = this.CurrentState;

            if (this.CurrentState.Died)
            {
                this.CalculateWay(this.ghost.SpawnPoint);
                return false;
            }

            if (!this.CurrentState.initialized || this.CurrentState.needToGoToSpawn)
            {
                this.CalculateWay(this.GetStartPoint());
                return false;
            }


            return true;
        }

    }
}