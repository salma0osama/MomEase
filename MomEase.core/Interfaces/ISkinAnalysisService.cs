using Microsoft.AspNetCore.Http;
using MomEase.core.DTOS.SkinAnalysisDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ISkinAnalysisService
    {
        Task<SkinAnalysisResponseDto> AnalyzeNewImageAsync(
            int userId,
            SkinAnalysisRequestDto request,
            IFormFile image);
        Task<List<SkinAnalysisResponseDto>> GetUserAnalysesAsync(int userId);
        Task<List<SkinAnalysisResponseDto>> GetChildAnalysesAsync(int childId);
        Task<SkinAnalysisResponseDto> GetAnalysisByIdAsync(int id);
        Task DeleteAnalysisAsync(int id);
        Task<List<DiseaseDto>> GetAllDiseasesAsync();
        Task<DiseaseDto> GetDiseaseByIdAsync(int id);
    }
}
