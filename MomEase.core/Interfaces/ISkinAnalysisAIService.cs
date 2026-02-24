using Microsoft.AspNetCore.Http;
using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISkinAnalysisAIService
    {
        Task<SkinAnalysisDiseaseName> AnalyzeImageAsync(IFormFile image);
    }
}
