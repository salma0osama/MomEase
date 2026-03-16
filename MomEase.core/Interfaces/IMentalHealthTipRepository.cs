using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IMentalHealthTipRepository
    {
        Task<MentalHealthTip?> GetRandomTipAsync(int userId, string severityLevel);
        Task MarkTipAsSentAsync(int userId, int tipId);
    }
}
