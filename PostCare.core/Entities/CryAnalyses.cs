using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class CryAnalyses
    {
        public int CryId { get; set; }
        public int UserId { get; set; }
        public int ChildId { get; set; }
        public string AudioUrl { get; set; }
        public string Result { get; set; }
        public int? CryreasonId { get; set; }

        // Navigation Properties
        public virtual Users User { get; set; }
        public virtual Child Child { get; set; }
        public virtual CryReasons CryReason { get; set; }
    }
}
