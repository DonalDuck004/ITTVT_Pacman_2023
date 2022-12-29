using System;
using System.Drawing;

namespace PacManWPF.Game.Abs
{
    public interface IBasePG
    {
        Point Position { get; }
    }

    public abstract class BasePG : IBasePG
    {

        public bool IsDied = false;

        public abstract Point Position { get; }
    }
}
