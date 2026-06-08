using MomEase.core.DTOS.CryAnalysisDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface ICryReasonsService
    {
        Task<List<CryReasonResponseDto>> GetAllAsync();
        Task<CryReasonResponseDto> GetByIdAsync(int id);
        Task<CryReasonResponseDto> CreateAsync(CryReasonCreateDto dto);
        Task<CryReasonResponseDto> UpdateAsync(int id, CryReasonUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
