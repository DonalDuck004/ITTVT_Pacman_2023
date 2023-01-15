using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using PacManWPF.Utils;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Threading.Tasks;
using System;
using System.Xml.Linq;
using System.Threading.Tasks.Dataflow;
using System.Collections;
using Microsoft;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace PacManWPF.Game.PGs.Movers.Abs
{

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

        protected ChainNode? done = null;

        protected virtual void NearPoints(ChainNode from,
                                          Point to)
        {


            Queue<ChainNode> o = new();
            o.Enqueue(from);
            ChainNode item;
            List<Point> near;
            Point tmp;
            
            while (true)
            {
                try
                {
                    item = o.Dequeue();
                }catch (InvalidOperationException)
                {
                    return;
                }

                if (this.done is not null && this.done.CountBefore <= (item.CountBefore + 1))
                    continue;

                near = new();

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
                        if (done is null || done.CountBefore > (item.CountBefore + 1))
                            done = new ChainNode() { Point = x, Previus = item };
                    }
                    else
                        o.Enqueue(new ChainNode() { Point = x, Previus = item });
                }
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
                    return new() { Point = route, Previus = null };

            this.done = null;
            var ths = new Thread[routes.Count];

            for (int i = 0; i < ths.Length; i++)
            {
                ths[i] = new Thread(new ParameterizedThreadStart((idx) => this.NearPoints(new ChainNode() { Point = routes[(int)idx!], Previus = null }, to)));
                ths[i].Start(i);
            }

            for (int i = 0; i < ths.Length; i++)
            {
                ths[i].Join();
            }

            return this.done!;
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

        public virtual void Loaded()
        {
            this.OldState = null;
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