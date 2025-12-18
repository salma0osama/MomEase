using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class PostReports
    {
        public class PostReport
        {
            public int Id { get; set; }
            public string Reason { get; set; }
            public DateTime CreatedAt { get; set; }

            // ========= Reporter (Mother) =========
            public string ReporterId { get; set; }
            public Users Reporter { get; set; }

            // ========= Reviewed By (Admin) =========
            public string? ReviewedById { get; set; }
            public Users ReviewedBy { get; set; }
        }


        // Navigation Properties
        public virtual CommunityPosts Post { get; set; }
        public virtual Users User { get; set; }
    }
}
