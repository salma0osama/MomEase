using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS
{
    public class ResetPasswordDto
    {
        [Required]
        public string Email { get; set; }
        public string OtpCode { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        //[Required]
        //[Compare("NewPassword")]
        //public string ConfirmPassword { get; set; }
    }
}
