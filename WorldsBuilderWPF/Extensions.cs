using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WorldsBuilderWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public static class Extensions
    {
        public static string ZFill(this string src, int len, char filler = '0')
        {
            if (src.Length > len)
                return src;

            return new string(filler, len - src.Length) + src;
        }

        public static IEnumerable<T[]> Split<T>(this IEnumerable<T> array, int size)
        {
            T[] rt = array.ToArray();
            int to = rt.Length / size;

            for (int i = 0; i < to; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }
    }
}
