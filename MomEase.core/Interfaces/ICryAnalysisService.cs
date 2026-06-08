using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.CryAnalysisDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{

    public interface ICryAnalysisService
    {
        Task<CryAnalysisResponseDto> AnalyzeNewAudioAsync(int userId, CryAnalysisRequestDto request, IFormFile audio);
        Task<List<CryAnalysisResponseDto>> GetUserAnalysesAsync(int userId);
        Task<List<CryAnalysisResponseDto>> GetChildAnalysesAsync(int childId);
        Task<CryAnalysisResponseDto> GetAnalysisByIdAsync(int id);
        Task DeleteAnalysisAsync(int id);
    }
}
