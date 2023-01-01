using PacManWPF.Game.PGs;
using PacManWPF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PacManWPF.Animations
{
    public class PacmanAnimation : AnimationTimeline
    {
        protected override Freezable CreateInstanceCore() => this;
        public override Type TargetPropertyType => typeof(Brush);

        public int Grad = 0;

        public PacmanAnimation() {
            this.RepeatBehavior = RepeatBehavior.Forever;
        }


        private MatrixTransform GetTransform()
        {
            var transform = Matrix.Identity;
            transform.RotateAt(Grad, 0.5, 0.5);

            if (Pacman.INSTANCE.IsDrugged)
                transform.ScaleAt(2, 2, 0.5, 0.5);
            else
                transform.ScaleAt(1, 1, 0.5, 0.5);

            return new(transform);
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            Debug.Assert(animationClock.CurrentProgress is not null);

            var value = (int)Math.Floor(animationClock.CurrentProgress.Value / 0.4);
            var img = ResourcesLoader.GetImage(ResourcesLoader.PacManAnimationPaths[value]);
            img.RelativeTransform = GetTransform();
            return img;
        }
    }
}
