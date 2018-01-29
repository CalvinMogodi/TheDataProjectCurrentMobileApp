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
        public User UserType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string IdNumber { get; set; }
        public string EmployeeNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
    }
}
