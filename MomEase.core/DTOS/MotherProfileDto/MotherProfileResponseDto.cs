using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.MotherProfileDto
{
    /// <summary>
    /// DTO for mother profile response
    /// </summary>
    public class MotherProfileResponseDto
    {
        public int MotherId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsFirstTimeMother { get; set; }
        public int NumberOfChildren { get; set; }
        public string? MentalHealthStatus { get; set; }
        public string? HealthStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
