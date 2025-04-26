namespace PacManWPF.Utils
{
    public class Singleton<T> where T : Singleton<T>
    {
        public static T INSTANCE = (T)Activator.CreateInstance(typeof(T), true)!;
    }
}
