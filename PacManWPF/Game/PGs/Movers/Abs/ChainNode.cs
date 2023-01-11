using System.Drawing;

namespace PacManWPF.Game.PGs.Movers.Abs
{
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
                    if (this._CachedCount is null)
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
}