using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.Animation;
using PacManWPF.Game;


namespace PacManWPF.Animations
{


    internal class SpecialFoodAnimation : AnimationTimeline, Abs.IStoppable, Abs.IInterruptable
    {
        protected override Freezable CreateInstanceCore() => this;
        public override Type TargetPropertyType => typeof(double);
        public Guid Id;

        private bool StopRequired = false;
        private bool InterruptRequired = false;

        public SpecialFoodAnimation()
        {
            this.RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(10));
            PacmanGame.INSTANCE.AddPendingAnimation(this);
        }


        public void Stop()
        {
            this.StopRequired = true;
        }

        public void Interrupt()
        {
            this.InterruptRequired = true;
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (this.InterruptRequired)
                return 1.0;

            if (this.StopRequired)
                return 0.0;

            Debug.Assert(animationClock.CurrentProgress is not null);

            return (double)((animationClock.CurrentProgress.Value > 0.5) ? 1 : 0);
        }
    }
}
