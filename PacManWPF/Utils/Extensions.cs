using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PacManWPF.Game.PGs;
using System.Windows.Controls;

namespace PacManWPF.Utils
{

    public static class Extensions
    {
        public static System.Drawing.Point Fix(this System.Drawing.Point point)
        {
            if (point.X == -1)
                point.X = Config.CHUNK_WC - 1;
            else if (point.X == Config.CHUNK_WC)
                point.X = 0;

            if (point.Y == -1)
                point.Y = Config.CHUNK_HC - 1;
            else if (point.Y == Config.CHUNK_HC)
                point.Y = 0;

            return point;
        }

        public static string ZFill(this string src, int len, char filler = '0')
        {
            if (src.Length > len)
                return src;

            return new string(filler, len - src.Length) + src;
        }
    }
}
