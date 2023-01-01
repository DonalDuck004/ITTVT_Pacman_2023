using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace PacManWPF.Animations
{
    internal class SpecialFoodAnimation : AnimationTimeline
    {
        protected override Freezable CreateInstanceCore() => this;
        public override Type TargetPropertyType => typeof(double);
        public Guid Id;

        public SpecialFoodAnimation()
        {
            this.RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(10));
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            Debug.Assert(animationClock.CurrentProgress is not null);
  

            return (double)((animationClock.CurrentProgress.Value > 0.5) ? 1 : 0);
        }
    }
}
