using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.AdminDTO
{
    public class ChangeRoleDto
    {
        [Required(ErrorMessage = "Role is required")]
        public Role Role { get; set; } // MOTHER or ADMIN
    }
}
