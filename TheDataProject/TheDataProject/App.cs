using System;
using TheDataProject.Models;

namespace TheDataProject
{
    public class App
    {
        public static void Initialize()
        {
            ServiceLocator.Instance.Register<IDataStore<Facility, Building, User,Picture, DBPicture, Location, Person, DeedsInfo>, MockDataStore>();
        }
    }
}
