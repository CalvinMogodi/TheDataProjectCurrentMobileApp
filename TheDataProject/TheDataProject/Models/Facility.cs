using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TheDataProject.Models;

namespace TheDataProject
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientCode { get; set; }
        public string SettlementType { get; set; }
        public string Zoning { get; set; }      
        public string IDPicture { get; set; }
        public BoundryPolygon Polygon { get; set; }
        public DeedsInfo DeedsInfo { get; set; }
        public Person ResposiblePerson { get; set; }
        public Location Location { get; set; }
        public List<Building> Buildings { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ModifiedUserId { get; set; }
        public byte[] Photo { get; set; }
    }
}
