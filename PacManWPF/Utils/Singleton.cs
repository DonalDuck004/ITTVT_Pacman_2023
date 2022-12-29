using System;

namespace PacManWPF.Utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Singleton<T> where T : Singleton<T>
    {
        public static T INSTANCE = (T)Activator.CreateInstance(typeof(T), true);
    }
}
