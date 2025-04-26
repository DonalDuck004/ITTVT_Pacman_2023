using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PacManWPF_Canvas.MapEntries.Abs;
using PacManWPF_Canvas.MapEntries.Characters.Effects;
using PacManWPF_Canvas.MapEntries.Characters.Effects.Abs;
using PacManWPF_Canvas.MapEntries.Static;
using PacManWPF_Canvas.Points;
using PacManWPF_Canvas.Utils;
using PacManWPF_Canvas.Worlds;
using XamlAnimatedGif;

namespace PacManWPF_Canvas.MapEntries.Characters
{
    public class PacmanMapEntry : CenteredMapEntry, IDynamicDraw
    {
        public override int ZIndex { get; protected set; } = 2;
        protected override int Size { get; } = 32;
        public bool CanEatGhosts { get; private set; }



        private Direction direction = Direction.Right;
        private Direction direction_guard = Direction.Right;

        private PacmanEffect[] Effects;

        private PacmanEffect BaseEffect;

        private bool IsSpecular(Direction d1, Direction d2)
        {
            return d1 == Direction.Left && d2 == Direction.Right ||
                    d1 == Direction.Right && d2 == Direction.Left ||
                    d1 == Direction.Up && d2 == Direction.Down ||
                    d1 == Direction.Down && d2 == Direction.Up;
        }

        private Direction AlignPositionAxis(DoublePoint double_point, Direction going_to, ref double inc)
        {
            if (going_to == direction)
                return going_to;

            if (IsSpecular(direction, going_to))
                return going_to;

            var int_point = double_point.ToIntFloorPoint();
            if ((going_to == Direction.Left || going_to == Direction.Right) && (direction == Direction.Up || direction == Direction.Down) && double_point.Y - int_point.Y <= 0.5 && double_point.Y - int_point.Y + inc > 0.5)
            {
                var new_y = int_point.Y + 0.5;
                inc -= Math.Abs(new_y - double_point.Y);
                double_point.SetY(new_y);
            }
            else if ((going_to == Direction.Up || going_to == Direction.Down) && (direction == Direction.Left || direction == Direction.Right) && double_point.X - int_point.X <= 0.5 && double_point.X - int_point.X + inc > 0.5)
            {
                var new_x = int_point.X + 0.5;
                inc -= Math.Abs(new_x - double_point.X);
                double_point.SetX(new_x);
            }
            else
                going_to = direction;

            return going_to;
        }


        private static Dictionary<Direction, Direction[]> DIRECTION_FALLBACKS = new Dictionary<Direction, Direction[]>
        {
            [Direction.Up] = [Direction.Left, Direction.Down, Direction.Right],
            [Direction.Right] = [Direction.Up, Direction.Left, Direction.Down],
            [Direction.Left] = [Direction.Down, Direction.Right, Direction.Up],
            [Direction.Down] = [Direction.Right, Direction.Up, Direction.Left],
        };

        private Direction _UpdatePosition(Direction going_to, GameMapTransaction transaction, double inc)
        {
#if DEBUG
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                return going_to;
#endif

            var tmp = Point.Clone();
            if (going_to_wall(copy_apply_increment(tmp, going_to, 1).Normalize().ToIntFloorPoint()))
                going_to = this.direction;
            // ignore invalid changes 

            var src_inc = inc;

            var new_going_to = AlignPositionAxis(tmp, going_to, ref inc);
            this.direction_guard = this.direction = going_to;
            going_to = new_going_to;


            Debug.Assert(inc <= src_inc);
            Debug.Assert(inc > 0);
            Debug.Assert(Math.Abs(Point.Y - tmp.Y) <= src_inc);
            Debug.Assert(Math.Abs(Point.X - tmp.X) <= src_inc);

            DoublePoint? tmp2 = null;
            bool conflict_solved = false;

            IEnumerable<Direction> dirs = [going_to];
            if (!IsSpecular(going_to, direction))
                dirs = dirs.Concat([direction]);
            dirs = dirs.Concat(DIRECTION_FALLBACKS[going_to]);

            bool force_guard = false;

            foreach (var i_direction in dirs)
            {
                if (IsSpecular(i_direction, going_to))
                    continue;

                tmp2 = copy_apply_increment(tmp, i_direction, inc).Normalize();
                if (!going_to_wall(tmp2.ToIntFloorPoint()))
                {
                    if (going_to_wall(copy_apply_increment(tmp, i_direction, 1).Normalize().ToIntFloorPoint()))
                    {
                        if (i_direction == Direction.Up || i_direction == Direction.Down)
                        {
                            var new_y = Math.Floor(tmp2.Y) + 0.5;
                            tmp2.SetY(i_direction == Direction.Up ? Math.Max(tmp2.Y, new_y) : Math.Min(new_y, tmp2.Y));

                            if (tmp2.Y == new_y)
                            {
                                force_guard = true;
                                continue;
                            }
                        }
                        else
                        {
                            var new_x = Math.Floor(tmp2.X) + 0.5;
                            tmp2.SetX(i_direction == Direction.Left ? Math.Max(tmp2.X, new_x) : Math.Min(new_x, tmp2.X));

                            if (tmp2.X == new_x)
                            {
                                force_guard = true;
                                continue;
                            }
                        }

                    }

                    going_to = i_direction;
                    if (force_guard)
                        transaction.SuggestedFallback = going_to;

                    conflict_solved = true;
                    break;
                }
            }

            if (!conflict_solved)
            {
                tmp.Normalize();
                Point.Update(tmp.X, tmp.Y);
                Debug.Assert(Point.X == tmp.X || Point.Y == tmp.Y);
                return going_to;
            }

            Debug.Assert(tmp2 is not null);
            Point.Update(tmp2);
            Debug.Assert(Point.X == tmp.X || Point.Y == tmp.Y);

            return going_to;

            bool going_to_wall(IntPoint point)
            {
                return transaction.At(point).OfType<IPacmanUnwalkable>().Any();
            }

            DoublePoint copy_apply_increment(DoublePoint point, Direction direction, double v)
            {
                var rt = point.Clone();
                apply_increment(rt, direction, v);
                return rt;
            }


            void apply_increment(DoublePoint point, Direction direction, double v)
            {
                if (direction == Direction.Left)
                    point.SetX(point.X - v);
                else if (direction == Direction.Right)
                    point.SetX(point.X + v);
                else if (direction == Direction.Up)
                    point.SetY(point.Y - v);
                else if (direction == Direction.Down)
                    point.SetY(point.Y + v);
            }
        }

        private void UpdatePosition(Direction going_to, GameMapTransaction transaction, double inc)
        {
            if (going_to == direction)
                direction = direction_guard;

            going_to = _UpdatePosition(going_to, transaction, inc);

            Point.SetX(Math.Round(Point.X, 5));
            Point.SetY(Math.Round(Point.Y, 5));

            if (going_to != direction)
            {
                direction_guard = direction;
                direction = going_to;
            }
        }


        public override void Process(GameMapTransaction transaction)
        {
            var entries = transaction.At(Point.ToIntFloorPoint());
            var died = false;
            foreach (var item in entries)
            {
                if (item is GhostMapEntry x && !died)
                    died = x.CheckKill(this);
            }
            // todo handled die

            var active_effects = Effects.Where(x => x.IsActive).DefaultIfEmpty(BaseEffect);
            var inc = Math.Min(active_effects.Select(x => x.PatchSpeed()).Sum(), 1);

            this.CanEatGhosts = active_effects.Any(x => x.CanEatGhosts);

            Direction? new_direction = transaction.Status.Left ? Direction.Left : transaction.Status.Right ? Direction.Right : transaction.Status.Top ? Direction.Up : transaction.Status.Bottom ? Direction.Down : null;

            if (new_direction is not null)
            {
                if (direction == 0)
                    direction = new_direction.Value;
                UpdatePosition(new_direction.Value, transaction, inc);
            }

            foreach (var effect in active_effects)
                effect.Tick(transaction, (TransformGroup)Element.RenderTransform);

            foreach (var item in Effects)
                item.CheckTrigger(transaction, entries);

            foreach (var item in entries)
            {
                if (item is FoodMapEntry)
                   transaction.Drop(item);
            }

        }


        private Direction last_draw_direction = Direction.Right;
        private RotateTransform rotate_animation = new RotateTransform();


        private void _DrawDirection()
        {
            if (IsSpecular(direction, last_draw_direction))
            {
                rotate_animation.Angle = (int)direction;
                return;
            }

            var x = (TransformGroup)Element.RenderTransform;
            x.AddUniqueRef(rotate_animation);

            int currentAngle = ((int)last_draw_direction + 360) % 360;
            int targetAngle = ((int)direction + 360) % 360;

            int delta = targetAngle - currentAngle;

            if (delta > 180)
                targetAngle = currentAngle - (360 - delta);
            else if (delta < -180)
                targetAngle = currentAngle + 360 + delta;

            var animation = new DoubleAnimation
            {
                From = currentAngle,
                To = targetAngle,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };


            rotate_animation.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void DrawDirection()
        {
            if (last_draw_direction == direction)
                return;

            rotate_animation.BeginAnimation(RotateTransform.AngleProperty, null);
            _DrawDirection();

            last_draw_direction = direction;
        }

        public void Draw(Canvas canvas, double scale)
        {
            DrawDirection();

            var active_effects = Effects.Where(x => x.IsActive).DefaultIfEmpty(BaseEffect);
            foreach (var effect in active_effects)
                effect.Transform((TransformGroup)Element.RenderTransform);
        }

        public void AddPoints(double v)
        {
        }

        public PacmanMapEntry() : base(new Image())
        {
            Effects = [new PowerPelletPacmanEffect(this)];
            BaseEffect = new BasePacmanEffect(this);

            Point.SetY(11.5);
            Point.SetX(17.5);

            var transform = Matrix.Identity;
            transform.RotateAt((int)direction, 0.5, 0.5);
            Element.RenderTransformOrigin = new(0.5, 0.5);
            Element.RenderTransform = new TransformGroup();

            AnimationBehavior.SetSourceUri((Image)Element, ResourcesLoader.GetUri(ResourcesLoader.PACMAN_PATH));
        }
    }
}