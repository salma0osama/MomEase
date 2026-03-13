using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.DeviceTokenDto
{
    public class RegisterDeviceTokenDto
    {
        [Required]
        [MaxLength(500)]
        public string DeviceToken { get; set; }
    }
}
