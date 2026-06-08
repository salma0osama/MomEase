using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.CryAnalysisDto;
using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ICryAnalysisAIService
    {
        Task<(CryReasonName prediction, double confidence, Dictionary<string, double> allScores)>
            AnalyzeAudioAsync(IFormFile audio);
    }

    
}
