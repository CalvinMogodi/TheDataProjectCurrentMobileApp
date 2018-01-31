using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDataProject.Models
{
    public class BoundryPolygon
    {
        public int Id { get; set; }
        public string PolygonType { get; set; }
        public string Remarks { get; set; }
        public string Actions { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
    }
}
