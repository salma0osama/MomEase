using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AdminDTO
{
    public class ChangeStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public bool IsActive { get; set; } // true = تفعيل, false = تعطيل
    }
}
