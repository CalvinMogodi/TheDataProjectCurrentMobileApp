using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDataProject.Models
{
    public class Location
    {
        public string Id { get; set; }
        public string LocalMunicipality { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string Province { get; set; }
        public GPSCoordinate Coordinates { get; set; }
        public BoundryPolygon BoundaryPolygon { get; set; }
    }
}
