using System;
using System.Collections.ObjectModel;
using TheDataProject.Models;

namespace TheDataProject
{
    public class Facility
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<Building> Buildings { get; set; }
    }
}
