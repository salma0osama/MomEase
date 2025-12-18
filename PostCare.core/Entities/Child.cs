using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Child
    {
        public int ChildId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public FeedingType? FeedingType { get; set; }
        public string PhotoUrl { get; set; }

        // Navigation Properties
        public virtual Users User { get; set; }
        public virtual ICollection<GrowthRecords> GrowthRecords { get; set; }
        public virtual ICollection<GrowthReports> GrowthReports { get; set; }
        public virtual ICollection<ChildSleepRecord> ChildSleepRecords { get; set; }
        public virtual ICollection<ChildFeedingRecord> ChildFeedingRecords { get; set; }
        public virtual ICollection<ChildVaccination> ChildVaccinations { get; set; }
        public virtual ICollection<SkinAnalyses> SkinAnalyses { get; set; }
        public virtual ICollection<CryAnalyses> CryAnalyses { get; set; }
    }

}
