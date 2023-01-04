using PacManWPF.Game.PGs;
using PacManWPF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;

namespace PacManWPF.Animations
{
    internal class GhostAnimation : AnimationTimeline
    {
        protected override Freezable CreateInstanceCore() => this;
        public override Type TargetPropertyType => typeof(Thickness);

        public double From;
        public double To;
        private char dir;


        public GhostAnimation(double from, double to, char d)
        {
            this.From = from;
            this.To = to;
            this.dir = d;
        }


        private double getV(double progress)
        {
            if (this.To < this.From)
                return this.To + (1 - progress) * (this.From - this.To);

            return this.From + (this.To - this.From) * progress;
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            Debug.Assert(animationClock.CurrentProgress is not null);

            if (dir == 'r')
                return new Thickness(0, 0, getV(animationClock.CurrentProgress.Value), 0);

            return new Thickness(getV(animationClock.CurrentProgress.Value), 0, 0, 0);
        }
    }
}
