using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using PacManWPF.Game;
using PacManWPF.Game.Worlds;
using PacManWPF.Utils;

namespace PacManWPF
{
    public partial class UIWindow : Window
    {
        private void OnClosed(object sender, EventArgs e)
        {
            SoundEffectsPlayer.StopAll();
            // TODO Wait for scoredb thread
            Environment.Exit(0);
        }

    }
}
