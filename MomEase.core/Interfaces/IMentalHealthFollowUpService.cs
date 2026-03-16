using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IMentalHealthFollowUpService
    {
        Task CreateFollowUpPlanAsync(int userId, int assessmentResultId, string severityLevel);
        Task SendDueAssessmentRemindersAsync();
        Task SendDueTipsAsync();
    }
}
