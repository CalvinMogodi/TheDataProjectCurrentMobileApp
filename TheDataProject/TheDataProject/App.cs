using System;

namespace TheDataProject
{
    public class App
    {
        public static void Initialize()
        {
            ServiceLocator.Instance.Register<IDataStore<Facility>, MockDataStore>();
        }
    }
}
