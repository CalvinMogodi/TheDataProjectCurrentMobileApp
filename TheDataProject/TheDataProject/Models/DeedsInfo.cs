using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDataProject.Models
{
    public class DeedsInfo
    {
        public int Id { get; set; }
        public string ErFNumber { get; set; }
        public string TitleDeedNumber { get; set; }
        public string Extent { get; set; }
        public string OwnerInfomation { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedUserId { get; set; }
    }
}
