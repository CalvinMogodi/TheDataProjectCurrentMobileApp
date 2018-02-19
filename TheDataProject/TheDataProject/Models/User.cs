using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDataProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string RespondMessage { get; set; }
        public string Role { get; set; }
        public List<Facility> Facilities { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ModifiedUserId { get; set; }
    }
}
