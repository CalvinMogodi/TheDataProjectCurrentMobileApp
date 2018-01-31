using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDataProject.Models
{
    public class Building
    {
        public int Id { get; set; }
        public string BuildingNumber { get; set; }
        public string Name { get; set; }
        public string BuildingType { get; set; }
        public string BuildingStandard { get; set; }
        public string Status { get; set; }
        public int NumberOfFloors { get; set; }
        public double FootPrintArea { get; set; }
        public double ImprovedArea { get; set; }
        public string Heritage { get; set; }
        public string OccupationYear { get; set; }
        public bool DisabledAccess { get; set; }
        public string DisabledComment { get; set; }
        public string ConstructionDescription { get; set; }
        public GPSCoordinate GPSCoordinates { get; set; }
        public string Photo { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedUserId { get; set; }

    }
}
