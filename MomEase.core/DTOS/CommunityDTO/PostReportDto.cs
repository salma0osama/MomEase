using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.CommunityDTO
{
    public class CreateReportDto
    {
        public string Reason { get; set; } = string.Empty;
    }
     public class ReviewReportDto
        {
            // Dismiss = رفض البلاغ, DeletePost = حذف البوست, Warn = تحذير
            public string Action { get; set; } = string.Empty;
            public string? AdminNote { get; set; }
        }

        public class PostReportDto
        {
            public int ReportId { get; set; }
            public int PostId { get; set; }
            public int ReporterId { get; set; }
            public string ReporterName { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string? ReviewedByName { get; set; }
            public string? Action { get; set; }
            public string? AdminNote { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ReviewedAt { get; set; }
            public bool IsReviewed { get; set; }
        }
    }
