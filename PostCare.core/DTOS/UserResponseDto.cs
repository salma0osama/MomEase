using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? Age { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ChildrenCount { get; set; }
        public bool HasMotherProfile { get; set; }
    }
}
