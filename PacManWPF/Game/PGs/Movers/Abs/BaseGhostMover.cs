using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PacManWPF.Utils;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace PacManWPF.Game.PGs.Movers.Abs
{

    public interface IGhostMover
    {
        Point GetPos();
        bool NextFrame();
    }

    public abstract class BaseGhostMover : IGhostMover
    {
        protected record GhostState(bool Died, bool PacmanDrugged, bool InGate, bool initialized, bool needToGoToSpawn);

        protected Ghost ghost;
        protected GhostState? OldState = null;

        public BaseGhostMover(Ghost self) { 
            this.ghost = self;
        }

        public abstract Point GetPos();
        public abstract Point GetStartPoint();

        protected List<Point>? goto_way = null;

        protected virtual void NearPoints(Point from, 
                                          Point to,
                                          List<Point> root,
                                          List<List<Point>> done)
        {
            if (done.Count != 0)
                lock (done)
                    if (root.Count > done.Select(x => x.Count).Order().First())
                        return;


            List<Point> near = new();
            Point tmp;

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X - 1, from.Y).Fix()) && !root.Contains(tmp))
                near.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X + 1, from.Y).Fix())  && !root.Contains(tmp))
                near.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X, from.Y - 1).Fix()) && !root.Contains(tmp))
                near.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X, from.Y + 1).Fix()) && !root.Contains(tmp))
                near.Add(tmp);

            if (near.Count == 0)
                return;

            foreach (var item in near)
            {
                if (item == to)
                {
                    root.Add(item);
                    lock (done)
                        done.Add(root);
                    return;
                }
            }

            var e = near.GetEnumerator();
            e.MoveNext();
            var first = e.Current;


            List<Point> bck;

            while (e.MoveNext())
            {
                bck = new(root)
                {
                    e.Current
                }; // Shallow copy
                NearPoints(e.Current, to, bck, done);
            }

            root.Add(first);
            NearPoints(first, to, root, done);
        }


        protected virtual List<Point> GetWay(Point from, Point to)
        {
            if (from == to)
                return new() { from, to };

            Point tmp;
            List<Point> roots = new List<Point>();

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X - 1, from.Y).Fix()))
                roots.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X + 1, from.Y).Fix()))
                roots.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X, from.Y - 1).Fix()))
                roots.Add(tmp);

            if (PacmanGame.INSTANCE.FreeAreas.Contains(tmp = new Point(from.X, from.Y + 1).Fix()))
                roots.Add(tmp);


            foreach (var root in roots)
                if (root == to)
                    return new() { from, root };

            List<Thread> threads = new List<Thread>();
            List<List<Point>> done = new List<List<Point>>();

            Thread th;
            foreach (var root in roots)
            {
                th = new(() => NearPoints(root, to, new() { from, root }, done));
                th.Start();
                threads.Add(th);
            }
                

            foreach (var thread in threads)
                thread.Join();

            return done.OrderBy(x => x.Count).First();
        }

        protected virtual void GotoStartPoint(Point to)
        {
            var from = ghost.EffectivePosition;

            if (this.goto_way is null) {
                this.goto_way = GetWay(from, to);
                this.goto_way.RemoveAt(0);
            }

            Grid.SetColumn(this.ghost.CeilObject, this.goto_way[0].X);
            Grid.SetRow(this.ghost.CeilObject, this.goto_way[0].Y);
            
            this.goto_way.RemoveAt(0);
        }

        public virtual bool NextFrame() // False no read, True read .GetPos() result
        {
            var state = new GhostState(ghost.IsDied, Pacman.INSTANCE.IsDrugged, ghost.InGate, ghost.Initialized, ghost.NeedToGoToSpawn);
            if (state != this.OldState && this.goto_way is not null)
                this.goto_way = null;

            this.OldState = state;

            if (state.Died)
            {
                this.GotoStartPoint(this.ghost.SpawnPoint);
                return false;
            }

            if (!state.initialized || state.needToGoToSpawn)
            {
                this.GotoStartPoint(this.GetStartPoint());
                return false;
            }


            return true;
        }

    }
}