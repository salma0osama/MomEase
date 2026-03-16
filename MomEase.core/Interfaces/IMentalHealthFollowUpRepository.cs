using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IMentalHealthFollowUpRepository
    {
        Task<MentalHealthFollowUp> CreateAsync(MentalHealthFollowUp followUp);
        Task<MentalHealthFollowUp?> GetActiveByUserIdAsync(int userId);
        Task<IEnumerable<MentalHealthFollowUp>> GetDueAssessmentRemindersAsync();
        Task<IEnumerable<MentalHealthFollowUp>> GetDueTipsAsync();
        Task<MentalHealthFollowUp> UpdateAsync(MentalHealthFollowUp followUp);
        Task CompleteFollowUpAsync(int followUpId);
    }
}
