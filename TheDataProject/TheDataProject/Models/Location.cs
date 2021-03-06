﻿using System;
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
        public string Region { get; set; }
        public GPSCoordinate GPSCoordinates { get; set; }
        public List<BoundryPolygon> BoundryPolygon { get; set; }
        public int? FacilityId { get; set; }
    }
}
