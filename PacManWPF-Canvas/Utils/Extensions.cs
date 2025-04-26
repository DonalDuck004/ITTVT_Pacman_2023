using System.Windows.Media;

namespace PacManWPF_Canvas.Utils
{
    public static class Extensions
    {
        public static void RemoveRef(this TransformGroup transform_group, Transform transform)
        {
            var idx = -1;
            for (int i = 0; i < transform_group.Children.Count; i++)
            {
                if (ReferenceEquals(transform_group.Children[i], transform))
                {
                    idx = i;
                    break;
                }
            }

            if (idx == -1)
                return;

            transform_group.Children.RemoveAt(idx);
        }

        public static void AddUniqueRef(this TransformGroup transform_group, Transform transform)
        {

            transform_group.RemoveRef(transform);
            transform_group.Children.Add(transform);
        }
    }
}