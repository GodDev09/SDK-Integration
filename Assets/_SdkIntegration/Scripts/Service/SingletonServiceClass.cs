namespace ATSoft
{
    public class SingletonServiceClass<T> where T : class, new()
    {
        public static T Instance { get; } = new T();
    }
}